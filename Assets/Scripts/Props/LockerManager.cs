using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class LockerManager : NetworkBehaviour
{
    public List<GameObject> lockers; // glisse tes 8 casiers ici
    public int lockersToActivate = 5;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SetupLockers();
    }

    void SetupLockers()
    {
        // Copie de la liste
        List<GameObject> shuffled = new List<GameObject>(lockers);

        // Mélange
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rnd = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rnd]) = (shuffled[rnd], shuffled[i]);
        }

        // Active les X premiers, désactive le reste
        for (int i = 0; i < shuffled.Count; i++)
        {
            bool active = i < lockersToActivate;
            SetLockerStateClientRpc(shuffled[i].GetComponent<NetworkObject>().NetworkObjectId, active);
        }
    }

    [ClientRpc]
    void SetLockerStateClientRpc(ulong netId, bool active)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netId, out var netObj))
        {
            netObj.gameObject.SetActive(active);
        }
    }
}
