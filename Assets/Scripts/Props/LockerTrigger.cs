using UnityEngine;

public class LockerTrigger : MonoBehaviour
{
    public LockerNetwork locker;

    void OnTriggerEnter(Collider other)
    {
        // Debug.Log("TRIGGER ENTER : " + other.name);

        if (other.CompareTag("fantome") || other.CompareTag("chasseur"))
        {
            locker.SetPlayerNearby(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("fantome") || other.CompareTag("chasseur"))
        {
            locker.SetPlayerNearby(false);
        }
    }
}
