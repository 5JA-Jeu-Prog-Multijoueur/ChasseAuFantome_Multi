using UnityEngine;
using TMPro;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager singleton;
    [SerializeField] private GameObject panelSelectionHostClient;
    [SerializeField] private GameObject panelClientConfig;

    [SerializeField] private GameObject PanelAttenteServeur;
    [SerializeField] private GameObject PanellAttenteClient;
    [SerializeField] private TMP_InputField champsIPClient;
    [SerializeField] public TextMeshProUGUI champsNbJoueurs;
    // Bouton pour lancer la partie côté serveur
    [SerializeField] private GameObject boutonLancerPartie;


    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /*Callback du bouton Host. 
    On désactive le panel de départ (Host/Client) et on active le panel d'attente côté côté host.
    Démarrage de la coroutine ConfigureTransportAndStartNgoAsHost() de la classe RelayManager.
    */
    public void NavigationPanelHost()
    {
        panelSelectionHostClient.SetActive(false);
        PanelAttenteServeur.SetActive(true);

        RelayManager.instance.StartCoroutine(RelayManager.instance.ConfigureTransportAndStartNgoAsHost());
    }

    /*Callback du bouton Client. 
    On désactive le panel de départ (Host/Client) et on active le panel d'attente côté côté client.
    */
    public void NavigationPanelClient()
    {
        panelSelectionHostClient.SetActive(false);
        panelClientConfig.SetActive(true);
    }

    /*Callback du bouton pour rejoindre la partie comme client en fournissant un code. 
    Démarrage de la coroutine ConfigureTransportAndStartNgoAsConnectingPlayer() de la classe RelayManager.
    */
    public void ClientRejointHote()
    {
        if (champsIPClient.text != "")
        {
            RelayManager.instance.StartCoroutine(RelayManager.instance.ConfigureTransportAndStartNgoAsConnectingPlayer());
        }
    }

    public void CachePanelsConfig()
    {
        panelSelectionHostClient.SetActive(false);
        panelClientConfig.SetActive(false);
    }

     /*Appelée par le GameManager, côté serveur uniquement. Affichage du panel d'attente de la connexion
     d'un 2e joueur 
    */
    public void AfficheAttenteServeur()
    {
        PanelAttenteServeur.SetActive(true);
    }

    /*Appelée par le GameManager, côté client uniquement. Affichage du panel d'attente que l'hôte
    lance la partie 
    */
    public void AfficheAttenteClient()
    {
        panelClientConfig.SetActive(false);
        PanellAttenteClient.SetActive(true);
    }

    // Affichage du bouton pour lancer la partie côté serveur
    public void AfficheBoutonLancerPartie()
    {
        boutonLancerPartie.SetActive(true);
    }
}
