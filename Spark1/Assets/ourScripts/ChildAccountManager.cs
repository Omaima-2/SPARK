using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;


// Helper class to store child reference data
public class ChildReference : MonoBehaviour
{
    public string childId;
}

[System.Serializable]
public class ChildAccount
{  
    public string id;
    public string name;
    public int streak;
    public int avatarId; //  0 for first avatar, 1 for second avatar
}

public class ChildAccountManager : MonoBehaviour
{
    // Singleton instance
    public static ChildAccountManager Instance { get; private set; }
    
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
    public TextMeshProUGUI Error_addChildName;
    
    // UI Elements for Child Info/Switch
    [Header("Child Info UI")]
    public GameObject childInfoPopup;
    public TextMeshProUGUI childInfoNameText;
    public TextMeshProUGUI childInfoStreakText;
    public Image childInfoAvatarImage; // New: Image to show avatar in child info
    public Button switchToChildButton;
    public Button closeChildInfoButton;
    public Button deleteChildButton;
    
    // UI for Child Mode
    [Header("Child Mode UI")]
    public GameObject parentPanel; // Parent panel to hide/show
    public GameObject childModePanel; // Child mode panel to hide/show
    public TextMeshProUGUI childModeNameText;
    public TextMeshProUGUI childModeStreakText;
    public Image childModeAvatarImage; // New: Image to show avatar in child mode
    public Button switchToParentButton;
    public Button storyButton;

    
    [Header("Avatar Selection")]
    public Button avatar0Button;  // Button with avatar0 image
    public Button avatar1Button;  // Button with avatar1 image
    public Sprite avatar0Sprite;  // Sprite for avatar0
    public Sprite avatar1Sprite;  // Sprite for avatar1
    
    private int selectedAvatarId = 0;  // Default to first avatar
    
   [Header("Avatar Selection UI")]
   public GameObject avatar0Border; // A border object (child of avatar0Button or separate)
   public GameObject avatar1Border; // A border object (child of avatar1Button or separate)


    // State tracking
    private List<ChildAccount> childAccounts = new List<ChildAccount>();
    private ChildAccount selectedChild; // Currently selected/active child
    private bool isInChildMode = false;
    
    // Constants
    private const int MAX_CHILDREN = 6;

    void Awake()
    {
        // Setup singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Wait for Firebase to initialize before setting up
        StartCoroutine(WaitForFirebaseInitialization());
    }

    void OnEnable()
    {
        // Subscribe to login state changes
        if (FirebaseController.Instance != null)
        {
            FirebaseController.Instance.OnUserChanged += OnUserChanged;
            Debug.Log("ChildAccountManager: Subscribed to user change events");
        }
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        if (FirebaseController.Instance != null)
        {
            FirebaseController.Instance.OnUserChanged -= OnUserChanged;
            Debug.Log("ChildAccountManager: Unsubscribed from user change events");
        }
    }

    void OnUserChanged(Firebase.Auth.FirebaseUser oldUser, Firebase.Auth.FirebaseUser newUser)
    {
        Debug.Log("👥 OnUserChanged triggered in ChildAccountManager.");
        Debug.Log($"ChildAccountManager: User changed from {oldUser?.UserId} to {newUser?.UserId}");
        
        // Clear all child data
        childAccounts.Clear();
        selectedChild = null;
        isInChildMode = false;
        
        // Clear UI
        foreach (Transform child in childListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Reset UI panels
        if (parentPanel != null) parentPanel.SetActive(true);
        if (childModePanel != null) childModePanel.SetActive(false);
        if (childInfoPopup != null) childInfoPopup.SetActive(false);
        if (addChildPopup != null) addChildPopup.SetActive(false);
        
        // If new user is logged in, load their children
        if (newUser != null)
        {
            user = newUser;
            auth = FirebaseAuth.DefaultInstance;
            LoadChildAccounts();
        }
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        Debug.Log("Waiting for Firebase to initialize...");
        
        // Wait until FirebaseController is initialized
        while (FirebaseController.Instance == null || !FirebaseController.Instance.IsFirebaseInitialized)
        {
            yield return new WaitForSeconds(0.2f);
        }
        
        Debug.Log("Firebase is initialized via FirebaseController, continuing setup...");
        
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
        
        // Setup avatar selection buttons
        avatar0Button.onClick.AddListener(() => SelectAvatar(0));
        avatar1Button.onClick.AddListener(() => SelectAvatar(1));
       
        // Start with the first avatar selected by default
        SelectAvatar(0);

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
          //  Debug.LogError("User not logged in. Cannot access child accounts.");
        }
    }
     
    public async void LoadChildAccounts()
{
    try
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("User authentication lost. Cannot load child accounts.");
            return;
        }

        user = auth.CurrentUser;
        string parentUserId = user.UserId;

