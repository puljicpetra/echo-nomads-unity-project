using UnityEngine;
using Invector.vCharacterController;

public class PlayerPersistence : MonoBehaviour
{
    [Header("Persistence Settings")]
    [SerializeField] private bool enablePersistence = true;
    [SerializeField] private string saveDataPrefix = "EchoNomads_";
    [SerializeField] private bool saveOnCheckpointActivation = true;
    
    [Header("Player Components")]
    [SerializeField] private vThirdPersonController playerController;
    [SerializeField] private Transform playerTransform;
      private void Start()
    {
        // Auto-find player components if not assigned
        if (playerController == null)
        {
            playerController = GetComponent<vThirdPersonController>();
        }
        
        if (playerTransform == null)
        {
            playerTransform = transform;
        }
        
        // Try to subscribe to checkpoint events - may need to wait for CheckpointManager
        StartCoroutine(SubscribeToCheckpointEvents());
        
        // Load saved position if persistence is enabled (with a small delay to ensure CheckpointManager is ready)
        if (enablePersistence)
        {
            StartCoroutine(DelayedLoadPlayerPosition());
        }
    }
    
    private System.Collections.IEnumerator SubscribeToCheckpointEvents()
    {
        // Wait for CheckpointManager to be available
        int attempts = 0;
        while (CheckpointManager.Instance == null && attempts < 100) // Wait up to ~5 seconds
        {
            yield return new WaitForSeconds(0.05f);
            attempts++;
        }
        
        if (saveOnCheckpointActivation && CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnCheckpointActivated += OnCheckpointActivated;
            Debug.Log("PlayerPersistence: Successfully subscribed to checkpoint events");
        }
        else
        {
            Debug.LogWarning("PlayerPersistence: Failed to subscribe to checkpoint events - CheckpointManager not found");
        }
    }
    
