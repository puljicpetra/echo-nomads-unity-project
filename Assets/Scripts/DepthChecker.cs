using UnityEngine;
using Invector.vCharacterController;
using System.Collections.Generic;

public class DepthChecker : MonoBehaviour
{
    [Header("Depth Monitoring Settings")]
    [SerializeField] private float deathDepth = -50f;
    [SerializeField] private bool enableDepthChecking = true;
    [SerializeField] private float checkInterval = 0.5f; // Check every 0.5 seconds for performance
    
    [Header("Zone-Based Disable Settings")]
    [SerializeField] private bool useZoneBasedDisabling = true;
    [SerializeField] private List<DepthCheckDisableZone> disableZones = new List<DepthCheckDisableZone>();
    
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
    private bool isInDisableZone = false;
    private int activeDisableZones = 0; // Counter for overlapping zones

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

        // Check if player is in a disable zone
        if (useZoneBasedDisabling && IsPlayerInDisableZone())
        {
            if (debugMode)
            {
                Debug.Log("DepthChecker: Player is in disable zone, skipping depth check");
            }
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

    bool IsPlayerInDisableZone()
    {
        if (playerObject == null) return false;
        
        Vector3 playerPos = playerObject.transform.position;
        
        foreach (var zone in disableZones)
        {
            if (zone != null && zone.IsPointInZone(playerPos))
            {
                return true;
            }
        }
        
        return activeDisableZones > 0; // Also check trigger-based zones
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

    // Zone management methods
    public void RegisterDisableZone(DepthCheckDisableZone zone)
    {
        if (!disableZones.Contains(zone))
        {
            disableZones.Add(zone);
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Registered disable zone '{zone.name}'");
            }
        }
    }

    public void UnregisterDisableZone(DepthCheckDisableZone zone)
    {
        if (disableZones.Remove(zone))
        {
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Unregistered disable zone '{zone.name}'");
            }
        }
    }

    // Called by trigger zones
    public void OnPlayerEnterDisableZone(string zoneName = "")
    {
        activeDisableZones++;
        isInDisableZone = true;
        
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Player entered disable zone {zoneName}. Active zones: {activeDisableZones}");
        }
    }

    public void OnPlayerExitDisableZone(string zoneName = "")
    {
        activeDisableZones = Mathf.Max(0, activeDisableZones - 1);
        isInDisableZone = activeDisableZones > 0;
        
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Player exited disable zone {zoneName}. Active zones: {activeDisableZones}");
        }
    }

    // Manual zone control methods
    public void DisableDepthCheckingInArea(bool disable = true)
    {
        if (disable)
        {
            activeDisableZones++;
            isInDisableZone = true;
        }
        else
        {
            activeDisableZones = Mathf.Max(0, activeDisableZones - 1);
            isInDisableZone = activeDisableZones > 0;
        }
        
        if (debugMode)
        {
            Debug.Log($"DepthChecker: Manual zone control - Depth checking {(isInDisableZone ? "disabled" : "enabled")}");
        }
    }

    // Getters for external systems
    public float GetCurrentPlayerDepth()
    {
        return playerObject != null ? playerObject.transform.position.y : float.MinValue;
    }

    public float GetDeathDepth() => deathDepth;
    public bool IsDepthCheckingEnabled() => enableDepthChecking && !isInDisableZone;
    public bool IsRespawning() => isRespawning;
    public bool IsInDisableZone() => isInDisableZone;
    public int GetActiveDisableZoneCount() => activeDisableZones;

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
                
                // Change color if in disable zone
                if (isInDisableZone)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(playerObject.transform.position, 1.5f);
                }
                
                // Draw line from player to death depth
                Gizmos.color = Color.white;
                Gizmos.DrawLine(
                    playerObject.transform.position,
                    new Vector3(playerObject.transform.position.x, deathDepth, playerObject.transform.position.z)
                );
            }
            
            // Draw disable zones
            if (useZoneBasedDisabling)
            {
                Gizmos.color = Color.cyan;
                foreach (var zone in disableZones)
                {
                    if (zone != null)
                    {
                        zone.DrawGizmos();
                    }
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Always show gizmos when selected, even if debug mode is off
        OnDrawGizmos();
    }

    void OnTriggerEnter(Collider other)
    {
        if (useZoneBasedDisabling && other.CompareTag("DepthCheckDisableZone"))
        {
            // Entering a disable zone
            activeDisableZones++;
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Entered disable zone ({activeDisableZones} active zones)");
            }
            
            // Check if we need to disable depth checking
            if (activeDisableZones > 0)
            {
                EnableDepthChecking(false);
                isInDisableZone = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (useZoneBasedDisabling && other.CompareTag("DepthCheckDisableZone"))
        {
            // Exiting a disable zone
            activeDisableZones--;
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Exited disable zone ({activeDisableZones} active zones)");
            }
            
            // Check if we need to re-enable depth checking
            if (activeDisableZones <= 0)
            {
                EnableDepthChecking(true);
                isInDisableZone = false;
            }
        }
    }

    // Manual control for entering/exiting disable zones (useful for other scripts or events)
    public void EnterDisableZone()
    {
        if (!isInDisableZone)
        {
            activeDisableZones++;
            isInDisableZone = true;
            
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Manually entered disable zone ({activeDisableZones} active zones)");
            }
            
            EnableDepthChecking(false);
        }
    }

    public void ExitDisableZone()
    {
        if (isInDisableZone)
        {
            activeDisableZones--;
            isInDisableZone = false;
            
            if (debugMode)
            {
                Debug.Log($"DepthChecker: Manually exited disable zone ({activeDisableZones} active zones)");
            }
            
            if (activeDisableZones <= 0)
            {
                EnableDepthChecking(true);
            }
        }
    }
}
