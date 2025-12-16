using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Gestisce lo stato logico del gioco del Frigo 1:
/// - temperatura e deterioramento carne
/// - blocco temperatura usando punti HACCP
/// - condizioni di fallimento
/// - condizione di vittoria (chiave finale)
/// </summary>
public class Fridge1State : MonoBehaviour
{
    // Singleton comodo da usare da altri script (termometro, ecc.)
    public static Fridge1State Instance { get; private set; }

    // ---------------------------------------------------------------------
    // UI
    // ---------------------------------------------------------------------
    [Header("UI")]
    [SerializeField] private Slider temperatureSlider;
    [SerializeField] private Slider meatDecaySlider;

    // ---------------------------------------------------------------------
    // Temperatura (in °C, internamente convertita 0-1 per lo slider)
    // ---------------------------------------------------------------------
    [Header("Temperatura frigo")]
    [Tooltip("Temperatura minima rappresentata (°C).")]
    [SerializeField] private float minTemperatureC = 0f;

    [Tooltip("Temperatura massima rappresentata (°C).")]
    [SerializeField] private float maxTemperatureC = 12f;

    [Tooltip("Temperatura iniziale in °C.")]
    [SerializeField] private float startTemperatureC = 4f;

    [Tooltip("Velocità con cui aumenta la temperatura (°C al secondo) quando non è bloccata.")]
    [SerializeField] private float temperatureIncreasePerSecondC = 0.2f;

    [Tooltip("Temperatura di warning: da qui in poi la luce inizia a lampeggiare.")]
    [SerializeField] private float warningTemperatureC = 6f;

    [Tooltip("Temperatura di fallimento: da qui in poi il frigo fallisce.")]
    [SerializeField] private float failTemperatureC = 8f;

    // ---------------------------------------------------------------------
    // Deterioramento carne (0-1)
    // ---------------------------------------------------------------------
    [Header("Deterioramento carne")]
    [Tooltip("Valore iniziale deterioramento (0 = fresca, 1 = completamente deteriorata).")]
    [SerializeField] private float startMeatDecay01 = 0f;

    [Tooltip("Velocità con cui aumenta il deterioramento per secondo.")]
    [SerializeField] private float meatDecayPerSecond01 = 0.03f;

    [Tooltip("Soglia (0-1) oltre la quale si considera la carne deteriorata.")]
    [SerializeField] private float meatFailThreshold01 = 0.9f;

    // ---------------------------------------------------------------------
    // Blocca temperatura usando punti HACCP
    // ---------------------------------------------------------------------
    [Header("Blocca temperatura usando punti HACCP")]
    [Tooltip("Se true, all'avvio il blocco temperatura è attivo (se ci sono punti).")]
    [SerializeField] private bool freezeActiveOnStart = false;

    [Tooltip("Punti HACCP spesi ogni 'freezeCostInterval' mentre il blocco è attivo.")]
    [SerializeField] private int freezeCostPerTick = 5;

    [Tooltip("Ogni quanti secondi viene scalato il costo dal punteggio HACCP.")]
    [SerializeField] private float freezeCostInterval = 5f;

    [Tooltip("Durata massima del blocco in secondi (0 = illimitato finché ci sono punti).")]
    [SerializeField] private float freezeMaxDuration = 0f;

    public bool IsTemperatureFrozen => tempFrozen;


    // ---------------------------------------------------------------------
    // Luce di warning + luci ambiente (opzionale)
    // ---------------------------------------------------------------------
    [Header("Luce di warning")]
    [SerializeField] private Light warningLight;
    [SerializeField] private float warningBlinkSpeed = 4f;

    [Header("Luci ambientali da attenuare in warning/fail")]
    [SerializeField] private Light[] ambientLights;
    [SerializeField] private float normalAmbientIntensity = 1f;
    [SerializeField] private float dimmedAmbientIntensity = 0.3f;

    // ---------------------------------------------------------------------
    // Obiettivi / vittoria
    // ---------------------------------------------------------------------
    [Header("Obiettivi per completare il gioco")]
    [Tooltip("Elenco degli oggetti che devono essere posizionati correttamente.")]
    [SerializeField] private FoodItem[] requiredItems;

