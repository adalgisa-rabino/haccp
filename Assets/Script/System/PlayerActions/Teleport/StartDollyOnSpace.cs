using UnityEngine;

public class StartDollyOnSpace : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Prende automaticamente l’Animator presente sullo stesso GameObject
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("⚠️ Nessun Animator trovato su questo GameObject!");
        }
    }

    void Update()
    {
        if (animator == null) return;

        // Quando premi la barra spaziatrice, invia il trigger "Go"
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Go");
        }
    }
}
