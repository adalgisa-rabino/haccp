using UnityEngine;
using TMPro;

public class HaccpScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        if (scoreText == null)
            scoreText = GetComponent<TMP_Text>();

        if (HaccpGameState.Instance != null)
        {
            // inizializza
            UpdateScore(HaccpGameState.Instance.Score);

            HaccpGameState.Instance.OnScoreChanged += UpdateScore;
        }
    }

    private void OnDestroy()
    {
        if (HaccpGameState.Instance != null)
            HaccpGameState.Instance.OnScoreChanged -= UpdateScore;
    }

    private void UpdateScore(int newScore)
    {
        if (scoreText != null)
            scoreText.text = $"{newScore}";
    }
}
