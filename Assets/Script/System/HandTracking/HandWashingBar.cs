using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class HandWashingBar : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] UDPReceive udpReceive;
    [SerializeField] int totalCount = 441;       // combinazioni totali inviate da UDP (mancanti)
    [SerializeField] int targetCompletions = 220; // combinazioni da completare per riempire la barra

    [Header("UI")]
    [SerializeField] Image fillImage; // usare un'immagine con Fill Method impostato
    [SerializeField] Color startColor = Color.red;
    [SerializeField] Color endColor = Color.green;

    int currentCount;

    void Update()
    {
        if (udpReceive == null || fillImage == null || string.IsNullOrWhiteSpace(udpReceive.data))
            return;

        // accetta stringhe tipo "123" o "123.0"
        if (!int.TryParse(udpReceive.data, NumberStyles.Integer, CultureInfo.InvariantCulture, out currentCount))
        {
            if (float.TryParse(udpReceive.data, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedFloat))
            {
                currentCount = Mathf.RoundToInt(parsedFloat);
            }
            else
            {
                return;
            }
        }

        // currentCount rappresenta i mancanti: 0 = completato, totalCount = nulla soddisfatta
        int clampedTotal = Mathf.Max(1, totalCount);
        currentCount = Mathf.Clamp(currentCount, 0, clampedTotal);

        // calcolo completati e normalizzo rispetto a targetCompletions
        int satisfied = clampedTotal - currentCount;
        int required = Mathf.Max(1, targetCompletions);
        float completedNormalized = Mathf.Clamp01((float)satisfied / required);
        fillImage.fillAmount = completedNormalized;
        fillImage.color = Color.Lerp(startColor, endColor, completedNormalized);
    }
}
