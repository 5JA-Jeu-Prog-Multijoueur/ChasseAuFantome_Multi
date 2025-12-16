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
            PlayerRole.Fantome,
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
        //  VISUEL : visible pour TOUT LE MONDE
        chasseurRoot.SetActive(roleActuel == PlayerRole.Chasseur);
        fantomeRoot.SetActive(roleActuel == PlayerRole.Fantome);

        //  CONTRÔLE : seulement pour l'owner
        if (!IsOwner)
        {
            chasseurController.enabled = false;
            fantomeController.enabled = false;
            return;
        }

        chasseurController.enabled = roleActuel == PlayerRole.Chasseur;
        fantomeController.enabled = roleActuel == PlayerRole.Fantome;
    }
}
