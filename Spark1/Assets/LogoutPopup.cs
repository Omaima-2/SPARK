using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Optional if you switch scenes on logout

public class LogoutPopup : MonoBehaviour
{
    public GameObject logoutPanel;   // Assign your confirmation panel
    public Button cancelButton;      // Assign "Cancel" button

    void Start()
    {
        logoutPanel.SetActive(false); // Hide at start

  
        cancelButton.onClick.AddListener(OnCancelLogout);
    }

    public void ShowLogoutPopup()
    {
        logoutPanel.SetActive(true);
    }

 
    public void HideLogoutPopup()
    {
        logoutPanel.SetActive(false);
    }


    void OnCancelLogout()
    {
        logoutPanel.SetActive(false);
    }
}
