
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChildItemUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI childNameText;
    public Button childButton;
    
    private ChildAccount childData;
    private System.Action<ChildAccount> onChildSelected;
    
    public void Setup(ChildAccount childAccount, System.Action<ChildAccount> selectCallback)
    {
        childData = childAccount;
        onChildSelected = selectCallback;
        
        // Set the name text
        if (childNameText != null)
        {
            childNameText.text = childAccount.name;
        }
        
        // Add button listener
        if (childButton != null)
        {
            childButton.onClick.RemoveAllListeners();
            childButton.onClick.AddListener(OnChildButtonClicked);
        }
    }
    
    void OnChildButtonClicked()
    {
        if (onChildSelected != null)
        {
            onChildSelected(childData);
        }
    }
}