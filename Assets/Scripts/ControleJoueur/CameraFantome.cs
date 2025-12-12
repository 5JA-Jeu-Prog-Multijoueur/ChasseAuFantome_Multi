using UnityEngine;

public class CameraFantome : MonoBehaviour
{
    public Transform target;
    public float distance = 1.0f;
    public float height = 2.0f;
    public float rotationSpeed = 5.0f;
    public LayerMask collisionMask;

    private float currentAngle;

    void Update()
    {
        float horizontalInput = Input.GetAxis("Mouse X");
        float newAngle = currentAngle + horizontalInput * rotationSpeed;

        // Position testée (pour voir si la rotation serait bloquée)
        Vector3 offset = new Vector3(0, height, -distance);
        Quaternion testRotation = Quaternion.Euler(0, newAngle, 0);
        Vector3 testPosition = target.position + testRotation * offset;

        Vector3 testDirection = testPosition - target.position;
        float testDist = testDirection.magnitude;

        bool blocked = Physics.Raycast(target.position, testDirection.normalized, testDist, collisionMask);

        if (!blocked)
        {
            currentAngle = newAngle;
        }

        // Calcul final de la position
        Quaternion finalRotation = Quaternion.Euler(0, currentAngle, 0);
        Vector3 finalPosition = target.position + finalRotation * offset;

        // Ajout : stoppe un peu avant un mur
        Vector3 finalDir = finalPosition - target.position;
        float finalDist = finalDir.magnitude;

        if (Physics.Raycast(target.position, finalDir.normalized, out RaycastHit hit, finalDist, collisionMask))
        {
            finalPosition = hit.point - finalDir.normalized * 0.35f; // distance de sécurité
        }

        transform.position = finalPosition;
        transform.LookAt(target);
    }
}
