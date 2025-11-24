// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using Unity.Services.Authentication;
// using TMPro;
// using UnityEngine.UI;
// using System.Threading.Tasks;

// public class SceneFinUI : NetworkBehaviour
// {
//     // Rendre l'instance accessible (Singleton)
//     public static SceneFinUI Instance { get; private set; }

//     // Variable réseau pour synchroniser le gagnant 
//     // -1 = pas de gagnant défini
//     // 0 = Joueur 0 gagne
//     // 1 = Joueur 1 gagne
//     // 2 = Joueur 2 gagne
//     // etc...
//     public NetworkVariable<int> idJoueurGagnant = new NetworkVariable<int>(-1);

//     // Référence à l'UI qui sera trouvée dynamiquement dans la scène "SceneFin"
//     private TMP_Text _texteJoueurGagnant;
    
//     // Noms des scènes (à vérifier dans Build Settings)
//     private const string SceneJeu = "Jeu";
//     private const string SceneFin = "SceneFin"; 

//     // scene du lobby il faut juste passer par le bootstrap pour reset tout
//     private const string SceneLobby = "Bootstrap";

//     // --- LOGIQUE DE PERSISTANCE ET D'INITIALISATION ---

//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             // Assure que l'objet persiste entre les scènes (DDOL)
//             DontDestroyOnLoad(gameObject);
            
//             // S'abonne à l'événement de chargement de scène pour mettre à jour l'UI
//             SceneManager.sceneLoaded += OnSceneLoaded;
//         }
//         else if (Instance != this)
//         {
//             // Détruire les doublons (ceux créés dans une nouvelle scène)
//             Destroy(gameObject);
//         }
//     }

//     public override void OnNetworkSpawn()
//     {
//         // S'abonner à l'événement de changement de la NetworkVariable
//         idJoueurGagnant.OnValueChanged += UpdateUI;

//         // Mise à jour immédiate si la valeur est déjà définie (pour un client rejoignant tard)
//         if (idJoueurGagnant.Value != -1)
//         {
//              // Utilise la valeur actuelle pour la mise à jour de l'UI
//              UpdateUIText(_texteJoueurGagnant, idJoueurGagnant.Value);
//         }
//     }

//     // Fonction de Callback qui se déclenche après chaque chargement de scène
//     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         // Vérifiez si c'est la scène de fin de partie qui est chargée
//         if (scene.name == SceneFin)
//         {
//             // Trouvez le GameObject par son NOM
//             GameObject winningTextGO = GameObject.Find("TMP-JoueurGagnant");
            
//             if (winningTextGO != null)
//             {
//                 _texteJoueurGagnant = winningTextGO.GetComponent<TMP_Text>();
//             }

//             if (_texteJoueurGagnant != null)
//             {
//                 UpdateUIText(_texteJoueurGagnant, idJoueurGagnant.Value);
//             }

//             // Bouton 1: Rejouer (Mêmes joueurs)
//             Button rejouerButton = GameObject.Find("Btn-Rejouer")?.GetComponent<Button>();
//             if (rejouerButton != null)
//             {
//                 rejouerButton.onClick.RemoveAllListeners();
//                 rejouerButton.onClick.AddListener(Rejouer);
//             }
            
//             // Bouton 2: Nouvelle Partie (Retour Lobby)
//             Button nouvellePartieButton = GameObject.Find("Btn-NouvellePartie")?.GetComponent<Button>();
//             if (nouvellePartieButton != null)
//             {
//                 nouvellePartieButton.onClick.RemoveAllListeners();
//                 nouvellePartieButton.onClick.AddListener(NouvellePartie);
//             }
//         }
//         else
//         {
//             // Remet la référence à null lorsque l'on quitte la scène de fin
//             _texteJoueurGagnant = null;
//         }
//     }

//     private void OnDestroy()
//     {
//         // Se désabonner des événements pour éviter les fuites de mémoire
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//         if (idJoueurGagnant != null)
//         {
//             idJoueurGagnant.OnValueChanged -= UpdateUI;
//         }
//     }

//     // --- LOGIQUE D'AFFICHAGE ET DE MISE À JOUR ---

//     private void UpdateUI(int oldValue, int newValue)
//     {
//         // Mettre à jour l'UI avec la nouvelle valeur synchronisée
//         UpdateUIText(_texteJoueurGagnant, newValue);
//     }

