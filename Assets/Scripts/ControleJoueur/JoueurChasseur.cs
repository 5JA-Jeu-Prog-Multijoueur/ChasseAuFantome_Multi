using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class JoueurChasseur : MonoBehaviour
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

    public static int compteJournee;
    public TextMeshProUGUI affichageJournee;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked; // Permet de ne plus voir le curseur
        tempsActuel = tempsDepars; // Le temps commence au maximum
        compteJournee += 1; // On compte les journees
        //affichageJournee.text = "Jour " + compteJournee.ToString(); // Affiche le numéro de la journée

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
        // Gestion du temps
        if (tempsActuel > 0)
            tempsActuel -= Time.deltaTime;

        // Déplacement sans glissement
        Vector3 move = (transform.forward * forceDeplacement) 
                    + (transform.right * forceDeplacementH);

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    public void PrendreObjet(string objet)
    {
        objetEnMain = objet;
    }

    public void DonnerObjet(string objet)
    {
        objetEnMain = objet;
        objetEnMain = null;
    }
}