using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class FantomeSons : NetworkBehaviour
{
    [Header("Configuration Audio")]
    [SerializeField] private AudioClip criClip;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float intervalleMoyen = 15f;
    [SerializeField] private float variation = 2f;

    private void Start()
    {
        StartCoroutine(BoucleCriFantome());
    }

    private IEnumerator BoucleCriFantome()
    {
        while (true)
        {
            float attente = intervalleMoyen + Random.Range(-variation, variation);
            yield return new WaitForSeconds(attente);

            if (IsSpawned && IsServer)
            {
                // On cherche si l'objet LUI-MÊME ou un de ses ENFANTS a le tag
                bool estUnFantome = false;
                
                // On vérifie le parent
                if (gameObject.CompareTag("fantome")) estUnFantome = true;
                
                // Si pas trouvé, on cherche dans les enfants
                if (!estUnFantome)
                {
                    foreach (Transform enfant in transform)
                    {
                        if (enfant.CompareTag("fantome"))
                        {
                            estUnFantome = true;
                            break;
                        }
                    }
                }

                if (estUnFantome)
                {
                    Debug.Log("[AUDIO] Identifié comme Fantôme, envoi du cri.");
                    JouerCriClientRpc();
                }
            }
        }
    }

    [ClientRpc]
    private void JouerCriClientRpc()
    {
        if (audioSource != null && criClip != null)
        {
            audioSource.PlayOneShot(criClip);
        }
    }
}