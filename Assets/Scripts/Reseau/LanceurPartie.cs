using UnityEngine;
using UnityEngine.SceneManagement;

public class LancementServeur : MonoBehaviour
{
    // Nom de la scène à charger
    [SerializeField] string NomSceneDepart;
    void Start()
    {
        // Charger la scène spécifiée au démarrage
        SceneManager.LoadScene(NomSceneDepart);
    }

   
}