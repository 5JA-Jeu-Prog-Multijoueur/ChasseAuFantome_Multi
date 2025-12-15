using UnityEngine;

public class TesteurSons3D : MonoBehaviour
{
    [Header("Assignation des sons 3D")]
    public AudioSource sonH;
    public AudioSource sonJ;
    public AudioSource sonK;
    public AudioSource sonL;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (sonH != null) sonH.Play();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (sonJ != null) sonJ.Play();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (sonK != null) sonK.Play();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (sonL != null) sonL.Play();
        }
    }
}
