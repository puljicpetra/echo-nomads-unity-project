using UnityEngine;
using Invector.vCharacterController;
using System.Collections.Generic;

/// <summary>
/// Zone-based depth checker that manages fall death triggers instead of global depth monitoring.
/// This provides better performance and more flexible placement of death zones.
/// </summary>
public class DepthChecker : MonoBehaviour
{
    [Header("Fall Death Management")]
    [SerializeField] private bool autoFindTriggers = true;
    [SerializeField] private List<FallDeathTrigger> fallDeathTriggers = new List<FallDeathTrigger>();
    
    [Header("Legacy Support (Deprecated)")]
    [SerializeField] private bool enableLegacyDepthChecking = false;
    [SerializeField] private float legacyDeathDepth = -50f;
    [SerializeField] private float checkInterval = 2f; // Reduced frequency since it's deprecated
    
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
    private int activeFallTriggers = 0;

    // Events
    public System.Action OnPlayerFellTooDeep;
    public System.Action OnPlayerRespawned;    void Start()
    {
        FindPlayer();
        
        if (autoFindTriggers)
        {
            FindAllFallDeathTriggers();
        }
        
        RegisterTriggerEvents();
        lastCheckTime = Time.time;
        
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Initialized with {fallDeathTriggers.Count} fall death triggers");
        }
    }

    void Update()
    {
        // Legacy depth checking (deprecated but kept for compatibility)
        if (enableLegacyDepthChecking && !isRespawning)
        {
            if (Time.time - lastCheckTime >= checkInterval)
            {
                CheckLegacyPlayerDepth();
                lastCheckTime = Time.time;
            }
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

    void FindAllFallDeathTriggers()
    {
        FallDeathTrigger[] sceneTriggers = FindObjectsOfType<FallDeathTrigger>();
        
        foreach (var trigger in sceneTriggers)
        {
            if (!fallDeathTriggers.Contains(trigger))
            {
                fallDeathTriggers.Add(trigger);
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Found {sceneTriggers.Length} FallDeathTrigger components in scene");
        }
    }

    void RegisterTriggerEvents()
    {
        foreach (var trigger in fallDeathTriggers)
        {
            if (trigger != null)
            {
                // Subscribe to trigger events
                trigger.OnPlayerFellIntoTrigger += OnPlayerFellIntoTrigger;
                trigger.OnPlayerRespawned += OnPlayerRespawnedFromTrigger;
                
                if (debugMode)
                {
                    Debug.Log($"DepthChecker: Registered events for trigger '{trigger.GetTriggerName()}'");
                }
            }
        }
    }

    void UnregisterTriggerEvents()
    {
        foreach (var trigger in fallDeathTriggers)
        {
            if (trigger != null)
            {
                // Unsubscribe from trigger events
                trigger.OnPlayerFellIntoTrigger -= OnPlayerFellIntoTrigger;
                trigger.OnPlayerRespawned -= OnPlayerRespawnedFromTrigger;
            }
        }
    }

    // Event handlers for fall death triggers
    void OnPlayerFellIntoTrigger(GameObject player)
    {
        activeFallTriggers++;
        isRespawning = true;
        
        // Trigger our event
        OnPlayerFellTooDeep?.Invoke();
        
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Player fell into trigger (active triggers: {activeFallTriggers})");
        }
    }

    void OnPlayerRespawnedFromTrigger(GameObject player)
    {
        activeFallTriggers = Mathf.Max(0, activeFallTriggers - 1);
        
        // Only mark as not respawning if no other triggers are active
        if (activeFallTriggers == 0)
        {
            isRespawning = false;
        }
        
        // Trigger our event
        OnPlayerRespawned?.Invoke();
        
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Player respawned from trigger (active triggers: {activeFallTriggers})");
        }
    }    // Legacy depth checking (deprecated but kept for backwards compatibility)
    void CheckLegacyPlayerDepth()
    {
        if (playerObject == null)
        {
            FindPlayer(); // Try to find player again if lost
            return;
        }

        // Skip depth checking if in a disable zone
        if (IsDepthCheckingDisabled())
        {
            return;
        }

        float currentY = playerObject.transform.position.y;
        
        if (currentY <= legacyDeathDepth)
        {
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Player fell below legacy death depth ({currentY} <= {legacyDeathDepth}). Initiating respawn...");
            }
            
            HandleLegacyPlayerFallDeath();
        }
    }

    void HandleLegacyPlayerFallDeath()
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
                Debug.Log("DepthChecker: Player respawned at checkpoint (legacy method)");
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
    }    // Public methods for external control and backwards compatibility
    public void SetDeathDepth(float newDepth)
    {
        legacyDeathDepth = newDepth;
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Legacy death depth set to {legacyDeathDepth}");
        }
    }

    public void EnableDepthChecking(bool enable)
    {
        enableLegacyDepthChecking = enable;
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Legacy depth checking {(enable ? "enabled" : "disabled")}");
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
        
        HandleLegacyPlayerFallDeath();
    }

    // Fall Death Trigger management methods
    public void RegisterFallDeathTrigger(FallDeathTrigger trigger)
    {
        if (trigger != null && !fallDeathTriggers.Contains(trigger))
        {
            fallDeathTriggers.Add(trigger);
            
            // Register events
            trigger.OnPlayerFellIntoTrigger += OnPlayerFellIntoTrigger;
            trigger.OnPlayerRespawned += OnPlayerRespawnedFromTrigger;
            
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Registered fall death trigger '{trigger.GetTriggerName()}'");
            }
        }
    }

    public void UnregisterFallDeathTrigger(FallDeathTrigger trigger)
    {
        if (trigger != null && fallDeathTriggers.Remove(trigger))
        {
            // Unregister events
            trigger.OnPlayerFellIntoTrigger -= OnPlayerFellIntoTrigger;
            trigger.OnPlayerRespawned -= OnPlayerRespawnedFromTrigger;
            
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Unregistered fall death trigger '{trigger.GetTriggerName()}'");
            }
        }
    }

    // Getters for external systems
    public float GetCurrentPlayerDepth()
    {
        return playerObject != null ? playerObject.transform.position.y : float.MinValue;
    }

    public float GetLegacyDeathDepth() => legacyDeathDepth;
    public bool IsLegacyDepthCheckingEnabled() => enableLegacyDepthChecking;
    public bool IsRespawning() => isRespawning;
    public int GetActiveFallTriggerCount() => activeFallTriggers;
    public int GetRegisteredTriggerCount() => fallDeathTriggers.Count;
    public List<FallDeathTrigger> GetAllFallDeathTriggers() => new List<FallDeathTrigger>(fallDeathTriggers);

    void OnDrawGizmos()
    {
        if (debugMode)
        {
            // Draw legacy death depth line if enabled
            if (enableLegacyDepthChecking)
            {
                Gizmos.color = Color.red;
                Vector3 center = Vector3.zero;
                
                if (playerObject != null)
                {
                    center = playerObject.transform.position;
                }
                
                // Draw a large horizontal plane at death depth
                Vector3 depthPosition = new Vector3(center.x, legacyDeathDepth, center.z);
                Gizmos.DrawWireCube(depthPosition, new Vector3(100f, 0.1f, 100f));
                
                // Draw warning zone (5 units above death depth)
                Gizmos.color = Color.yellow;
                Vector3 warningPosition = new Vector3(center.x, legacyDeathDepth + 5f, center.z);
                Gizmos.DrawWireCube(warningPosition, new Vector3(100f, 0.1f, 100f));
            }
            
            // Draw current player position if available
            if (playerObject != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerObject.transform.position, 1f);
                
                // Change color if respawning
                if (isRespawning)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(playerObject.transform.position, 1.5f);
                }
                
                // Draw line from player to legacy death depth if enabled
                if (enableLegacyDepthChecking)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(
                        playerObject.transform.position,
                        new Vector3(playerObject.transform.position.x, legacyDeathDepth, playerObject.transform.position.z)
                    );
                }
            }
            
            // Draw connections to registered fall death triggers
            Gizmos.color = Color.cyan;
            foreach (var trigger in fallDeathTriggers)
            {
                if (trigger != null && playerObject != null)
                {
                    Gizmos.DrawLine(transform.position, trigger.transform.position);
                }
            }
        }
    }    void OnDrawGizmosSelected()
    {
        // Always show gizmos when selected, even if debug mode is off
        OnDrawGizmos();
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        UnregisterTriggerEvents();
    }
    
    // Legacy disable zone support (for backwards compatibility)
    private List<string> activeDisableZones = new List<string>();
    
    public void OnPlayerEnterDisableZone(string zoneName)
    {
        if (!activeDisableZones.Contains(zoneName))
        {
            activeDisableZones.Add(zoneName);
            
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Player entered disable zone '{zoneName}' - Legacy depth checking disabled");
            }
        }
    }
    
    public void OnPlayerExitDisableZone(string zoneName)
    {
        if (activeDisableZones.Remove(zoneName))
        {
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Player exited disable zone '{zoneName}' - Active disable zones: {activeDisableZones.Count}");
            }
        }
    }
    
    public void RegisterDisableZone(object zone)
    {
        // Legacy support - zones register themselves automatically
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Registered disable zone '{zone.GetType().Name}'");
        }
    }
    
    public void UnregisterDisableZone(object zone)
    {
        // Legacy support - zones unregister themselves automatically
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Unregistered disable zone '{zone.GetType().Name}'");
        }
    }
    
    private bool IsDepthCheckingDisabled()
    {
        return activeDisableZones.Count > 0;
    }
}
