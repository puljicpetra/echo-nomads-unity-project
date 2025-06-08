using UnityEngine;
using Invector.vCharacterController;

/// <summary>
/// Trigger zone that detects when the player falls into it and teleports them to the nearest checkpoint.
/// This replaces the global depth checking system with a more flexible zone-based approach.
/// </summary>
public class FallDeathTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string triggerName = "Fall Death Zone";
    [SerializeField] private bool teleportToNearestCheckpoint = true;
    [SerializeField] private Transform specificRespawnPoint; // Optional: specific respawn instead of nearest checkpoint
    
    [Header("Audio Settings")]
    [SerializeField] private string fallDeathSoundName = "PlayerFallDeath";
    [SerializeField] private string respawnSoundName = "PlayerRespawn";
    
    [Header("Visual Settings")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = Color.red;
    [SerializeField] private bool showOnlyWhenSelected = false;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    // Private fields
    private bool isProcessingFall = false;
    private BoxCollider triggerCollider;

    // Events
    public System.Action<GameObject> OnPlayerFellIntoTrigger;
    public System.Action<GameObject> OnPlayerRespawned;

    void Start()
    {
        SetupTriggerCollider();
        
        if (debugMode)
        {
            Debug.Log($"FallDeathTrigger '{triggerName}' initialized at {transform.position}");
        }
    }

    void SetupTriggerCollider()
    {
        // Get or create collider
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
            triggerCollider.size = new Vector3(10f, 5f, 10f); // Default size
            
            if (debugMode)
            {
                Debug.Log($"FallDeathTrigger '{triggerName}': Added BoxCollider component");
            }
        }
        
        // Ensure it's a trigger
        triggerCollider.isTrigger = true;
        
        // Make sure it's on a layer that can interact with the player
        if (gameObject.layer == 0) // Default layer
        {
            if (debugMode)
            {
                Debug.Log($"FallDeathTrigger '{triggerName}': Using default layer - consider using a dedicated layer");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if it's the player and we're not already processing a fall
        if (other.CompareTag("Player") && !isProcessingFall)
        {
            if (debugMode)
            {
                Debug.Log($"FallDeathTrigger '{triggerName}': Player entered trigger zone");
            }
            
            HandlePlayerFall(other.gameObject);
        }
    }

    void HandlePlayerFall(GameObject player)
    {
        if (isProcessingFall) return; // Prevent multiple triggers
        
        isProcessingFall = true;
        
        // Trigger event
        OnPlayerFellIntoTrigger?.Invoke(player);
        
        // Play fall death sound
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(fallDeathSoundName))
        {
            AudioManager.Instance.Play(fallDeathSoundName);
            
            if (debugMode)
            {
                Debug.Log($"FallDeathTrigger '{triggerName}': Playing fall death sound '{fallDeathSoundName}'");
            }
        }
        
        // Respawn player after a short delay to allow sound to play
        Invoke(nameof(RespawnPlayer), 0.1f);
    }

    void RespawnPlayer()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError($"FallDeathTrigger '{triggerName}': Player object not found for respawn!");
            isProcessingFall = false;
            return;
        }

        if (teleportToNearestCheckpoint)
        {
            RespawnAtNearestCheckpoint(playerObj);
        }
        else if (specificRespawnPoint != null)
        {
            RespawnAtSpecificPoint(playerObj);
        }
        else
        {
            Debug.LogError($"FallDeathTrigger '{triggerName}': No respawn method configured!");
            isProcessingFall = false;
            return;
        }
        
        // Play respawn sound with delay
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(respawnSoundName))
        {
            Invoke(nameof(PlayRespawnSound), 0.5f);
        }
        
        // Trigger respawn event
        OnPlayerRespawned?.Invoke(playerObj);
        
        // Re-enable processing after respawn completes
        Invoke(nameof(ReenableFallProcessing), 2f);
        
        if (debugMode)
        {
            Debug.Log($"FallDeathTrigger '{triggerName}': Player respawned");
        }
    }

    void RespawnAtNearestCheckpoint(GameObject player)
    {
        if (CheckpointManager.Instance != null)
        {
            // Find and teleport to nearest checkpoint
            var nearestCheckpoint = CheckpointManager.Instance.FindNearestCheckpoint(player.transform.position);
            if (nearestCheckpoint != null)
            {
                // Set as active checkpoint and respawn there
                CheckpointManager.Instance.SaveCheckpoint(nearestCheckpoint);
                nearestCheckpoint.RespawnPlayerHere();
                
                if (debugMode)
                {
                    Debug.Log($"FallDeathTrigger '{triggerName}': Respawned at nearest checkpoint '{nearestCheckpoint.CheckpointId}'");
                }
            }
            else
            {
                Debug.LogError($"FallDeathTrigger '{triggerName}': No checkpoints found!");
                // Fallback: respawn at current position + offset
                RespawnAtFallbackPosition(player);
            }
        }
        else
        {
            Debug.LogError($"FallDeathTrigger '{triggerName}': CheckpointManager not found!");
            // Fallback: respawn at current position + offset
            RespawnAtFallbackPosition(player);
        }
    }

    void RespawnAtSpecificPoint(GameObject player)
    {
        if (specificRespawnPoint == null)
        {
            Debug.LogError($"FallDeathTrigger '{triggerName}': Specific respawn point is null!");
            RespawnAtFallbackPosition(player);
            return;
        }
        
        TeleportPlayer(player, specificRespawnPoint.position, specificRespawnPoint.rotation);
        
        if (debugMode)
        {
            Debug.Log($"FallDeathTrigger '{triggerName}': Respawned at specific point '{specificRespawnPoint.name}'");
        }
    }

    void RespawnAtFallbackPosition(GameObject player)
    {
        // Emergency fallback: respawn above the trigger
        Vector3 fallbackPosition = transform.position + Vector3.up * 10f;
        TeleportPlayer(player, fallbackPosition, player.transform.rotation);
        
        Debug.LogWarning($"FallDeathTrigger '{triggerName}': Using fallback respawn position");
    }

    void TeleportPlayer(GameObject player, Vector3 position, Quaternion rotation)
    {
        // Safely teleport the player using the same method as checkpoints
        var playerController = player.GetComponent<vThirdPersonController>();
        var playerRigidbody = player.GetComponent<Rigidbody>();
        
        // Disable components temporarily
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }
        
        // Teleport
        player.transform.position = position;
        player.transform.rotation = rotation;
        
        // Re-enable components after physics update
        StartCoroutine(ReenablePlayerComponents(playerController, playerRigidbody));
    }

    System.Collections.IEnumerator ReenablePlayerComponents(vThirdPersonController controller, Rigidbody rb)
    {
        yield return new WaitForFixedUpdate(); // Wait one physics frame
        
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero; // Clear any velocity
            rb.angularVelocity = Vector3.zero;
        }
        
        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    void PlayRespawnSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(respawnSoundName))
        {
            AudioManager.Instance.Play(respawnSoundName);
        }
    }

    void ReenableFallProcessing()
    {
        isProcessingFall = false;
        
        if (debugMode)
        {
            Debug.Log($"FallDeathTrigger '{triggerName}': Re-enabled fall processing");
        }
    }

    // Public methods for external control
    public void SetTriggerName(string newName)
    {
        triggerName = newName;
    }

    public void SetTeleportToNearest(bool useNearest)
    {
        teleportToNearestCheckpoint = useNearest;
    }

    public void SetSpecificRespawnPoint(Transform respawnPoint)
    {
        specificRespawnPoint = respawnPoint;
        teleportToNearestCheckpoint = false; // Switch to specific point mode
    }

    public void SetTriggerSize(Vector3 size)
    {
        if (triggerCollider != null)
        {
            triggerCollider.size = size;
        }
    }

    public void EnableTrigger(bool enabled)
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = enabled;
        }
    }

    // Manual trigger for testing
    [ContextMenu("Test Fall Trigger")]
    public void TestFallTrigger()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            HandlePlayerFall(player);
        }
        else
        {
            Debug.LogWarning($"FallDeathTrigger '{triggerName}': No player found for testing");
        }
    }

    // Getters
    public bool IsProcessingFall() => isProcessingFall;
    public string GetTriggerName() => triggerName;
    public bool IsTeleportToNearestEnabled() => teleportToNearestCheckpoint;
    public Transform GetSpecificRespawnPoint() => specificRespawnPoint;

    // Gizmo drawing
    void OnDrawGizmos()
    {
        if (!showGizmo || showOnlyWhenSelected) return;
        DrawTriggerGizmo();
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;
        DrawTriggerGizmo();
    }

    void DrawTriggerGizmo()
    {
        Gizmos.color = gizmoColor;
        
        // Draw trigger area
        if (triggerCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(triggerCollider.center, triggerCollider.size);
            
            // Draw filled version with transparency
            Color fillColor = gizmoColor;
            fillColor.a = 0.1f;
            Gizmos.color = fillColor;
            Gizmos.DrawCube(triggerCollider.center, triggerCollider.size);
        }
        else
        {
            // Fallback if no collider
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 5f);
        }
        
        // Draw label
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.white;
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, triggerName);
        #endif
        
        // Draw connection to specific respawn point
        if (!teleportToNearestCheckpoint && specificRespawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, specificRespawnPoint.position);
            Gizmos.DrawWireSphere(specificRespawnPoint.position, 1f);
        }
    }
}
