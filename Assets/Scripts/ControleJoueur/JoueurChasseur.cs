using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;


public class JoueurChasseur : NetworkBehaviour
{
    public float vitesse;
    float forceDeplacement;
    float forceDeplacementH;
    public float vitesseTourne;
    public string objetEnMain;
    public Transform mains;

    Rigidbody rb;
    //Animator animator;

    public float tempsActuel; // Variable de gestion du temps
    public float tempsDepars;
    public Image niveauTemps;

    public GameObject CanvasFin; // Variable du canvas affichant le score


    // FLASHLIGHT
    public Light spotLight;
    private bool isOn = false; // État actuel

    public float rayDistance = 20f; // Distance maximale du faisceau
    public LayerMask hitLayers; // Ce que la lampe peur voir

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked; // Permet de ne plus voir le curseur

        if (spotLight != null)
        {
            spotLight.enabled = false; // Commence éteinte
        }
         

    }
   
    void Update()
    {
        if (!IsOwner) return;

        forceDeplacement  = Input.GetAxis("Vertical") * vitesse;
        forceDeplacementH = Input.GetAxis("Horizontal") * vitesse;

        float valeurTourne = Input.GetAxis("Mouse X") * vitesseTourne;
        transform.Rotate(0f, valeurTourne, 0f);

        
        // Si le joueur appuie sur la touche F, il allume la flashlight ( toggle)
        if(Input.GetKeyDown(KeyCode.F))
        {
            isOn = !isOn;
            spotLight.enabled = isOn;
        }

        if(isOn)
        {
            // Le raycast s'active
            DoRaycast();
        }

    }

    void FixedUpdate()
    {
        // Gestion du temps
        if (tempsActuel > 0)
            tempsActuel -= Time.deltaTime;

        // Déplacement sans glissement
        Vector3 move = (transform.forward * forceDeplacement) 
                    + (transform.right * forceDeplacementH);

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    
    }


    void DoRaycast()
    {
        RaycastHit hit;

        // Ligne rouge = direction du rayon
        Debug.DrawRay(spotLight.transform.position,
                    spotLight.transform.forward * rayDistance,
                    Color.red);

        if (Physics.Raycast(spotLight.transform.position,
                            spotLight.transform.forward,
                            out hit,
                            rayDistance,
                            hitLayers))
        {
            Debug.DrawLine(spotLight.transform.position, hit.point, Color.yellow);

            if (hit.collider.CompareTag("fantome"))
            {
                JoueurFantome fantome = hit.collider.GetComponentInParent<JoueurFantome>();

                if (fantome != null)
                {
                    print("Le fantome perd sa vie");
                    fantome.PrendreDegats(20f);
                }
            }
        }
    }
}