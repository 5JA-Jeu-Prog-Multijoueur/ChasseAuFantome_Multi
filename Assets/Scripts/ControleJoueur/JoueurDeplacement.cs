using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class JoueursDeplacement : NetworkBehaviour
{

    public float vitesse = 5f;
    public float vitesseTourne = 3f;

    float forceDeplacement;
    float forceDeplacementH;

    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        forceDeplacement = Input.GetAxis("Vertical") * vitesse;
        forceDeplacementH = Input.GetAxis("Horizontal") * vitesse;

        float valeurTourne = Input.GetAxis("Mouse X") * vitesseTourne;
        transform.Rotate(0f, valeurTourne, 0f);

        if (GetComponent<Rigidbody>().linearVelocity.magnitude > 0)
        {
            GetComponentInChildren<Animator>().SetBool("Marche", true);
        }

        else
        {
            GetComponentInChildren<Animator>().SetBool("Marche", false);
        }
    }

    // =====================================================
    // PHYSIQUE (SEULEMENT OWNER)
    // =====================================================
    void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector3 move = (transform.forward * forceDeplacement)
                     + (transform.right * forceDeplacementH);

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}
