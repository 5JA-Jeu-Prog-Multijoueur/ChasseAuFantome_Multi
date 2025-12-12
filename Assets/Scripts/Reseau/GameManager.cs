using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using Unity.VisualScripting;
using System.Data;
using System.Globalization;

public class GameManager : NetworkBehaviour
{
    public static GameManager singleton { get; private set; }
   
    public GameObject prefabChasseur;
    public GameObject prefabFantome;
    public Action OnDebutPartie; // Création d'une action auquel d'autres scripts pourront s'abonner.

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
    // Abonnement au callback OnClientConnectedCallback qui lancera la fonction OnNouveauClientConnecte.
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback += OnNouveauClientConnecte;
    }

    // Désabonnement du callback OnClientConnectedCallback.
    public override void OnNetworkDespawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback -= OnNouveauClientConnecte;
    }

    /* Fonction qui sera appelée lors du callback OnClientConnectedCallback
    Gestion de l'affichage et du début de la partie en fonction du nombre de clients connectés.
    Si juste un client : c'est l'hôte... on affiche un panneau d'attente
    Si deux client : on lance la partie
    */
    //private void OnNouveauClientConnecte(ulong obj)
    //{
    //  print("Un nouveau client est connecté, id : " + obj);
    //Si pas le serveur, on affiche le panel d'attente côté client. 
    //if (!IsServer)
    //{
    //  NavigationManager.singleton.AfficheAttenteClient();
    //return;
    //}

    /* Si on est le serveur :
    Si un seul client connecté (le serveur) on affiche la panel d'attente d'un second joueur
    Si deux client connecté, on affiche le panel avec le bouton qui permet de lancer la partie
    */
    //if (NetworkManager.Singleton.ConnectedClients.Count == 1)
    //{
    //  NavigationManager.singleton.AfficheAttenteServeur();


    //        }
    //      /* Lancement de la partie à 3 joueurs ou plus */
    //else if (NetworkManager.Singleton.ConnectedClients.Count >= 3)
    //    else if (NetworkManager.Singleton.ConnectedClients.Count >= 2 &&
    //NetworkManager.Singleton.ConnectedClients.Count <= 4)
    //{
    //  NavigationManager.singleton.AfficheBoutonLancerPartie();
    //}

    // Mise à jour du nombre de clients connectés
    //  UpdatePlayerCount();
    //}
    private void OnNouveauClientConnecte(ulong clientId)
    {
        print("Un nouveau client est connecté, id : " + clientId);

        // ============================================================
        // 1) SI ON EST DU CÔTÉ CLIENT (PAS LE SERVEUR)
        // ============================================================
        if (!IsServer)
        {
            NavigationManager.singleton.AfficheAttenteClient();
            return;
        }


        // ============================================================
        // 2) UI côté serveur
        // ============================================================
        if (NetworkManager.Singleton.ConnectedClients.Count == 1)
        {
            NavigationManager.singleton.AfficheAttenteServeur();
        }
        else if (NetworkManager.Singleton.ConnectedClients.Count >= 2 &&
                 NetworkManager.Singleton.ConnectedClients.Count <= 4)
        {
            NavigationManager.singleton.AfficheBoutonLancerPartie();
        }

        // ============================================================
        // 3) COMPTEUR DE JOUEURS
        // ============================================================
        UpdatePlayerCount();
    }

    public void LancerPartie()
    {
        if (!IsServer) return;


        NetworkManager.Singleton.SceneManager.LoadScene("Jeu", LoadSceneMode.Single);
    }



    // Mise à jour du nombre de joueurs connectés dans l'interface
    public void UpdatePlayerCount()
    {
        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
        NavigationManager.singleton.champsNbJoueurs.text = playerCount.ToString();
    }

    public static void ChargementSceneJeu()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Jeu", LoadSceneMode.Single);
    }

    public void LancementHote(string adresseIP)
    {
        UnityTransport utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(adresseIP, 7777);

        NetworkManager.Singleton.StartHost();
        NavigationManager.singleton.CachePanelsConfig();
    }

    public void LancementClient(string adresseIP)
    {
        UnityTransport utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(adresseIP, 7777);

        NetworkManager.Singleton.StartClient();
        NavigationManager.singleton.CachePanelsConfig();
    }

    public void LancementClientRelay()
    {
        RelayManager.instance.StartCoroutine(RelayManager.instance.ConfigureTransportAndStartNgoAsConnectingPlayer());

    }




    /*
    Dans cette fonction, on invoque l'action OnDebutPartie. Tous les scripts abonné à cette action exécuteront
    la fonction qu'ils ont associée à cette action.
    */
    public void DebutSimulation()
    {
        OnDebutPartie?.Invoke();
    }


    //public void CreationJoueurs()
    //{
    //    int nbJoueurs = NetworkManager.Singleton.ConnectedClients.Count;
    //
    //    foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
    //    {
    //        // Instanciation d’un joueur pour chaque client
    //        GameObject newPlayer = Instantiate(prefabFantome);
    //
    //        // Spawn avec ownership selon le client owner
    //        newPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(client.ClientId);
    //    }
    //}

    // Callback quand une scène est chargée
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

    public void SpawnJoueursDansScene()
    {
        if (!IsServer)
            return; // Seul le serveur spawn les joueurs

        // Récupérer tous les clients connectés
        var clients = NetworkManager.Singleton.ConnectedClientsList;

        for (int i = 0; i < clients.Count; i++)
        {
            var client = clients[i];

            // Choisir le prefab selon l’ordre de connexion
            GameObject prefabToSpawn = (i == 0) ? prefabChasseur : prefabFantome;

            // Instancier le joueur côté serveur
            GameObject playerInstance = Instantiate(prefabToSpawn);

            // Spawn réseau avec ownership du client
            NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(client.ClientId);

            // Assigner le rôle sur le NetworkVariable
            PlayerData pdata = playerInstance.GetComponent<PlayerData>();
            pdata.role.Value = (prefabToSpawn == prefabChasseur)
                               ? PlayerRole.Chasseur
                               : PlayerRole.Fantome;

            Debug.Log($"Spawned {pdata.role.Value} pour ClientId {client.ClientId}");
        }
    }



}


