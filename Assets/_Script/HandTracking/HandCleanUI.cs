using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Assicurati di avere questa using!

public class HandCleanUI : MonoBehaviour
{
    [SerializeField] private UDPReceive udpTot;
    [SerializeField] private GameObject handCleanUI;
    [SerializeField] private Image fillImage;

    private const float maxPoints = 441f;
    private const float minPoints = 220f;

    public void Start()
    {

    }

    public void Update()
    {
        if (udpTot != null && !string.IsNullOrWhiteSpace(udpTot.data))
        {
            // Prova a convertire il dato ricevuto in float
            if (float.TryParse(udpTot.data, out float currentPoints))
            {
                // Calcola la percentuale di riempimento tra minPoints e maxPoints
                float fillAmount = Mathf.InverseLerp(maxPoints, minPoints, currentPoints);
                fillImage.fillAmount = fillAmount;

                fillImage.color = Color.Lerp(Color.red, Color.green, fillAmount);
            }
        }
    }
}