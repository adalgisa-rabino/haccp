using UnityEngine;

public class FridgeDoorAutoOpen : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openTriggerName = "Open";

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        animator.SetTrigger(openTriggerName);
    }
}
