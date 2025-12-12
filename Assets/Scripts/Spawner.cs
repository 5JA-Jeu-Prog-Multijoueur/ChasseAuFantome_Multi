using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : NetworkBehaviour
{
    public GameObject hostPrefab;
    public GameObject clientPrefab;
    public BoxCollider spawnZone;

    private void Awake()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode mode,
        System.Collections.Generic.List<ulong> clientsCompleted,
        System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        // Only spawn players when the Game scene is loaded
        if (sceneName != "Jeu") return;  // ← change to your scene name

        foreach (ulong clientId in clientsCompleted)
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        GameObject prefabToSpawn;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            prefabToSpawn = hostPrefab;
        }
            
        else
        {
            prefabToSpawn = clientPrefab;
        }
            
        Vector3 spawnPos = GetRandomPointInArea(spawnZone);

        GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        obj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private Vector3 GetRandomPointInArea(BoxCollider area)
    {
        Vector3 center = area.center + area.transform.position;
        Vector3 size = area.size;

        float x = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
        float y = Random.Range(center.y - size.y / 2, center.y + size.y / 2);
        float z = Random.Range(center.z - size.z / 2, center.z + size.z / 2);

        return new Vector3(x, y, z);
    }

}
