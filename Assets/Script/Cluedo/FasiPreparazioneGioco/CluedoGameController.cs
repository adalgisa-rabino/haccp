using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entry-point della logica Cluedo HACCP:
/// - carica indizi
/// - costruisce cataloghi (colpevoli/armi/luoghi)
/// - sceglie la terna segreta
/// - filtra gli escludenti che colpirebbero la terna
/// - estrae N indizi dal pool risultante
/// </summary>
public class CluedoGameController : MonoBehaviour
{
    // evento che notifica il completamento del setup (da usare da altri manager)
    public event Action OnSetupComplete;

    [Header("Parametri partita")]
    [Tooltip("Numero di indizi da mostrare/piazzare nella scena")]
    public int numeroIndiziDaPescare = 12;

    [Header("Debug")]
    public bool logDettagli = true;

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

    public void AvviaNuovaPartita()
    {
        // 1) Carica DB
        var loader = ClueLoader.Load();
        if (loader.indizi.Count == 0)
        {
            Debug.LogError("[Cluedo] Nessun indizio disponibile.");
            return;
        }

        // 2) Cataloghi
        var costruttore = new CostruttoreCataloghiIndizi(loader.indizi);
        tuttiColpevoli = costruttore.Colpevoli;
        tutteArmi = costruttore.Armi;
        tuttiLuoghi = costruttore.Luoghi;

        // 3) Terna segreta
        var selettore = new SelettoreTernaSegreta(tuttiColpevoli, tutteArmi, tuttiLuoghi);
        (segretoColpevole, segretoArma, segretoLuogo) = selettore.Seleziona();

        if (logDettagli)
            Debug.Log($"[Cluedo] Segreto → {segretoColpevole} | {segretoArma} | {segretoLuogo}");

        // 4) Filtro escludenti contro la terna
        var filtro = new FiltroEscludenti(segretoColpevole, segretoArma, segretoLuogo);
        poolDopoFiltro = filtro.Applica(loader.indizi);

        // 5) Estrazione N indizi
        var estrattore = new EstrattoreIndizi();
        indiziEstratti = estrattore.Estrai(poolDopoFiltro, numeroIndiziDaPescare);

        if (logDettagli)
        {
            Debug.Log($"[Cluedo] Pool filtrato: {poolDopoFiltro.Count} | Estratti: {indiziEstratti.Count}");
            for (int i = 0; i < indiziEstratti.Count; i++)
            {
                var c = indiziEstratti[i];
                Debug.Log($" • [{i + 1}] ({c.categoria}/{c.tipo}) {c.testo}");
            }
        }

        // notifica i listener che il setup è completato
        OnSetupComplete?.Invoke();
    }
}