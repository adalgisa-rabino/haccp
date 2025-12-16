using UnityEngine;
using System;

/// <summary>
/// stato dei punti HACCP globali nel gioco e metodi per gestirli.
/// </summary>
public class HaccpScoreState : MonoBehaviour
{
    // variabile static e assegnata in Awake() con Instance = this; questo fa sì che
    // in tutto il gioco ci sia una sola copia di HaccpScoreState e che
    // gli altri script possono accedere a questa istanza tramite HaccpScoreState.Instance.
    public static HaccpScoreState Instance { get; private set; }

    public int Score { get; private set; }

    // Evento per aggiornare le UI di tutte le scene
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnScoreDelta; // delta (+ o -)
    public event Action OnScoreDepleted;

    [Header("Debug")]
    [SerializeField] private int startScore = 0;

    private void Awake()
    {
        //per evitare che due oggetti nella scena diventino GameState.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Score = startScore;
    }

    public void AddScore(int delta)
    {
        Score += delta;
        if (Score < 0)
            Score = 0;

        Debug.Log($"[HaccpScoreState] AddScore chiamato. delta={delta}, score prima={Score}");

        OnScoreChanged?.Invoke(Score);
        OnScoreDelta?.Invoke(delta);

        Debug.Log($"[HaccpScoreState] Eventi emessi. delta={delta}, score dopo={Score}");


        if (Score == 0)
        {
            Debug.Log("[HACCP] Punteggio a 0: gioco bloccato.");
            Debug.Log($"[HaccpScoreState] AddScore delta={delta}, Score={Score}");
            OnScoreDelta?.Invoke(delta);

        }
    }

    public bool HasEnoughScore(int amount)
    {
        return Score >= amount;
    }

    /// <summary>
    /// Prova a spendere dei punti HACCP.
    /// Ritorna true se l'operazione va a buon fine, false se non ci sono abbastanza punti.
    /// </summary>
    public bool TrySpendScore(int amount)
    {
        if (amount <= 0)
            return true;

        if (Score < amount)
            return false;

        AddScore(-amount);
        return true;
    }
}
