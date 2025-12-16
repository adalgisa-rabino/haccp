using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FridgeThermometerButton : MonoBehaviour, IPointerDownHandler
{
    // Evento per il tutorial (premuto il termometro)
    public static event Action OnThermometerPressed;

    [SerializeField] private Fridge1State fridgeState;

    [Header("UI")]
    [SerializeField] private Image fillImage;    // Image del Fill Area dello Slider
    [SerializeField] private Image handleImage;  // Image dell'Handle (opzionale)

    [Header("Colors")]
    [SerializeField] private Color normalFillColor = Color.white;
    [SerializeField] private Color frozenFillColor = Color.cyan;
    [SerializeField] private Color normalHandleColor = Color.white;
    [SerializeField] private Color frozenHandleColor = Color.cyan;

    private void Start()
    {
        RefreshColors();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("CLICK TERMOMETRO RICEVUTO");

        if (fridgeState == null)
        {
            Debug.LogError("FridgeState NON assegnato nel termometro!");
            return;
        }

        // Toggle dello stato temperatura
        fridgeState.ToggleTemperatureFreeze();

        // Notifica il tutorial
        OnThermometerPressed?.Invoke();

        // Aggiorna subito il colore (evita un frame di ritardo)
        RefreshColors();
    }

    private void Update()
    {
        // Serve se lo stato freeze cambia da altri script
        RefreshColors();
    }

    private void RefreshColors()
    {
        if (fridgeState == null) return;

        bool frozen = fridgeState.IsTemperatureFrozen;

        if (fillImage != null)
            fillImage.color = frozen ? frozenFillColor : normalFillColor;

        if (handleImage != null)
            handleImage.color = frozen ? frozenHandleColor : normalHandleColor;
    }
}
