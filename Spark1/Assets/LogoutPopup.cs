using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Optional if you switch scenes on logout

public class LogoutPopup : MonoBehaviour
{
    public GameObject logoutPanel;   // Assign your confirmation panel
    public Button confirmButton;     // Assign "Log Out" button
    public Button cancelButton;      // Assign "Cancel" button

    void Start()
    {
        logoutPanel.SetActive(false); // Hide at start

        confirmButton.onClick.AddListener(OnConfirmLogout);
        cancelButton.onClick.AddListener(OnCancelLogout);
    }

    public void ShowLogoutPopup()
    {
        //messageText.text = "Are you sure you want to log out? We’ll miss you!";
        logoutPanel.SetActive(true);
    }

    void OnConfirmLogout()
    {
        Debug.Log("✅ Logging out...");
        // Do your logout logic here (e.g., FirebaseAuth.SignOut())
        SceneManager.LoadScene("LoginScene"); // Or any other scene
    }

    void OnCancelLogout()
    {
        logoutPanel.SetActive(false);
    }
}
