using UnityEngine;

/// <summary>
/// Simple component to disable depth checking when player enters trigger area.
/// Attach to any GameObject with a trigger collider.
/// </summary>
public class DepthCheckDisableTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string triggerName = "Disable Zone";
    [SerializeField] private bool debugMode = false;
    
    private DepthChecker depthChecker;
    private bool playerInTrigger = false;
    
    void Start()
    {
        // Find the DepthChecker in the scene
        depthChecker = FindObjectOfType<DepthChecker>();
        if (depthChecker == null)
        {
            Debug.LogWarning($"DepthCheckDisableTrigger '{name}': No DepthChecker found in scene!");
        }
        
        // Ensure we have a trigger collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"DepthCheckDisableTrigger '{name}': No collider found! Please add a collider and set it as trigger.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"DepthCheckDisableTrigger '{name}': Collider is not set as trigger!");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerInTrigger)
        {
            playerInTrigger = true;
            
            if (depthChecker != null)
            {
                depthChecker.OnPlayerEnterDisableZone(triggerName);
            }
            
            if (debugMode)
            {
                Debug.Log($"DepthCheckDisableTrigger '{triggerName}': Player entered - depth checking disabled");
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInTrigger)
        {
            playerInTrigger = false;
            
            if (depthChecker != null)
            {
                depthChecker.OnPlayerExitDisableZone(triggerName);
            }
            
            if (debugMode)
            {
                Debug.Log($"DepthCheckDisableTrigger '{triggerName}': Player exited - depth checking may be re-enabled");
            }
        }
    }
    
    // Public methods
    public void SetTriggerName(string newName)
    {
        triggerName = newName;
    }
    
    public bool IsPlayerInTrigger() => playerInTrigger;
    
    public void EnableDebugMode(bool enable)
    {
        debugMode = enable;
    }
}
