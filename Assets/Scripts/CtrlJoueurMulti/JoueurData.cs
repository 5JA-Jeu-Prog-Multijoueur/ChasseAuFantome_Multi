using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    [Header("Visuels (enfants du prefab joueur)")]
    [SerializeField] private GameObject chasseurRoot;
    [SerializeField] private GameObject fantomeRoot;

    [Header("Contrôleurs (scripts)")]
    [SerializeField] private JoueurChasseur chasseurController;
    [SerializeField] private JoueurFantome fantomeController;

    [Header("Rôle")]
    public NetworkVariable<PlayerRole> role =
        new NetworkVariable<PlayerRole>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        // Sécurité : tout désactiver au départ (visuel + contrôle)
        chasseurRoot.SetActive(false);
        fantomeRoot.SetActive(false);

        chasseurController.enabled = false;
        fantomeController.enabled = false;

        // S'abonner AVANT d'appliquer la logique
        role.OnValueChanged += OnRoleChanged;

        // ESSENTIEL : appliquer l'état ACTUEL (même si la valeur n'a pas changé)
        AppliquerRole(role.Value);

        Debug.Log(
            $"[PlayerData] Spawn | Owner={IsOwner} | Role={role.Value} | ClientId={OwnerClientId}"
        );
    }

    public override void OnNetworkDespawn()
    {
        role.OnValueChanged -= OnRoleChanged;
    }

    private void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        AppliquerRole(newRole);
    }

    private void AppliquerRole(PlayerRole roleActuel)
    {
        chasseurRoot.SetActive(false);
        fantomeRoot.SetActive(false);

        if (roleActuel == default)
            return;

        if (roleActuel == PlayerRole.Chasseur)
        {
            chasseurRoot.SetActive(true);
        }

        else if(roleActuel == PlayerRole.Fantome)
        {
            fantomeRoot.SetActive(true);
        }

        chasseurController.enabled = IsOwner && roleActuel == PlayerRole.Chasseur;
        fantomeController.enabled = IsOwner && roleActuel == PlayerRole.Fantome;

        SetLocalCamera(chasseurRoot, IsOwner && roleActuel == PlayerRole.Chasseur);
        SetLocalCamera(fantomeRoot, IsOwner && roleActuel == PlayerRole.Fantome);
    }

    private void SetLocalCamera(GameObject root, bool enable)
{
    Camera cam = root.GetComponentInChildren<Camera>(true);
    if (cam != null)
        cam.enabled = enable;

    AudioListener audio = root.GetComponentInChildren<AudioListener>(true);
    if (audio != null)
        audio.enabled = enable;

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
