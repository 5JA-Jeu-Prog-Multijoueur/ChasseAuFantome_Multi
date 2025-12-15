using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class JoueurFantome : NetworkBehaviour
{
    [Header("Déplacement")]
    public float vitesse = 5f;
    public float vitesseTourne = 3f;

    float forceDeplacement;
    float forceDeplacementH;

    Rigidbody rb;

    [Header("Santé")]
    public float santeDepars = 100f;
    public Image niveauSante;
    public Transform cible;

    private NetworkVariable<float> santeActuel =
        new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    [Header("Interaction")]
    public GameObject toucheE;
    public bool playerInside;

    // =====================================================
    // INITIALISATION RÉSEAU
    // =====================================================
    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        // 🔹 UI uniquement pour le joueur local
        if (!IsOwner)
        {
            if (toucheE) toucheE.SetActive(false);
            if (niveauSante) niveauSante.gameObject.SetActive(false);
            return;
        }

        santeActuel.OnValueChanged += OnSanteChange;
        UpdateBarreVie();
    }

    // =====================================================
    // INPUT JOUEUR LOCAL
    // =====================================================
    void Update()
    {
        if (!IsOwner) return;

        forceDeplacement = Input.GetAxis("Vertical") * vitesse;
        forceDeplacementH = Input.GetAxis("Horizontal") * vitesse;

        float valeurTourne = Input.GetAxis("Mouse X") * vitesseTourne;
        transform.Rotate(0f, valeurTourne, 0f);
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

    // =====================================================
    // DÉGÂTS (SERVEUR AUTORITAIRE)
    // =====================================================
    [ServerRpc]
    public void PrendreDegatsServerRpc(float degats)
    {
        santeActuel.Value -= degats;

        if (santeActuel.Value < 0)
            santeActuel.Value = 0;

        if (santeActuel.Value == 0)
        {
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
        Debug.Log("Fantôme mort !");

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
