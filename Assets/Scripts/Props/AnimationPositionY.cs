using UnityEngine;
using Unity.Netcode;

public class AnimationsPositionY : NetworkBehaviour
{
    public float moveSpeed = 2f;
    public float moveAmount = 4200f;
    public float openDelay = 2f;

    public NetworkVariable<bool> isOpening = new(false);
    public NetworkVariable<bool> isClosing = new(false);
    public NetworkVariable<bool> isOpen = new(false);
    public NetworkVariable<bool> isClosed = new(true);

    public bool playerInside;

    private float startY;
    private float targetY;

    // Gestion sons
    public AudioSource audioSource;     
    public AudioClip doorClip;  

    void Start()
    {
        startY = transform.position.y;
        targetY = startY + moveAmount;
    }

    void Update()
    {
        // ----- OPEN -----
        if (playerInside && Input.GetKeyDown(KeyCode.E) && isClosed.Value && !isOpening.Value)
        {
            OpenDoorServerRpc();
        }

        if (IsServer && isOpening.Value)
        {
            MoveTo(targetY);

            if (Reached(targetY))
            {
                isOpening.Value = false;
                isOpen.Value = true;
                Invoke(nameof(CloseDoor), openDelay);
            }
        }

        // ----- CLOSE -----
        if (IsServer && isClosing.Value)
        {
            MoveTo(startY);

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

    void CloseDoor()
    {
        isOpen.Value = false;
        isClosing.Value = true;
    }

    void MoveTo(float target)
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, target, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    bool Reached(float target)
    {
        return Mathf.Abs(transform.position.y - target) < 0.0001f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("fantome"))
            playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("fantome"))
            playerInside = false;
    }
}

