using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject welcomePanel;
    public GameObject loginPanel;
    public GameObject signupPanel;

    // Method to open a specific panel
    public void OpenPanel(GameObject panelToOpen)
    {
        // Deactivate all panels first
        welcomePanel.SetActive(false);
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);

        // Activate the selected panel
        panelToOpen.SetActive(true);
    }
}
