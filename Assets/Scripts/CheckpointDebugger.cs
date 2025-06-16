using UnityEngine;

/// <summary>
/// A simple debugging tool to help understand checkpoint save/load behavior.
/// Add this to any GameObject in your scene and press the debug keys to see what's happening.
/// </summary>
public class CheckpointDebugger : MonoBehaviour
{    [Header("Debug Controls")]
    [SerializeField] private KeyCode debugInfoKey = KeyCode.F1;
    [SerializeField] private KeyCode clearSaveDataKey = KeyCode.F2;
    [SerializeField] private KeyCode forceSaveKey = KeyCode.F3;
    [SerializeField] private KeyCode testCheckpointKey = KeyCode.F4;
    [SerializeField] private KeyCode showNearestKey = KeyCode.F5;
    
    private void Update()
    {
        // Debug checkpoint info
        if (Input.GetKeyDown(debugInfoKey))
        {
            DebugCheckpointInfo();
        }
        
        // Clear save data for testing
        if (Input.GetKeyDown(clearSaveDataKey))
        {
            ClearAllSaveData();
        }
          // Force save current state
        if (Input.GetKeyDown(forceSaveKey))
        {
            ForceSaveCurrentState();
        }
          // Test checkpoint activation
        if (Input.GetKeyDown(testCheckpointKey))
        {
            TestCheckpointActivation();
        }
        
        // Show nearest checkpoint info
        if (Input.GetKeyDown(showNearestKey))
        {
            ShowNearestCheckpointInfo();
        }
    }
    
    void DebugCheckpointInfo()
    {
        Debug.Log("=== CHECKPOINT DEBUGGER ===");
          // CheckpointManager info
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.DebugLogCheckpointInfo();
            Debug.Log($"Nearest-as-Active Mode: {CheckpointManager.Instance.IsUsingNearestAsActive()}");
            Debug.Log($"Update Interval: Every {CheckpointManager.Instance.GetUpdateInterval()} seconds");
        }
        else
        {
            Debug.LogWarning("CheckpointManager.Instance is null!");
        }
        
