using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Gestisce la visualizzazione del termometro 3D:
/// - scala il cilindro rosso in base alla temperatura (0–1) letta da Fridge1State
/// - al click/tap sul termometro richiama ToggleTemperatureFreeze().
/// La base del cilindro resta ferma anche se il pivot è al centro.
/// </summary>
public class FridgeThermometerView : MonoBehaviour, IPointerDownHandler
{
    [Header("Riferimenti")]
    [Tooltip("Stato logico del frigo 1 (temperatura, carne, freeze).")]
    [SerializeField] private Fridge1State fridgeState;

    [Tooltip("Cilindro rosso che rappresenta la colonna di temperatura.")]
    [SerializeField] private Transform temperatureCylinder;

    [Header("Mapping altezza")]
    [Tooltip("Altezza (scala Y) del cilindro a temperatura minima (0).")]
    [SerializeField] private float minScaleY = 0.02f;

    [Tooltip("Altezza (scala Y) del cilindro a temperatura massima (1).")]
    [SerializeField] private float maxScaleY = 0.14f;

    [Tooltip("Se true, la colonna sale quando la temperatura aumenta. Se false, è invertita.")]
    [SerializeField] private bool increaseWithTemperature = true;

    private Vector3 baseScale;
    private Vector3 baseLocalPos;

    void Awake()
    {
        if (temperatureCylinder == null)
            temperatureCylinder = transform; // fallback

        baseScale = temperatureCylinder.localScale;      // es. (0.05, 0.14, 0.14)
        baseLocalPos = temperatureCylinder.localPosition;
    }

    void Update()
    {
        if (fridgeState == null || temperatureCylinder == null)
            return;

        float t = Mathf.Clamp01(fridgeState.GetTemperature01());

        // Se vuoi che colonna alta = freddo, metti increaseWithTemperature = false
        if (!increaseWithTemperature)
            t = 1f - t;

        // Nuova altezza (scala Y) tra min e max
        float newY = Mathf.Lerp(minScaleY, maxScaleY, t);

        // 1) Aggiorna la scala: X e Z restano quelli originali
        Vector3 s = baseScale;
        s.y = newY;
        temperatureCylinder.localScale = s;

        // 2) Compensa il pivot al centro: sposta il cilindro in su di metà
        //    della variazione di scala rispetto alla scala di riferimento.
        //    Così la base resta ferma e cresce solo verso l'alto.
        float deltaScaleY = newY - baseScale.y;
        Vector3 p = baseLocalPos;
        p.y = baseLocalPos.y + deltaScaleY * 0.5f;
        temperatureCylinder.localPosition = p;
    }

    /// <summary>
    /// Click/tap sul termometro: toggla il blocco temperatura.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (fridgeState == null)
            return;

        fridgeState.ToggleTemperatureFreeze();
    }
}
