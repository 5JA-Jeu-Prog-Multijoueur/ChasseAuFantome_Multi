using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class RotatingDoor : NetworkBehaviour
{
    public float openAngle = 90f;
    public float rotateSpeed = 120f; // degrees per second

    public NetworkVariable<bool> isOpening = new(false);
    public NetworkVariable<bool> isClosing = new(false);
    public NetworkVariable<bool> isOpen = new(false);
    public NetworkVariable<bool> isClosed = new(true);

    private float startY;
    private float targetY;

    public bool playerInside;

    // Gestion sons
    public AudioSource audioSource;
    public AudioClip doorClip;

    void Start()
    {
        startY = transform.eulerAngles.y;
    }

   void Update()
{
    OuvrirPorte();
}

void OuvrirPorte()
{
    targetY = startY + openAngle;

    if (playerInside && Input.GetKeyDown(KeyCode.E) && isClosed.Value && !isOpening.Value)
    {
        OpenDoorServerRpc();
    }

    if (IsServer && isOpening.Value)
    {
        RotateTo(targetY);

        if (Reached(targetY))
        {
            isOpening.Value = false;
            isOpen.Value = true;
            Invoke(nameof(FermerPorte), 2f);
        }
    }
}

void FermerPorte()
{
    isOpen.Value = false;
    isClosing.Value = true;
}

void LateUpdate()
{
    if (IsServer && isClosing.Value)
    {
        RotateTo(startY);

        if (Reached(startY))
        {
            isClosing.Value = false;
            isClosed.Value = true;
        }
    }
}

[ServerRpc(RequireOwnership = false)]
void OpenDoorServerRpc()
{
    if (!isClosed.Value) return;

    isOpening.Value = true;
    isClosed.Value = false;

    PlayDoorSoundClientRpc();
}

[ClientRpc]
void PlayDoorSoundClientRpc()
{
    if (audioSource && doorClip)
    {
        audioSource.PlayOneShot(doorClip);
    }
}

void RotateTo(float target)
{
    float y = Mathf.MoveTowardsAngle(
        transform.eulerAngles.y,
        target,
        rotateSpeed * Time.deltaTime
    );

    transform.rotation = Quaternion.Euler(0f, y, 0f);
}

bool Reached(float target)
{
    return Mathf.Abs(
        Mathf.DeltaAngle(transform.eulerAngles.y, target)
    ) < 0.1f;
}

void OnTriggerEnter(Collider other) { 

    if (other.CompareTag("fantome") || other.CompareTag("chasseur")) 
        playerInside = true; 
    } 

void OnTriggerExit(Collider other) { 
    
    if (other.CompareTag("fantome") || other.CompareTag("chasseur")) 
        playerInside = false; 
    }

}
