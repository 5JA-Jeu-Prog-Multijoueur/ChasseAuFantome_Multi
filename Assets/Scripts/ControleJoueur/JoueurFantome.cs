using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

// Script du joueur fantôme (multijoueur avec Netcode)
public class JoueurFantome : NetworkBehaviour
{
    [Header("Santé")]
    // Santé de départ du fantôme
    public float santeDepars = 100f;
    // Image UI représentant la barre de vie
    public Image niveauSante;

    // Santé actuelle synchronisée sur le réseau (modifiée uniquement par le serveur)
    private NetworkVariable<float> santeActuel =
        new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    [Header("Interaction")]
    // UI indiquant au joueur d'appuyer sur la touche F
    public GameObject toucheF;
    // Indique si le joueur est dans une zone interactive
    public bool playerInside;

    [Header("Affichage")]
    // Canvas affiché à la fin de la partie
    public GameObject CanvasFin;

    // Appelé lorsque l'objet réseau apparaît
    public override void OnNetworkSpawn()
    {
        // Si ce joueur n'est pas le propriétaire, on cache les UI locales
        if (!IsOwner)
        {
            if (toucheF) toucheF.SetActive(false);
            if (niveauSante) niveauSante.gameObject.SetActive(false);
        }

        // Écoute les changements de santé
        santeActuel.OnValueChanged += OnSanteChange;

        // Mise à jour initiale de la barre de vie
        UpdateBarreVie();
    }

    // =====================================================
    // DÉGÂTS (AUTORITÉ SERVEUR)
    // =====================================================

    // RPC appelé pour infliger des dégâts (exécuté sur le serveur)
    [ServerRpc]
    public void PrendreDegatsServerRpc(float degats)
    {
        // Réduit la santé actuelle
        santeActuel.Value -= degats;

        // Vérifie si le fantôme est mort
        if (santeActuel.Value <= 0)
        {
            santeActuel.Value = 0;
            FantomeMort();
        }
    }

    // Appelé quand la valeur de la santé change
    void OnSanteChange(float oldValue, float newValue)
    {
        // Met à jour l'interface utilisateur
        UpdateBarreVie();
    }

    // Met à jour visuellement la barre de vie
    void UpdateBarreVie()
    {
        if (niveauSante)
            niveauSante.fillAmount = santeActuel.Value / santeDepars;
    }

    // Gestion de la mort du fantôme
    void FantomeMort()
    {
        // Désactive tous les colliders pour empêcher les interactions
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;
        FinDePartie();
    }

    // =====================================================
    // TRIGGERS (JOUEUR LOCAL UNIQUEMENT)
    // =====================================================

    // Détection de l'entrée dans une zone interactive
    void OnTriggerEnter(Collider other)
    {
        // Seul le joueur propriétaire déclenche ces actions
        if (!IsOwner) return;

        // Vérifie si l'objet est une zone d'interaction valide
        if (other.CompareTag("CachetteMur") ||
            other.CompareTag("PorteA") ||
            other.CompareTag("PorteB") ||
            other.CompareTag("PorteArc"))
        {
            // Le joueur est dans une zone interactive
            playerInside = true;

            // Affiche l'indication "Appuyer sur F"
            if (toucheF)
                toucheF.SetActive(true);
        }
    }

    // Détection de la sortie d'une zone interactive
    void OnTriggerExit(Collider other)
    {
        // Seul le joueur propriétaire déclenche ces actions
        if (!IsOwner) return;

        // Vérifie si l'objet était une zone d'interaction valide
        if (other.CompareTag("CachetteMur") ||
            other.CompareTag("PorteA") ||
            other.CompareTag("PorteB") ||
            other.CompareTag("PorteArc"))
        {
            // Le joueur quitte la zone interactive
            playerInside = false;

            // Cache l'indication "Appuyer sur F"
            toucheF.SetActive(false);
        }
    }


    // Affiche l'écran de fin
    void FinDePartie()
    {
        if (CanvasFin)
            CanvasFin.SetActive(true);
    }
}
