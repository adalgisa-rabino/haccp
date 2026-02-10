// HandManagerUI.cs
using UnityEngine;
using UnityEngine.UI;

public class HandUIManager : MonoBehaviour
{
    [Header("Interaction Target")]
    // 1. AGGIUNTA: Riferimento allo script che gestisce lo sporco
    [SerializeField] private DirtGridController dirtController; 

    [Header("References")]
    [SerializeField] private Camera referenceCamera; // Camera ZED/World
    [SerializeField] private Camera uiCamera;        // 2. AGGIUNTA: Camera UI (lascia vuoto se usi Overlay)
    [SerializeField] private ZedWristAnchors wristProvider;

    // ... (Il resto delle variabili Header rimane uguale) ...
    [Header("Anchors")]
    [SerializeField] private Transform leftWristAnchor;
    [SerializeField] private Transform rightWristAnchor;
    [SerializeField] private RectTransform leftCircle;
    [SerializeField] private RectTransform rightCircle;
    [SerializeField] private Image leftCircleImage;
    [SerializeField] private Image rightCircleImage;
    [SerializeField] private Graphic leftQuestion;
    [SerializeField] private Graphic rightQuestion;

    [Header("Behavior")]
    [SerializeField] private bool freezeOnLost = true;
    [SerializeField] private bool hideIfNeverTracked = true;
    [SerializeField] private float lostGraceSeconds = 0.15f;
    [SerializeField] private float lerpSpeed = 18f;

    [Header("Mapping")]
    [SerializeField] private bool mirrorXOnScreen = true;

    [Header("Visual")]
    [SerializeField] private Color trackedColor = Color.white;
    [SerializeField] private Color lostColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private float questionFadeSpeed = 10f;

    [Header("Calibration")]
    [SerializeField] private float motionGain = 1.0f;
    [SerializeField] private Vector2 motionOffset = Vector2.zero;

    private bool leftHadValidPos;
    private bool rightHadValidPos;
    private Vector2 leftLastAnchoredPos;
    private Vector2 rightLastAnchoredPos;
    private float leftLostTimer;
    private float rightLostTimer;

    void Awake()
    {
        if (referenceCamera == null) referenceCamera = Camera.main;

        SetGraphicAlpha(leftQuestion, 0f);
        SetGraphicAlpha(rightQuestion, 0f);

        if (hideIfNeverTracked)
        {
            SetGraphicAlpha(leftCircleImage, 0f);
            SetGraphicAlpha(rightCircleImage, 0f);
        }
    }

    void Update()
    {
        if (referenceCamera == null) referenceCamera = Camera.main;

        // Gestione Mano Sinistra
        UpdateHandUI(
            isLeft: true,
            anchor: leftWristAnchor,
            circle: leftCircle,
            circleImage: leftCircleImage,
            question: leftQuestion,
            hadValid: ref leftHadValidPos,
            lastAnchoredPos: ref leftLastAnchoredPos,
            lostTimer: ref leftLostTimer
        );

        // Gestione Mano Destra
        UpdateHandUI(
            isLeft: false,
            anchor: rightWristAnchor,
            circle: rightCircle,
            circleImage: rightCircleImage,
            question: rightQuestion,
            hadValid: ref rightHadValidPos,
            lastAnchoredPos: ref rightLastAnchoredPos,
            lostTimer: ref rightLostTimer
        );
    }

