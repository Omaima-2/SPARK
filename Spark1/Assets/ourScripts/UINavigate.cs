using UnityEngine;

public class UIPanelNavigation : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject welcomePanel;
    public GameObject SignupPanel;

    // Function to Show the Login Panel
    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        welcomePanel.SetActive(false);
    }
    public void ShowLoginPanel2()
    {
        loginPanel.SetActive(true);
        SignupPanel.SetActive(false);
    }
}
