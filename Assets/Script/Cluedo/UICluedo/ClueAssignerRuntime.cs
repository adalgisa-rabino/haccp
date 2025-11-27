using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Assegna indizi ai target specificati. Si sottoscrive all'evento del controller
/// per eseguire l'assegnazione immediatamente dopo il setup dei dati.
/// </summary>
public class ClueAssignerRuntime : MonoBehaviour
{
    public List<GameObject> targetObjects = new List<GameObject>();
    public int numberToAssign = 0;

    // assegna in Inspector al GameController (evita FindObjectOfType)
    public CluedoGameController cluedoController;

    void OnEnable()
    {
        if (cluedoController != null)
            cluedoController.OnSetupComplete += OnControllerReady;
    }

    void OnDisable()
    {
        if (cluedoController != null)
            cluedoController.OnSetupComplete -= OnControllerReady;
    }

    void OnControllerReady()
    {
        AssignNow();
    }


    public void AssignNow()
    {
        List<Clue> clues = (cluedoController != null && cluedoController.indiziEstratti != null && cluedoController.indiziEstratti.Count > 0)
            ? new List<Clue>(cluedoController.indiziEstratti)
            : new List<Clue>(ClueLoader.Load().indizi);

        int toTake = numberToAssign > 0 ? numberToAssign : targetObjects.Count;
        var selected = new EstrattoreIndizi().Estrai(clues, toTake);

        Debug.Log($"[ClueAssignerRuntime] selected count = {selected.Count}");
        for (int si = 0; si < selected.Count; si++)
        {
            Debug.Log($"[ClueAssignerRuntime] #{si} id='{selected[si]?.id}' categoria='{selected[si]?.categoria}' tipo='{selected[si]?.tipo}'");
        }

        int assignCount = Mathf.Min(selected.Count, targetObjects.Count);
        for (int i = 0; i < targetObjects.Count; i++)
        {
            var go = targetObjects[i];
            if (go == null) continue;
            var targetComp = go.GetComponent<ClueTarget>() ?? go.AddComponent<ClueTarget>();
            if (i < assignCount) targetComp.AssignClue(selected[i]);
            else targetComp.AssignClue(null);

            Debug.Log($"[ClueAssignerRuntime] Assegnato a {go.name}: id='{(i<assignCount?selected[i]?.id:"null")}'");
        }
    }


}