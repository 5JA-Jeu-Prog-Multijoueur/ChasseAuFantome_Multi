using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class JoueurChasseur : NetworkBehaviour
{
    [Header("Déplacement")]
    public float vitesse = 5f;
    public float vitesseTourne = 3f;

    float forceDeplacement;
    float forceDeplacementH;

    Rigidbody rb;

    [Header("Temps")]
    public float tempsDepars = 120f;
    public Image niveauTemps;

    private NetworkVariable<float> tempsActuel =
        new NetworkVariable<float>(
            120f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    [Header("Fin de partie")]
    public GameObject CanvasFin;

    [Header("Lampe")]
    public Light spotLight;
    public float rayDistance = 20f;
    public LayerMask hitLayers;

    private NetworkVariable<bool> lampeAllumee =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // =====================================================
    // INIT RÉSEAU
    // =====================================================
    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        if (!IsOwner)
        {
            if (spotLight) spotLight.enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;

        lampeAllumee.OnValueChanged += OnLampeChange;
        tempsActuel.OnValueChanged += OnTempsChange;

        UpdateUI();
    }

    // =====================================================
    // INPUT LOCAL
    // =====================================================
    void Update()
    {
        if (!IsOwner) return;

        forceDeplacement = Input.GetAxis("Vertical") * vitesse;
        forceDeplacementH = Input.GetAxis("Horizontal") * vitesse;

        float valeurTourne = Input.GetAxis("Mouse X") * vitesseTourne;
        transform.Rotate(0f, valeurTourne, 0f);

        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleLampeServerRpc();
        }
    }

    // =====================================================
    // PHYSIQUE
    // =====================================================
    void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector3 move = (transform.forward * forceDeplacement)
                     + (transform.right * forceDeplacementH);

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    // =====================================================
    // LAMPE TORCHE
    // =====================================================
    [ServerRpc]
    void ToggleLampeServerRpc()
    {
        lampeAllumee.Value = !lampeAllumee.Value;
    }

    void OnLampeChange(bool oldValue, bool newValue)
    {
        if (spotLight)
            spotLight.enabled = newValue;

        if (newValue)
            DoRaycast();
    }

    // =====================================================
    // RAYCAST SERVEUR
    // =====================================================
    void DoRaycast()
    {
        if (!IsServer) return;

        if (Physics.Raycast(spotLight.transform.position,
                            spotLight.transform.forward,
                            out RaycastHit hit,
                            rayDistance,
                            hitLayers))
        {
            if (hit.collider.CompareTag("fantome"))
            {
                JoueurFantome fantome =
                    hit.collider.GetComponentInParent<JoueurFantome>();

                if (fantome != null)
                {
                    fantome.PrendreDegatsServerRpc(20f);
                }
            }
        }
    }

    // =====================================================
    // TEMPS
    // =====================================================
    void OnTempsChange(float oldValue, float newValue)
    {
        UpdateUI();

        if (newValue <= 0)
        {
            FinDePartie();
        }
    }

    void UpdateUI()
    {
        if (niveauTemps)
            niveauTemps.fillAmount = tempsActuel.Value / tempsDepars;
    }

    void FinDePartie()
    {
        if (CanvasFin)
            CanvasFin.SetActive(true);
    }
}
