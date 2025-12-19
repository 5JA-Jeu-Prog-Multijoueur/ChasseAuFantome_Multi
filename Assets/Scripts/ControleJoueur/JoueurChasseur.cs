using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

// Script du joueur chasseur (multijoueur avec Netcode)
public class JoueurChasseur : NetworkBehaviour
{
    [Header("Temps")]
    // Temps total de la partie
    public float tempsDepars = 500f;
    // Image UI représentant le temps restant
    public Image niveauTemps;
    // Canvas affiché à la fin de la partie
    public GameObject CanvasFin;

    [Header("Lampe")]
    // Lumière du chasseur
    public Light spotLight;
    // Distance du rayon de détection
    public float rayDistance = 20f;
    // Couches détectées par le raycast
    public LayerMask hitLayers;

    // Temps actuel synchronisé sur le réseau (écrit uniquement par le serveur)
    private NetworkVariable<float> tempsActuel =
        new NetworkVariable<float>(
            120f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // État de la lampe (allumée / éteinte) synchronisé sur le réseau
    private NetworkVariable<bool> lampeAllumee =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // Appelé quand l'objet réseau apparaît
    public override void OnNetworkSpawn()
    {
        // Si ce joueur n'est pas le propriétaire, on désactive la lampe
        if (!IsOwner)
        {
            if (spotLight) spotLight.enabled = false;
            return;
        }

        // Bloque le curseur au centre de l'écran
        Cursor.lockState = CursorLockMode.Locked;

        // Abonnement aux changements de valeurs réseau
        lampeAllumee.OnValueChanged += OnLampeChange;
        tempsActuel.OnValueChanged += OnTempsChange;

        // Mise à jour initiale de l'UI
        UpdateUI();
    }

    void Update()
    {
        // Seul le joueur propriétaire peut contrôler ce script
        if (!IsOwner) return;

        // Gestion de la lampe
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Demande au serveur d'allumer/éteindre la lampe
            ToggleLampeServerRpc();
        }

        // Raycast effectué chaque frame
        DoRaycast();
    }

    // RPC exécuté sur le serveur
    [ServerRpc]
    void ToggleLampeServerRpc()
    {
        // Inverse l'état de la lampe
        lampeAllumee.Value = !lampeAllumee.Value;

        // Si la lampe est allumée, on lance un raycast côté serveur
        if (lampeAllumee.Value)
            DoRaycast(); // runs on server because this is a ServerRpc
    }

    // Appelé quand l'état de la lampe change
    void OnLampeChange(bool oldValue, bool newValue)
    {
        // Active ou désactive la lumière
        if (spotLight)
            spotLight.enabled = newValue;

        // Ancienne logique possible
        // if (newValue)
        //     DoRaycast();
    }

    // Fonction de détection par rayon
    void DoRaycast()
    {
        RaycastHit hit;

        // Origine du rayon (position de la lampe)
        Vector3 origin = spotLight.transform.position;
        // Direction du rayon (devant la lampe)
        Vector3 direction = spotLight.transform.forward;

        // Dessine le rayon en rouge dans la scène (debug)
        Debug.DrawRay(origin, direction * rayDistance, Color.red);

        // Lance le raycast
        if (Physics.Raycast(origin, direction, out hit, rayDistance))
        {
            // Dessine une ligne jusqu'au point d'impact
            Debug.DrawLine(origin, hit.point, Color.yellow);

            // Affiche les infos de l'objet touché
            Debug.Log($"Hit: {hit.collider.name} | Tag: {hit.collider.tag}");

            // Vérifie si l'objet touché est un fantôme
            if (hit.collider.CompareTag("fantome"))
            {
                // Récupère le script JoueurFantome
                JoueurFantome fantome =
                    hit.collider.GetComponent<JoueurFantome>();
                // hit.collider.GetComponentInParent<JoueurFantome>();

                if (fantome != null)
                {
                    // Applique des dégâts au fantôme
                    Debug.Log("Le fantôme perd sa vie");
                    fantome.PrendreDegatsServerRpc(20f);
                    
                    // Déclenche la fin de partie avec victoire
                    SceneFinManager.Instance.AnnoncerVictoireEtChargerSceneServerRpc(0);
                }
                else
                {
                    // Avertissement si le script n'est pas trouvé
                    Debug.LogWarning("Collider tagged 'fantome' but no JoueurFantome found");
                }
            }
        }
    }

    // Appelé quand le temps change
    void OnTempsChange(float oldValue, float newValue)
    {
        // Met à jour l'UI
        UpdateUI();

        // Si le temps est écoulé, fin de partie
        if (newValue <= 0)
            FinDePartie();
    }

    // Met à jour la barre de temps
    void UpdateUI()
    {
        if (!IsOwner) return;

        if (niveauTemps)
            niveauTemps.fillAmount = tempsActuel.Value / tempsDepars;
    }

    // Affiche l'écran de fin
    void FinDePartie()
    {
        if (CanvasFin)
            CanvasFin.SetActive(true);
    }
}
