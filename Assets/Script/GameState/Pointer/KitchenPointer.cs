using UnityEngine;
using UnityEngine.EventSystems;

public class KitchenPointer : MonoBehaviour, IPointerDownHandler
{
    public enum Target { Frigo, Lavandino, Indizio}
    [SerializeField] private Target target;
    [SerializeField] private ClueTarget ClueTarget;
    
    
    private Color originalColor;
    private Renderer rend;

    
    
    // Aggiungi questo dentro KitchenPointer.cs
    public void Setup(Target nuovoTarget, ClueTarget nuovoClueTarget)
    {
        this.target = nuovoTarget;
        this.ClueTarget = nuovoClueTarget;
        
        // Inizializza i riferimenti ai renderer per il luccichio
        this.rend = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (this.rend != null) this.originalColor = rend.material.color;
    }

    public void SetHighlight(Color c)
    {
        if (rend != null) rend.material.color = c;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (target == Target.Indizio && ClueTarget != null)
        {
            ClueTarget.Reveal();
            if (rend != null) rend.material.color = originalColor; // Toglie il luccichio
            GetComponent<Collider>().enabled = false; // Non più cliccabile
            this.enabled = false;
        }
        
        if (target == Target.Frigo)
        {
            GameManager.Instance.EnterFrigoMinigame();
        }
        else if (target == Target.Lavandino)
        {
            GameManager.Instance.EnterLavandinoMinigame();
        }
    }
}