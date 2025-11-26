using UnityEngine;
using UnityEngine.UI;

public class JoueurFantome : MonoBehaviour
{

    // VIE DU JOUEUR FANTOME
    public float santeActuel; // Variable de la barre de sante
    public float santeDepars; // Valeur de la sante de dépars commencant par 100
    public Image niveauSante; // Image de la barre de santé

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        santeActuel = santeDepars; // La sante commence au maximum pour commencer

    }

    // Update is called once per frame
    void Update()
    {
        // Si le joueur se fait touché par le raycast du spotLight du chasseur sa santé descant

        
    }
}
