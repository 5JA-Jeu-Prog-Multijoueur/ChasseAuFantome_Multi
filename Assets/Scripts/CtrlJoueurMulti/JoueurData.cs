using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    [Header("Contrôles")]
    public JoueurChasseur ChasseurController;
    public JoueurFantome FantomeController;

    [Header("Rôle du joueur")]
    public NetworkVariable<PlayerRole> role = new NetworkVariable<PlayerRole>(
        PlayerRole.Fantome,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // ==============================
        // 1) Si ce n’est pas notre joueur → désactiver les contrôles/caméras locales
        // ==============================
        if (!IsOwner)
        {
            ChasseurController.enabled = false;
            FantomeController.enabled = false;
            return;
        }

        // ==============================
        // 2) Activation du script et de la caméra selon le rôle
        // ==============================
        if (role.Value == PlayerRole.Chasseur)
        {
            ChasseurController.enabled = true;
            FantomeController.enabled = false;
        }
        else
        {
            ChasseurController.enabled = false;
            FantomeController.enabled = true;
        }

        // ==============================
        // 3) S’abonner aux changements de rôle si jamais nécessaire
        // ==============================
        role.OnValueChanged += OnRoleChanged;


        // Debug : affichage du rôle et du clientId
        Debug.Log($"Mon joueur est {role.Value} (ClientId={OwnerClientId})");
    }

    private void OnDestroy()
    {
        role.OnValueChanged -= OnRoleChanged;
    }

    // Si le rôle change dynamiquement
    private void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        if (!IsOwner) return;

        if (newRole == PlayerRole.Chasseur)
        {
            ChasseurController.enabled = true;
            FantomeController.enabled = false;
        }
        else
        {
            ChasseurController.enabled = false;
            FantomeController.enabled = true;
        }
    }
}