//     // Fonction d'aide pour mettre à jour le texte
//     private void UpdateUIText(TMP_Text targetText, int winnerId)
//     {
//         if (targetText != null && winnerId != -1)
//         {
//             targetText.text = "Victoire! Le joueur " + winnerId + " a gagné la partie.";
//         }
//     }

//     // --- LOGIQUE D'APPEL ET DE CHANGEMENT DE SCÈNE (NETWORK) ---

//     // Fonction appelée par JeuManager.GererVictoire() côté HÔTE
//     [ServerRpc(RequireOwnership = false)]
//     public void AnnoncerVictoireEtChargerSceneServerRpc(int gagnant)
//     {
//         if (!IsServer) return;
        
//         // 1. Stocke et synchronise le gagnant
//         idJoueurGagnant.Value = gagnant; 
        
//         // 2. Change la scène pour tous les clients (sans déconnexion)
//         NetworkManager.Singleton.SceneManager.LoadScene(SceneFin, LoadSceneMode.Single);
//     }


//     // --- LOGIQUE DES BOUTONS DANS SCENEFIN ---

//     // Bouton 1 : Recommencer la partie (Mêmes joueurs, même session)
//     public void Rejouer()
//     {
//         if (!IsHost) return; 
        
//         // Réinitialiser la NetworkVariable
//         idJoueurGagnant.Value = -1;
        
//         RecommencerPartieServerRpc();
//     }

//     [ServerRpc(RequireOwnership = false)]
//     private void RecommencerPartieServerRpc()
//     {
//         // L'Hôte recharge la scène de Jeu pour tout le monde (NetworkManager reste actif)
//         NetworkManager.Singleton.SceneManager.LoadScene(SceneJeu, LoadSceneMode.Single);
//     }
    
//     // Bouton 2 : Nouvelle Partie (Retour au Lobby)
//     public void NouvellePartie()
//     {
//         if (!IsHost) return; 
        
//         // Réinitialiser la NetworkVariable
//         idJoueurGagnant.Value = -1;

//         RetournerAuLobbyServerRpc();
//     }

//     [ServerRpc(RequireOwnership = false)]
//     private void RetournerAuLobbyServerRpc()
//     {
//         // 1. Envoyer l'instruction aux clients de nettoyer et de changer de scène
//         RetourLobbyClientRPC();

//         // 2. Nettoyage de l'Hôte
//         if (AuthenticationService.Instance.IsSignedIn)
//         {
//             AuthenticationService.Instance.SignOut();
//         }
        
//         // 3. L'Hôte arrête son propre réseau
//         NetworkManager.Singleton.Shutdown(); 

//         // 4. Nettoyage des Singletons (Crucial pour le re-bootstrap)
//         CleanUpSingletons();

//         // 5. L'Hôte charge la scène du lobby en mode local
//         SceneManager.LoadScene(SceneLobby);
//     }

//     [ClientRpc]
//     private void RetourLobbyClientRPC()
//     {
//         if (IsHost) return; 

//         // 1. Nettoyage du Client
//         if (AuthenticationService.Instance.IsSignedIn)
//         {
//             AuthenticationService.Instance.SignOut(); 
//         }
        
//         // 2. Les Clients s'arrêtent
//         NetworkManager.Singleton.Shutdown();
        
//         // 3. Nettoyage des Singletons (Crucial pour le re-bootstrap)
//         CleanUpSingletons();

//         // 4. Chargement du Lobby
//         SceneManager.LoadScene(SceneLobby); 
//     }
    
//     // Fonction dédiée au nettoyage des objets persistants (Singletons)
//     private void CleanUpSingletons()
//     {   
//         if (NetworkManager.Singleton != null)
//         {
//             Destroy(NetworkManager.Singleton.gameObject); 
//         }
        
//         if (RelayManager.instance != null)
//         {
//             Destroy(RelayManager.instance.gameObject);
//         }
        
//         if (NavigationManager.singleton != null)
//         {
//             Destroy(NavigationManager.singleton.gameObject);
//         }
        
//         // Destruire CET objet aussi, car il sera recréé par le Bootstrap
//         if (Instance != null) 
//         {
//             Destroy(gameObject); 
//             Instance = null;
//         }
//     }
// }