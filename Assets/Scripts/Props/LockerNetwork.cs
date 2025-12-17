using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class LockerNetwork : NetworkBehaviour
{
    [Header("Références")]
    public Animator doorAnimator;      // Animator de la porte (enfant)
    public AudioSource audioSource;     // AudioSource (parent ou enfant)
    public AudioClip openCloseClip;

    [Header("Paramètres")]
    public float interactCooldown = 0.5f;

    // État réseau
    private NetworkVariable<bool> isOpen =
        new NetworkVariable<bool>(false);

    private NetworkVariable<bool> isAnimating =
        new NetworkVariable<bool>(false);

    [SerializeField] private float animationDuration = 4.0f;
        private Coroutine unlockCoroutine;


    // Local
    private bool playerNearby;
    private float lastInteractTime;

    void Update()
    {
        if (!IsClient) return;

        if (playerNearby)
            Debug.Log("PLAYER NEARBY CONFIRMED");

        if (playerNearby &&
            Input.GetKeyDown(KeyCode.F) &&
            Time.time - lastInteractTime > interactCooldown)
        {
            Debug.Log("F PRESSED");
            lastInteractTime = Time.time;
            ToggleLockerServerRpc();
        }
    }

    // ===================== RPC =====================

    [ServerRpc(RequireOwnership = false)]
    void ToggleLockerServerRpc()
    {
        if (isAnimating.Value)
            return;

        isAnimating.Value = true;
        isOpen.Value = !isOpen.Value;

        if (unlockCoroutine != null)
            StopCoroutine(unlockCoroutine);

        unlockCoroutine = StartCoroutine(UnlockAfterDelay());
    }

    IEnumerator UnlockAfterDelay()
    {
        yield return new WaitForSeconds(animationDuration);
        isAnimating.Value = false;
    }

    // ===================== SYNC =====================

    public override void OnNetworkSpawn()
    {
        isOpen.OnValueChanged += OnLockerStateChanged;

        // Initialisation SANS transition
        if (doorAnimator)
            doorAnimator.Play(isOpen.Value ? "porteOuverte" : "porteFermee", 0, 1f);
    }

    void OnDestroy()
    {
        if (isOpen != null)
            isOpen.OnValueChanged -= OnLockerStateChanged;
    }

    void OnLockerStateChanged(bool oldValue, bool newValue)
    {
        if (doorAnimator)
        {
            doorAnimator.Play(doorAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);
            doorAnimator.SetBool("ouvrir", newValue);
        }

        if (!IsClient) return;

        if (audioSource && openCloseClip)
            audioSource.PlayOneShot(openCloseClip);
    }

    // ===================== TRIGGERS =====================
    public void SetPlayerNearby(bool value)
    {
        playerNearby = value;
    }
}
