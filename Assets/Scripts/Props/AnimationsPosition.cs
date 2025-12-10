using UnityEngine;

public class AnimationsPosition : MonoBehaviour
{
    public float moveSpeed = 2f;      // units per second
    public Animator animator;

    private bool shouldMove = false;
    public bool playerInside;
    private float startZ;
    private float targetZ;

    void Start()
    {
        startZ = transform.position.z;
        targetZ = startZ + 2f; // ✅ add exactly +2 on Z
    }

    void Update()
    {

        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            shouldMove = true;

            if (animator != null)
                animator.SetBool("ouvre", true);
        }

        if (!shouldMove) return;

        Vector3 pos = transform.position;
        pos.z = Mathf.MoveTowards(pos.z, targetZ, moveSpeed * Time.deltaTime);
        transform.position = pos;

        // Stop when reached
        if (Mathf.Abs(pos.z - targetZ) < 0.01f)
            shouldMove = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("fantome")) {
            playerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("fantome"))
            playerInside = false;
    }
}
