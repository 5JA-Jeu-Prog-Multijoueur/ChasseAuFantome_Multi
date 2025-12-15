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
        if (!IsServer) return;

        if (scene.name == "Jeu")
        {
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
            var client = clients[i];

            // Vérifie qu’un player object n’existe pas déjà pour ce client
            if (NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(client.ClientId) != null)
                continue;

            // Instancie le PlayerRoot unique
            GameObject player = Instantiate(prefabJoueurParDefaut);
            NetworkObject netObj = player.GetComponent<NetworkObject>();

            // Spawn en tant que PlayerObject pour ce client
            netObj.SpawnAsPlayerObject(client.ClientId, true);

            // Récupère PlayerData pour assigner le rôle
            PlayerData pdata = player.GetComponent<PlayerData>();

            // Premier client connecté = Chasseur, les autres = Fantôme
            bool estChasseur = (i == 0);
            pdata.role.Value = estChasseur ? PlayerRole.Chasseur : PlayerRole.Fantome;

            Debug.Log($"Spawn {pdata.role.Value} pour Client {client.ClientId}");
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
