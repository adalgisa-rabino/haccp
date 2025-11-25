using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ClueTarget : MonoBehaviour
{
    public Clue clue;
    public string clueId;

    // Evento semplice: chiunque può sottoscriversi per ricevere il Clue rivelato
    public static Action<Clue> OnClueRevealed;

    public void AssignClue(Clue c)
    {
        clue = c;
        clueId = c != null ? c.id : null;
    }

    void OnMouseDown()
    {
        Reveal();
    }

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
    }
}