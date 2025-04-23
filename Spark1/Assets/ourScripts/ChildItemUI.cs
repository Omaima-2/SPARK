using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChildItemUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI childNameText;
    public Image avatarImage; // Image component to display the avatar
    
    // References to the avatar sprites that will be set in the Inspector
    [Header("Avatar Sprites")]
    public Sprite avatar0Sprite; // Assign this in the Inspector
    public Sprite avatar1Sprite; // Assign this in the Inspector
    
    private ChildAccount childData;
    private System.Action<ChildAccount> onChildSelected;
    
    void Awake()
    {
        // Add a Button component to this GameObject if it doesn't already have one
        Button itemButton = GetComponent<Button>();
        if (itemButton == null)
        {
            itemButton = gameObject.AddComponent<Button>();
        }
        
        // Set up the click handler
        itemButton.onClick.AddListener(OnChildItemClicked);
    }
    
    public void Setup(ChildAccount childAccount, System.Action<ChildAccount> selectCallback)
    {
        childData = childAccount;
        onChildSelected = selectCallback;
        
        // Set the name text
        if (childNameText != null)
        {
            childNameText.text = childAccount.name;
        }
        else
        {
            Debug.LogError($"childNameText not assigned for child: {childAccount.name}");
        }
        
        // Set the avatar image based on avatarId
        if (avatarImage != null)
        {
            // Get the correct avatar sprite based on the avatarId
            if (childAccount.avatarId == 0 && avatar0Sprite != null)
            {
                avatarImage.sprite = avatar0Sprite;
            }
            else if (childAccount.avatarId == 1 && avatar1Sprite != null)
            {
                avatarImage.sprite = avatar1Sprite;
            }
            else
            {
                Debug.LogWarning($"Missing avatar sprite for avatarId: {childAccount.avatarId}");
            }
        }
        else
        {
            Debug.LogWarning($"avatarImage not assigned for child: {childAccount.name}");
        }
    }
    
    void OnChildItemClicked()
    {
        Debug.Log($"Child item clicked: {childData?.name} (ID: {childData?.id}, Avatar: {childData?.avatarId})");
        if (onChildSelected != null && childData != null)
        {
            onChildSelected(childData);
        }
        else
        {
            Debug.LogError("Cannot select child: callback or data is null");
        }
    }
}