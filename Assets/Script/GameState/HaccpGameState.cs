using UnityEngine;
using System;

public class HaccpGameState : MonoBehaviour
{
    public static HaccpGameState Instance { get; private set; }

    public int Score { get; private set; }

    // Evento per aggiornare le UI di tutte le scene
    public event Action<int> OnScoreChanged;

    [Header("Debug")]
    [SerializeField] private int startScore = 0;

    private void Awake()
    {
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
        OnScoreChanged?.Invoke(Score);
        Debug.Log($"[HACCP] Score aggiornato: {Score} (delta: {delta})");
    }
}
