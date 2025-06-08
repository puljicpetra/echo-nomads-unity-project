using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [SerializeField] private string targetSceneName = "NextScene";
    [SerializeField] private int targetSceneIndex = -1; // Use -1 to use scene name instead
    [SerializeField] private bool useSceneIndex = false;
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionDelay = 0.5f;
    [SerializeField] private bool showLoadingScreen = false;
    [SerializeField] private string loadingScreenScene = "LoadingScreen";
    
    [Header("Audio Settings")]
    [SerializeField] private string transitionSoundName = "SceneTransition";
    [SerializeField] private bool playTransitionSound = true;
    
    [Header("Player Requirements")]
    [SerializeField] private bool requirePlayerGrounded = false;
    [SerializeField] private bool disablePlayerControlsDuringTransition = true;
    [SerializeField] private bool saveProgressBeforeTransition = true;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject transitionEffect;
    [SerializeField] private bool fadeScreen = true;
    [SerializeField] private float fadeTime = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Private fields
    private bool isTransitioning = false;
    private bool playerInTrigger = false;
    private GameObject playerObject;
    private Invector.vCharacterController.vThirdPersonController playerController;
    
    // Events
    public System.Action<string> OnSceneTransitionStarted;
    public System.Action<string> OnSceneTransitionCompleted;
    
    void Start()
    {
        // Ensure we have a trigger collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"SceneTransitionTrigger '{name}': No collider found! Adding BoxCollider...");
            col = gameObject.AddComponent<BoxCollider>();
        }
        
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            if (debugMode)
            {
                Debug.Log($"SceneTransitionTrigger '{name}': Set collider as trigger");
            }
        }
        
        // Validate scene settings
        ValidateSceneSettings();
    }
    
    void ValidateSceneSettings()
    {
        if (useSceneIndex)
        {
            if (targetSceneIndex < 0 || targetSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"SceneTransitionTrigger '{name}': Invalid scene index {targetSceneIndex}! " +
                              $"Valid range: 0-{SceneManager.sceneCountInBuildSettings - 1}");
            }
        }
        else
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError($"SceneTransitionTrigger '{name}': Target scene name is empty!");
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning && !playerInTrigger)
        {
            playerInTrigger = true;
            playerObject = other.gameObject;
            playerController = playerObject.GetComponent<Invector.vCharacterController.vThirdPersonController>();
            
            if (debugMode)
            {
                Debug.Log($"SceneTransitionTrigger '{name}': Player entered trigger zone");
            }
            
            // Check if player meets requirements
            if (CanTransition())
            {
                StartTransition();
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInTrigger)
        {
            playerInTrigger = false;
            
            if (debugMode)
            {
                Debug.Log($"SceneTransitionTrigger '{name}': Player exited trigger zone");
            }
        }
    }
    
    bool CanTransition()
    {
        if (requirePlayerGrounded && playerController != null)
        {
            if (!playerController.isGrounded)
            {
                if (debugMode)
                {
                    Debug.Log($"SceneTransitionTrigger '{name}': Player not grounded, transition blocked");
                }
                return false;
            }
        }
        
        return true;
    }
    
    void StartTransition()
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        
        if (debugMode)
        {
            string sceneName = useSceneIndex ? $"Index: {targetSceneIndex}" : targetSceneName;
            Debug.Log($"SceneTransitionTrigger '{name}': Starting transition to {sceneName}");
        }
        
        // Trigger event
        string targetScene = useSceneIndex ? targetSceneIndex.ToString() : targetSceneName;
        OnSceneTransitionStarted?.Invoke(targetScene);
        
        // Save progress if requested
        if (saveProgressBeforeTransition)
        {
            SaveProgress();
        }
        
        // Disable player controls if requested
        if (disablePlayerControlsDuringTransition && playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Play transition sound
        if (playTransitionSound && !string.IsNullOrEmpty(transitionSoundName))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(transitionSoundName);
            }
        }
        
        // Show transition effect
        if (transitionEffect != null)
        {
            GameObject effectInstance = Instantiate(transitionEffect, transform.position, transform.rotation);
            DontDestroyOnLoad(effectInstance);
        }
        
        // Start the actual transition
        StartCoroutine(PerformTransition());
    }
    
    IEnumerator PerformTransition()
    {
        // Wait for transition delay
        if (transitionDelay > 0)
        {
            yield return new WaitForSeconds(transitionDelay);
        }
        
        // Handle screen fade
        if (fadeScreen)
        {
            yield return StartCoroutine(FadeScreen());
        }
        
        // Load the target scene
        if (showLoadingScreen && !string.IsNullOrEmpty(loadingScreenScene))
        {
            yield return StartCoroutine(LoadSceneWithLoadingScreen());
        }
        else
        {
            yield return StartCoroutine(LoadSceneDirectly());
        }
        
        // Trigger completion event
        string targetScene = useSceneIndex ? targetSceneIndex.ToString() : targetSceneName;
        OnSceneTransitionCompleted?.Invoke(targetScene);
    }
    
    IEnumerator FadeScreen()
    {
        // This is a simple fade implementation
        // You might want to replace this with your own fade system
        if (debugMode)
        {
            Debug.Log($"SceneTransitionTrigger '{name}': Fading screen for {fadeTime} seconds");
        }
        
        yield return new WaitForSeconds(fadeTime);
    }
    
    IEnumerator LoadSceneDirectly()
    {
        AsyncOperation asyncLoad;
        
        if (useSceneIndex)
        {
            asyncLoad = SceneManager.LoadSceneAsync(targetSceneIndex);
        }
        else
        {
            asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        }
        
        // Wait until the scene is loaded
        while (!asyncLoad.isDone)
        {
            if (debugMode)
            {
                Debug.Log($"Loading progress: {asyncLoad.progress * 100f:F1}%");
            }
            yield return null;
        }
        
        if (debugMode)
        {
            Debug.Log($"SceneTransitionTrigger '{name}': Scene loaded successfully");
        }
    }
    
    IEnumerator LoadSceneWithLoadingScreen()
    {
        // Load loading screen first
        yield return SceneManager.LoadSceneAsync(loadingScreenScene);
        
        // Then load the target scene
        AsyncOperation asyncLoad;
        
        if (useSceneIndex)
        {
            asyncLoad = SceneManager.LoadSceneAsync(targetSceneIndex);
        }
        else
        {
            asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        }
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    
    void SaveProgress()
    {
        if (debugMode)
        {
            Debug.Log($"SceneTransitionTrigger '{name}': Saving progress before transition");
        }
          // Save checkpoint data
        if (CheckpointManager.Instance != null)
        {
            // Use reflection to call the private SaveCheckpointData method
            var saveMethod = CheckpointManager.Instance.GetType().GetMethod("SaveCheckpointData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            saveMethod?.Invoke(CheckpointManager.Instance, null);
        }
          // Save player position
        var playerPersistence = FindObjectOfType<PlayerPersistence>();
        if (playerPersistence != null)
        {
            playerPersistence.SavePlayerPosition();
        }
        
        // Save any other game state as needed
        // Add your custom save logic here
    }
    
    // Public methods for external control
    public void TriggerTransition()
    {
        if (!isTransitioning)
        {
            // Find player if not already found
            if (playerObject == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    playerController = playerObject.GetComponent<Invector.vCharacterController.vThirdPersonController>();
                }
            }
            
            StartTransition();
        }
    }
    
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
        useSceneIndex = false;
        ValidateSceneSettings();
    }
    
    public void SetTargetScene(int sceneIndex)
    {
        targetSceneIndex = sceneIndex;
        useSceneIndex = true;
        ValidateSceneSettings();
    }
    
    public void EnableTransition(bool enable)
    {
        enabled = enable;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = enable;
        }
        
        if (debugMode)
        {
            Debug.Log($"SceneTransitionTrigger '{name}': Transition {(enable ? "enabled" : "disabled")}");
        }
    }
    
    // Getters
    public bool IsTransitioning() => isTransitioning;
    public bool IsPlayerInTrigger() => playerInTrigger;
    public string GetTargetSceneName() => useSceneIndex ? $"Index: {targetSceneIndex}" : targetSceneName;
    
    void OnDrawGizmos()
    {
        if (debugMode)
        {
            // Draw trigger area
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = isTransitioning ? Color.red : (playerInTrigger ? Color.yellow : Color.green);
                
                if (col is BoxCollider box)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
                }
                else
                {
                    // Fallback for other collider types
                    Gizmos.DrawWireCube(transform.position, Vector3.one);
                }
            }
            
            // Draw label
            Vector3 labelPos = transform.position + Vector3.up * 2f;
            string label = $"Scene Transition\n→ {GetTargetSceneName()}";
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, label);
            #endif
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Always show gizmos when selected
        OnDrawGizmos();
    }
}
