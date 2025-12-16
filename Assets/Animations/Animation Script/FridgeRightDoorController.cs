using UnityEngine;

<<<<<<< HEAD
public enum DoorSide { Left, Right }

=======
>>>>>>> feature/player-wasd-teleport
[RequireComponent(typeof(Collider))]
public class FridgeDoorController : MonoBehaviour
{
    [SerializeField] private Animator animator;

<<<<<<< HEAD
    // Indica se questa porta � la destra o la sinistra
    [SerializeField] private DoorSide side;

    private bool isOpen;
=======
    private bool isOpen;
    private int openHash;
>>>>>>> feature/player-wasd-teleport
    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
<<<<<<< HEAD

        // Porta inizialmente chiusa
        isOpen = false;
        animator.SetBool("Open", false);

        // Forza lo stato "chiuso" corretto al frame finale
        if (side == DoorSide.Right)
            animator.Play("FridgeRightDoorClosed", 0, 1f);
        else
            animator.Play("FridgeLeftDoorClosed", 0, 1f);

        animator.Update(0f);
=======
        
        openHash = Animator.StringToHash("Open");

        // Porta inizialmente chiusa
            isOpen = false;
            animator.SetBool(openHash, isOpen);

        // Forza un'update immediato per applicare il parametro all'Animator al caricamento
        // (utile se l'Animator usa PlayOnAwake o ha transizioni immediate)
            animator.Update(0f);
        
>>>>>>> feature/player-wasd-teleport
    }

    void OnMouseDown()
    {
        isOpen = !isOpen;
<<<<<<< HEAD
        animator.SetBool("Open", isOpen);
    }
}
=======
        animator.SetBool(openHash, isOpen);
    }
}
>>>>>>> feature/player-wasd-teleport
