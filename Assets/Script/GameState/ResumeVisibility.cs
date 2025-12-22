using UnityEngine;

public class ResumeVisibility : MonoBehaviour
{
    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPauseChanged += HandlePause;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPauseChanged -= HandlePause;
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    void HandlePause(bool paused)
    {
        gameObject.SetActive(paused);
    }
}
