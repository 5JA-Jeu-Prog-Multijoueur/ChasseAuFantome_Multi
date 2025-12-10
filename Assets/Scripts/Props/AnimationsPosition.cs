using UnityEngine;

public class AnimationsPosition : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveAmount = 2f;
    public float openDelay = 2f;

    public bool isOpening;
    public bool isClosing;
    public bool isOpen;
    public bool isClosed = true;

    public bool playerInside;

    private float startZ;
    private float targetZ;

    void Start()
    {
        startZ = transform.position.z;
        targetZ = startZ + moveAmount;
    }

    void Update()
    {
        // ----- OPEN -----
        if (playerInside && Input.GetKeyDown(KeyCode.E) && isClosed)
        {
            isOpening = true;
            isClosed = false;
        }

        if (isOpening)
        {
            MoveTo(targetZ);

            if (Reached(targetZ))
            {
                isOpening = false;
                isOpen = true;
                Invoke(nameof(CloseDoor), openDelay);
            }
        }

        // ----- CLOSE -----
        if (isClosing)
        {
            MoveTo(startZ);

            if (Reached(startZ))
            {
                isClosing = false;
                isClosed = true;
            }
        }
    }

    void CloseDoor()
    {
        isOpen = false;
        isClosing = true;
    }

    void MoveTo(float target)
    {
        Vector3 pos = transform.position;
        pos.z = Mathf.MoveTowards(pos.z, target, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    bool Reached(float target)
    {
        return Mathf.Abs(transform.position.z - target) < 0.0001f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("fantome"))
            playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("fantome"))
            playerInside = false;
    }
}
