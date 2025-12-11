using UnityEngine;

public class AnimationsPositionY : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveAmount = 4200f;
    public float openDelay = 2f;

    public bool isOpening;
    public bool isClosing;
    public bool isOpen;
    public bool isClosed = true;

    public bool playerInside;

    private float startY;
    private float targetY;

    void Start()
    {
        startY = transform.position.y;
        targetY = startY + moveAmount;
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
            MoveTo(targetY);

            if (Reached(targetY))
            {
                isOpening = false;
                isOpen = true;
                Invoke(nameof(CloseDoor), openDelay);
            }
        }

        // ----- CLOSE -----
        if (isClosing)
        {
            MoveTo(startY);

            if (Reached(startY))
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
        pos.y = Mathf.MoveTowards(pos.y, target, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    bool Reached(float target)
    {
        return Mathf.Abs(transform.position.y - target) < 0.0001f;
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

