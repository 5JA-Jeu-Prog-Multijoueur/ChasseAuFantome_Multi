using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    [Header("Contrôleurs")]
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
        // Sécurité
        if (chasseurController == null || fantomeController == null)
        {
            Debug.LogError("PlayerData : contrôleurs non assignés !");
            return;
        }

        // Toujours désactiver par défaut
        chasseurController.enabled = false;
        fantomeController.enabled = false;

        // S'abonner AVANT logique
        role.OnValueChanged += OnRoleChanged;

        // Appliquer état initial
        AppliquerRole(role.Value);

        Debug.Log(
            $"PlayerData spawn | Owner={IsOwner} | Role={role.Value} | ClientId={OwnerClientId}"
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
        // Seul l'owner peut contrôler
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
