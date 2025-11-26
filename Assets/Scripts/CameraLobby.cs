using UnityEngine;

public class CameraLobby : MonoBehaviour
{
    [Header("Vitesse de rotation en degrés/sec")]
    public float rotationSpeed = 10f;

    [Header("Inclinaison verticale de la caméra (optionnelle)")]
    public float tiltAngle = 20f;

    void Start()
    {
        // Optionnel : ajuste automatiquement l'angle vertical de la caméra
        if (transform.childCount > 0)
        {
            Transform cam = transform.GetChild(0);
            cam.localRotation = Quaternion.Euler(tiltAngle, 0f, 0f);
        }
    }

    void Update()
    {
        // Rotation continue autour de l'axe Y
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
