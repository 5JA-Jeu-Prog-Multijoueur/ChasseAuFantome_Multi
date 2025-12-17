using UnityEngine;
using Unity.Netcode;

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
            Debug.Log("F PRESSED");
        {
            lastInteractTime = Time.time;
            ToggleLockerServerRpc();
        }
    }

    // ===================== RPC =====================

    [ServerRpc(RequireOwnership = false)]
    void ToggleLockerServerRpc()
    {
        isOpen.Value = !isOpen.Value;
    }

    // ===================== SYNC =====================

    public override void OnNetworkSpawn()
    {
        isOpen.OnValueChanged += OnLockerStateChanged;
        OnLockerStateChanged(false, isOpen.Value);
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

        if (audioSource && openCloseClip)
            audioSource.PlayOneShot(openCloseClip);
    }

    // ===================== TRIGGERS =====================
    public void SetPlayerNearby(bool value)
    {
        playerNearby = value;
    }
}
