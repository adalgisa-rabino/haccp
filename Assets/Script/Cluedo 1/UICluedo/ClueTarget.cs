using System;
using UnityEngine;
[RequireComponent(typeof(Collider))]
public class ClueTarget : MonoBehaviour
{
    public Clue clue;
    public string clueId;

    // Evento semplice: chiunque pu� sottoscriversi per ricevere il Clue rivelato
    public static Action<Clue> OnClueRevealed;

    public void AssignClue(Clue c)
    {
        clue = c;
        clueId = c != null ? c.id : null;
    }

    /*
    void OnMouseDown()
    {
        Reveal();
    }
    */

    public void Reveal()
    {
        if (clue == null)
        {
            Debug.Log($"[ClueTarget] Nessun indizio assegnato a {name}");
            return;
        }

        // notifica gli ascoltatori
        OnClueRevealed?.Invoke(clue);

        // log leggero per debug
        Debug.Log($"[ClueTarget] Rivelato: {clue.id}");

        // 2. DISABILITA L'INTERAZIONE (La modifica che cercavi)
        // Disattiviamo il collider così l'oggetto non intercetta più il click del mouse
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false; 
            Debug.Log($"[ClueTarget] Interazione rimossa da {name} per evitare click multipli.");
        }
    }
}
