using UnityEngine;

public class UIPanelNavigation : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject welcomePanel;

    // Function to Show the Login Panel
    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        welcomePanel.SetActive(false);
    }
}
