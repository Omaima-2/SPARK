using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    public GameObject popupPanel; // Reference to the popup panel

    void Start()
    {
        popupPanel.SetActive(false); // Make sure the popup is hidden at the start
    }

    public void ShowPopup()
    {
        popupPanel.SetActive(true); // Show the popup
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false); // Hide the popup
    }
}

