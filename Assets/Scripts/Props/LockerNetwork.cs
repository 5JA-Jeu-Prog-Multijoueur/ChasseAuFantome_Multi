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

    [SerializeField] private float openDuration = 1f;
    [SerializeField] private float closeDuration = 1.5f;
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
            Time.time - lastInteractTime > interactCooldown &&
            !isAnimating.Value)
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

        bool opening = !isOpen.Value;
        isOpen.Value = opening;

        if (unlockCoroutine != null)
            StopCoroutine(unlockCoroutine);

        float duration = opening ? openDuration : closeDuration;
        unlockCoroutine = StartCoroutine(UnlockAfterDelay(duration));
    }

    IEnumerator UnlockAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
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
            doorAnimator.SetBool("ouvrir", newValue);

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
