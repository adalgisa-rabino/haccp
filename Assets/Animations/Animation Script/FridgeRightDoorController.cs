using UnityEngine;

public enum DoorSide { Left, Right }

[RequireComponent(typeof(Collider))]
public class FridgeDoorController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // Indica se questa porta è la destra o la sinistra
    [SerializeField] private DoorSide side;

    private bool isOpen;
    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();

        // Porta inizialmente chiusa
        isOpen = false;
        animator.SetBool("Open", false);

        // Forza lo stato "chiuso" corretto al frame finale
        if (side == DoorSide.Right)
            animator.Play("FridgeRightDoorClosed", 0, 1f);
        else
            animator.Play("FridgeLeftDoorClosed", 0, 1f);

        animator.Update(0f);
    }

    void OnMouseDown()
    {
        isOpen = !isOpen;
        animator.SetBool("Open", isOpen);
    }
}
