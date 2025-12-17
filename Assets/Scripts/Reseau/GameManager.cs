using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager singleton { get; private set; }

    [Header("Prefab joueur")]
    public GameObject prefabJoueurParDefaut;

    public Action OnDebutPartie;

    // =====================================================
    // SINGLETON
    // =====================================================
    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =====================================================
    // NETWORK CALLBACKS
    // =====================================================
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnNouveauClientConnecte;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnNouveauClientConnecte;
        }
    }

    // =====================================================
    // CONNEXION CLIENT
    // =====================================================
    private void OnNouveauClientConnecte(ulong clientId)
    {
        Debug.Log($"Client connecté : {clientId}");

        // UI côté CLIENT
        if (IsClient && !IsServer)
        {
            NavigationManager.singleton.AfficheAttenteClient();
            return;
        }

        // UI côté SERVEUR
        if (IsServer)
        {
            int count = NetworkManager.Singleton.ConnectedClients.Count;

            if (count == 1)
            {
                NavigationManager.singleton.AfficheAttenteServeur();
            }
            else if (count >= 2)
            {
                NavigationManager.singleton.AfficheBoutonLancerPartie();
            }

            UpdatePlayerCount();
        }
    }

    public void UpdatePlayerCount()
    {
        int count = NetworkManager.Singleton.ConnectedClients.Count;
        NavigationManager.singleton.champsNbJoueurs.text = count.ToString();
    }

    // =====================================================
    // LANCEMENT DE LA PARTIE
    // =====================================================
    public void LancerPartie()
    {
        if (!IsServer) return;

        // Charger la scène "Jeu" côté réseau
        NetworkManager.Singleton.SceneManager.LoadScene("Jeu", LoadSceneMode.Single);
    }

    // =====================================================
    // DÉMARRAGE HOST / CLIENT
    // =====================================================
    public void LancementHote(string ip)
    {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(ip, 7777);

        NetworkManager.Singleton.StartHost();
        NavigationManager.singleton.CachePanelsConfig();
    }

    public void LancementClient(string ip)
    {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(ip, 7777);

        NetworkManager.Singleton.StartClient();
        NavigationManager.singleton.CachePanelsConfig();
    }

    public void LancementClientRelay()
    {
        RelayManager.instance.StartCoroutine(
            RelayManager.instance.ConfigureTransportAndStartNgoAsConnectingPlayer()
        );
    }

    // =====================================================
    // CHARGEMENT SCÈNE
    // =====================================================
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded : {scene.name} | IsServer={IsServer}");
        if (!IsServer) return;

        if (scene.name == "Jeu")
        {
            // Spawn tous les joueurs via GameManager
            SpawnJoueursDansScene();

            DebutSimulation();
        }
    }

    // =====================================================
    // SPAWN DES JOUEURS
    // =====================================================

    private void SpawnJoueursDansScene()
    {
        if (!IsServer) return;

        var clients = NetworkManager.Singleton.ConnectedClientsList;

        for (int i = 0; i < clients.Count; i++)
        {
            ulong clientId = clients[i].ClientId;

            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.identity;

            if (Spawner.singleton != null && Spawner.singleton.spawnPoints.Length > 0)
            {
                int index = i % Spawner.singleton.spawnPoints.Length;
                spawnPos = Spawner.singleton.spawnPoints[index].position;
                spawnRot = Spawner.singleton.spawnPoints[index].rotation;
            }

            // Crée le joueur
            GameObject player = Instantiate(prefabJoueurParDefaut, spawnPos, spawnRot);
            NetworkObject netObj = player.GetComponent<NetworkObject>();

            // Spawn réseau — give ownership to the connected client
            netObj.SpawnWithOwnership(clientId);
            // netObj.SpawnAsPlayerObject(clientId, true);

            // Assigner le rôle avant le spawn
            PlayerData pdata = player.GetComponent<PlayerData>();
            pdata.role.Value = (i == 0) ? PlayerRole.Chasseur : PlayerRole.Fantome;

            Debug.Log($"Spawn Player {clientId} | Role={pdata.role.Value}");
        }
    }


    // =====================================================
    // DÉBUT DE SIMULATION
    // =====================================================
    public void DebutSimulation()
    {
        OnDebutPartie?.Invoke();
    }
}
