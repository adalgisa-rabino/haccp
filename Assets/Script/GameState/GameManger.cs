using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameFlowState
{
    InMenu,

    InPause,
    InIntro,
    InRistorante,
    InMinigioco
}

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene build indices")]
    [SerializeField] int sceneMenu = 0;
    [SerializeField] int sceneIntro = 1;
    [SerializeField] int sceneRistorante = 2;
    [SerializeField] int sceneMinigioocoLavandino = 4;
    [SerializeField] int sceneMinigiocoFrigo = 3;

    public GameFlowState FlowState { get; private set; } = GameFlowState.InMenu;

    public int Coins { get; private set; }
    readonly HashSet<string> foundClues = new HashSet<string>();

    public bool LavandinoCompleted { get; private set; }
    public bool FrigoCompleted { get; private set; }

    public int CurrentSceneIndex { get; private set; }

    public event Action<GameFlowState> OnFlowStateChanged;
    public event Action<int> OnCoinsChanged;
    public event Action<string> OnClueFound;
    public event Action OnProgressChanged;

    // ---- PAUSA (aggiunto) ----
    public bool IsPaused { get; private set; }
    // --------------------------

    bool isLoading;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // sicurezza: quando parte il gioco, timeScale deve essere normale
        Time.timeScale = 1f;
        IsPaused = false;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        CurrentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        UpdateFlowStateForScene(CurrentSceneIndex);
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;

        // sicurezza: se distruggi il GameManager mentre eri in pausa, non lasciare timeScale a 0
        Time.timeScale = 1f;
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoading = false;
        CurrentSceneIndex = scene.buildIndex;
        UpdateFlowStateForScene(CurrentSceneIndex);

        // quando cambi scena, tolgo la pausa per evitare blocchi
        if (IsPaused)
            SetPause(false);

        if (FlowState == GameFlowState.InIntro)
            TryStartIntroDialogue();
    }

    void UpdateFlowStateForScene(int buildIndex)
    {
        var newState = FlowState;

        if (buildIndex == sceneMenu) newState = GameFlowState.InMenu;
        else if (buildIndex == sceneIntro) newState = GameFlowState.InIntro;
        else if (buildIndex == sceneRistorante) newState = GameFlowState.InRistorante;
        else newState = GameFlowState.InMinigioco;

        if (newState != FlowState)
        {
            FlowState = newState;
            OnFlowStateChanged?.Invoke(FlowState);
        }
    }

    public void StartNewGame()
    {
        Coins = 0;

        //nessun indizio trovato sino ad ora
        foundClues.Clear();

        //stato dei minigiochi
        LavandinoCompleted = false;
        FrigoCompleted = false;

        OnCoinsChanged?.Invoke(Coins);
        OnProgressChanged?.Invoke();

        // se riparti da nuovo gioco, assicurati che non sia in pausa
        if (IsPaused) SetPause(false);

        //MODIFICO QUI E INSERISCO SCENA DI INTRODUZIONE 
        LoadScene(sceneIntro);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Per fermare il Play in Editor
#endif
    }


    public void SetPause(bool pause)
    {

        if (IsPaused == pause) return;

        IsPaused = pause;
        Time.timeScale = pause ? 0f : 1f;

        //voglio disabilitare i tasti per muovermi all'interno del gioco quando sono in pausa



    }
    // ------------------------------

    public void GoToMenu()
    {
        LoadScene(sceneMenu);
    }

    public void SkipIntro()
    {
        LoadScene(sceneRistorante);
    }

    public void ContinueAfterIntroDialog()
    {
        LoadScene(sceneRistorante);
    }

    public void EnterLavandinoMinigame()
    {
        if (LavandinoCompleted) return;
        LoadScene(sceneMinigioocoLavandino);
    }

    public void EnterFrigoMinigame()
    {
        if (FrigoCompleted) return;
        LoadScene(sceneMinigiocoFrigo);
    }

    public void CompleteLavandinoMinigame(int coinsReward, IEnumerable<string> cluesReward)
    {
        LavandinoCompleted = true;
        AddCoins(coinsReward);
        AddClues(cluesReward);

        OnProgressChanged?.Invoke();
        LoadScene(sceneRistorante);
    }

    public void CompleteFrigoMinigame(int coinsReward, IEnumerable<string> cluesReward)
    {
        FrigoCompleted = true;
        AddCoins(coinsReward);
        AddClues(cluesReward);

        OnProgressChanged?.Invoke();
        LoadScene(sceneRistorante);
    }

    public bool HasClue(string clueId) => foundClues.Contains(clueId);

    public void AddClue(string clueId)
    {
        if (string.IsNullOrWhiteSpace(clueId)) return;

        if (foundClues.Add(clueId))
            OnClueFound?.Invoke(clueId);
    }

    void AddClues(IEnumerable<string> clues)
    {
        if (clues == null) return;
        foreach (var c in clues) AddClue(c);
    }

    public void AddCoins(int amount)
    {
        if (amount == 0) return;
        Coins = Mathf.Max(0, Coins + amount);
        OnCoinsChanged?.Invoke(Coins);
    }

    void TryStartIntroDialogue()
    {
        var dialogTrigger = FindObjectOfType<DialogTrigger>();
        if (dialogTrigger != null)
            dialogTrigger.PlayDialogue();
    }

    public void LoadScene(int buildIndex)
    {
        if (isLoading) return;
        if (buildIndex < 0) return;

        isLoading = true;
        SceneManager.LoadScene(buildIndex);

        if (SceneManager.GetActiveScene().buildIndex == sceneIntro)
        {
            Debug.Log("Loading scene index: " + buildIndex);
        }
    }

    public void ExitMinigame()
    {
        // Torniamo semplicemente al ristorante senza cambiare lo stato dei progressi
        LoadScene(sceneRistorante);
    }
}
