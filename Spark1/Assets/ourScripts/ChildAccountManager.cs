using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using System;
using System.Collections;

[System.Serializable]
public class ChildAccount
{
    public string id;
    public string name;
    public int streak;
}

public class ChildAccountManager : MonoBehaviour
{
    // Firebase references
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore db;
    
    // UI Elements for Parent View
    [Header("Parent Home UI")]
    public Transform childListContainer; // Container for child accounts on home page
    public GameObject childListItemPrefab; // Prefab for child list items
    public Button addChildButton; // Regular button to add child
    
    // UI Elements for Add Child Popup
    [Header("Add Child Popup")]
    public GameObject addChildPopup;
    public TMP_InputField childNameInput;
    public Button confirmAddChildButton;
    public Button cancelAddChildButton;
    
    // UI Elements for Child Info/Switch
    [Header("Child Info UI")]
    public GameObject childInfoPopup;
    public TextMeshProUGUI childInfoNameText;
    public TextMeshProUGUI childInfoStreakText;
    public Button switchToChildButton;
    public Button closeChildInfoButton;
    public Button deleteChildButton;
    
    // UI for Child Mode
    [Header("Child Mode UI")]
    public GameObject parentPanel; // Parent panel to hide/show
    public GameObject childModePanel; // Child mode panel to hide/show
    public TextMeshProUGUI childModeNameText;
    public TextMeshProUGUI childModeStreakText;
    public Button switchToParentButton;
    
    // State tracking
    private List<ChildAccount> childAccounts = new List<ChildAccount>();
    private ChildAccount selectedChild; // Currently selected/active child
    private bool isInChildMode = false;
    
    // Constants
    private const int MAX_CHILDREN = 6;


    void Start()
{
    // Wait for Firebase to initialize before setting up
    StartCoroutine(WaitForFirebaseInitialization());
}

private IEnumerator WaitForFirebaseInitialization()
{
    // Make sure you have the proper using statement at the top of your file
    // using System.Collections;
    
    Debug.Log("Waiting for Firebase to initialize...");
    
    // Wait until Firebase is available
    while (FirebaseAuth.DefaultInstance == null)
    {
        yield return new WaitForSeconds(0.2f);
    }
    
    Debug.Log("Firebase is initialized, continuing setup...");
    
    // Now we can initialize our manager
    InitializeManager();
}

private void InitializeManager()
{
    // Set up button listeners
    addChildButton.onClick.AddListener(ShowAddChildPopup);
    confirmAddChildButton.onClick.AddListener(AddNewChild);
    cancelAddChildButton.onClick.AddListener(HideAddChildPopup);
    closeChildInfoButton.onClick.AddListener(HideChildInfoPopup);
    switchToChildButton.onClick.AddListener(SwitchToChildMode);
    switchToParentButton.onClick.AddListener(SwitchToParentMode);
    deleteChildButton.onClick.AddListener(DeleteSelectedChild);
    
    // Get Firebase instances
    auth = FirebaseAuth.DefaultInstance;
    db = FirebaseFirestore.DefaultInstance;
    
    // Check if user is logged in
    if (auth.CurrentUser != null)
    {
        user = auth.CurrentUser;
        LoadChildAccounts();
    }
    else
    {
        Debug.LogError("User not logged in. Cannot access child accounts.");
    }
}
    
    
   
    
    private async void LoadChildAccounts()
    {
        try
        {
            childAccounts.Clear();
            
            // Query for all child accounts linked to this parent
            QuerySnapshot querySnapshot = await db.Collection("users")
                .Document(user.UserId)
                .Collection("children")
                .GetSnapshotAsync();
                
            foreach (DocumentSnapshot childDoc in querySnapshot.Documents)
            {
                var childData = childDoc.ToDictionary();
                
                ChildAccount child = new ChildAccount
                {
                    id = childDoc.Id,
                    name = childData.TryGetValue("name", out object nameObj) ? nameObj.ToString() : "Unknown"
                };
                
                // Get streak if it exists
                if (childData.TryGetValue("streak", out object streakObj) && streakObj is long streakLong)
                {
                    child.streak = (int)streakLong;
                }
                
                childAccounts.Add(child);
            }
            
            // Refresh the child list UI
            RefreshChildList();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading child accounts: {e.Message}");
        }
    }
    
