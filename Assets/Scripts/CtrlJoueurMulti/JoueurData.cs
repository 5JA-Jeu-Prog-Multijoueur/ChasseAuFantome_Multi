using Unity.Netcode;
using UnityEngine;

// =====================================================
// SCRIPT : PlayerData
// Rôle : Gère le rôle du joueur (Chasseur ou Fantôme)
//        ainsi que l'activation des visuels, contrôleurs
//        et caméra locale pour chaque joueur.
// =====================================================
public class PlayerData : NetworkBehaviour
{
    [Header("Visuels (enfants du prefab joueur)")]
    // Racine visuelle du chasseur (GameObject à activer)
    [SerializeField] private GameObject chasseurRoot;

    // Racine visuelle du fantôme (GameObject à activer)
    [SerializeField] private GameObject fantomeRoot;

    [Header("Contrôleurs (scripts)")]
    // Script de contrôle du chasseur
    [SerializeField] private JoueurChasseur chasseurController;

    // Script de contrôle du fantôme
    [SerializeField] private JoueurFantome fantomeController;

    [Header("Rôle")]
    // Variable réseau représentant le rôle du joueur
    // - Lisible par tous
    // - Écriture uniquement par le serveur
    public NetworkVariable<PlayerRole> role =
        new NetworkVariable<PlayerRole>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // =====================================================
    // SPAWN RÉSEAU
    // =====================================================
    public override void OnNetworkSpawn()
    {
        // Sécurité : tout désactiver au départ
        // - visuels
        // - scripts de contrôle
        chasseurRoot.SetActive(false);
        fantomeRoot.SetActive(false);

        chasseurController.enabled = false;
        fantomeController.enabled = false;

        // S'abonner au changement de rôle avant d'appliquer la logique
        role.OnValueChanged += OnRoleChanged;

        // Appliquer le rôle actuel même si la valeur n'a pas changé
        AppliquerRole(role.Value);

        // Log pour debug
        Debug.Log(
            $"[PlayerData] Spawn | Owner={IsOwner} | Role={role.Value} | ClientId={OwnerClientId}"
        );
    }

    // =====================================================
    // DESPAWN RÉSEAU
    // =====================================================
    public override void OnNetworkDespawn()
    {
        // Se désabonner de l'événement pour éviter les erreurs
        role.OnValueChanged -= OnRoleChanged;
    }

    // =====================================================
    // CHANGEMENT DE RÔLE
    // =====================================================
    private void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        // Appliquer le nouveau rôle dès qu'il change
        AppliquerRole(newRole);
    }

    // Active/désactive les visuels et scripts selon le rôle
    private void AppliquerRole(PlayerRole roleActuel)
    {
        // Tout désactiver par défaut
        chasseurRoot.SetActive(false);
        fantomeRoot.SetActive(false);

        // Si le rôle n'est pas défini, on quitte
        if (roleActuel == default)
            return;

        // Activation des visuels selon le rôle
        if (roleActuel == PlayerRole.Chasseur)
        {
            chasseurRoot.SetActive(true);
        }
        else if (roleActuel == PlayerRole.Fantome)
        {
            fantomeRoot.SetActive(true);
        }

        // Activer les scripts uniquement si le joueur est local et correspond au rôle
        chasseurController.enabled = IsOwner && roleActuel == PlayerRole.Chasseur;
        fantomeController.enabled = IsOwner && roleActuel == PlayerRole.Fantome;

        // Configurer la caméra et l'audio pour le joueur local
        SetLocalCamera(chasseurRoot, IsOwner && roleActuel == PlayerRole.Chasseur);
        SetLocalCamera(fantomeRoot, IsOwner && roleActuel == PlayerRole.Fantome);
    }

    // =====================================================
    // GESTION DE LA CAMERA LOCALE
    // =====================================================
    private void SetLocalCamera(GameObject root, bool enable)
    {
        // Récupère la caméra enfant (même inactive)
        Camera cam = root.GetComponentInChildren<Camera>(true);
        if (cam != null)
            cam.enabled = enable;

        // Récupère l'AudioListener enfant (même inactive)
        AudioListener audio = root.GetComponentInChildren<AudioListener>(true);
        if (audio != null)
            audio.enabled = enable;

        // Si la caméra est activée, on désactive la caméra principale de la scène
        if (enable && cam != null)
        {
            Camera sceneCam = Camera.main;
            if (sceneCam != null && !sceneCam.transform.IsChildOf(transform) && sceneCam != cam)
            {
                sceneCam.enabled = false;
                AudioListener al = sceneCam.GetComponent<AudioListener>();
                if (al != null)
                    al.enabled = false;
            }
        }
    }
}
