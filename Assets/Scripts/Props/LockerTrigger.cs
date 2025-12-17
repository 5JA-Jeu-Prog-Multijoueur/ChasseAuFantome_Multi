using UnityEngine;

public class LockerTrigger : MonoBehaviour
{
    private LockerNetwork locker;

    void Awake()
    {
        locker = GetComponentInParent<LockerNetwork>();
    }

    void OnTriggerEnter(Collider other)
    {
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
