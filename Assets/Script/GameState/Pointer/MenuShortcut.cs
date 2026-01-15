using UnityEngine;

public class MenuShortcuts : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Stessa azione del bottone "Calibration"
            if (GameManager.Instance != null)
                GameManager.Instance.GoToCalbrationScene();
            else
                Debug.LogError("GameManager instance not found (MenuShortcuts).");
        }
    }
}
