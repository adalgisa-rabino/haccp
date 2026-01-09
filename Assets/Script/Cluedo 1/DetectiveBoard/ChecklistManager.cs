using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

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

    private List<Vector2> posizioniDisponibili = new List<Vector2>();

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

    void GeneraGrigliaPosizioni() {
    RectTransform rect = containerDestro.GetComponent<RectTransform>();
    // Definisci quante righe e colonne vuoi (es. 3x4)
    int colonne = 3;
    int righe = 4;
    float stepX = rect.rect.width / colonne;
    float stepY = rect.rect.height / righe;

    for (int i = 0; i < colonne; i++) {
        for (int j = 0; j < righe; j++) {
            // Calcola il centro di ogni cella
            float x = (-rect.rect.width / 2) + (i * stepX) + (stepX / 2);
            float y = (-rect.rect.height / 2) + (j * stepY) + (stepY / 2);
            posizioniDisponibili.Add(new Vector2(x, y));
        }
    }
}

    void AggiungiIndizioAlMuro(Clue indizio)
    {
        if (containerDestro == null || indizioPrefab == null) return;

        // 1. Istanzia la Polaroid come figlia di 'dx'
        GameObject nuovaCard = Instantiate(indizioPrefab, containerDestro);
        RectTransform rectCard = nuovaCard.GetComponent<RectTransform>();
        RectTransform rectContainer = containerDestro.GetComponent<RectTransform>();

        // 2. Forza le ancore al centro per permettere il posizionamento casuale relativo
        rectCard.anchorMin = new Vector2(0.5f, 0.5f);
        rectCard.anchorMax = new Vector2(0.5f, 0.5f);
        rectCard.pivot = new Vector2(0.5f, 0.5f);

        // 3. Calcola i limiti (Width e Height del rettangolo blu 'dx')
        // Sottraiamo un margine (es. 80) per evitare che i post-it tocchino il legno
        float margine = 80f;
        float limiteX = (rectContainer.rect.width / 2) - margine;
        float limiteY = (rectContainer.rect.height / 2) - margine;

        // 4. Genera la posizione casuale
        float posX = Random.Range(-limiteX, limiteX);
        float posY = Random.Range(-limiteY, limiteY);

        rectCard.anchoredPosition = new Vector2(posX, posY);

        // 5. Applica la rotazione casuale
        nuovaCard.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));

        // 6. Setup e Animazione
        nuovaCard.GetComponent<ChecklistItem>().Setup(indizio.testo);
        StartCoroutine(AnimazioneComparsa(nuovaCard.transform));
    }

    


    IEnumerator AnimazioneComparsa(Transform target)
    {
        float durata = 0.3f; // Durata dell'animazione
        float tempo = 0;

        // Partiamo da scala zero
        target.localScale = Vector3.zero;

        while (tempo < durata)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / durata;

            // Effetto "Overshoot" (rimbalzo): va un po' oltre 1 e poi torna indietro
            float scala = Mathf.Lerp(0, 1.1f, progresso);
            if (progresso > 0.8f) scala = Mathf.Lerp(1.1f, 1.0f, (progresso - 0.8f) * 5);

            target.localScale = new Vector3(scala, scala, 1);
            yield return null;
        }

        target.localScale = Vector3.one;
    }
    
    
}