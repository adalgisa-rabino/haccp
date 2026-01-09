using System.Collections.Generic;
using UnityEngine;

public class ClueAssignerRuntime : MonoBehaviour
{
    public List<GameObject> targetObjects = new List<GameObject>(); // Trascina qui TUTTI i possibili oggetti
    public Color highlightColor = new Color(1f, 0.9f, 0.5f, 1f); // Colore luccichio
    public CluedoGameController cluedoController;

    void OnEnable() { if (cluedoController != null) cluedoController.OnSetupComplete += AssignNow; }
    void OnDisable() { if (cluedoController != null) cluedoController.OnSetupComplete -= AssignNow; }

    public void AssignNow()
{
    // 1. Recupera gli indizi dal controller
    List<Clue> clues = (cluedoController != null && cluedoController.indiziEstratti != null)
        ? new List<Clue>(cluedoController.indiziEstratti)
        : new List<Clue>();

    if (clues.Count == 0) return;

    // 2. Mischia la lista degli oggetti (Shuffle)
    List<GameObject> shuffledTargets = new List<GameObject>(targetObjects);
    for (int i = 0; i < shuffledTargets.Count; i++)
    {
        GameObject temp = shuffledTargets[i];
        int randomIndex = Random.Range(i, shuffledTargets.Count);
        shuffledTargets[i] = shuffledTargets[randomIndex];
        shuffledTargets[randomIndex] = temp;
    }

    // 3. Assegna gli indizi ai primi N oggetti della lista mischiata
    int count = Mathf.Min(clues.Count, shuffledTargets.Count);

    for (int i = 0; i < shuffledTargets.Count; i++)
    {
        GameObject obj = shuffledTargets[i];
        if (obj == null) continue;

        if (i < count)
        {
            // --- FORZATURA COMPONENTI ---

            // Aggiunge un MeshCollider se non c'è nessun tipo di collider
            if (obj.GetComponent<Collider>() == null)
            {
                // Se è un oggetto complesso, il MeshCollider è più preciso
                MeshCollider mc = obj.AddComponent<MeshCollider>();
                mc.convex = true; // Necessario per far funzionare i click su mesh
            }

            // Aggiunge ClueTarget e assegna l'indizio
            ClueTarget ct = obj.GetComponent<ClueTarget>() ?? obj.AddComponent<ClueTarget>();
            ct.AssignClue(clues[i]);

            // Aggiunge KitchenPointer e lo configura
            KitchenPointer kp = obj.GetComponent<KitchenPointer>() ?? obj.AddComponent<KitchenPointer>();
            
            // Usiamo il metodo Setup che abbiamo creato prima (vedi sotto)
            kp.Setup(KitchenPointer.Target.Indizio, ct);
            
            // Applica il colore di feedback
            kp.SetHighlight(highlightColor);

            Debug.Log($"[AUTO-CONFIG] {obj.name} ora è un indizio!");
        }
        else
        {
            // Se l'oggetto non è stato scelto, assicuriamoci che non sia cliccabile
            if (obj.TryGetComponent(out KitchenPointer oldKp)) oldKp.enabled = false;
        }
    }
}
}