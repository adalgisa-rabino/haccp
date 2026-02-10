using UnityEngine;

public class GameManagerDishWater : MonoBehaviour
{
    public enum WashGameState
    {
        PlanningProtocol,    
        FoodWasteRemoval,    
        WaterOpening,        
        SurfaceCleaning,     
        DisinfectionQuiz,    
        Finished
    }

    public static GameManagerDishWater Instance { get; private set; }
    
    [Header("STATO ATTUALE")]
    [SerializeField] private WashGameState _currentState = WashGameState.PlanningProtocol;

    [Header("ANIMATIONS")]
    public Animator faucetAnimator; 

    [Header("SCRIPTS/LOGIC")]
    public StainSpawner stainSpawner;
    public DebrisCollector wasteController; // Assicurati di trascinarlo
    public Phase2Manager cleaningManager;       // Assicurati di trascinarlo

    [Header("UI PANELS")]
    public GameObject protocolPanel; 
    public GameObject foodWastePanel; 
    public GameObject boilerUI;      
    public GameObject quizUI;        

    private void Awake() { Instance = this; }

    void Start() {
        // Forza l'avvio della prima fase
        ChangeState(WashGameState.PlanningProtocol);
    }

    // --- TRUCCO PER L'INSPECTOR ---
    // Questo permette di testare i cambi di stato direttamente dall'Inspector
    private void OnValidate() {
        if (Application.isPlaying && Instance != null) {
            ChangeState(_currentState);
        }
    }

    public void ChangeState(WashGameState newState)
    {
        _currentState = newState;
        Debug.Log("Switching to: " + newState);

        // 1. Reset totale: Spegniamo tutto prima di attivare la fase corretta
        ResetAllPhases();

        // 2. Attiviamo solo quello che serve per lo stato attuale
        switch (_currentState)
        {
            case WashGameState.PlanningProtocol:
                protocolPanel.SetActive(true);
                break;

            case WashGameState.FoodWasteRemoval:
                foodWastePanel.SetActive(true);
                if(wasteController != null) wasteController.enabled = true;
                break;

            case WashGameState.WaterOpening:
                if(faucetAnimator != null) faucetAnimator.SetBool("Open", true);
                // Dopo 2 secondi passa automaticamente alla pulizia
                Invoke("AutoGoToCleaning", 2f);
                break;

            case WashGameState.SurfaceCleaning:
                boilerUI.SetActive(true);
                if(stainSpawner != null) stainSpawner.enabled = true;
                if(cleaningManager != null) cleaningManager.enabled = true;
                break;

            case WashGameState.DisinfectionQuiz:
                quizUI.SetActive(true);
                break;

            case WashGameState.Finished:
                Debug.Log("PROCEDURA COMPLETATA!");
                break;
        }
    }

    private void ResetAllPhases()
    {
        // Spegne tutti i pannelli
        if(protocolPanel) protocolPanel.SetActive(false);
        if(foodWastePanel) foodWastePanel.SetActive(false);
        if(boilerUI) boilerUI.SetActive(false);
        if(quizUI) quizUI.SetActive(false);

        // Disabilita gli script logici
        if(stainSpawner) stainSpawner.enabled = false;
        if(wasteController) wasteController.enabled = false;
        if(cleaningManager) cleaningManager.enabled = false;
    }

    void AutoGoToCleaning() { ChangeState(WashGameState.SurfaceCleaning); }
}