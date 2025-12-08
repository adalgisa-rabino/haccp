using UnityEngine;
using TMPro;

public class HaccpScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        if (scoreText == null)
            scoreText = GetComponent<TMP_Text>();

        if (HaccpScoreState.Instance != null)
        {
            // inizializza
            UpdateScore(HaccpScoreState.Instance.Score);

            HaccpScoreState.Instance.OnScoreChanged += UpdateScore;
        }
    }

    private void OnDestroy()
    {
        if (HaccpScoreState.Instance != null)
            HaccpScoreState.Instance.OnScoreChanged -= UpdateScore;
    }

    private void UpdateScore(int newScore)
    {
        if (scoreText != null)
            scoreText.text = $"{newScore}";
    }
}