        Debug.Log($"📥 LoadChildAccounts for Firebase user: {parentUserId}");

        DocumentSnapshot parentDoc = await db.Collection("users").Document(parentUserId).GetSnapshotAsync();
        if (!parentDoc.Exists)
        {
            Debug.LogError($"Parent document does not exist for ID: {parentUserId}");
            return;
        }

        childAccounts.Clear();

        QuerySnapshot querySnapshot = await db.Collection("users")
            .Document(parentUserId)
            .Collection("children")
            .GetSnapshotAsync();

        Debug.Log($"Found {querySnapshot.Count} child accounts");

        foreach (DocumentSnapshot childDoc in querySnapshot.Documents)
        {
            var childData = childDoc.ToDictionary();

            ChildAccount child = new ChildAccount
            {
                id = childDoc.Id,
                name = childData.TryGetValue("name", out object nameObj) ? nameObj.ToString() : "Unknown",
                streak = 0,
                avatarId = 0
            };

            // Load streak
            if (childData.TryGetValue("streak", out object streakObj) && streakObj is long streakLong)
            {
                child.streak = (int)streakLong;
            }
           
            if (childData.TryGetValue("lastVisitTime", out object visitObj) && visitObj is Timestamp lastVisitTimestamp)
{
    DateTime lastVisit = lastVisitTimestamp.ToDateTime().Date;
    DateTime today = DateTime.UtcNow.Date;

    if (lastVisit < today.AddDays(-1))
    {
        if (child.streak != 0)
        {
            DocumentReference childRef = db.Collection("users")
                .Document(parentUserId)
                .Collection("children")
                .Document(child.id);

            await childRef.UpdateAsync("streak", 0);
            child.streak = 0;
            Debug.Log($"⏳ Reset streak for child {child.name}");
        }
    }
}

            // Load avatar
            if (childData.TryGetValue("avatarId", out object avatarObj) && avatarObj is long avatarLong)
            {
                child.avatarId = (int)avatarLong;
            }

            childAccounts.Add(child);
            Debug.Log($"Added child: {child.name} with ID: {child.id}, avatar: {child.avatarId}");
        }

