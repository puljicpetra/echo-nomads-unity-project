using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CheckpointManager : MonoBehaviour
{
    // Singleton instance
    public static CheckpointManager Instance { get; private set; }    [Header("Checkpoint Settings")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool useNearestAsActive = true;
    [SerializeField] private float updateInterval = 1f;
    
    [Header("Persistence Settings")]
    [SerializeField] private bool enablePersistence = true;
    [SerializeField] private string saveDataPrefix = "EchoNomads_";
    
    // Registered checkpoints
    private List<Checkpoint> allCheckpoints = new List<Checkpoint>();
    private Checkpoint currentActiveCheckpoint;
    private Checkpoint lastActivatedCheckpoint;

    // Events for other systems to listen to
    public System.Action<Checkpoint> OnCheckpointActivated;
    public System.Action<Checkpoint> OnActiveCheckpointChanged;
    public System.Action<Checkpoint> OnPlayerRespawned;

    private float lastUpdateTime;

    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }    void Start()
    {
        // Load saved data if persistence is enabled
        if (enablePersistence)
        {
            LoadCheckpointData();
        }
        
        // Find and register any existing checkpoints in the scene
        FindExistingCheckpoints();
        
        // Set up the starting checkpoint if available
        SetupStartingCheckpoint();
        
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        if (useNearestAsActive && Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateNearestActiveCheckpoint();
            lastUpdateTime = Time.time;
        }
    }    void FindExistingCheckpoints()
    {
        Checkpoint[] sceneCheckpoints = FindObjectsOfType<Checkpoint>();
        foreach (var checkpoint in sceneCheckpoints)
        {
            RegisterCheckpoint(checkpoint);
        }
        
        // Finalize loading after all checkpoints are registered
        FinalizeCheckpointLoading();
        
        if (debugMode)
        {
            Debug.Log($"CheckpointManager: Found {sceneCheckpoints.Length} checkpoints in scene");
        }
    }

    void SetupStartingCheckpoint()
    {
        // Find the starting checkpoint and activate it
        Checkpoint startingCheckpoint = allCheckpoints.Find(cp => cp.IsStartingCheckpoint);
        if (startingCheckpoint != null)
        {
            SetActiveCheckpoint(startingCheckpoint);
            if (debugMode)
            {
                Debug.Log($"CheckpointManager: Set starting checkpoint '{startingCheckpoint.CheckpointId}'");
            }
        }
        else if (allCheckpoints.Count > 0)
        {
            // If no starting checkpoint is designated, use the first one
            SetActiveCheckpoint(allCheckpoints[0]);
            if (debugMode)
            {
                Debug.Log($"CheckpointManager: No starting checkpoint found, using first checkpoint '{allCheckpoints[0].CheckpointId}'");
            }
        }
    }    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint != null && !allCheckpoints.Contains(checkpoint))
        {
            allCheckpoints.Add(checkpoint);
            
            // Check if this checkpoint was previously discovered (from save data)
            if (enablePersistence && IsCheckpointDiscovered(checkpoint.CheckpointId))
            {
                checkpoint.MarkAsDiscovered();
            }
            // Mark all checkpoints as "discovered" in nearest mode if not loading from save
            else if (useNearestAsActive)
            {
                checkpoint.MarkAsDiscovered();
            }
            
            if (debugMode)
            {
                Debug.Log($"CheckpointManager: Registered checkpoint '{checkpoint.CheckpointId}'");
            }
        }
    }

    public void UnregisterCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint != null && allCheckpoints.Contains(checkpoint))
        {
            allCheckpoints.Remove(checkpoint);
            
            // If this was the active checkpoint, find a new one
            if (currentActiveCheckpoint == checkpoint)
            {
                FindNewActiveCheckpoint();
            }
            
            if (debugMode)
            {
                Debug.Log($"CheckpointManager: Unregistered checkpoint '{checkpoint.CheckpointId}'");
            }
        }
    }    public void SaveCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint != null)
        {
            // Mark checkpoint as reached (handles first-time effects and state)
            checkpoint.MarkAsReached();
            
            SetActiveCheckpoint(checkpoint);
            lastActivatedCheckpoint = checkpoint;
            
            // Save checkpoint data if persistence is enabled
            if (enablePersistence)
            {
                SaveCheckpointData();
            }
            
            // Trigger event
            OnCheckpointActivated?.Invoke(checkpoint);
            
            if (debugMode)
            {
                Debug.Log($"CheckpointManager: Saved checkpoint '{checkpoint.CheckpointId}' (auto-save from nearest system)");
            }
        }
    }

    void SetActiveCheckpoint(Checkpoint checkpoint)
    {
        // Deactivate current checkpoint visual state
        if (currentActiveCheckpoint != null && currentActiveCheckpoint != checkpoint)
        {
            currentActiveCheckpoint.DeactivateAsCurrentCheckpoint();
        }
        
        // Set new active checkpoint
        currentActiveCheckpoint = checkpoint;
        currentActiveCheckpoint.SetAsCurrentCheckpoint();

        // Trigger active checkpoint changed event
        OnActiveCheckpointChanged?.Invoke(checkpoint);
    }

    public void RespawnAtCurrentCheckpoint()
    {
        if (currentActiveCheckpoint != null)
        {
            currentActiveCheckpoint.RespawnPlayerHere();
            OnPlayerRespawned?.Invoke(currentActiveCheckpoint);
            
            if (debugMode)
            {
                Debug.Log($"CheckpointManager: Respawned player at checkpoint '{currentActiveCheckpoint.CheckpointId}'");
            }
        }
        else
        {
            Debug.LogWarning("CheckpointManager: No active checkpoint available for respawn!");
        }
    }

    public void TeleportToNearestCheckpoint()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("CheckpointManager: Player not found!");
            return;
        }

        Checkpoint nearestCheckpoint = FindNearestCheckpoint(playerObj.transform.position);
        if (nearestCheckpoint != null)
        {
            // Set as current checkpoint and respawn there
            SetActiveCheckpoint(nearestCheckpoint);
            nearestCheckpoint.RespawnPlayerHere();
            
            if (debugMode)
            {
                Debug.Log($"CheckpointManager: Teleported to nearest checkpoint '{nearestCheckpoint.CheckpointId}'");
            }
        }
        else
        {
            Debug.LogWarning("CheckpointManager: No checkpoints available for teleportation!");
        }
    }

    public Checkpoint FindNearestCheckpoint(Vector3 position)
    {
        if (allCheckpoints.Count == 0) return null;

        Checkpoint nearest = null;
        float shortestDistance = float.MaxValue;

        foreach (var checkpoint in allCheckpoints)
        {
            if (checkpoint != null)
            {
                float distance = Vector3.Distance(position, checkpoint.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearest = checkpoint;
                }
            }
        }

        return nearest;
    }

    void FindNewActiveCheckpoint()
    {
        if (lastActivatedCheckpoint != null && allCheckpoints.Contains(lastActivatedCheckpoint))
        {
            SetActiveCheckpoint(lastActivatedCheckpoint);
        }
        else if (allCheckpoints.Count > 0)
        {
            // Find a starting checkpoint or use the first available
            Checkpoint startingCheckpoint = allCheckpoints.Find(cp => cp.IsStartingCheckpoint);
            SetActiveCheckpoint(startingCheckpoint ?? allCheckpoints[0]);
        }
        else
        {
            currentActiveCheckpoint = null;
        }
    }    void UpdateNearestActiveCheckpoint()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null || allCheckpoints.Count == 0) return;

        Checkpoint nearestCheckpoint = FindNearestCheckpoint(playerObj.transform.position);
        
        if (nearestCheckpoint != null && nearestCheckpoint != currentActiveCheckpoint)
        {
            // This is a checkpoint progression - save it automatically
            SaveCheckpoint(nearestCheckpoint);
            
            if (debugMode)
            {
                Debug.Log($"CheckpointManager: Active checkpoint changed to '{nearestCheckpoint.CheckpointId}' (nearest to player) - Auto-saved!");
            }
        }
    }    // Public getters for external systems
    public Checkpoint GetCurrentCheckpoint() => currentActiveCheckpoint;
    public Checkpoint GetLastActivatedCheckpoint() => lastActivatedCheckpoint;
    public List<Checkpoint> GetAllCheckpoints() => new List<Checkpoint>(allCheckpoints);
    public int GetCheckpointCount() => allCheckpoints.Count;
    public bool IsUsingNearestAsActive() => useNearestAsActive;
    public float GetUpdateInterval() => updateInterval;
    public bool IsDebugMode() => debugMode;// Method to toggle between manual and automatic modes
    public void SetNearestAsActiveMode(bool enabled)
    {
        useNearestAsActive = enabled;
        
        if (enabled)
        {
            // Mark all checkpoints as discovered
            foreach (var checkpoint in allCheckpoints)
            {
                checkpoint.MarkAsDiscovered();
            }
            
            // Update to nearest immediately
            UpdateNearestActiveCheckpoint();
        }
        
        if (debugMode)
        {
            Debug.Log($"CheckpointManager: Nearest-as-active mode {(enabled ? "enabled" : "disabled")}");
        }
    }

    // === PERSISTENCE SYSTEM ===
      void SaveCheckpointData()
    {
        if (!enablePersistence) return;

        try
        {
            // Save current active checkpoint with scene info
            if (currentActiveCheckpoint != null)
            {
                PlayerPrefs.SetString($"{saveDataPrefix}ActiveCheckpoint", currentActiveCheckpoint.CheckpointId);
                PlayerPrefs.SetString($"{saveDataPrefix}ActiveCheckpointScene", currentActiveCheckpoint.gameObject.scene.name);
            }

            // Save last activated checkpoint with scene info
            if (lastActivatedCheckpoint != null)
            {
                PlayerPrefs.SetString($"{saveDataPrefix}LastActivatedCheckpoint", lastActivatedCheckpoint.CheckpointId);
                PlayerPrefs.SetString($"{saveDataPrefix}LastActivatedCheckpointScene", lastActivatedCheckpoint.gameObject.scene.name);
            }

            // Save discovered checkpoints
            var discoveredCheckpoints = allCheckpoints
                .Where(cp => cp != null && cp.IsDiscovered)
                .Select(cp => cp.CheckpointId)
                .ToArray();
            
            string discoveredIds = string.Join(",", discoveredCheckpoints);
            PlayerPrefs.SetString($"{saveDataPrefix}DiscoveredCheckpoints", discoveredIds);

            // Save checkpoint states (activated status)
            var activatedCheckpoints = allCheckpoints
                .Where(cp => cp != null && cp.IsActivated)
                .Select(cp => cp.CheckpointId)
                .ToArray();
            
            string activatedIds = string.Join(",", activatedCheckpoints);
            PlayerPrefs.SetString($"{saveDataPrefix}ActivatedCheckpoints", activatedIds);

            // Save persistence settings
            PlayerPrefs.SetInt($"{saveDataPrefix}UseNearestAsActive", useNearestAsActive ? 1 : 0);
            
            // Save timestamp
            PlayerPrefs.SetString($"{saveDataPrefix}SaveTime", System.DateTime.Now.ToBinary().ToString());

            PlayerPrefs.Save();

            if (debugMode)
            {
                Debug.Log($"CheckpointManager: Saved checkpoint data - Active: {currentActiveCheckpoint?.CheckpointId}, Discovered: {discoveredCheckpoints.Length}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CheckpointManager: Failed to save checkpoint data - {e.Message}");
        }
    }    void LoadCheckpointData()
    {
        if (!enablePersistence) return;

        try
        {
            // Check if save data exists
            if (!PlayerPrefs.HasKey($"{saveDataPrefix}SaveTime"))
            {
                if (debugMode)
                {
                    Debug.Log("CheckpointManager: No save data found, starting fresh");
                }
                return;
            }

            // Load persistence settings
            if (PlayerPrefs.HasKey($"{saveDataPrefix}UseNearestAsActive"))
            {
                useNearestAsActive = PlayerPrefs.GetInt($"{saveDataPrefix}UseNearestAsActive") == 1;
            }

            // Get saved checkpoint IDs and scenes
            string savedActiveId = PlayerPrefs.GetString($"{saveDataPrefix}ActiveCheckpoint", "");
            string savedActiveScene = PlayerPrefs.GetString($"{saveDataPrefix}ActiveCheckpointScene", "");
            string savedLastActivatedId = PlayerPrefs.GetString($"{saveDataPrefix}LastActivatedCheckpoint", "");
            string savedLastActivatedScene = PlayerPrefs.GetString($"{saveDataPrefix}LastActivatedCheckpointScene", "");
            
            // Check if we're in the same scene as the saved checkpoint
            string currentScene = gameObject.scene.name;
            bool inSameSceneAsActive = string.IsNullOrEmpty(savedActiveScene) || savedActiveScene == currentScene;
            bool inSameSceneAsLast = string.IsNullOrEmpty(savedLastActivatedScene) || savedLastActivatedScene == currentScene;            
            // Store for later when checkpoints are registered, but only if we're in the right scene
            if (!string.IsNullOrEmpty(savedActiveId) && inSameSceneAsActive)
            {
                PlayerPrefs.SetString($"{saveDataPrefix}PendingActiveCheckpoint", savedActiveId);
            }
            if (!string.IsNullOrEmpty(savedLastActivatedId) && inSameSceneAsLast)
            {
                PlayerPrefs.SetString($"{saveDataPrefix}PendingLastActivatedCheckpoint", savedLastActivatedId);
            }

            if (debugMode)
            {
                string saveTime = PlayerPrefs.GetString($"{saveDataPrefix}SaveTime", "Unknown");
                long timeBinary = System.Convert.ToInt64(saveTime);
                System.DateTime saveDateTime = System.DateTime.FromBinary(timeBinary);
                Debug.Log($"CheckpointManager: Loading checkpoint data from {saveDateTime}");
                Debug.Log($"CheckpointManager: Current scene: {currentScene}, Saved active scene: {savedActiveScene}, In same scene: {inSameSceneAsActive}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CheckpointManager: Failed to load checkpoint data - {e.Message}");
        }
    }

    bool IsCheckpointDiscovered(string checkpointId)
    {
        if (!enablePersistence) return false;
        
        string discoveredIds = PlayerPrefs.GetString($"{saveDataPrefix}DiscoveredCheckpoints", "");
        return !string.IsNullOrEmpty(discoveredIds) && discoveredIds.Contains(checkpointId);
    }

    bool IsCheckpointActivated(string checkpointId)
    {
        if (!enablePersistence) return false;
        
        string activatedIds = PlayerPrefs.GetString($"{saveDataPrefix}ActivatedCheckpoints", "");
        return !string.IsNullOrEmpty(activatedIds) && activatedIds.Contains(checkpointId);
    }

    void RestoreSavedCheckpointStates()
    {
        if (!enablePersistence) return;

        try
        {
            // Restore discovered states
            string discoveredIds = PlayerPrefs.GetString($"{saveDataPrefix}DiscoveredCheckpoints", "");
            if (!string.IsNullOrEmpty(discoveredIds))
            {
                string[] discovered = discoveredIds.Split(',');
                foreach (string id in discovered)
                {
                    var checkpoint = allCheckpoints.Find(cp => cp.CheckpointId == id);
                    if (checkpoint != null)
                    {
                        checkpoint.MarkAsDiscovered();
                    }
                }
            }

            // Restore activated states
            string activatedIds = PlayerPrefs.GetString($"{saveDataPrefix}ActivatedCheckpoints", "");
            if (!string.IsNullOrEmpty(activatedIds))
            {
                string[] activated = activatedIds.Split(',');
                foreach (string id in activated)
                {
                    var checkpoint = allCheckpoints.Find(cp => cp.CheckpointId == id);
                    if (checkpoint != null && !checkpoint.IsActivated)
                    {
                        checkpoint.ActivateCheckpoint();
                    }
                }
            }

            // Restore active checkpoint
            string pendingActiveId = PlayerPrefs.GetString($"{saveDataPrefix}PendingActiveCheckpoint", "");
            if (!string.IsNullOrEmpty(pendingActiveId))
            {
                var activeCheckpoint = allCheckpoints.Find(cp => cp.CheckpointId == pendingActiveId);
                if (activeCheckpoint != null)
                {
                    SetActiveCheckpoint(activeCheckpoint);
                }
                PlayerPrefs.DeleteKey($"{saveDataPrefix}PendingActiveCheckpoint");
            }

            // Restore last activated checkpoint
            string pendingLastId = PlayerPrefs.GetString($"{saveDataPrefix}PendingLastActivatedCheckpoint", "");
            if (!string.IsNullOrEmpty(pendingLastId))
            {
                var lastCheckpoint = allCheckpoints.Find(cp => cp.CheckpointId == pendingLastId);
                if (lastCheckpoint != null)
                {
                    lastActivatedCheckpoint = lastCheckpoint;
                }
                PlayerPrefs.DeleteKey($"{saveDataPrefix}PendingLastActivatedCheckpoint");
            }

            if (debugMode)
            {
                Debug.Log($"CheckpointManager: Restored checkpoint states from save data");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CheckpointManager: Failed to restore checkpoint states - {e.Message}");
        }
    }    public void ClearSaveData()
    {
        if (!enablePersistence) return;

        try
        {
            PlayerPrefs.DeleteKey($"{saveDataPrefix}ActiveCheckpoint");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}ActiveCheckpointScene");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}LastActivatedCheckpoint");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}LastActivatedCheckpointScene");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}DiscoveredCheckpoints");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}ActivatedCheckpoints");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}UseNearestAsActive");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}SaveTime");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}PendingActiveCheckpoint");
            PlayerPrefs.DeleteKey($"{saveDataPrefix}PendingLastActivatedCheckpoint");
            PlayerPrefs.Save();

            if (debugMode)
            {
                Debug.Log("CheckpointManager: Cleared all save data");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CheckpointManager: Failed to clear save data - {e.Message}");
        }
    }// Call this after all checkpoints are registered to restore states
    void FinalizeCheckpointLoading()
    {
        if (enablePersistence && allCheckpoints.Count > 0)
        {
            RestoreSavedCheckpointStates();
            
            // If no checkpoint was restored from save data (cross-scene scenario), 
            // check if we should spawn at the nearest checkpoint or use saved position
            if (currentActiveCheckpoint == null)
            {
                HandleCrossSceneSpawn();
            }
        }
    }
    
    void HandleCrossSceneSpawn()
    {        // Check if PlayerPersistence has a saved position for this scene
        string currentScene = gameObject.scene.name;
        string savedPlayerScene = PlayerPrefs.GetString($"{saveDataPrefix}Player_Scene", "");
        
        if (savedPlayerScene == currentScene && PlayerPrefs.HasKey($"{saveDataPrefix}Player_SaveTime"))
        {
            // PlayerPersistence will handle spawning - just set nearest checkpoint as active
            if (useNearestAsActive)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    // Wait a frame for PlayerPersistence to position the player, then find nearest checkpoint
                    StartCoroutine(SetNearestCheckpointAfterPlayerSpawn());
                }
            }
            
            if (debugMode)
            {
                Debug.Log("CheckpointManager: Cross-scene scenario - PlayerPersistence will handle spawn position");
            }
        }
        else
        {
            // No saved position for this scene, use starting checkpoint or first available
            Checkpoint startingCheckpoint = allCheckpoints.Find(cp => cp.IsStartingCheckpoint);
            if (startingCheckpoint != null)
            {
                SetActiveCheckpoint(startingCheckpoint);
                if (debugMode)
                {
                    Debug.Log($"CheckpointManager: Cross-scene scenario - Using starting checkpoint '{startingCheckpoint.CheckpointId}'");
                }
            }
            else if (allCheckpoints.Count > 0)
            {
                SetActiveCheckpoint(allCheckpoints[0]);
                if (debugMode)
                {
                    Debug.Log($"CheckpointManager: Cross-scene scenario - Using first checkpoint '{allCheckpoints[0].CheckpointId}'");
                }
            }
        }
    }
    
    System.Collections.IEnumerator SetNearestCheckpointAfterPlayerSpawn()
    {
        yield return new WaitForEndOfFrame(); // Wait for PlayerPersistence to position player
        yield return new WaitForFixedUpdate(); // Wait one more frame
        
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && allCheckpoints.Count > 0)
        {
            Checkpoint nearestCheckpoint = FindNearestCheckpoint(playerObj.transform.position);
            if (nearestCheckpoint != null)
            {
                SetActiveCheckpoint(nearestCheckpoint);
                if (debugMode)
                {
                    Debug.Log($"CheckpointManager: Set nearest checkpoint '{nearestCheckpoint.CheckpointId}' as active after cross-scene spawn");
                }
            }
        }
    }    // Debug methods
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugLogCheckpointInfo()
    {
        Debug.Log($"=== Checkpoint Manager Debug Info ===");
        Debug.Log($"Total Checkpoints: {allCheckpoints.Count}");
        Debug.Log($"Current Active: {(currentActiveCheckpoint != null ? currentActiveCheckpoint.CheckpointId : "None")}");
        Debug.Log($"Last Activated: {(lastActivatedCheckpoint != null ? lastActivatedCheckpoint.CheckpointId : "None")}");
        Debug.Log($"Current Scene: {gameObject.scene.name}");
        
        // Show saved data
        if (enablePersistence)
        {
            string savedActiveId = PlayerPrefs.GetString($"{saveDataPrefix}ActiveCheckpoint", "None");
            string savedActiveScene = PlayerPrefs.GetString($"{saveDataPrefix}ActiveCheckpointScene", "None");
            string savedLastId = PlayerPrefs.GetString($"{saveDataPrefix}LastActivatedCheckpoint", "None");
            string savedLastScene = PlayerPrefs.GetString($"{saveDataPrefix}LastActivatedCheckpointScene", "None");
            
            Debug.Log($"Saved Active Checkpoint: {savedActiveId} (Scene: {savedActiveScene})");
            Debug.Log($"Saved Last Checkpoint: {savedLastId} (Scene: {savedLastScene})");
        }
        
        for (int i = 0; i < allCheckpoints.Count; i++)
        {
            var cp = allCheckpoints[i];
            Debug.Log($"  [{i}] {cp.CheckpointId} - Active: {cp.IsActivated} - Starting: {cp.IsStartingCheckpoint}");
        }
    }

    void OnDrawGizmos()
    {
        if (debugMode && Application.isPlaying)
        {
            // Draw connections between checkpoints
            Gizmos.color = Color.cyan;
            for (int i = 0; i < allCheckpoints.Count - 1; i++)
            {
                if (allCheckpoints[i] != null && allCheckpoints[i + 1] != null)
                {
                    Gizmos.DrawLine(
                        allCheckpoints[i].transform.position + Vector3.up * 3f,
                        allCheckpoints[i + 1].transform.position + Vector3.up * 3f
                    );
                }
            }

            // Highlight current active checkpoint
            if (currentActiveCheckpoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentActiveCheckpoint.transform.position + Vector3.up * 4f, 1f);
            }
        }
    }
}
