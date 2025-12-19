using Unity.Netcode;
using UnityEngine;

public class ChasseurSons : NetworkBehaviour
{
    [Header("Configuration Audio")]
    [SerializeField] private AudioClip sonMarche;
    [SerializeField] private AudioSource marcheSource; 
    
    [Header("Réglages Détection")]
    [SerializeField] private float seuilVitesse = 0.01f; 

    private NetworkVariable<bool> estEnTrainDeMarcher = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    
    private Vector3 _dernierePosition;

    private void Start()
    {
        _dernierePosition = transform.position;
        if (marcheSource != null) 
        {
            marcheSource.clip = sonMarche;
            marcheSource.loop = true;
            marcheSource.playOnAwake = false;
            
            // On lance le son immédiatement mais en muet
            marcheSource.volume = 0;
            marcheSource.Play();
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            float distanceParcourue = Vector3.Distance(transform.position, _dernierePosition);
            
            // Calcul plus précis de la vitesse
            bool bougeActuellement = distanceParcourue > (seuilVitesse * Time.deltaTime);

            if (bougeActuellement != estEnTrainDeMarcher.Value)
            {
                ModifierEtatMarcheServerRpc(bougeActuellement);
            }
            
            _dernierePosition = transform.position;
        }

        ActualiserAudio(estEnTrainDeMarcher.Value);
    }

    [ServerRpc]
    private void ModifierEtatMarcheServerRpc(bool nouveauEtat)
    {
        estEnTrainDeMarcher.Value = nouveauEtat;
    }

    private void ActualiserAudio(bool enMarche)
    {
        if (marcheSource == null) return;

        // On utilise Lerp pour une transition fluide du volume (optionnel mais plus joli)
        float volumeCible = enMarche ? 1f : 0f;
        marcheSource.volume = Mathf.MoveTowards(marcheSource.volume, volumeCible, Time.deltaTime * 5f);
    }
}