        // Player position info
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"Player Position: {player.transform.position}");
            Debug.Log($"Player Scene: {player.gameObject.scene.name}");
        }
        
        // PlayerPrefs info
        string prefix = "EchoNomads_";
        Debug.Log("=== SAVED PLAYERPREFS DATA ===");
        Debug.Log($"Active Checkpoint: {PlayerPrefs.GetString($"{prefix}ActiveCheckpoint", "None")}");
        Debug.Log($"Active Checkpoint Scene: {PlayerPrefs.GetString($"{prefix}ActiveCheckpointScene", "None")}");
        Debug.Log($"Last Checkpoint: {PlayerPrefs.GetString($"{prefix}LastActivatedCheckpoint", "None")}");
        Debug.Log($"Last Checkpoint Scene: {PlayerPrefs.GetString($"{prefix}LastActivatedCheckpointScene", "None")}");
        Debug.Log($"Player Scene: {PlayerPrefs.GetString($"{prefix}Player_Scene", "None")}");
        
        if (PlayerPrefs.HasKey($"{prefix}Player_SaveTime"))
        {
            Debug.Log($"Player Position Saved: Yes");
            Vector3 savedPos = new Vector3(
                PlayerPrefs.GetFloat($"{prefix}Player_PosX", 0),
                PlayerPrefs.GetFloat($"{prefix}Player_PosY", 0),
                PlayerPrefs.GetFloat($"{prefix}Player_PosZ", 0)
            );
            Debug.Log($"Saved Player Position: {savedPos}");
        }
        else
        {
            Debug.Log($"Player Position Saved: No");
        }
    }
    
    void ClearAllSaveData()
    {
        Debug.Log("=== CLEARING ALL SAVE DATA ===");
        
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.ClearSaveData();
        }
        
        var playerPersistence = FindObjectOfType<PlayerPersistence>();
        if (playerPersistence != null)
        {
            playerPersistence.ClearSaveData();
        }
        
        Debug.Log("All save data cleared! Next game load will start fresh.");
    }
    
    void ForceSaveCurrentState()
    {
        Debug.Log("=== FORCE SAVING CURRENT STATE ===");
          // Force save current checkpoint
        if (CheckpointManager.Instance != null && CheckpointManager.Instance.GetCurrentCheckpoint() != null)
        {
            CheckpointManager.Instance.SaveCheckpoint(CheckpointManager.Instance.GetCurrentCheckpoint());
            Debug.Log($"Saved current checkpoint: {CheckpointManager.Instance.GetCurrentCheckpoint().CheckpointId}");
        }
        
        // Also demonstrate saving current player position (alternative method)
        var playerPersistence = FindObjectOfType<PlayerPersistence>();
        if (playerPersistence != null)
        {
            playerPersistence.SavePlayerPosition();
            Debug.Log("Also saved current player position (manual save method)");
        }
          Debug.Log("Current state saved!");
    }
    
    void TestCheckpointActivation()
    {
        Debug.Log("=== TESTING CHECKPOINT ACTIVATION ===");
        
        // Find nearest checkpoint to player and manually activate it
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && CheckpointManager.Instance != null)
        {
            var nearestCheckpoint = CheckpointManager.Instance.FindNearestCheckpoint(player.transform.position);
            if (nearestCheckpoint != null)
            {
                Debug.Log($"Manually activating nearest checkpoint: {nearestCheckpoint.CheckpointId}");
                nearestCheckpoint.ActivateCheckpoint();
            }
            else
            {
                Debug.LogWarning("No checkpoints found to activate");
            }
        }        else
        {
            Debug.LogWarning("Player or CheckpointManager not found");
        }
    }
    
    void ShowNearestCheckpointInfo()
    {
        Debug.Log("=== NEAREST CHECKPOINT INFO ===");
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && CheckpointManager.Instance != null)
        {
            var nearestCheckpoint = CheckpointManager.Instance.FindNearestCheckpoint(player.transform.position);
            var currentCheckpoint = CheckpointManager.Instance.GetCurrentCheckpoint();
            
            Debug.Log($"Player Position: {player.transform.position}");
            Debug.Log($"Current Active Checkpoint: {(currentCheckpoint != null ? currentCheckpoint.CheckpointId : "None")}");
            Debug.Log($"Nearest Checkpoint: {(nearestCheckpoint != null ? nearestCheckpoint.CheckpointId : "None")}");
            
            if (nearestCheckpoint != null)
            {
                float distance = Vector3.Distance(player.transform.position, nearestCheckpoint.transform.position);
                Debug.Log($"Distance to Nearest: {distance:F2} units");
                Debug.Log($"Nearest Checkpoint Position: {nearestCheckpoint.transform.position}");
            }
            
            // List all checkpoints with distances
            var allCheckpoints = CheckpointManager.Instance.GetAllCheckpoints();
            Debug.Log($"All Checkpoints ({allCheckpoints.Count}):");
            foreach (var checkpoint in allCheckpoints)
            {
                if (checkpoint != null)
                {
                    float dist = Vector3.Distance(player.transform.position, checkpoint.transform.position);
                    string status = checkpoint.IsActivated ? "[ACTIVE]" : "[INACTIVE]";
                    Debug.Log($"  - {checkpoint.CheckpointId} {status}: {dist:F2} units away");
                }
            }
        }
        else
        {
            Debug.LogWarning("Player or CheckpointManager not found");
        }
    }
    
    void OnGUI()
    {
        // Simple on-screen instructions
        GUI.color = Color.white;
        GUI.backgroundColor = Color.black;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 120));
        GUILayout.Label("CHECKPOINT DEBUGGER", GUI.skin.box);        GUILayout.Label($"{debugInfoKey}: Show Debug Info");
        GUILayout.Label($"{clearSaveDataKey}: Clear Save Data");
        GUILayout.Label($"{forceSaveKey}: Force Save Current State");
        GUILayout.Label($"{testCheckpointKey}: Test Checkpoint Activation");
        GUILayout.Label($"{showNearestKey}: Show Nearest Checkpoint Info");
        GUILayout.EndArea();
    }
}
