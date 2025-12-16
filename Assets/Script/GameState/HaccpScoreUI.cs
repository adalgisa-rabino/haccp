using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class HaccpScoreUI : MonoBehaviour
{
    [Header("Score Text")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Coin Animation")]
    [SerializeField] private RectTransform mainCoin;        // icona moneta principale
    [SerializeField] private RectTransform coinBurstRoot;   // parent UI per le mini-monetine
    [SerializeField] private Image miniCoinPrefab;

    [SerializeField] private float burstDuration = 0.6f;
    [SerializeField] private float scatterX = 20f;
    [SerializeField] private float travelY = 50f;
    [SerializeField] private int maxCoinsPerEvent = 10;

    private void Start()
    {
        if (scoreText == null)
            scoreText = GetComponent<TMP_Text>();

        if (HaccpScoreState.Instance != null)
        {
            // inizializza
            UpdateScore(HaccpScoreState.Instance.Score);

            // testo
            HaccpScoreState.Instance.OnScoreChanged += UpdateScore;

            // animazione monetine
            HaccpScoreState.Instance.OnScoreDelta += PlayCoinAnimation;
        }
    }

    private void OnDestroy()
    {
        if (HaccpScoreState.Instance != null)
        {
            HaccpScoreState.Instance.OnScoreChanged -= UpdateScore;
            HaccpScoreState.Instance.OnScoreDelta -= PlayCoinAnimation;
        }
    }

    private void UpdateScore(int newScore)
    {
        if (scoreText != null)
            scoreText.text = $"{newScore}";
    }

    // =======================
    // ANIMAZIONE MONETINE
    // =======================
    private void PlayCoinAnimation(int delta)
    {
        if (delta == 0) return;
        if (miniCoinPrefab == null || coinBurstRoot == null || mainCoin == null)
            return;

        int count = Mathf.Clamp(Mathf.Abs(delta), 1, maxCoinsPerEvent);
        float dir = delta > 0 ? 1f : -1f;

        // posizione di partenza: moneta principale
        Vector2 origin;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            coinBurstRoot,
            RectTransformUtility.WorldToScreenPoint(null, mainCoin.position),
            null,
            out origin
        );

        for (int i = 0; i < count; i++)
        {
            var coin = Instantiate(miniCoinPrefab, coinBurstRoot);
            coin.gameObject.SetActive(true);

            var rt = coin.rectTransform;
            rt.anchoredPosition = origin;
            rt.localScale = Vector3.one;

            float x = Random.Range(-scatterX, scatterX);
            float y = Random.Range(travelY * 0.7f, travelY);
            Vector2 target = origin + new Vector2(x, dir * y);

            StartCoroutine(AnimateCoin(rt, coin, origin, target));
        }
    }

    private IEnumerator AnimateCoin(RectTransform rt, Graphic g, Vector2 start, Vector2 end)
    {
        float t = 0f;
        Color baseColor = g.color;

        while (t < burstDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / burstDuration);

            rt.anchoredPosition = Vector2.Lerp(start, end, k);

            var c = baseColor;
            c.a = 1f - k;
            g.color = c;

            yield return null;
        }

        Destroy(g.gameObject);
    }
}
