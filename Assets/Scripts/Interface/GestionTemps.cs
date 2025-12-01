using TMPro;
using UnityEngine;

public class GestionTemps : MonoBehaviour
{
    public float tempsDepars = 60f; // exemple : 60 secondes
    private float tempsActuel;

    public TextMeshProUGUI affichageCompteur;

    void Start()
    {
        // Le temps actuel commence avec le temps de départ
        tempsActuel = tempsDepars;
    }

    void Update()
    {
        // Décrémentation avec le temps réel
        if (tempsActuel > 0)
        {
            tempsActuel -= Time.deltaTime;
        }

        // Empêche que le temps devienne négatif
        tempsActuel = Mathf.Max(tempsActuel, 0);

        // Affichage avec arrondi (ex: 59 au lieu de 59.3847)
        affichageCompteur.text = Mathf.CeilToInt(tempsActuel).ToString();

        // Condition de fin
        if (tempsActuel == 0)
        {
            Debug.Log("Fin de la partie");
        }
    }
}
