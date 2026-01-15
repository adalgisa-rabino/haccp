using UnityEngine;

public class CalibrationShortcut : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Stessa azione del bottone "Menu"
            if (GameManager.Instance != null)
                GameManager.Instance.GoToMenu();
            else
                Debug.LogError("GameManager instance not found (MenuShortcuts).");
        } 
        else if(Input.GetKeyDown(KeyCode.C))
        {
            // Stessa azione del bottone "Calibration"
            if (GameManager.Instance != null)
                GameManager.Instance.GoToCalbrationScene();
            else
                Debug.LogError("GameManager instance not found (MenuShortcuts).");
        }
    }
}
