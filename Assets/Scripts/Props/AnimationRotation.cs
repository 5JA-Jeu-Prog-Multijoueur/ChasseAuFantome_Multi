using UnityEngine;
using System.Collections;

public class RotatingDoor : MonoBehaviour
{
    public float openAngle = 90f;
    public float rotateSpeed = 120f; // degrees per second

    public bool isOpening;
    public bool isOpen;
    public bool isClosed = true;
    public bool isClosing = false;

    private float startY;
    private float targetY;

    public bool playerInside;

    void Start()
    {
        startY = transform.eulerAngles.y;
    }

   void Update()
{
    OuvrirPorte();
}

void OuvrirPorte()
{
    targetY = startY + openAngle;

    if (playerInside && Input.GetKeyDown(KeyCode.E) && isClosed)
    {
        isOpening = true;
        isClosed = false;
    }

    if (isOpening)
    {
        RotateTo(targetY);

        if (Reached(targetY))
        {
            isOpening = false;
            isOpen = true;
            Invoke(nameof(FermerPorte), 2f);
        }
    }
}

void FermerPorte()
{
    isOpen = false;
    isClosing = true;
}

void LateUpdate()
{
    if (isClosing)
    {
        RotateTo(startY);

        if (Reached(startY))
        {
            isClosing = false;
            isClosed = true;
        }
    }
}

void RotateTo(float target)
{
    float y = Mathf.MoveTowardsAngle(
        transform.eulerAngles.y,
        target,
        rotateSpeed * Time.deltaTime
    );

    transform.rotation = Quaternion.Euler(0f, y, 0f);
}

bool Reached(float target)
{
    return Mathf.Abs(
        Mathf.DeltaAngle(transform.eulerAngles.y, target)
    ) < 0.1f;
}

void OnTriggerEnter(Collider other) { 

    if (other.CompareTag("fantome")) 
        playerInside = true; 
    } 

void OnTriggerExit(Collider other) { 
    
    if (other.CompareTag("fantome")) 
        playerInside = false; 
    }

}