    private void RefreshChildList()
    {
        // Check if prefab is assigned
        if (childListItemPrefab == null)
        {
            Debug.LogError("Child list item prefab is not assigned in the Inspector!");
            return;
        }
        
        // Clear existing list items
        foreach (Transform child in childListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create list items for each child
        foreach (ChildAccount child in childAccounts)
        {
            // Instantiate the prefab
            GameObject listItem = Instantiate(childListItemPrefab, childListContainer);
            
            // Find UI elements in the prefab - using your structure with kid_name
            TextMeshProUGUI nameText = listItem.GetComponentInChildren<TextMeshProUGUI>();
            
            // Validate component is found
            if (nameText == null)
            {
                Debug.LogError("Child list item prefab is missing TextMeshProUGUI component!");
                continue;
            }
            
            // Set up the text
            nameText.text = child.name;
            
            // Add button click handler
            Button button = listItem.GetComponent<Button>();
            if (button != null)
            {
                // Create a local copy of child for the lambda
                ChildAccount childCopy = child;
                button.onClick.AddListener(() => ShowChildInfo(childCopy));
            }
            else
            {
                // Try to add a button component if it doesn't exist
                button = listItem.AddComponent<Button>();
                if (button != null)
                {
                    ChildAccount childCopy = child;
                    button.onClick.AddListener(() => ShowChildInfo(childCopy));
                }
                else
                {
                    Debug.LogError("Could not add Button component to child list item!");
                }
            }
        }
        
        // Update add button visibility based on max children
        addChildButton.gameObject.SetActive(childAccounts.Count < MAX_CHILDREN);
    }
    
    private void ShowChildInfo(ChildAccount child)
    {
        selectedChild = child;
        
        // Update child info popup
        childInfoNameText.text = child.name;
        childInfoStreakText.text = $"Streak: {child.streak}";
        
        // Show the popup
        childInfoPopup.SetActive(true);
    }
    
    private void HideChildInfoPopup()
    {
        childInfoPopup.SetActive(false);
    }
    
    private void ShowAddChildPopup()
    {
        childNameInput.text = "";
        addChildPopup.SetActive(true);
    }
    
    private void HideAddChildPopup()
    {
        addChildPopup.SetActive(false);
    }
    
    private async void AddNewChild()
    {
        if (childAccounts.Count >= MAX_CHILDREN)
        {
            Debug.LogWarning("Maximum number of child accounts reached.");
            return;
        }
        
        string childName = childNameInput.text.Trim();
        
        if (string.IsNullOrEmpty(childName))
        {
            Debug.LogWarning("Child name cannot be empty.");
            return;
        }
        
        try
        {
            // Create a new child account in Firestore
            DocumentReference newChildRef = db.Collection("users")
                .Document(user.UserId)
                .Collection("children")
                .Document(); // Auto-generate ID
                
            Dictionary<string, object> childData = new Dictionary<string, object>
            {
                { "name", childName },
                { "streak", 0 },
                { "createdAt", FieldValue.ServerTimestamp }
            };
            
            await newChildRef.SetAsync(childData);
            
            // Create local object
            ChildAccount newChild = new ChildAccount
            {
                id = newChildRef.Id,
                name = childName,
                streak = 0
            };
            
            // Add to local list
            childAccounts.Add(newChild);
            
            // Refresh UI
            RefreshChildList();
            
            // Hide the popup
            HideAddChildPopup();
            
            Debug.Log($"Added new child account: {childName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error adding child account: {e.Message}");
        }
    }
    
    private void SwitchToChildMode()
    {
        if (selectedChild == null)
        {
            Debug.LogWarning("No child selected.");
            return;
        }
        
        isInChildMode = true;
        
        // Update child mode UI
        childModeNameText.text = selectedChild.name;
        childModeStreakText.text = $"Streak: {selectedChild.streak}";
        
        // Hide parent panel and show child panel
        parentPanel.SetActive(false);
        childModePanel.SetActive(true);
        childInfoPopup.SetActive(false);
        
        Debug.Log($"Switched to child mode: {selectedChild.name}");
    }
    
    public void SwitchToParentMode()
    {
        isInChildMode = false;
        
        // Show parent panel and hide child panel
        parentPanel.SetActive(true);
        childModePanel.SetActive(false);
        
        Debug.Log("Switched back to parent mode");
    }
    
    // Call this method to increment the streak for the current child
    public async Task IncrementStreak()
    {
        if (!isInChildMode || selectedChild == null)
        {
            Debug.LogWarning("Not in child mode or no child selected.");
            return;
        }
        
        try
        {
            int newStreak = selectedChild.streak + 1;
            
            // Update in Firestore
            DocumentReference childRef = db.Collection("users")
                .Document(user.UserId)
                .Collection("children")
                .Document(selectedChild.id);
                
            await childRef.UpdateAsync("streak", newStreak);
            
            // Update local data
            selectedChild.streak = newStreak;
            
            // Update UI
            childModeStreakText.text = $"Streak: {newStreak}";
            
            Debug.Log($"Incremented streak for {selectedChild.name}: {newStreak}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating streak: {e.Message}");
        }
    }
    
    // Call this to get the currently active child (or null if in parent mode)
    public ChildAccount GetActiveChild()
    {
        return isInChildMode ? selectedChild : null;
    }
    
    // Check if we're in child mode
    public bool IsInChildMode()
    {
        return isInChildMode;
    }
    
    // Get child account by ID
    public ChildAccount GetChildById(string id)
    {
        return childAccounts.Find(child => child.id == id);
    }
    
    // Delete the currently selected child account
    public async void DeleteSelectedChild()
    {
        if (selectedChild == null)
        {
            Debug.LogWarning("No child selected to delete.");
            return;
        }
        
        try
        {
            // Delete from Firestore
            DocumentReference childRef = db.Collection("users")
                .Document(user.UserId)
                .Collection("children")
                .Document(selectedChild.id);
                
            await childRef.DeleteAsync();
            
            // Remove from local list
            childAccounts.Remove(selectedChild);
            
            // Refresh UI
            RefreshChildList();
            
            // Hide the popup
            HideChildInfoPopup();
            
            Debug.Log($"Deleted child account: {selectedChild.name}");
            
            // Clear selected child
            selectedChild = null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting child account: {e.Message}");
        }
    }
}