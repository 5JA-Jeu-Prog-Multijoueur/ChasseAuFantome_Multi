using UnityEngine;
using Unity.Netcode;

public class AnimationsPosition : NetworkBehaviour
{
    [Header("Réglages Mouvement")]
    public float moveSpeed = 2f;
    public float moveAmount = 2f;
    public float openDelay = 3f; // Variable publique pour ton délai

    [Header("Synchronisation")]
    public NetworkVariable<bool> isOpening = new(false);
    public NetworkVariable<bool> isClosing = new(false);

    public bool playerInside;
    private float startZ;
    private float targetZ;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip doorClip;

    void Start()
    {
        startZ = transform.position.z;
        targetZ = startZ + moveAmount;
    }

    void Update()
    {
        // 1. Détection de l'input local
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            // On demande au serveur d'ouvrir la porte
            OpenDoorServerRpc();
        }

        // 2. Mouvement (calculé chez tout le monde pour la fluidité)
        if (isOpening.Value)
        {
            MoveTo(targetZ);
            
            if (IsServer && Reached(targetZ))
            {
                isOpening.Value = false;
                // Attend 'openDelay' secondes avant de fermer
                Invoke(nameof(StartClosing), openDelay); 
            }
        }
        else if (isClosing.Value)
        {
            MoveTo(startZ);

            if (IsServer && Reached(startZ))
            {
                isClosing.Value = false;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void OpenDoorServerRpc()
    {
        // On vérifie que la porte est fermée et immobile avant d'ouvrir
        if (isOpening.Value || isClosing.Value || Reached(targetZ)) return;

        isOpening.Value = true;
        PlayDoorSoundClientRpc(); // Joue le son chez tout le monde
    }

    [ClientRpc]
    void PlayDoorSoundClientRpc()
    {
        if (audioSource && doorClip)
        {
            audioSource.PlayOneShot(doorClip);
        }
    }

    void StartClosing()
    {
        if (IsServer) isClosing.Value = true;
    }

    void MoveTo(float target)
    {
        Vector3 pos = transform.position;
        pos.z = Mathf.MoveTowards(pos.z, target, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    bool Reached(float target)
    {
        return Mathf.Abs(transform.position.z - target) < 0.01f;
    }

    // --- TRIGGERS ---
    void OnTriggerEnter(Collider other)
    {
        // On vérifie le tag (attention à la majuscule si ton tag est "Fantome")
        if (other.CompareTag("Fantome") || other.CompareTag("Chasseur"))
            playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Fantome") || other.CompareTag("Chasseur"))
            playerInside = false;
    }
}