    [Tooltip("Numero totale di oggetti richiesti. Se 0, usa requiredItems.Length.")]
    [SerializeField] private int totalRequired = 0;

    private int correctlyPlacedCount = 0;

    [Header("Chiave di vittoria")]
    [Tooltip("Prefab della chiave che appare quando il gioco è completato.")]
    [SerializeField] private GameObject keyPrefab;

    [Tooltip("Distanza dalla camera alla quale spawnare la chiave.")]
    [SerializeField] private float keyDistance = 1.5f;

    [Tooltip("Velocità di rotazione della chiave (gradi/sec).")]
    [SerializeField] private float keyRotationSpeed = 45f;

    private GameObject spawnedKey;
    private bool victoryUnlocked = false;

    // ---------------------------------------------------------------------
    // Penalty
    // ---------------------------------------------------------------------

    [SerializeField] private int penaltyWrongShelfOnCheck = 5;
    [SerializeField] private int penaltyUnpackagedOnCheck = 5;
    [SerializeField] private int penaltyExpiredNotDiscardedOnCheck = 10;
    [SerializeField] private int bonusPerfectCheck = 10;

    // ---------------------------------------------------------------------
    // Debug / eventi
    // ---------------------------------------------------------------------
    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    public event Action OnFridgeFailed;
    public event Action OnMeatFailed;

    // ---------------------------------------------------------------------
    // Stato interno
    // ---------------------------------------------------------------------
    float currentTemperatureC;
    float currentMeatDecay01;

    bool tempFrozen;
    float freezeTimer;
    float freezeDurationTimer;


    // --- Modalità tutorial temperatura ---
    bool tutorialTempMode = false;
    float normalTempIncreasePerSecondC;

    bool gameEnded = false;

    // ---------------------------------------------------------------------
    // Lifecycle
    // ---------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {

        // Salva valore normale per ripristino dopo tutorial
        normalTempIncreasePerSecondC = temperatureIncreasePerSecondC;
        // Inizializza slider
        if (temperatureSlider != null)
        {
            temperatureSlider.minValue = 0f;
            temperatureSlider.maxValue = 1f;
        }

        if (meatDecaySlider != null)
        {
            meatDecaySlider.minValue = 0f;
            meatDecaySlider.maxValue = 1f;
        }

        // Inizializza temperature e carne
        currentTemperatureC = Mathf.Clamp(startTemperatureC, minTemperatureC, maxTemperatureC);
        currentMeatDecay01 = Mathf.Clamp01(startMeatDecay01);

        // Se totalRequired non è impostato, usa la lunghezza dell'array
        if (totalRequired <= 0 && requiredItems != null)
            totalRequired = requiredItems.Length;

        // Iscrizione agli eventi HACCP (se esiste lo stato globale)
        if (HaccpScoreState.Instance != null)
        {
            HaccpScoreState.Instance.OnScoreDepleted += OnScoreDepletedHandler;
        }

        // Freeze iniziale opzionale
        tempFrozen = false;
        if (freezeActiveOnStart)
            SetTemperatureFreeze(true);

