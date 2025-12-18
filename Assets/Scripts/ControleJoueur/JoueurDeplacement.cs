using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(NetworkTransform))]
public class JoueurDeplacement : NetworkBehaviour
{
    [Header("D�placement")]
    public float vitesse = 5f;
    public float vitesseTourne = 3f;

    private void Awake()
    {
        // Recentre le mesh enfant sur le pivot du parent
        transform.GetChild(0).localPosition = new Vector3(0f, transform.GetChild(0).localPosition.y, 0f);


    }

    void Update()
    {
        if (!IsOwner) return;

        float moveForward = Input.GetAxis("Vertical") * vitesse;
        float moveRight = Input.GetAxis("Horizontal") * vitesse;

        float rotateY = Input.GetAxis("Mouse X") * vitesseTourne;
        transform.Rotate(0f, rotateY, 0f);

        Vector3 move = transform.forward * moveForward + transform.right * moveRight;
        transform.position += move * Time.deltaTime;

        if(move.sqrMagnitude > 0.01f)
        {
            // Animation de marche peut être ajoutée ici
            GetComponentInChildren<Animator>()?.SetBool("Marche", true);
        }

        else
        {
            GetComponentInChildren<Animator>()?.SetBool("Marche", false); 
        }
    }
}

