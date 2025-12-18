using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using TMPro;
using UnityEngine.UI;

public class SceneFinManager : NetworkBehaviour
{
    // Rendre l'instance accessible (Singleton)
    public static SceneFinManager Instance { get; private set; }

    // Variable réseau pour synchroniser le gagnant 
    // -1 = pas de gagnant défini, 0+ = ID du joueur
    public NetworkVariable<int> idJoueurGagnant = new NetworkVariable<int>(-1);

    // Référence à l'UI
    private TMP_Text _texteJoueurGagnant;
    
    // Noms des scènes
    private const string SceneJeu = "Jeu";
    private const string SceneFin = "SceneFin"; 
    private const string SceneLobby = "Bootstrap";

    // --- LOGIQUE DE PERSISTANCE ET D'INITIALISATION ---

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        idJoueurGagnant.OnValueChanged += UpdateUI;

        // Mise à jour immédiate si déjà défini
        if (idJoueurGagnant.Value != -1)
        {
             UpdateUIText(_texteJoueurGagnant, idJoueurGagnant.Value);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneFin)
        {
            // 1. NETTOYAGE RADICAL DES JOUEURS (ORPHELINS)
            NettoyerObjetsJoueurs();

            // 2. CONFIGURATION CURSEUR
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 3. RECHERCHE UI
            GameObject winningTextGO = GameObject.Find("TxtGagnant");
            if (winningTextGO != null)
            {
                _texteJoueurGagnant = winningTextGO.GetComponent<TMP_Text>();
            }

            if (_texteJoueurGagnant != null)
            {
                UpdateUIText(_texteJoueurGagnant, idJoueurGagnant.Value);
            }

            // 4. CONFIGURATION BOUTONS
            ConfigurerBoutons();
        }
        else
        {
            _texteJoueurGagnant = null;
        }
    }

    private void NettoyerObjetsJoueurs()
    {
        // Recherche par type de script (PlayerData)
        PlayerData[] scripts = GameObject.FindObjectsByType<PlayerData>(FindObjectsSortMode.None);
        foreach (PlayerData p in scripts)
        {
            Destroy(p.gameObject);
        }

        // Recherche par nom de clone au cas où
        GameObject[] tousLesObjets = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject go in tousLesObjets)
        {
            if (go.name.Contains("JoueurParDefaut"))
            {
                Destroy(go);
            }
        }
    }

    private void ConfigurerBoutons()
    {
        Button rejouerButton = GameObject.Find("ButtonRejouer")?.GetComponent<Button>();
        if (rejouerButton != null)
        {
            rejouerButton.onClick.RemoveAllListeners();
            rejouerButton.onClick.AddListener(Rejouer);
            // Seul l'hôte peut relancer la scène
            rejouerButton.interactable = IsHost; 
        }
        
        Button nouvellePartieButton = GameObject.Find("ButtonNouvellePartie")?.GetComponent<Button>();
        if (nouvellePartieButton != null)
        {
            nouvellePartieButton.onClick.RemoveAllListeners();
            nouvellePartieButton.onClick.AddListener(NouvellePartie);
            nouvellePartieButton.interactable = IsHost;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (idJoueurGagnant != null)
        {
            idJoueurGagnant.OnValueChanged -= UpdateUI;
        }
    }

    // --- LOGIQUE D'AFFICHAGE ---

    private void UpdateUI(int oldValue, int newValue)
    {
        UpdateUIText(_texteJoueurGagnant, newValue);
    }

    private void UpdateUIText(TMP_Text targetText, int winnerId)
    {
        if (targetText != null && winnerId != -1)
        {
            targetText.text = "Victoire! Le joueur " + winnerId + " a gagné la partie.";
        }
    }

    // --- LOGIQUE RÉSEAU ---

    [ServerRpc(RequireOwnership = false)]
    public void AnnoncerVictoireEtChargerSceneServerRpc(int gagnant)
    {
        if (!IsServer) return;

        idJoueurGagnant.Value = gagnant;

        // Despawn propre des joueurs pour tout le monde
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                client.PlayerObject.Despawn(true);
            }
        }

        NetworkManager.Singleton.SceneManager.LoadScene(SceneFin, LoadSceneMode.Single);
    }

    public void Rejouer()
    {
        if (!IsHost) return; 
        idJoueurGagnant.Value = -1;
        RecommencerPartieServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RecommencerPartieServerRpc()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(SceneJeu, LoadSceneMode.Single);
    }
    
    public void NouvellePartie()
    {
        if (!IsHost) return; 
        idJoueurGagnant.Value = -1;
        RetournerAuLobbyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RetournerAuLobbyServerRpc()
    {
        RetourLobbyClientRPC();
        // Petit délai pour laisser le RPC partir avant le shutdown de l'hôte
        Invoke(nameof(ShutdownAndExit), 0.1f);
    }

    [ClientRpc]
    private void RetourLobbyClientRPC()
    {
        if (IsHost) return; 
        ShutdownAndExit();
    }

    private void ShutdownAndExit()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
        }
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        CleanUpSingletons();
        SceneManager.LoadScene(SceneLobby);
    }
    
    private void CleanUpSingletons()
    {   
        // Nettoyage final des joueurs
        NettoyerObjetsJoueurs();

        if (GameManager.singleton != null) Destroy(GameManager.singleton.gameObject);
        if (NetworkManager.Singleton != null) Destroy(NetworkManager.Singleton.gameObject); 
        if (RelayManager.instance != null) Destroy(RelayManager.instance.gameObject);
        if (NavigationManager.singleton != null) Destroy(NavigationManager.singleton.gameObject);
        
        if (Instance != null) 
        {
            Instance = null;
            Destroy(gameObject); 
        }
    }
}