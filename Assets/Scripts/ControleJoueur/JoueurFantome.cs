using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class JoueurFantome : NetworkBehaviour
{
    [Header("Santé")]
    public float santeDepars = 100f;
    public Image niveauSante;

    private NetworkVariable<float> santeActuel =
        new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    [Header("Interaction")]
    public GameObject toucheE;
    public bool playerInside;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (toucheE) toucheE.SetActive(false);
            if (niveauSante) niveauSante.gameObject.SetActive(false);
        }

        santeActuel.OnValueChanged += OnSanteChange;
        UpdateBarreVie();
    }

    // =====================================================
    // DÉGÂTS (SERVER AUTHORITY)
    // =====================================================
    [ServerRpc]
    public void PrendreDegatsServerRpc(float degats)
    {
        santeActuel.Value -= degats;
        if (santeActuel.Value <= 0)
        {
            santeActuel.Value = 0;
            FantomeMort();
        }
    }

    void OnSanteChange(float oldValue, float newValue)
    {
        UpdateBarreVie();
    }

    void UpdateBarreVie()
    {
        if (niveauSante)
            niveauSante.fillAmount = santeActuel.Value / santeDepars;
    }

    void FantomeMort()
    {
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    // =====================================================
    // TRIGGERS (LOCAL PLAYER)
    // =====================================================
    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("CachetteMur") ||
            other.CompareTag("PorteA") ||
            other.CompareTag("PorteB") ||
            other.CompareTag("PorteArc"))
        {
            playerInside = true;
            if (toucheE)
                toucheE.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("CachetteMur") ||
            other.CompareTag("PorteA") ||
            other.CompareTag("PorteB") ||
            other.CompareTag("PorteArc"))
        {
            playerInside = false;
            toucheE.SetActive(false);
        }
    }
}
