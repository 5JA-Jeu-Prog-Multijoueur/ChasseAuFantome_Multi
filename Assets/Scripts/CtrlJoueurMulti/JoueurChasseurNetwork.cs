using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;  // IMPORTANT : nécessaire pour le multijoueur Netcode

public class JoueurChasseurNetwork : NetworkBehaviour
{
    // -----------------------------
    // VARIABLES DE DÉPLACEMENT
    // -----------------------------

    public float vitesse;         // vitesse de déplacement avant / arrière
    float forceDeplacement;       // valeur brute reçue du clavier (Z / S)
    float forceDeplacementH;      // valeur brute reçue du clavier (Q / D)
    public float vitesseTourne;   // vitesse de rotation de la souris

    Rigidbody rb; // Rigidbody pour déplacer le joueur

    // -----------------------------
    // VARIABLES DE TEMPS
    // -----------------------------
    public float tempsActuel;   // Temps restant (compte à rebours)
    public float tempsDepars;   // Temps initial (non utilisé ici, mais utile)
    public Image niveauTemps;   // Barre de temps à l'écran (UI)

    // -----------------------------
    // LAMPE TORCHE (FLASHLIGHT)
    // -----------------------------
    public Light spotLight; // Référence à la lampe

    // NetworkVariable = variable synchronisée sur tous les joueurs
    // Only Owner peut écrire, Everyone peut lire
    private NetworkVariable<bool> isLightOn = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner
    );

    // -----------------------------
    // RAYCAST DE LA LAMPE
    // -----------------------------
    public float rayDistance = 20f; // Distance maximale de la lumière
    public LayerMask hitLayers;     // Ce que la lampe peut toucher (fantôme, murs…)


    // ==========================================================
    //                        START()
    // ==========================================================

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Cache le curseur pour un FPS
        Cursor.lockState = CursorLockMode.Locked;

        // On s'assure que la lampe est éteinte au début
        if (spotLight != null)
            spotLight.enabled = false;

        // Chaque fois que la NetworkVariable change,
        // on met à jour la lampe pour *tous* les joueurs
        isLightOn.OnValueChanged += (oldValue, newValue) =>
        {
            if (spotLight != null)
                spotLight.enabled = newValue;
        };
    }


    // ==========================================================
    //                        UPDATE()
    // ==========================================================
    void Update()
    {
        // IMPORTANT :
        // Seul le joueur qui possède cet objet doit pouvoir le contrôler.
        // Sinon tous les joueurs contrôlent le même personnage !
        if (!IsOwner) 
            return;

        // --- DÉPLACEMENT ---
        forceDeplacement  = Input.GetAxis("Vertical") * vitesse;
        forceDeplacementH = Input.GetAxis("Horizontal") * vitesse;

        // --- ROTATION (souris) ---
        float valeurTourne = Input.GetAxis("Mouse X") * vitesseTourne;
        transform.Rotate(0f, valeurTourne, 0f);

        // --- GESTION DE LA LAMPE TORCHE (touche F) ---
        if (Input.GetKeyDown(KeyCode.F))
        {
            // On inverse la variable réseau
            isLightOn.Value = !isLightOn.Value;
        }

        // Si la lampe est allumée, on lance un Raycast
        if (isLightOn.Value)
            DoRaycast();
    }


    // ==========================================================
    //                    FIXED UPDATE (physique)
    // ==========================================================
    void FixedUpdate()
    {
        if (!IsOwner) 
            return;

        // --- GESTION DU TEMPS ---
        if (tempsActuel > 0)
            tempsActuel -= Time.deltaTime;

        // --- DÉPLACEMENT DU RIGIDBODY ---
        Vector3 move =
            (transform.forward * forceDeplacement) +
            (transform.right   * forceDeplacementH);

        // Pas de glissement, vitesse contrôlée
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }


    // ==========================================================
    //                      RAYCAST / LA LAMPE
    // ==========================================================
    void DoRaycast()
    {
        RaycastHit hit;

        // Debug ray visible dans la scène (ligne rouge)
        Debug.DrawRay(
            spotLight.transform.position,
            spotLight.transform.forward * rayDistance,
            Color.red
        );

        // On lance un vrai Raycast dans Unity
        if (Physics.Raycast(
                spotLight.transform.position,         // position de départ
                spotLight.transform.forward,          // direction
                out hit,                              // ce que ça touche
                rayDistance,                          // distance max
                hitLayers                             // couches valides
        ))
        {
            // Ligne jaune jusqu'à l'objet touché
            Debug.DrawLine(spotLight.transform.position, hit.point, Color.yellow);

            // Si on touche un fantôme
            if (hit.collider.CompareTag("fantome"))
            {
                // On récupère l’objet réseau touché
                var fantomeNet = hit.collider.GetComponentInParent<NetworkObject>();

                if (fantomeNet != null)
                {
                    // On demande au serveur d’appliquer les dégâts
                    DemanderDegatsServerRpc(fantomeNet.NetworkObjectId);
                }
            }
        }
    }


    // ==========================================================
    //      RPC SERVEUR : le serveur applique les dégâts
    // ==========================================================

    // ServerRpc = exécuté *uniquement* sur le serveur
    // Cela empêche la triche (les clients ne peuvent pas s'inventer des dégâts)
    [ServerRpc]
    void DemanderDegatsServerRpc(ulong cibleId)
    {
        // On retrouve l'objet réseau via son ID
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(cibleId, out NetworkObject obj))
        {
            JoueurFantome fantome = obj.GetComponent<JoueurFantome>();

            if (fantome != null)
            {
                // On applique les dégâts
                fantome.PrendreDegats(20f);
            }
        }
    }
}
