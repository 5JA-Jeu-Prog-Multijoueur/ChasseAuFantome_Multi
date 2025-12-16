using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static Spawner singleton;

    [Header("Points de spawn")]
    public Transform[] spawnPoints; // Assignés dans l'inspecteur

    private void Awake()
    {
        // Singleton pour accès facile depuis GameManager
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject); // optionnel, si tu veux garder le Spawner entre les scènes
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
