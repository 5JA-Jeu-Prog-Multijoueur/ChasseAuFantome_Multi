using UnityEngine;
using UnityEngine.UI;

public class JoueurFantome : MonoBehaviour
{

    public float vitesse;
    float forceDeplacement;
    float forceDeplacementH;
    public float vitesseTourne;
    public string objetEnMain;
    public Transform mains;

    public GameObject toucheE;
    // public Animator porte;
    public bool playerInside;

    Rigidbody rb;


    public float santeDepars = 100f;
    public float santeActuel;
    public Image niveauSante;
    public Transform cible;


    void Start()
    {

        rb = GetComponent<Rigidbody>();
        santeActuel = santeDepars;
        UpdateBarreVie();
        toucheE.SetActive(false);
    }

    void Update()
    {
        forceDeplacement  = Input.GetAxis("Vertical") * vitesse;
        forceDeplacementH = Input.GetAxis("Horizontal") * vitesse;

        float valeurTourne = Input.GetAxis("Mouse X") * vitesseTourne;
        transform.Rotate(0f, valeurTourne, 0f);

        // void Update()
    {
   
    }
    }

    void FixedUpdate()
    {

        // Déplacement sans glissement
        Vector3 move = (transform.forward * forceDeplacement) 
                    + (transform.right * forceDeplacementH);

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    
    }

    public void PrendreDegats(float degats)
    {
        // Regarde vers le joueur (optionnel)
        if (cible != null)
            transform.LookAt(cible);

        // Enlève les dégâts
        santeActuel -= degats;

        // Empêche les valeurs négatives
        if (santeActuel < 0)
            santeActuel = 0;

        // Mets à jour la barre
        UpdateBarreVie();

        // Le joueur est mort
        if (santeActuel == 0)
        {
            FantomeMort();
        }
    }

    void UpdateBarreVie()
    {
        niveauSante.fillAmount = santeActuel / santeDepars;
    }

    void FantomeMort()
    {
        Debug.Log("Fantôme mort !");
        
        // Empêche le raycast de le toucher encore
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Tu peux rajouter une animation, disparition, etc.
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("CachetteMur") || other.CompareTag("PorteA") || other.CompareTag("PorteB") || other.CompareTag("PorteArc")) {
            playerInside = true;
            toucheE.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("CachetteMur") || other.CompareTag("PorteA") || other.CompareTag("PorteB") || other.CompareTag("PorteArc")) {
            playerInside = false;
            toucheE.SetActive(false);
        }

        if (other.CompareTag("PorteA"))
        {
            toucheE.SetActive(false);
        }
    }

}