        UpdateUI();
        HandleWarningLight(initial: true);
    }

    void OnDestroy()
    {
        if (HaccpScoreState.Instance != null)
            HaccpScoreState.Instance.OnScoreDepleted -= OnScoreDepletedHandler;
    }

    // ---------------------------------------------------------------------
    // Update
    // ---------------------------------------------------------------------
    void Update()
    {
        if (gameEnded || victoryUnlocked)
            return;

        // 1. Temperatura
        if (!tempFrozen)
        {
            currentTemperatureC += temperatureIncreasePerSecondC * Time.deltaTime;
            currentTemperatureC = Mathf.Clamp(currentTemperatureC, minTemperatureC, maxTemperatureC);
        }
        else
        {
            HandleFreezeCost();
        }
        if (!tutorialTempMode && currentTemperatureC >= failTemperatureC)
        {
            HandleFridgeFail();
        }
        // 2. Deterioramento carne
        currentMeatDecay01 = Mathf.Clamp01(
            currentMeatDecay01 + meatDecayPerSecond01 * Time.deltaTime
        );

        if (currentMeatDecay01 >= meatFailThreshold01)
        {
            HandleMeatFail();
        }

        UpdateUI();
        HandleWarningLight(initial: false);

        // Se le condizioni cambiassero nel tempo (es. punteggio), controlla vittoria anche qui.
        CheckVictory();
    }

    // ---------------------------------------------------------------------
    // UI
    // ---------------------------------------------------------------------
    void UpdateUI()
    {
        if (temperatureSlider != null)
        {
            float t01 = Mathf.InverseLerp(minTemperatureC, maxTemperatureC, currentTemperatureC);
            temperatureSlider.value = t01;
        }

        if (meatDecaySlider != null)
            meatDecaySlider.value = currentMeatDecay01;
    }

    // ---------------------------------------------------------------------
    // Warning light + ambient
    // ---------------------------------------------------------------------
    void HandleWarningLight(bool initial)
    {
        if (warningLight == null)
            return;


        if (tutorialTempMode)
        {
            // In tutorial non mostriamo warning/fail visivi
            warningLight.enabled = false;
            DimAmbientLights(false);
            return;
        }
        // Zona di fallimento → luce accesa fissa
        if (currentTemperatureC >= failTemperatureC)
        {
            warningLight.enabled = true;
            warningLight.intensity = 50f;
            DimAmbientLights(true);
            return;
        }

        // Zona di warning → lampeggia
        if (currentTemperatureC >= warningTemperatureC)
        {
            warningLight.enabled = true;
            warningLight.intensity = 10f;

            if (!initial)
            {
                float osc = (Mathf.Sin(Time.time * warningBlinkSpeed) * 0.5f) + 0.5f;
                warningLight.intensity = Mathf.Lerp(0f, 10f, osc);
            }
            else
            {
                warningLight.intensity = 0f;
            }

            DimAmbientLights(true);
        }
        else
        {
            warningLight.enabled = false;
            DimAmbientLights(false);
        }
    }

    void DimAmbientLights(bool dim)
    {
        if (ambientLights == null) return;

        float target = dim ? dimmedAmbientIntensity : normalAmbientIntensity;

        foreach (var l in ambientLights)
        {
            if (l == null) continue;
            l.intensity = target;
        }
    }

    // ---------------------------------------------------------------------
    // Freeze e costo in punti HACCP
    // ---------------------------------------------------------------------
    void HandleFreezeCost()
    {
        freezeTimer += Time.deltaTime;

        // Durata massima del blocco (se > 0)
        if (freezeMaxDuration > 0f)
        {
            freezeDurationTimer += Time.deltaTime;
            if (freezeDurationTimer >= freezeMaxDuration)
            {
                if (logDebug) Debug.Log("[Fridge1State] Freeze scaduto per durata massima.");
                SetTemperatureFreeze(false);
                return;
            }
        }

        if (freezeTimer >= freezeCostInterval)
        {
            freezeTimer = 0f;

            if (HaccpScoreState.Instance == null)
            {
                if (logDebug) Debug.LogWarning("[Fridge1State] Niente HaccpScoreState, disattivo freeze.");
                SetTemperatureFreeze(false);
                return;
            }

            bool ok = HaccpScoreState.Instance.TrySpendScore(freezeCostPerTick);
            if (!ok)
            {
                if (logDebug) Debug.Log("[Fridge1State] Punti HACCP insufficienti, disattivo freeze.");
                SetTemperatureFreeze(false);
            }
            else if (logDebug)
            {
                Debug.Log($"[Fridge1State] Spesi {freezeCostPerTick} punti HACCP per mantenere il blocco temperatura.");
            }
        }
    }

    void OnScoreDepletedHandler()
    {
        if (gameEnded || victoryUnlocked) return;

        gameEnded = true;

        if (logDebug)
            Debug.Log("[Fridge1State] Punti HACCP esauriti: blocco gioco frigo.");

        // Qui eventualmente puoi disabilitare selezioni, audio, ecc.
    }

    // ---------------------------------------------------------------------
    // Fail handlers
    // ---------------------------------------------------------------------
    void HandleFridgeFail()
    {
        if (gameEnded || victoryUnlocked) return;
        gameEnded = true;

        if (logDebug)
            Debug.Log("[Fridge1State] Temperatura troppo alta: fine gioco frigo 1.");

        OnFridgeFailed?.Invoke();
    }

    void HandleMeatFail()
    {
        if (gameEnded || victoryUnlocked) return;
        gameEnded = true;

        if (logDebug)
            Debug.Log("[Fridge1State] Carne deteriorata: fine gioco frigo 1.");

        OnMeatFailed?.Invoke();
    }

    // ---------------------------------------------------------------------
    // API pubblica per freeze (chiamata da UI / termometro)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Abilita/disabilita una modalità tutorial in cui:
    /// - la temperatura cresce molto lentamente
    /// - non viene mai innescato il fallimento per temperatura
    /// - vengono disattivati warning light e dimming ambientale
    /// </summary>
    public void SetTutorialTemperatureMode(bool enabled, float tutorialIncreasePerSecondC = 0.01f)
    {
        tutorialTempMode = enabled;

        if (enabled)
        {
            temperatureIncreasePerSecondC = Mathf.Max(0f, tutorialIncreasePerSecondC);
        }
        else
        {
            temperatureIncreasePerSecondC = normalTempIncreasePerSecondC;
        }

        HandleWarningLight(initial: true);
        UpdateUI();
    }

    public void SetTemperatureFreeze(bool value)
    {
        if (value == tempFrozen)
            return;

        tempFrozen = value;
        freezeTimer = 0f;
        freezeDurationTimer = 0f;

        if (logDebug)
            Debug.Log("[Fridge1State] Freeze temperatura " + (tempFrozen ? "ATTIVO" : "DISATTIVATO"));
    }

    public void ToggleTemperatureFreeze()
    {
        SetTemperatureFreeze(!tempFrozen);
    }

    // ---------------------------------------------------------------------
    // API pubblica per altri script
    // ---------------------------------------------------------------------
    public float GetTemperature01()
    {
        return Mathf.InverseLerp(minTemperatureC, maxTemperatureC, currentTemperatureC);
    }

    public float GetMeatDecay01()
    {
        return currentMeatDecay01;
    }

    public float GetTemperatureC()
    {
        return currentTemperatureC;
    }

    // ---------------------------------------------------------------------
    // VITTORIA
    // ---------------------------------------------------------------------

    /// <summary>
    /// Chiamare questo metodo da FridgeItemPositionControl (o simile)
    /// quando un FoodItem viene posizionato CORRETTAMENTE nella zona giusta.
    /// </summary>
    public void NotifyCorrectPlacement(FoodItem item)
    {
        if (victoryUnlocked || gameEnded) return;
        if (requiredItems == null || requiredItems.Length == 0) return;
        if (item == null) return;

        // verifica se l'item è tra quelli richiesti
        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (requiredItems[i] == item)
            {
                // Evita di contare lo stesso item più volte
                if (item.hasBeenCountedCorrectly)
                {
                    if (logDebug)
                        Debug.Log($"[Fridge1State] Item già conteggiato: {item.name}");
                    break;
                }

                item.hasBeenCountedCorrectly = true;

                if (logDebug)
                    Debug.Log($"[Fridge1State] Oggetto posizionato correttamente: {item.name}");

                correctlyPlacedCount++;
                break;
            }
        }

        CheckVictory();
    }



    void CheckVictory()
    {
        //if (logDebug)
        //    Debug.Log("hei sono nel checkvictory");

        if (victoryUnlocked || gameEnded) return;

        bool allItemsPlaced = correctlyPlacedCount >= totalRequired;
        bool temperatureOk = currentTemperatureC < failTemperatureC;
        bool pointsOk = (HaccpScoreState.Instance != null && HaccpScoreState.Instance.Score > 0);
        //if (logDebug)
        //    Debug.Log($"[Fridge1State] Verifica vittoria: ItemsPlaced={allItemsPlaced}, TempOk={temperatureOk}, PointsOk={pointsOk}");


        if (allItemsPlaced && temperatureOk && pointsOk)
        {
            UnlockVictory();
        }
    }

    void UnlockVictory()
    {
        victoryUnlocked = true;

        if (logDebug)
            Debug.Log("[Fridge1State] CONDIZIONI RISPETTATE: vittoria frigo 1, generazione chiave.");

        // Spawn chiave davanti alla camera
        if (keyPrefab == null)
        {
            if (logDebug) Debug.LogWarning("[Fridge1State] Nessun keyPrefab assegnato.");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            if (logDebug) Debug.LogWarning("[Fridge1State] Nessuna Camera.main trovata per spawnare la chiave.");
            return;
        }

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * keyDistance;
        spawnedKey = GameObject.Instantiate(keyPrefab, spawnPos, Quaternion.identity);

        // Aggiunge un semplice componente per farla ruotare
        var rotator = spawnedKey.AddComponent<RotateKey>();
        rotator.rotationSpeed = keyRotationSpeed;
    }

    public void TryFinalizeItem(FoodItem item)
    {
        if (item == null) return;
        if (victoryUnlocked || gameEnded) return;

        // Finalizza SOLO se è "corretto HACCP"
        // (Serve che FoodItem abbia IsHaccpOk(), come abbiamo aggiunto)
        if (!item.IsHaccpOk())
            return;

        // Blocca la selezione (niente farming / niente spostamenti)
        var selectable = item.GetComponent<Selectable>();
        if (selectable != null)
            selectable.LockSelection();

        // Conteggia per la vittoria UNA VOLTA
        if (!item.hasBeenCountedCorrectly)
        {
            item.hasBeenCountedCorrectly = true;
            NotifyCorrectPlacement(item);  // il tuo metodo, già con controllo anti-duplicato
        }

        // Aggiorna indicatori se li usi
        item.UpdateIndicators();
    }

    public string CheckChallengeAndApplyScore()
    {
        if (requiredItems == null || requiredItems.Length == 0)
            return "Nessun oggetto da controllare.";

        int wrongShelf = 0;
        int unpackagedPending = 0;
        int expiredPending = 0;

        foreach (var item in requiredItems)
        {
            if (item == null)
                continue;

            // Se è stato buttato correttamente, non genera errori
            if (item.isDiscarded)
                continue;

            if (!item.isCorrectlyPlaced)
                wrongShelf++;

            if (item.isCorrectlyPlaced && item.isUnpackaged)
                unpackagedPending++;

            if (item.isExpired && !item.isDiscarded)
                expiredPending++;
        }

        int penalty = 0;
        penalty += wrongShelf * penaltyWrongShelfOnCheck;
        penalty += unpackagedPending * penaltyUnpackagedOnCheck;
        penalty += expiredPending * penaltyExpiredNotDiscardedOnCheck;

        if (penalty > 0)
            HaccpScoreState.Instance?.AddScore(-penalty);
        else
            HaccpScoreState.Instance?.AddScore(bonusPerfectCheck);

        // Feedback livello 2 (categorie)
        if (penalty > 0)
        {
            return
                $"Controllo HACCP:\n" +
                $"- Ripiani errati: {wrongShelf}\n" +
                $"- Non impacchettati: {unpackagedPending}\n" +
                $"- Scaduti da smaltire: {expiredPending}\n" +
                $"Penalità: -{penalty}";
        }
        else
        {
            return "Controllo HACCP: tutto conforme ✔\nBonus applicato.";
        }
    }


}



/// <summary>
/// Semplice rotazione continua attorno all'asse Y.
/// </summary>
public class RotateKey : MonoBehaviour
{
    public float rotationSpeed = 45f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}
