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

    private bool porteOuvert = false;
    private bool insideBarrel = false;

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
    }

    void Update()
    {
        forceDeplacement  = Input.GetAxis("Vertical") * vitesse;
        forceDeplacementH = Input.GetAxis("Horizontal") * vitesse;

        float valeurTourne = Input.GetAxis("Mouse X") * vitesseTourne;
        transform.Rotate(0f, valeurTourne, 0f);

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
        if(other.CompareTag("PorteArc") || other.CompareTag("PorteA") || other.CompareTag("PorteB") || other.CompareTag("CachetteMur") || other.CompareTag("Barrel"))  
        {
            toucheE.SetActive(true);
        }

        else
        {
            toucheE.SetActive(false);
            porteOuvert = false;
        }
    }

    void OnTriggerStay(Collider other) {
        float ouvertPositionZ = 0.81f;
        float fermerPositionZ = -0.93f;
        float ouvertPositionY = -1.62f;
        float fermerPositionY = -0.85f;
        float vitessePosition = 1.2f;
        float ouvertRotationY = -178f;
        float fermerRotationY = -89.34f;
        float vitesseRotation = 90f;

        bool enMouvement = false;

        if(other.CompareTag("CachetteMur") && Input.GetKeyDown(KeyCode.E))
        {
            enMouvement = true;
            porteOuvert = !porteOuvert;
            float cibleZ = porteOuvert ? ouvertPositionZ : fermerPositionZ;

            if(enMouvement) {
                Vector3 position = other.transform.position;
                position.z = Mathf.MoveTowards(position.z, cibleZ, vitessePosition * Time.deltaTime);

                if(position.z >= cibleZ) {
                    enMouvement = false;
                    porteOuvert = true; 
                }
            }
        }

        else if(other.CompareTag("PorteArc") && Input.GetKeyDown(KeyCode.E)) {
            enMouvement = true;
            porteOuvert = !porteOuvert;
            float cibleY = porteOuvert ? ouvertPositionY : fermerPositionY;

            if(enMouvement) {
                Vector3 position = other.transform.position;
                position.y = Mathf.MoveTowards(position.y, cibleY, vitessePosition * Time.deltaTime);

                if(position.y >= cibleY) {
                    enMouvement = false;
                    porteOuvert = true; 
                }
            }
        }

        else if(other.CompareTag("PorteA") && Input.GetKeyDown(KeyCode.E)) {
            enMouvement = true;
            porteOuvert = !porteOuvert;
            float cibleRotationY = porteOuvert ? ouvertRotationY : fermerRotationY;

            if(enMouvement) {
                Vector3 rotation = other.transform.eulerAngles;
                rotation.y = Mathf.MoveTowardsAngle(rotation.y, cibleRotationY, vitesseRotation * Time.deltaTime);

                if(Mathf.Abs(Mathf.DeltaAngle(rotation.y, cibleRotationY)) < 0.1f) {
                    other.transform.eulerAngles = new Vector3(rotation.x, cibleRotationY, rotation.z);
                    porteOuvert = true;
                }
            }
        }

        else if(other.CompareTag("Barrel") && Input.GetKeyDown(KeyCode.E) && !insideBarrel) {
            Vector3 positionBarrel = other.transform.position;
            gameObject.transform.position = positionBarrel;
            insideBarrel = true;

            if(insideBarrel && Input.GetKeyDown(KeyCode.E)) {
            positionBarrel.z += 2;
            gameObject.transform.position = positionBarrel;
        }
        }
    }

    void OnTriggerExit(Collider other) {
        toucheE.SetActive(false);
        porteOuvert = false;
    }
}
