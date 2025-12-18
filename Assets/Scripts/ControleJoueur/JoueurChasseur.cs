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
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleLampeServerRpc();
        }
    }

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

    void DoRaycast()
    {
        if (!IsServer) return; // Assure que seul le serveur applique les dégâts

        // Origine et direction du raycast
        Vector3 origin = spotLight.transform.position;
        Vector3 direction = spotLight.transform.forward;

        // Dessine le rayon dans la scène pour debug
        Debug.DrawRay(origin, direction * rayDistance, Color.red, 0.1f);

        // Lance le raycast
        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance, hitLayers))
        {
            Debug.Log("Raycast a touché : " + hit.collider.name + " sur layer " + LayerMask.LayerToName(hit.collider.gameObject.layer));

            // Cherche le script JoueurFantome dans le parent
            JoueurFantome fantome = hit.collider.GetComponentInParent<JoueurFantome>();
            if (fantome != null)
            {
                fantome.PrendreDegatsServerRpc(20f); // Applique les dégâts
                Debug.Log("Fantôme touché !");
            }
            else
            {
                Debug.Log("Collider touché mais pas de JoueurFantome trouvé dans le parent");
            }
        }
        else
        {
            Debug.Log("Raycast n'a rien touché");
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
