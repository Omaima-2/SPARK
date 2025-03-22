using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private Vector3 playerPosition;
    private Quaternion playerRotation;
    private float animationTime;
    private static bool returningFromPath1 = false;

    public GameObject player;
    public Animator environmentAnimator;
    public string triggerStateName = "EnterPath1";

    private static GameManager instance;
    private bool isSwitchingScene = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager instance created and marked as DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("Duplicate GameManager found, destroying this instance");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Ensure references are found
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log(player != null ? "Player found" : "Player not found");
        }
        if (environmentAnimator == null)
        {
            environmentAnimator = FindObjectOfType<Animator>();
            Debug.Log(environmentAnimator != null ? "Animator found" : "Animator not found");
        }

        if (returningFromPath1)
        {
            Debug.Log("Returning from Path1, attempting to load state");
            LoadEnvironmentState();
            returningFromPath1 = false;
        }
    }

    void Update()
    {
        if (!isSwitchingScene && environmentAnimator != null && IsInTriggerState())
        {
            SwitchToPath1();
        }
    }

    bool IsInTriggerState()
    {
        AnimatorStateInfo stateInfo = environmentAnimator.GetCurrentAnimatorStateInfo(0);
        bool isTriggered = stateInfo.IsName(triggerStateName) && stateInfo.normalizedTime >= 0f;
        Debug.Log($"Checking trigger state: {triggerStateName}, IsTriggered: {isTriggered}, NormalizedTime: {stateInfo.normalizedTime}");
        return isTriggered;
    }

    void SwitchToPath1()
    {
        isSwitchingScene = true;
        SaveEnvironmentState();
        Debug.Log($"Switching to Path1, Saved State - Pos: {playerPosition}, Rot: {playerRotation}, AnimTime: {animationTime}");
        SceneManager.LoadScene("Path1");
        isSwitchingScene = false;
    }

    void SaveEnvironmentState()
    {
        if (player != null)
        {
            playerPosition = player.transform.position;
            playerRotation = player.transform.rotation;
        }
        if (environmentAnimator != null)
        {
            animationTime = environmentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }
    }

    public void ReturnToEnvironment()
    {
        if (!isSwitchingScene)
        {
            isSwitchingScene = true;
            returningFromPath1 = true;
            Debug.Log("Returning to Environment_Free");
            SceneManager.LoadScene("Environment_Free");
            isSwitchingScene = false;
        }
    }

    void LoadEnvironmentState()
    {
        if (player != null)
        {
            player.transform.position = playerPosition;
            player.transform.rotation = playerRotation;
            Debug.Log($"Player state restored - Pos: {playerPosition}, Rot: {playerRotation}");
        }
        else
        {
            Debug.LogError("Player is null in LoadEnvironmentState");
        }

        if (environmentAnimator != null)
        {
            environmentAnimator.Play(triggerStateName, 0, animationTime);
            Debug.Log($"Animator state restored - State: {triggerStateName}, Time: {animationTime}");
        }
        else
        {
            Debug.LogError("Animator is null in LoadEnvironmentState");
        }
    }
}