    private System.Collections.IEnumerator DelayedLoadPlayerPosition()
    {
        // Wait a frame to ensure all managers are initialized
        yield return new WaitForEndOfFrame();
        LoadPlayerPosition();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnCheckpointActivated -= OnCheckpointActivated;
        }
    }    private void OnCheckpointActivated(Checkpoint checkpoint)
    {
        if (enablePersistence && checkpoint != null)
        {
            Debug.Log($"PlayerPersistence: Checkpoint activated - {checkpoint.CheckpointId}, saving checkpoint position");
            SaveCheckpointPosition(checkpoint);
        }
    }
    
    public void SaveCheckpointPosition(Checkpoint checkpoint)
    {
        if (!enablePersistence || checkpoint == null) return;
        
        try
        {
            // Save checkpoint position (where player should spawn) instead of current player position
            Vector3 checkpointPosition = checkpoint.transform.position;
            // Add slight offset above checkpoint to avoid spawning inside ground
            Vector3 spawnPosition = checkpointPosition + Vector3.up * 0.5f;
            
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_PosX", spawnPosition.x);
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_PosY", spawnPosition.y);
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_PosZ", spawnPosition.z);
            
            // Save checkpoint rotation
            Vector3 rotation = checkpoint.transform.eulerAngles;
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_RotX", rotation.x);
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_RotY", rotation.y);
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_RotZ", rotation.z);
            
            // Save scene name for validation
            PlayerPrefs.SetString($"{saveDataPrefix}Player_Scene", checkpoint.gameObject.scene.name);
            
            // Save timestamp
            PlayerPrefs.SetString($"{saveDataPrefix}Player_SaveTime", System.DateTime.Now.ToBinary().ToString());
            
            PlayerPrefs.Save();
            
            Debug.Log($"PlayerPersistence: Saved checkpoint spawn position - {spawnPosition} (checkpoint: {checkpoint.CheckpointId}) in scene {checkpoint.gameObject.scene.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerPersistence: Failed to save checkpoint position - {e.Message}");
        }
    }
      public void SavePlayerPosition()
    {
        // This method saves the player's current position - use for manual saves or emergency saves
        if (!enablePersistence || playerTransform == null) return;
        
        try
        {
            // Save current player position
            Vector3 position = playerTransform.position;
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_PosX", position.x);
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_PosY", position.y);
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_PosZ", position.z);
            
            // Save rotation
            Vector3 rotation = playerTransform.eulerAngles;
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_RotX", rotation.x);
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_RotY", rotation.y);
            PlayerPrefs.SetFloat($"{saveDataPrefix}Player_RotZ", rotation.z);
            
            // Save scene name for validation
            PlayerPrefs.SetString($"{saveDataPrefix}Player_Scene", gameObject.scene.name);
            
            // Save timestamp
            PlayerPrefs.SetString($"{saveDataPrefix}Player_SaveTime", System.DateTime.Now.ToBinary().ToString());
            
            PlayerPrefs.Save();
            
            Debug.Log($"PlayerPersistence: Saved current player position - {position} in scene {gameObject.scene.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerPersistence: Failed to save player position - {e.Message}");
        }
    }
    
    public void LoadPlayerPosition()
    {
        if (!enablePersistence || playerTransform == null) return;
          try
        {
            // Check if save data exists
            if (!PlayerPrefs.HasKey($"{saveDataPrefix}Player_SaveTime"))
            {
                Debug.Log("PlayerPersistence: No saved player position found - no save time key");
                return;
            }
            
            // Check if we're in the same scene
            string savedScene = PlayerPrefs.GetString($"{saveDataPrefix}Player_Scene", "");
            if (!string.IsNullOrEmpty(savedScene) && savedScene != gameObject.scene.name)
            {
                Debug.Log($"PlayerPersistence: Saved position is for different scene ({savedScene}), not loading");
                return;
            }
            
            // Load position
            Vector3 savedPosition = new Vector3(
                PlayerPrefs.GetFloat($"{saveDataPrefix}Player_PosX", playerTransform.position.x),
                PlayerPrefs.GetFloat($"{saveDataPrefix}Player_PosY", playerTransform.position.y),
                PlayerPrefs.GetFloat($"{saveDataPrefix}Player_PosZ", playerTransform.position.z)
            );
            
            // Load rotation
            Vector3 savedRotation = new Vector3(
                PlayerPrefs.GetFloat($"{saveDataPrefix}Player_RotX", playerTransform.eulerAngles.x),
                PlayerPrefs.GetFloat($"{saveDataPrefix}Player_RotY", playerTransform.eulerAngles.y),
                PlayerPrefs.GetFloat($"{saveDataPrefix}Player_RotZ", playerTransform.eulerAngles.z)
            );
            
            // Apply the loaded position and rotation using the same method as checkpoint respawn
            RestorePlayerPosition(savedPosition, Quaternion.Euler(savedRotation));
            
            string saveTime = PlayerPrefs.GetString($"{saveDataPrefix}Player_SaveTime", "Unknown");
            long timeBinary = System.Convert.ToInt64(saveTime);
            System.DateTime saveDateTime = System.DateTime.FromBinary(timeBinary);
            
            Debug.Log($"PlayerPersistence: Loaded player position - {savedPosition} from {saveDateTime}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerPersistence: Failed to load player position - {e.Message}");
        }
    }
    
    private void RestorePlayerPosition(Vector3 position, Quaternion rotation)
    {
        if (playerController != null)
        {
            // Disable player controller temporarily to prevent issues during teleportation
            playerController.enabled = false;
        }
        
        // Disable rigidbody temporarily
        var playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }
        
        // Apply position and rotation
        playerTransform.position = position;
        playerTransform.rotation = rotation;
        
        // Re-enable components after a small delay
        StartCoroutine(ReenablePlayerComponents(playerController, playerRigidbody));
    }
    
    private System.Collections.IEnumerator ReenablePlayerComponents(
        vThirdPersonController controller, 
        Rigidbody rb)
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
    
    public void ClearSaveData()
    {
        if (!enablePersistence) return;
        
        try
        {
            PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_PosX");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_PosY");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_PosZ");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_RotX");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_RotY");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_RotZ");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_Scene");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_SaveTime");
            PlayerPrefs.Save();
            
            Debug.Log("PlayerPersistence: Cleared player position save data");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerPersistence: Failed to clear save data - {e.Message}");
        }
    }
    
    // Manual save method for external systems
    [ContextMenu("Save Player Position")]
    public void ManualSave()
    {
        SavePlayerPosition();
    }
    
    // Manual load method for external systems
    [ContextMenu("Load Player Position")]
    public void ManualLoad()
    {
        LoadPlayerPosition();
    }
}
