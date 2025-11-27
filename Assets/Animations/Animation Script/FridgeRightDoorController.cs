using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FridgeDoorController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private bool isOpen;
    private int openHash;
    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        
        openHash = Animator.StringToHash("Open");

        // Porta inizialmente chiusa
            isOpen = false;
            animator.SetBool(openHash, isOpen);

        // Forza un'update immediato per applicare il parametro all'Animator al caricamento
        // (utile se l'Animator usa PlayOnAwake o ha transizioni immediate)
            animator.Update(0f);
        
    }

    void OnMouseDown()
    {
        isOpen = !isOpen;
        animator.SetBool(openHash, isOpen);
    }
}