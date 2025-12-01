using UnityEngine;

public class Locker : MonoBehaviour
{
    // Référence à l'Animator de la porte (à glisser dans l'inspecteur)
    public Animator animateurPorte;

    // Indique si le joueur est dans la zone du trigger
    private bool joueurProche = false;

    // Indique si la porte est ouverte ou fermée
    private bool porteOuverte = false;

    void Update()
    {
             // Si le joueur est dans la zone ET appuie sur F
        if (joueurProche && Input.GetKeyDown(KeyCode.F))
        {
            // Si la porte est fermée → on met le booléen "ouvrir" à vrai
            if (!porteOuverte)
            {
                animateurPorte.SetBool("ouvrir", true);
                porteOuverte = true;
            }
            else // Si la porte est ouverte → on met le booléen "ouvrir" à faux
            {
                animateurPorte.SetBool("ouvrir", false);
                porteOuverte = false;
            }
        }
    }

    // Quand le joueur entre dans le trigger
    private void OnTriggerEnter(Collider other)
    {
        // On vérifie que c'est bien le joueur (tag à mettre dans Unity)
        if (other.CompareTag("fantome") || other.CompareTag("chasseur"))
        {
            joueurProche = true;
        }
    }

    // Quand le joueur sort de la zone du trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("fantome") || other.CompareTag("chasseur"))
        {
            joueurProche = false;
        }
    }
}