    private void UpdateHandUI(
        bool isLeft,
        Transform anchor,
        RectTransform circle,
        Image circleImage,
        Graphic question,
        ref bool hadValid,
        ref Vector2 lastAnchoredPos,
        ref float lostTimer
    )
    {
        if (circle == null || circleImage == null) return;

        bool trackedNow = GetTrackedState(isLeft);

        // --- Logica di calcolo posizione (Invariata) ---
        if (trackedNow)
        {
            lostTimer = 0f;
            if (anchor != null && referenceCamera != null)
            {
                Vector3 sp = referenceCamera.WorldToScreenPoint(anchor.position);
                if (sp.z > 0.001f)
                {
                    if (mirrorXOnScreen) sp.x = Screen.width - sp.x;

                    RectTransform parentRect = circle.parent as RectTransform;
                    if (parentRect != null)
                    {
                        // Nota: Qui usiamo null come camera per ScreenPointToLocalPoint se siamo in Overlay,
                        // altrimenti servirebbe la uiCamera.
                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, new Vector2(sp.x, sp.y), uiCamera, out Vector2 localPoint))
                        {
                            Vector2 pivot = Vector2.zero; 
                            localPoint = pivot + (localPoint - pivot) * motionGain + motionOffset;
                            lastAnchoredPos = localPoint;
                            hadValid = true;
                        }
                        else trackedNow = false;
                    }
                    else trackedNow = false;
                }
                else trackedNow = false;
            }
            else trackedNow = false;
        }
        else
        {
            lostTimer += Time.deltaTime;
            if (lostTimer < lostGraceSeconds) trackedNow = true;
        }

        if (!hadValid)
        {
            if (hideIfNeverTracked)
            {
                SetGraphicAlpha(circleImage, 0f);
                SetGraphicAlpha(question, 0f);
            }
            return;
        }

        // --- Movimento Visuale ---
        Vector2 targetPos = lastAnchoredPos;
        Vector2 current = circle.anchoredPosition;
        float t = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
        circle.anchoredPosition = Vector2.Lerp(current, targetPos, t);

        // --- 3. MODIFICA CRUCIALE: Cancellazione dello sporco ---
        // Se la mano è tracciata e abbiamo il riferimento al controller dello sporco
        if (trackedNow && dirtController != null)
        {
            // Convertiamo la posizione ATTUALE del pallino UI (che include già mirror, gain e lerp)
            // in coordinate schermo globali (Screen Point).
            Vector2 finalScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, circle.position);
            
            // Inviare il comando di cancellazione
            dirtController.EraseAtScreenPoint(finalScreenPos, uiCamera);
        }

        // --- Gestione Colori/Alpha (Invariata) ---
        if (trackedNow)
        {
            circleImage.color = Color.Lerp(circleImage.color, trackedColor, Time.deltaTime * questionFadeSpeed);
            SetGraphicAlpha(circleImage, 1f);
            SetGraphicAlpha(question, Mathf.MoveTowards(GetGraphicAlpha(question), 0f, Time.deltaTime * questionFadeSpeed));
        }
        else
        {
            circleImage.color = Color.Lerp(circleImage.color, lostColor, Time.deltaTime * questionFadeSpeed);
            if (freezeOnLost)
            {
                SetGraphicAlpha(circleImage, 1f);
                SetGraphicAlpha(question, Mathf.MoveTowards(GetGraphicAlpha(question), 1f, Time.deltaTime * questionFadeSpeed));
            }
            else
            {
                SetGraphicAlpha(circleImage, Mathf.MoveTowards(GetGraphicAlpha(circleImage), 0f, Time.deltaTime * questionFadeSpeed));
                SetGraphicAlpha(question, 0f);
            }
        }
    }

    // ... (Resto dei metodi helper invariati) ...
    private bool GetTrackedState(bool isLeft)
    {
        if (wristProvider != null) return isLeft ? wristProvider.leftTracked : wristProvider.rightTracked;
        Transform a = isLeft ? leftWristAnchor : rightWristAnchor;
        return a != null && a.gameObject.activeInHierarchy;
    }

    private static void SetGraphicAlpha(Graphic g, float a)
    {
        if (g == null) return;
        Color c = g.color;
        c.a = a;
        g.color = c;
    }

    private static float GetGraphicAlpha(Graphic g)
    {
        if (g == null) return 0f;
        return g.color.a;
    }
}