using UnityEngine;
using UnityEngine.UI;

public class HandUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera referenceCamera;
    [SerializeField] private ZedWristAnchors wristProvider;

    [Header("Anchors (optional, can be read from wristProvider)")]
    [SerializeField] private Transform leftWristAnchor;
    [SerializeField] private Transform rightWristAnchor;

    [Header("UI - circles (RectTransform)")]
    [SerializeField] private RectTransform leftCircle;
    [SerializeField] private RectTransform rightCircle;

    [Header("UI - circle graphics (Image)")]
    [SerializeField] private Image leftCircleImage;
    [SerializeField] private Image rightCircleImage;

    [Header("UI - question mark graphics (optional)")]
    [SerializeField] private Graphic leftQuestion;
    [SerializeField] private Graphic rightQuestion;

    [Header("Behavior")]
    [SerializeField] private bool freezeOnLost = true;
    [SerializeField] private bool hideIfNeverTracked = true;
    [SerializeField] private float lostGraceSeconds = 0.15f;  // anti flicker
    [SerializeField] private float lerpSpeed = 18f;           // smoothing UI

    [Header("Visual")]
    [SerializeField] private Color trackedColor = Color.white;
    [SerializeField] private Color lostColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private float questionFadeSpeed = 10f;

    private bool leftHadValidPos;
    private bool rightHadValidPos;

    private Vector2 leftLastScreenPos;
    private Vector2 rightLastScreenPos;

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
        ResolveAnchorsFromProviderIfNeeded();

        UpdateHandUI(
            isLeft: true,
            anchor: leftWristAnchor,
            circle: leftCircle,
            circleImage: leftCircleImage,
            question: leftQuestion,
            
            hadValid: ref leftHadValidPos,
            lastPos: ref rightLastScreenPos,
            lostTimer: ref leftLostTimer
        );

        UpdateHandUI(
            isLeft: false,
            anchor: rightWristAnchor,
            circle: rightCircle,
            circleImage: rightCircleImage,
            question: rightQuestion,
            hadValid: ref rightHadValidPos,
            lastPos: ref rightLastScreenPos,
            lostTimer: ref rightLostTimer
        );
    }

    private void ResolveAnchorsFromProviderIfNeeded()
    {
        if (wristProvider == null) return;

        if (leftWristAnchor == null)
        {
            // Se hai esposto i due anchor nel provider, assegna qui.
            // In alternativa trascinali a mano in Inspector e lascia stare.
            // leftWristAnchor = wristProvider.leftWristAnchor; // se lo rendi pubblico
        }

        if (rightWristAnchor == null)
        {
            // rightWristAnchor = wristProvider.rightWristAnchor; // se lo rendi pubblico
        }
    }

    private void UpdateHandUI(
        bool isLeft,
        Transform anchor,
        RectTransform circle,
        Image circleImage,
        Graphic question,
        ref bool hadValid,
        ref Vector2 lastPos,
        ref float lostTimer
    )
    {
        if (circle == null || circleImage == null) return;

        bool trackedNow = GetTrackedState(isLeft);

        if (trackedNow)
        {
            lostTimer = 0f;

            if (anchor != null)
            {
                Vector3 sp = referenceCamera != null
                    ? referenceCamera.WorldToScreenPoint(anchor.position)
                    : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 1f);

                // se il punto è dietro la camera, consideralo non valido
                if (sp.z > 0.001f)
                {
                    lastPos = new Vector2(sp.x, sp.y);
                    hadValid = true;
                }
                else
                {
                    trackedNow = false;
                }
            }
            else
            {
                trackedNow = false;
            }
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

        // Posizione UI: segue se tracked, altrimenti resta ferma sull’ultima
        Vector2 targetPos = lastPos;

        // smoothing della UI per evitare jitter
        Vector2 current = circle.position;
        float t = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
        circle.position = Vector2.Lerp(current, targetPos, t);

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
                // se non vuoi congelare, puoi far sparire gradualmente
                SetGraphicAlpha(circleImage, Mathf.MoveTowards(GetGraphicAlpha(circleImage), 0f, Time.deltaTime * questionFadeSpeed));
                SetGraphicAlpha(question, 0f);
            }
        }
    }

    private bool GetTrackedState(bool isLeft)
    {
        // Metodo migliore: usa i boolean del provider basati su confidence.
        if (wristProvider != null)
        {
            return isLeft ? wristProvider.leftTracked : wristProvider.rightTracked;
        }

        // Fallback se non hai provider: considera tracciato se l’anchor esiste ed è attivo.
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
