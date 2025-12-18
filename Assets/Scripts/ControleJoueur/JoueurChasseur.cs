using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class JoueurChasseur : NetworkBehaviour
{
    [Header("Temps")]
    public float tempsDepars = 120f;
    public Image niveauTemps;
    public GameObject CanvasFin;

    [Header("Lampe")]
    public Light spotLight;
    public float rayDistance = 20f;
    public LayerMask hitLayers;

    private NetworkVariable<float> tempsActuel =
        new NetworkVariable<float>(
            120f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private NetworkVariable<bool> lampeAllumee =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
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

    void Update()
    {
        if (!IsOwner) return;

        // Lampe
        // if (Input.GetKeyDown(KeyCode.F))
        // {
        //     ToggleLampeServerRpc();
        // }

        DoRaycast();
    }

    [ServerRpc]
    void ToggleLampeServerRpc()
    {
        lampeAllumee.Value = !lampeAllumee.Value;
        if (lampeAllumee.Value)
        DoRaycast(); // runs on server because this is a ServerRpc
    }

    void OnLampeChange(bool oldValue, bool newValue)
    {
        if (spotLight)
            spotLight.enabled = newValue;

        // if (newValue)
        //     DoRaycast();
    }

     void DoRaycast()
{
    RaycastHit hit;

    Vector3 origin = spotLight.transform.position;
    Vector3 direction = spotLight.transform.forward;

    // Ligne rouge = direction du rayon
    Debug.DrawRay(origin, direction * rayDistance, Color.red);

    if (Physics.Raycast(origin, direction, out hit, rayDistance))
    {
        Debug.DrawLine(origin, hit.point, Color.yellow);

        Debug.Log($"Hit: {hit.collider.name} | Tag: {hit.collider.tag}");

        // ✅ Tag check
        if (hit.collider.CompareTag("fantome"))
        {
            // ✅ Script may be on parent
            JoueurFantome fantome =
                hit.collider.GetComponent<JoueurFantome>();
                // hit.collider.GetComponentInParent<JoueurFantome>();

            if (fantome != null)
            {
                Debug.Log("Le fantôme perd sa vie");
                fantome.PrendreDegatsServerRpc(20f); // Applique les dégâts
            }
            else
            {
                Debug.LogWarning("Collider tagged 'fantome' but no JoueurFantome found");
            }
        }
    }
}




    void OnTempsChange(float oldValue, float newValue)
    {
        UpdateUI();
        if (newValue <= 0)
            FinDePartie();
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
