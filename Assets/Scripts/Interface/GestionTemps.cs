using TMPro;
using UnityEngine;

public class GestionTemps : MonoBehaviour
{
    public float tempsDepars = 60f; // exemple : 60 secondes
    private float tempsActuel;

    public TextMeshProUGUI affichageCompteur;

    void Start()
    {
        // Le temps actuel commence avec le temps de d�part
        tempsActuel = tempsDepars;
    }

    void Update()
    {
        // D�cr�mentation avec le temps r�el
        if (tempsActuel > 0)
        {
            tempsActuel -= Time.deltaTime;
        }

        // Emp�che que le temps devienne n�gatif
        tempsActuel = Mathf.Max(tempsActuel, 0);

        // Affichage avec arrondi (ex: 59 au lieu de 59.3847)
        affichageCompteur.text = Mathf.CeilToInt(tempsActuel).ToString();

        // Condition de fin
        if (tempsActuel == 0)
        {
            Debug.Log("Fin de la partie"); 
            SceneFinManager.Instance.AnnoncerVictoireEtChargerSceneServerRpc(0);
        }
    }
}
