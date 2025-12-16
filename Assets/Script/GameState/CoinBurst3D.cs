using System.Collections;
using UnityEngine;

public class CoinBurst3D : MonoBehaviour
{
    [Header("Hook punteggio")]
    [SerializeField] private HaccpScoreState scoreState; // se null usa Instance

    [Header("Sorgente animazione")]
    [SerializeField] private Transform mainCoin;         // moneta principale 3D (origine)
    [SerializeField] private GameObject miniCoinPrefab;  // prefab monetina 3D

    [Header("Burst")]
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float travelY = 0.25f;      // quanto su/giù in metri
    [SerializeField] private float scatterX = 0.08f;     // dispersione laterale
    [SerializeField] private float scatterZ = 0.08f;
    [SerializeField] private int maxCoinsPerEvent = 10;

    [Header("Animazione")]
    [SerializeField] private float randomSpin = 360f;    // gradi di spin casuale
    [SerializeField] private bool fadeOut = true;

    private void OnEnable()
    {
        Debug.Log($"[CoinBurst3D] OnEnable. scoreState iniziale={(scoreState ? scoreState.name : "NULL")}");
        if (scoreState == null) scoreState = HaccpScoreState.Instance;
        Debug.Log($"[CoinBurst3D] Dopo Instance. scoreState={(scoreState ? scoreState.name : "NULL")}");
        if (scoreState != null) scoreState.OnScoreDelta += HandleDelta;

    }

    private void OnDisable()
    {
        if (scoreState != null)
            scoreState.OnScoreDelta -= HandleDelta;
    }

    private void HandleDelta(int delta)
    {
        Debug.Log($"[CoinBurst3D] HandleDelta: {delta}");

        if (delta == 0) return;
        if (mainCoin == null || miniCoinPrefab == null) return;

        int count = Mathf.Clamp(Mathf.Abs(delta), 1, maxCoinsPerEvent);
        float dir = delta > 0 ? 1f : -1f;

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(miniCoinPrefab, mainCoin.position, Quaternion.identity, mainCoin.parent);
            // parent = mainCoin.parent così resta nello stesso “spazio HUD” (ancorato alla camera)

            // Piccola dispersione
            Vector3 start = mainCoin.position;
            Vector3 target = start + new Vector3(
                Random.Range(-scatterX, scatterX),
                dir * Random.Range(travelY * 0.7f, travelY),
                Random.Range(-scatterZ, scatterZ)
            );

            // Spin casuale
            Vector3 spinAxis = Random.onUnitSphere;
            float spinSpeed = Random.Range(-randomSpin, randomSpin);

            StartCoroutine(AnimateCoin(go, start, target, spinAxis, spinSpeed));
        }
    }

    private IEnumerator AnimateCoin(GameObject coin, Vector3 start, Vector3 end, Vector3 spinAxis, float spinSpeed)
    {
        float t = 0f;

        // Se vuoi fade, cerchiamo un Renderer con materiale (Standard/URP Lit con BaseColor)
        Renderer r = null;
        Material matInstance = null;
        Color baseColor = Color.white;

        if (fadeOut)
        {
            r = coin.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                // Istanzia materiale per non modificare sharedMaterial
                matInstance = r.material;
                baseColor = matInstance.color;
            }
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);

            // Movimento
            coin.transform.position = Vector3.Lerp(start, end, k);

            // Rotazione
            if (spinSpeed != 0f)
                coin.transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.World);

            // Fade
            if (fadeOut && matInstance != null)
            {
                var c = baseColor;
                c.a = 1f - k;
                matInstance.color = c;
            }

            yield return null;
        }

        Destroy(coin);
    }
}
