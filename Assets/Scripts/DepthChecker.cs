using UnityEngine;
using Invector.vCharacterController;

public class DepthChecker : MonoBehaviour
{
    [Header("Depth Monitoring Settings")]
    [SerializeField] private float deathDepth = -50f;
    [SerializeField] private bool enableDepthChecking = true;
    [SerializeField] private float checkInterval = 0.5f; // Check every 0.5 seconds for performance
    
    [Header("Audio Settings")]
    [SerializeField] private string fallDeathSoundName = "PlayerFallDeath";
    [SerializeField] private string respawnSoundName = "PlayerRespawn";
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    // Private fields
    private GameObject playerObject;
    private vThirdPersonController playerController;
    private float lastCheckTime;
    private bool isRespawning = false;

    // Events
    public System.Action OnPlayerFellTooDeep;
    public System.Action OnPlayerRespawned;

    void Start()
    {
        FindPlayer();
        lastCheckTime = Time.time;
    }

    void Update()
    {
        if (!enableDepthChecking || isRespawning) return;

        // Check depth at intervals for better performance
        if (Time.time - lastCheckTime >= checkInterval)
        {
            CheckPlayerDepth();
            lastCheckTime = Time.time;
        }
    }

    void FindPlayer()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<vThirdPersonController>();
            
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Found player at {playerObject.transform.position}");
            }
        }
        else
        {
            Debug.LogWarning("DepthChecker: Player object not found! Make sure player has 'Player' tag.");
        }
    }

    void CheckPlayerDepth()
    {
        if (playerObject == null)
        {
            FindPlayer(); // Try to find player again if lost
            return;
        }

        float currentY = playerObject.transform.position.y;
        
        if (currentY <= deathDepth)
        {
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Player fell below death depth ({currentY} <= {deathDepth}). Initiating respawn...");
            }
            
            HandlePlayerFallDeath();
        }
    }

    void HandlePlayerFallDeath()
    {
        if (isRespawning) return; // Prevent multiple respawn calls
        
        isRespawning = true;
        
        // Trigger event
        OnPlayerFellTooDeep?.Invoke();
        
        // Play fall death sound
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(fallDeathSoundName))
        {
            AudioManager.Instance.Play(fallDeathSoundName);
        }
        
        // Respawn at checkpoint
        RespawnPlayer();
    }

    void RespawnPlayer()
    {
        // Use CheckpointManager to respawn at current checkpoint
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnAtCurrentCheckpoint();
            
            // Play respawn sound with a small delay
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(respawnSoundName))
            {
                Invoke(nameof(PlayRespawnSound), 0.5f);
            }
            
            // Trigger respawn event
            OnPlayerRespawned?.Invoke();
            
            if (debugMode)
            {
                Debug.Log("DepthChecker: Player respawned at checkpoint");
            }
        }
        else
        {
            Debug.LogError("DepthChecker: CheckpointManager not found! Cannot respawn player.");
        }
        
        // Re-enable depth checking after a short delay
        Invoke(nameof(ReenableDepthChecking), 2f);
    }

    void PlayRespawnSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(respawnSoundName))
        {
            AudioManager.Instance.Play(respawnSoundName);
        }
    }

    void ReenableDepthChecking()
    {
        isRespawning = false;
    }

    // Public methods for external control
    public void SetDeathDepth(float newDepth)
    {
        deathDepth = newDepth;
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Death depth set to {deathDepth}");
        }
    }

    public void EnableDepthChecking(bool enable)
    {
        enableDepthChecking = enable;
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Depth checking {(enable ? "enabled" : "disabled")}");
        }
    }

    public void SetCheckInterval(float interval)
    {
        checkInterval = Mathf.Max(0.1f, interval); // Minimum 0.1 seconds
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Check interval set to {checkInterval} seconds");
        }
    }

    // Manual respawn trigger (useful for testing or external systems)
    public void ForceRespawn()
    {
        if (debugMode)
        {
            Debug.Log("DepthChecker: Force respawn triggered");
        }
        
        HandlePlayerFallDeath();
    }

    // Getters for external systems
    public float GetCurrentPlayerDepth()
    {
        return playerObject != null ? playerObject.transform.position.y : float.MinValue;
    }

    public float GetDeathDepth() => deathDepth;
    public bool IsDepthCheckingEnabled() => enableDepthChecking;
    public bool IsRespawning() => isRespawning;

    void OnDrawGizmos()
    {
        if (debugMode)
        {
            // Draw death depth line across the scene
            Gizmos.color = Color.red;
            Vector3 center = Vector3.zero;
            
            if (playerObject != null)
            {
                center = playerObject.transform.position;
            }
            
            // Draw a large horizontal plane at death depth
            Vector3 depthPosition = new Vector3(center.x, deathDepth, center.z);
            Gizmos.DrawWireCube(depthPosition, new Vector3(100f, 0.1f, 100f));
            
            // Draw warning zone (5 units above death depth)
            Gizmos.color = Color.yellow;
            Vector3 warningPosition = new Vector3(center.x, deathDepth + 5f, center.z);
            Gizmos.DrawWireCube(warningPosition, new Vector3(100f, 0.1f, 100f));
            
            // Draw current player position if available
            if (playerObject != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerObject.transform.position, 1f);
                
                // Draw line from player to death depth
                Gizmos.color = Color.white;
                Gizmos.DrawLine(
                    playerObject.transform.position,
                    new Vector3(playerObject.transform.position.x, deathDepth, playerObject.transform.position.z)
                );
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Always show gizmos when selected, even if debug mode is off
        OnDrawGizmos();
    }
}
