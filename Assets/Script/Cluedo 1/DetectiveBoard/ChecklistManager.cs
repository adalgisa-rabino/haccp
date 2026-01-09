using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChecklistManager : MonoBehaviour
{
    [Header("Riferimenti")]
    public CluedoGameController cluedoController;
    public GameObject prefabCarta;
    public GameObject indizioPrefab; // Trascina qui il prefab della Polaroid
    

    [Header("Contenitori (Grid Layout)")]
    public Transform containerColpevoli;
    public Transform containerArmi;
    public Transform containerLuoghi;
    public Transform containerDestro; // Trascina qui l'oggetto 'dx' della Hierarchy

    void OnEnable()
    {
        // Ci colleghiamo all'evento che hai già nel tuo CluedoGameController
        if (cluedoController != null)
            cluedoController.OnSetupComplete += IniziaPopolamento;
            ClueTarget.OnClueRevealed += AggiungiIndizioAlMuro;
    }

    void OnDisable()
    {
        if (cluedoController != null)
            cluedoController.OnSetupComplete -= IniziaPopolamento;
        ClueTarget.OnClueRevealed -= AggiungiIndizioAlMuro;
    }

    void IniziaPopolamento()
    {
        // Popoliamo le tre sezioni usando le liste generate dal controller
        PopolaCategoria(cluedoController.tuttiColpevoli, containerColpevoli);
        PopolaCategoria(cluedoController.tutteArmi, containerArmi);
        PopolaCategoria(cluedoController.tuttiLuoghi, containerLuoghi);
    }

    void PopolaCategoria(List<string> listaNomi, Transform container)
    {
        foreach (string nome in listaNomi)
        {
            GameObject nuovaCarta = Instantiate(prefabCarta, container);

            // --- AGGIUNGI QUESTO ---
            float rotazioneCasuale = Random.Range(-5f, 5f); // Ruota tra -5 e +5 gradi
            nuovaCarta.transform.localRotation = Quaternion.Euler(0, 0, rotazioneCasuale);
            // -----------------------

            nuovaCarta.GetComponent<ChecklistItem>().Setup(nome);
        }
    }
    
    void AggiungiIndizioAlMuro(Clue indizio) {

        Debug.Log($"Ricevuto indizio: {indizio.id} - Testo: {indizio.testo}");
        if (containerDestro == null) return;

        // Crea la Polaroid nel lato destro (dx)
        GameObject nuovaCard = Instantiate(indizioPrefab, containerDestro);
        
        // Passa il testo dell'indizio alla card (usando il metodo Setup che hai già)
        // Supponendo che 'Clue' abbia un campo 'description' o 'text'
        nuovaCard.GetComponent<ChecklistItem>().Setup(indizio.testo);

        // Effetto bacheca: rotazione casuale
        nuovaCard.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
    }
}