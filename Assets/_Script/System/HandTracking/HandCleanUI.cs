using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class HandCleanUI : MonoBehaviour
{
    [Header("Immagine della mano (type = Filled)")]
    [SerializeField] private Image handImage;

    [Header("Sorgente dati UDP")]
    [SerializeField] private UDPReceive udpReceive;
    [SerializeField] private bool autoPullFromUdp = true;
    [Tooltip("Chiave da cercare nel payload UDP (es. 'errorCount'). Lasciare vuoto se il payload è solo il numero.")]
    [SerializeField] private string udpValueKey = "errorCount";
    [SerializeField, Min(0.01f)] private float udpPollInterval = 0.05f;
    [SerializeField] private bool logUdpParsingErrors = false;

    [Header("Valore massimo di sporco (21^2 = 441)")]
    [SerializeField] private float maxErrorCount = 441f;

    [Header("Smoothing (0 = istantaneo, 5-10 = morbido)")]
    [SerializeField] private float lerpSpeed = 5f;

    private float currentCleanRatio = 0f;
    private float targetCleanRatio = 0f;
    private float udpPollTimer = 0f;

    void Update()
    {
        if (autoPullFromUdp)
            TryReadUdpValue();

        currentCleanRatio = Mathf.Lerp(currentCleanRatio, targetCleanRatio, Time.deltaTime * lerpSpeed);
        UpdateVisual(currentCleanRatio);
    }

    public void UpdateFromErrorCount(int errorCount)
    {
        float clamped = Mathf.Clamp(errorCount, 0f, maxErrorCount);

        float dirtyRatio = clamped / maxErrorCount;
        targetCleanRatio = 1f - dirtyRatio;          // 0 sporco, 1 pulito
    }

    private void UpdateVisual(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);

        // colore rosso → verde
        Color color = Color.Lerp(Color.red, Color.green, ratio);

        if (handImage != null)
        {
            handImage.fillAmount = ratio;  // RIEMPIMENTO
            handImage.color = color;       // COLORE
        }
    }

    public void ResetHand()
    {
        currentCleanRatio = 0f;
        targetCleanRatio  = 0f;
        UpdateVisual(0f);
    }

    private void TryReadUdpValue()
    {
        if (udpReceive == null)
            return;

        udpPollTimer -= Time.deltaTime;
        if (udpPollTimer > 0f)
            return;

        udpPollTimer = udpPollInterval;

        string payload = udpReceive.data;
        if (string.IsNullOrEmpty(payload))
            return;

        if (TryParseErrorCount(payload, out int errorCount))
        {
            UpdateFromErrorCount(errorCount);
        }
        else if (logUdpParsingErrors)
        {
            Debug.LogWarning($"[HandCleanUI] Impossibile estrarre errorCount dal payload UDP: {payload}");
        }
    }

    private bool TryParseErrorCount(string payload, out int errorCount)
    {
        payload = payload.Trim();
        if (payload.Length == 0)
        {
            errorCount = 0;
            return false;
        }

        if (int.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out errorCount))
            return true;

        if (!string.IsNullOrEmpty(udpValueKey))
        {
            int keyIndex = payload.IndexOf(udpValueKey, StringComparison.OrdinalIgnoreCase);
            if (keyIndex >= 0)
            {
                keyIndex += udpValueKey.Length;
                keyIndex = payload.IndexOfAny(new[] { ':', '=' }, keyIndex);
                if (keyIndex >= 0)
                    keyIndex++;

                while (keyIndex < payload.Length && char.IsWhiteSpace(payload[keyIndex]))
                    keyIndex++;

                int end = keyIndex;
                while (end < payload.Length && (char.IsDigit(payload[end]) || payload[end] == '-'))
                    end++;

                if (end > keyIndex)
                {
                    string numberSlice = payload.Substring(keyIndex, end - keyIndex);
                    if (int.TryParse(numberSlice, NumberStyles.Integer, CultureInfo.InvariantCulture, out errorCount))
                        return true;
                }
            }
        }

        string sanitized = payload.Trim('[', ']', '{', '}', '(', ')');
        string[] tokens = sanitized.Split(new[] { ',', ';', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 1 && int.TryParse(tokens[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out errorCount))
            return true;

        errorCount = 0;
        return false;
    }
}
