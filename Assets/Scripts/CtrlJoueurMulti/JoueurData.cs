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

        chasseurController.enabled = false;
        fantomeController.enabled = false;

        if (roleActuel == default)
            return;

        if (roleActuel == PlayerRole.Chasseur)
            chasseurRoot.SetActive(true);
        else
            fantomeRoot.SetActive(true);

        if (!IsOwner)
            return;

        chasseurController.enabled = roleActuel == PlayerRole.Chasseur;
        fantomeController.enabled = roleActuel == PlayerRole.Fantome;
    }

}
