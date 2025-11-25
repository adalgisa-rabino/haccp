using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Versione essenziale del controller di partita che usa SOLO l'asset ClueDatabase.
/// - Non legge JSON a runtime.
/// - Orchestrazione minima: costruisce cataloghi, seleziona terna segreta,
///   applica filtro escludenti ed estrae N indizi.
/// </summary>
public class CluedoGameControllerFromDB : MonoBehaviour
{
    [Header("Sorgente indizi (ScriptableObject)")]
    public ClueDatabase clueDatabase; // assegna l'asset ClueDatabase dall'Inspector

    [Header("Parametri partita")]
    [Tooltip("Numero di indizi da mostrare/piazzare nella scena")]
    public int numeroIndiziDaPescare = 15;

    [Header("Debug")]
    public bool logDettagli = false;

    // Stato pubblico consultabile da UI/Hotspot
    public string segretoColpevole { get; private set; }
    public string segretoArma { get; private set; }
    public string segretoLuogo { get; private set; }

    public List<Clue> poolDopoFiltro { get; private set; }
    public List<Clue> indiziEstratti { get; private set; }

    public List<string> tuttiColpevoli { get; private set; }
    public List<string> tutteArmi { get; private set; }
    public List<string> tuttiLuoghi { get; private set; }

    void Start()
    {
        AvviaNuovaPartita();
    }

    /// <summary>
    /// Avvia una nuova partita usando esclusivamente ClueDatabase.
    /// </summary>
    public void AvviaNuovaPartita()
    {
        if (clueDatabase == null || clueDatabase.indizi == null || clueDatabase.indizi.Count == 0)
        {
            Debug.LogError("[CluedoDB] ClueDatabase mancante o vuoto. Assegna l'asset in Inspector.");
            return;
        }

        // Fonte indizi (copia per evitare side-effect)
        var sourceIndizi = new List<Clue>(clueDatabase.indizi);

        // 1) Cataloghi
        var costruttore = new CostruttoreCataloghiIndizi(sourceIndizi);
        tuttiColpevoli = costruttore.Colpevoli;
        tutteArmi = costruttore.Armi;
        tuttiLuoghi = costruttore.Luoghi;

        // 2) Terna segreta
        var selettore = new SelettoreTernaSegreta(tuttiColpevoli, tutteArmi, tuttiLuoghi);
        (segretoColpevole, segretoArma, segretoLuogo) = selettore.Seleziona();

        if (logDettagli)
            Debug.Log($"[CluedoDB] Segreto → {segretoColpevole} | {segretoArma} | {segretoLuogo}");

        // 3) Filtro escludenti (rimuove Escludenti incompatibili con la terna)
        var filtro = new FiltroEscludenti(segretoColpevole, segretoArma, segretoLuogo);
        poolDopoFiltro = filtro.Applica(sourceIndizi);

        // 4) Estrazione N indizi
        var estrattore = new EstrattoreIndizi();
        indiziEstratti = estrattore.Estrai(poolDopoFiltro, numeroIndiziDaPescare);

        if (logDettagli)
        {
            Debug.Log($"[CluedoDB] Pool filtrato: {poolDopoFiltro.Count} | Estratti: {indiziEstratti.Count}");
        }
    }
}