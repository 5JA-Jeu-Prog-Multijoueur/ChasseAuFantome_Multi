using UnityEngine;
using Unity.Netcode;

public class TESTFin : NetworkBehaviour
{
    void Update()
    {
        // Seul l'HÔTE peut déclencher la fin de partie
        if (!IsHost) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("TEST : Fin de partie déclenchée");

            // Exemple : le joueur 0 gagne
            SceneFinManager.Instance.AnnoncerVictoireEtChargerSceneServerRpc(0);
        }
    }
}
