using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject welcomePanel;
    public GameObject signInPanel;
    public GameObject signUpPanel;
    public GameObject accountsPanel;
    public GameObject homePanel;

    // Start by showing only the Welcome panel
   
    public GameObject[] panels; // Add all panels in correct order (Welcome, SignIn, SignUp, Accounts, Home)

    void Start()
    {
        ShowPanelByIndex(NavigationData.targetPanelIndex); // ← This line is ESSENTIAL
    }


    public void ShowPanelByIndex(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
        }
    }

    // Optional: For button-based navigation if still needed
    public void ShowWelcome() { ShowPanelByIndex(0); }
    public void ShowSignIn() { ShowPanelByIndex(1); }
    public void ShowSignUp() { ShowPanelByIndex(2); }
    public void ShowAccounts() { ShowPanelByIndex(3); }
    public void ShowHome() { ShowPanelByIndex(4); }
}

