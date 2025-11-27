using System;
using UnityEngine;

public class HandWashing : MonoBehaviour
{
    [Header("Riferimenti mani ")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("Parametri lavaggio")]
    [SerializeField] float palmsContactDistance = 0.025f;
    [SerializeField] float requiredContactTime = 5.0f;
    [SerializeField] GameObject popUp;
    [SerializeField] float minPalmArea = 0.0002f;

    enum WashingState
    {
        WaitingHands,
        PalmsTouching,
        Completed
    }

    class HandPalmData
    {
        public Transform root;
        public Transform p0, p5, p17;
        public Vector3 palmCenter;
        public float palmArea;
    }

    readonly HandPalmData leftPalmData = new HandPalmData();
    readonly HandPalmData rightPalmData = new HandPalmData();

    WashingState state = WashingState.WaitingHands;
    float contactTimer;
    bool popupShown;

    void Start()
    {
        FindHandsByTag();
    }

    void Update()
    {
        if (!UpdatePalmData(leftPalmData) || !UpdatePalmData(rightPalmData))
        {
            state = WashingState.WaitingHands;
            contactTimer = 0f;
            return;
        }

        Vector3 L = leftPalmData.palmCenter;
        Vector3 R = rightPalmData.palmCenter;

        // distanza solo su X e Y (ignorando Z)
        Vector2 deltaXY = new Vector2(L.x - R.x, L.y - R.y);
        float distance = deltaXY.magnitude;

        Debug.Log("Distanza palmi (XY): " + distance.ToString("F4"));

        bool palmsTouching = distance <= palmsContactDistance;



        switch (state)
        {
            case WashingState.WaitingHands:
                if (palmsTouching)
                {
                    Debug.Log("Palmi a contatto, inizio timer...");
                    state = WashingState.PalmsTouching;
                    contactTimer = 0f;
                }
                break;

            case WashingState.PalmsTouching:
                if (palmsTouching)
                {
                    contactTimer += Time.deltaTime;
                    if (contactTimer >= requiredContactTime)
                    {
                        state = WashingState.Completed;
                        ShowPopup();
                    }
                }
                else
                {
                    contactTimer = 0f;
                    state = WashingState.WaitingHands;
                }
                break;

            case WashingState.Completed:
                if (!palmsTouching)
                {
                    float timerHidePopup = 0f;
                    timerHidePopup += Time.deltaTime;
                    state = WashingState.WaitingHands;
                    contactTimer = 0f;
                    if (timerHidePopup >= 1.0f) {
                        HidePopup();
                    }

                    
                }
                break;
        }
    }

    void ShowPopup()
    {
        if (popupShown) return;
        popupShown = true;
        if (popUp != null)
        {
            popUp.SetActive(true);
        }
        Debug.Log("Popup lavaggio mani attivato dopo " + requiredContactTime + "s.");
    }

    void HidePopup()
    {
        if (!popupShown) return;
        popupShown = false;
        if (popUp != null)
        {
            popUp.SetActive(false);
        }
        Debug.Log("Popup lavaggio mani disattivato.");
    }

    void FindHandsByTag()
    {
        GameObject[] handObjects = GameObject.FindGameObjectsWithTag("Hand");

        if (handObjects.Length < 2)
        {
            Debug.LogWarning("Servono due mani con tag 'Hand' in scena.");
            return;
        }

        Transform h0 = handObjects[0].transform;
        Transform h1 = handObjects[1].transform;

        float x0 = ComputePalmCenter(h0).x;
        float x1 = ComputePalmCenter(h1).x;

        if (x0 < x1)
        {
            SetupHandData(leftPalmData, h0);
            SetupHandData(rightPalmData, h1);
        }
        else
        {
            SetupHandData(leftPalmData, h1);
            SetupHandData(rightPalmData, h0);
        }

        leftHand = leftPalmData.root;
        rightHand = rightPalmData.root;
    }

    bool UpdatePalmData(HandPalmData data)
    {
        if (data?.root == null) return false;
        if (data.root.childCount <= 17) return false;

        data.p0 = data.root.GetChild(0);
        data.p5 = data.root.GetChild(5);
        data.p17 = data.root.GetChild(17);

        data.palmCenter = ComputePalmCenter(data.root);
        data.palmArea = ComputePalmArea(data);
        if (data.palmArea < minPalmArea || float.IsNaN(data.palmArea))
        {
            return false;
        }

        return true;
    }

    void SetupHandData(HandPalmData data, Transform root)
    {
        data.root = root;
        UpdatePalmData(data);
    }

    Vector3 ComputePalmCenter(Transform root)
    {
        Vector3 p0 = root.GetChild(0).position;
        Vector3 p5 = root.GetChild(5).position;
        Vector3 p17 = root.GetChild(17).position;

        return (p0 + p5 + p17) / 3f;
    }

    float ComputePalmArea(HandPalmData data)
    {
        Vector3 from0to5 = data.p5.position - data.p0.position;
        Vector3 from0to17 = data.p17.position - data.p0.position;
        Vector3 cross = Vector3.Cross(from0to5, from0to17);
        return cross.magnitude * 0.5f;
    }
}
