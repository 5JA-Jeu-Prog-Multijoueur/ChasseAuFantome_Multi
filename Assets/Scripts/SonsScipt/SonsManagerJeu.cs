using UnityEngine;

public class SonsManagerJeu : MonoBehaviour
{
    public AudioSource musiqueDeFond;

    void Start()
    {
        if (musiqueDeFond != null)
            musiqueDeFond.Play();
    }
}
