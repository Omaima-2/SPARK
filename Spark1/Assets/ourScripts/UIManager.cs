using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject welcomePanel;
    public GameObject signInPanel;
    public GameObject signUpPanel;
    public GameObject accountsPanel;
    public GameObject homePanel;

    // Start by showing only the Welcome panel
    void Start()
    {
        ShowPanel(welcomePanel);
    }

    // Function to show the selected panel and hide others
    public void ShowPanel(GameObject panelToShow)
    {
        // Hide all panels first
        welcomePanel.SetActive(false);
        signInPanel.SetActive(false);
        signUpPanel.SetActive(false);
        accountsPanel.SetActive(false);
        homePanel.SetActive(false);

        // Show the selected panel
        panelToShow.SetActive(true);
    }

    // Button Functions to Navigate
    public void ShowWelcome() { ShowPanel(welcomePanel); }
    public void ShowSignIn() { ShowPanel(signInPanel); }
    public void ShowSignUp() { ShowPanel(signUpPanel); }
    public void ShowAccounts() { ShowPanel(accountsPanel); }
    public void ShowHome() { ShowPanel(homePanel); }
}