        // Refresh UI
        RefreshChildList();
    }
    catch (Exception e)
    {
        Debug.LogError($"Error loading child accounts: {e.Message}");
        Debug.LogException(e);
    }
}

    
    void RefreshChildList()
    {
        Debug.Log($"🧼 Refreshing child list UI for {childAccounts.Count} children.");

        // Debug log to track execution
        Debug.Log($"Refreshing child list with {childAccounts.Count} children for parent: {auth.CurrentUser?.UserId}");
        
        // Check if prefab is assigned
        if (childListItemPrefab == null)
        {
            Debug.LogError("Child list item prefab is not assigned in the Inspector!");
            return;
        }
        
        // Clear existing list items by destroying them completely
        foreach (Transform child in childListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create new list items for each child
        foreach (ChildAccount childData in childAccounts)
        {
            // Debug each child being added
            Debug.Log($"Creating list item for child: {childData.name} (ID: {childData.id}, Avatar: {childData.avatarId})");
            
            // Instantiate the prefab
            GameObject listItem = Instantiate(childListItemPrefab, childListContainer);
            
            // Get the ChildItemUI component
            ChildItemUI childItemUI = listItem.GetComponent<ChildItemUI>();
            if (childItemUI != null)
            {
                // Assign avatar sprites to the item
                childItemUI.avatar0Sprite = avatar0Sprite;
                childItemUI.avatar1Sprite = avatar1Sprite;
                
                // Set up the child item with data and callback
                childItemUI.Setup(childData, ShowChildInfo);
            }
            else
            {
                Debug.LogError("Child list item prefab is missing ChildItemUI component!");
                
                // Try to add one as a fallback
                childItemUI = listItem.AddComponent<ChildItemUI>();
                if (childItemUI != null)
                {
                    // Find the required components
                    childItemUI.childNameText = listItem.GetComponentInChildren<TextMeshProUGUI>();
                    childItemUI.avatarImage = listItem.GetComponentInChildren<Image>();
                    
                    // Assign avatar sprites
                    childItemUI.avatar0Sprite = avatar0Sprite;
                    childItemUI.avatar1Sprite = avatar1Sprite;
                    
                    if (childItemUI.childNameText != null)
                    {
                        childItemUI.Setup(childData, ShowChildInfo);
                    }
                    else
                    {
                        Debug.LogError("Failed to find required components for ChildItemUI!");
                    }
                }
            }
        }
        
        // Update add button visibility based on max children
        addChildButton.gameObject.SetActive(childAccounts.Count < MAX_CHILDREN);
    }
    
    private void ShowChildInfo(ChildAccount child)
    {
        if (child == null)
        {
            Debug.LogError("Attempted to show info for null child!");
            return;
        }
        
        Debug.Log($"Showing info for child: {child.name} (ID: {child.id}, Avatar: {child.avatarId})");
        
        selectedChild = child;
        
        // Update child info popup
        childInfoNameText.text = child.name;
        childInfoStreakText.text = $"{child.streak}";
        
        // Set avatar in the child info popup
        if (childInfoAvatarImage != null)
        {
            if (child.avatarId == 0)
            {
                childInfoAvatarImage.sprite = avatar0Sprite;
            }
            else
            {
                childInfoAvatarImage.sprite = avatar1Sprite;
            }
        }
        
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
        selectedAvatarId = 0; // Reset to default avatar
        
        // Reset visual selection
        SelectAvatar(0);
        
        addChildPopup.SetActive(true);
    }
    
    private void HideAddChildPopup()
    {

        addChildPopup.SetActive(false);
        // Clear the input field
        childNameInput.text = "";

         if (Error_addChildName != null)
         {
          Error_addChildName.text = "";
          Error_addChildName.gameObject.SetActive(false);
       }
    }
    
    private void SelectAvatar(int avatarId)
{
    // Store which avatar is selected
    selectedAvatarId = avatarId;
    
    // Reset both buttons to normal state
    avatar0Button.GetComponent<Image>().color = Color.white;
    avatar1Button.GetComponent<Image>().color = Color.white;
    
    
    
    // Toggle border visibility
    if (avatar0Border != null) avatar0Border.SetActive(avatarId == 0);
    if (avatar1Border != null) avatar1Border.SetActive(avatarId == 1);
    
    Debug.Log($"Avatar {avatarId} selected");
}

    public Sprite GetAvatarSprite(int avatarId)
    {
        if (avatarId == 0)
        {
            return avatar0Sprite;
        }
        else
        {
            return avatar1Sprite;
        }
    }

    void DisplayError(string message , TextMeshProUGUI errorTextUI)
    {
        if (errorTextUI != null)
            {
                errorTextUI.gameObject.SetActive(false); // Force refresh
                errorTextUI.text = message;
                errorTextUI.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("errorTextUI is NULL! Assign it in the Inspector.");
            }
    }

    private async void AddNewChild()
    {
        // must add a UI error mesage here! 
        if (childAccounts.Count >= MAX_CHILDREN)
        {
            Debug.LogWarning("Maximum number of child accounts reached.");
            //DisplayError()
            return;
        }
        
        string childName = childNameInput.text.Trim();
        
        if (string.IsNullOrEmpty(childName))
        {
            Debug.LogWarning("Child name cannot be empty.");
            DisplayError("Child name cannot be empty.", Error_addChildName);
            return;
        }
        if (childName.Length > 20)
        {
            Debug.LogWarning("Child name cannot be longer than 20 characters.");
            DisplayError("Oops! Names can’t be longer than 20 characters.", Error_addChildName );
            return;
        }
        
        try
        {
            // Make sure user is still valid
            if (auth.CurrentUser == null)
            {
                Debug.LogError("User authentication lost. Cannot add child account.");
                return;
            }
            
            user = auth.CurrentUser;
            string parentUserId = user.UserId;
            
            // Debug log for parent ID
            Debug.Log($"Creating child with parent ID: {parentUserId} (from auth user: {auth.CurrentUser.UserId})");
            
            // Create a new child account in Firestore
            DocumentReference newChildRef = db.Collection("users")
                .Document(parentUserId)
                .Collection("children")
                .Document(); // Auto-generate ID
                
            Dictionary<string, object> childData = new Dictionary<string, object>
            {
                { "name", childName },
                { "streak", 0 },
                { "createdAt", FieldValue.ServerTimestamp },
                { "parentId", parentUserId }, // Explicitly store the parent ID for verification
                { "avatarId", selectedAvatarId }, // Use the selected avatar ID
                { "parentAuth", auth.CurrentUser.UserId }, // Store auth user ID for debugging
                { "lastVisitTime", FieldValue.ServerTimestamp },

            };
            
            await newChildRef.SetAsync(childData);
            
            Debug.Log($"Added new child '{childName}' with ID {newChildRef.Id} for parent {parentUserId}, with avatar {selectedAvatarId}");
            
            // Create local object
            ChildAccount newChild = new ChildAccount
            {
                id = newChildRef.Id,
                name = childName,
                streak = 0,
                avatarId = selectedAvatarId
            };
            
            // Add to local list
            childAccounts.Add(newChild);
            
            // Refresh UI
            RefreshChildList();
            
            // Hide the popup
            HideAddChildPopup();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error adding child account: {e.Message}");
            Debug.LogException(e);
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

        // Set avatar in child mode
        if (childModeAvatarImage != null)
        {
            if (selectedChild.avatarId == 0)
            {
                childModeAvatarImage.sprite = avatar0Sprite;
            }
            else
            {
                childModeAvatarImage.sprite = avatar1Sprite;
            }
        }

        // Hide parent panel and show child panel
        parentPanel.SetActive(false);
        childModePanel.SetActive(true);
        childInfoPopup.SetActive(false);

        Debug.Log($"Switched to child mode: {selectedChild.name}");

        // ✅ Save ONLY the child ID here
        PlayerPrefs.SetString("CurrentChildID", selectedChild.id);
        PlayerPrefs.Save();

        Debug.Log($"📝 Saved child session. ID: {selectedChild.id}");
    }


    public void SwitchToParentMode()
    {
        isInChildMode = false;
        
        // Show parent panel and hide child panel
        parentPanel.SetActive(true);
        childModePanel.SetActive(false);
        
        Debug.Log("Switched back to parent mode");
    }
    
    public async Task<bool> CheckAndUpdateStreak()
{
    Debug.Log("📍 CheckAndUpdateStreak() started");

    if (!isInChildMode || selectedChild == null)
    {
        Debug.LogWarning("❌ Not in child mode or no selected child");
        return false;
    }

    string parentId = auth.CurrentUser.UserId;
    DocumentReference childRef = db.Collection("users")
        .Document(parentId)
        .Collection("children")
        .Document(selectedChild.id);

    DocumentSnapshot snapshot = await childRef.GetSnapshotAsync();

    if (!snapshot.Exists)
    {
        Debug.LogWarning("❌ Child document not found in Firestore.");
        return false;
    }

    var data = snapshot.ToDictionary();
    DateTime now = DateTime.UtcNow.Date;
    DateTime? lastVisitDate = null;

    if (data.TryGetValue("lastVisitTime", out object lastVisitObj) && lastVisitObj is Timestamp ts)
    {
        lastVisitDate = ts.ToDateTime().Date;
        Debug.Log("📅 Last visit: " + lastVisitDate);
    }

    Debug.Log("📅 Today: " + now);

    if (selectedChild.streak == 0)
    {
        selectedChild.streak = 1;
        Debug.Log("🔥 First time, streak = 1");
    }
    else if (lastVisitDate < now.AddDays(-1))
    {
        selectedChild.streak = 0;
        Debug.Log("❌ Missed a day, reset to 0");
    }
    else if (lastVisitDate < now)
    {
        selectedChild.streak += 1;
        Debug.Log("🔥 New day! Streak increased to " + selectedChild.streak);
    }
    else
    {
        Debug.Log("✅ Already visited today. No increment.");
    }

    await childRef.UpdateAsync(new Dictionary<string, object>
    {
        { "streak", selectedChild.streak },
        { "lastVisitTime", Timestamp.FromDateTime(DateTime.UtcNow) }
    });

    Debug.Log("💾 Firestore updated");

    childModeStreakText.text = $"Streak: {selectedChild.streak}";
    return true;
}



public async void StoryButtonClicked()
{
    Debug.Log("🟢 StoryButtonClicked triggered");

    if (selectedChild == null)
    {
        Debug.LogWarning("❌ No selected child");
        return;
    }

    await CheckAndUpdateStreak(); // ✅ Update the streak safely
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
            // Make sure user is still valid
            if (auth.CurrentUser == null)
            {
                Debug.LogError("User authentication lost. Cannot delete child account.");
                return;
            }
            
            user = auth.CurrentUser;
            string parentUserId = user.UserId;
            
            // Delete from Firestore
            DocumentReference childRef = db.Collection("users")
                .Document(parentUserId)
                .Collection("children")
                .Document(selectedChild.id);
                
            await childRef.DeleteAsync();
            
            Debug.Log($"Deleted child account: {selectedChild.name} from parent: {parentUserId}");
            
            // Remove from local list
            childAccounts.Remove(selectedChild);
            
            // Refresh UI
            RefreshChildList();
            
            // Hide the popup
            HideChildInfoPopup();
            
            // Clear selected child
            selectedChild = null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting child account: {e.Message}");
            Debug.LogException(e);
        }
    }


    // Call this method to clear all child data (useful when logging out)
    public void ClearChildData()
    {
        // Clear the list
        childAccounts.Clear();
        
        // Clear any selected child
        selectedChild = null;
        
        // Reset child mode
        isInChildMode = false;
        
        // Clear the UI list
        foreach (Transform child in childListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Reset all panels to default state
        if (parentPanel != null) parentPanel.SetActive(true);
        if (childModePanel != null) childModePanel.SetActive(false);
        if (childInfoPopup != null) childInfoPopup.SetActive(false);
        if (addChildPopup != null) addChildPopup.SetActive(false);
        
        Debug.Log("Child account data cleared");
    }

}