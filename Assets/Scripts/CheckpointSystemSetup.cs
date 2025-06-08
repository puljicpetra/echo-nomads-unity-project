using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CheckpointSystemSetup : MonoBehaviour
{
    [Header("Auto Setup Options")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createCheckpointManagerIfMissing = true;
    [SerializeField] private bool createDepthCheckerIfMissing = true;
    
    [Header("Depth Checker Settings")]
    [SerializeField] private float defaultDeathDepth = -50f;
    [SerializeField] private float depthCheckInterval = 0.5f;
    
    [Header("Checkpoint Settings")]
    [SerializeField] private bool useNearestAsActive = true;
    [SerializeField] private float nearestUpdateInterval = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugMode = false;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupCheckpointSystem();
        }
    }

    [ContextMenu("Setup Checkpoint System")]
    public void SetupCheckpointSystem()
    {
        Debug.Log("=== Setting up Checkpoint System ===");
        
        SetupCheckpointManager();
        SetupDepthChecker();
        SetupCheatCodeManager();
        
        Debug.Log("=== Checkpoint System Setup Complete ===");
    }    void SetupCheckpointManager()
    {
        CheckpointManager existingManager = FindObjectOfType<CheckpointManager>();
        if (existingManager == null && createCheckpointManagerIfMissing)
        {
            GameObject managerObj = new GameObject("CheckpointManager");
            CheckpointManager manager = managerObj.AddComponent<CheckpointManager>();
            
            // Configure the checkpoint manager
            if (useNearestAsActive)
            {
                manager.SetNearestAsActiveMode(true);
            }
            
            Debug.Log($"✓ Created CheckpointManager (Mode: {(useNearestAsActive ? "Nearest-Active" : "Manual")})");
        }
        else if (existingManager != null)
        {
            // Configure existing manager
            existingManager.SetNearestAsActiveMode(useNearestAsActive);
            Debug.Log($"✓ CheckpointManager already exists (Mode set to: {(useNearestAsActive ? "Nearest-Active" : "Manual")})");
        }
        else
        {
            Debug.Log("⚠ CheckpointManager not found and auto-creation is disabled");
        }
    }void SetupDepthChecker()
    {
        DepthChecker existingChecker = FindObjectOfType<DepthChecker>();
        if (existingChecker == null && createDepthCheckerIfMissing)
        {
            GameObject checkerObj = new GameObject("DepthChecker");
            DepthChecker checker = checkerObj.AddComponent<DepthChecker>();
            
            // Use public methods to configure the DepthChecker instead of reflection
            checker.SetDeathDepth(defaultDeathDepth);
            checker.SetCheckInterval(depthCheckInterval);
            
            Debug.Log($"✓ Created DepthChecker with death depth: {defaultDeathDepth}");
        }
        else if (existingChecker != null)
        {
            Debug.Log("✓ DepthChecker already exists");
        }
        else
        {
            Debug.Log("⚠ DepthChecker not found and auto-creation is disabled");
        }
    }

    void SetupCheatCodeManager()
    {
        CheatCodeManager existingCheatManager = FindObjectOfType<CheatCodeManager>();
        if (existingCheatManager != null)
        {
            Debug.Log("✓ CheatCodeManager found - 'reset' cheat should be available");
        }
        else
        {
            Debug.Log("⚠ CheatCodeManager not found - cheat codes will not work");
        }
    }

    [ContextMenu("Setup Persistence System")]
    public void SetupPersistenceSystem()
    {
        Debug.Log("=== Setting up Persistence System ===");
        
        // Setup CheckpointManager persistence
        CheckpointManager manager = FindObjectOfType<CheckpointManager>();
        if (manager != null)
        {
            // Enable persistence through reflection since it might be private
            var persistenceField = manager.GetType().GetField("enablePersistence", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (persistenceField != null)
            {
                persistenceField.SetValue(manager, true);
                Debug.Log("✓ Enabled CheckpointManager persistence");
            }
        }
        
        // Setup PlayerPersistence
        PlayerPersistence playerPersistence = FindObjectOfType<PlayerPersistence>();
        if (playerPersistence == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerPersistence = player.AddComponent<PlayerPersistence>();
                Debug.Log("✓ Added PlayerPersistence component to Player");
            }
            else
            {
                Debug.LogWarning("⚠ Player not found - cannot add PlayerPersistence");
            }
        }
        else
        {
            Debug.Log("✓ PlayerPersistence already exists");
        }
        
        // Setup puzzle persistence
        ResonancePuzzle[] puzzles = FindObjectsOfType<ResonancePuzzle>();
        foreach (var puzzle in puzzles)
        {
            // Enable persistence through reflection
            var persistenceField = puzzle.GetType().GetField("enablePersistence", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (persistenceField != null)
            {
                persistenceField.SetValue(puzzle, true);
            }
        }
        Debug.Log($"✓ Enabled persistence for {puzzles.Length} puzzles");
        
        // Setup level persistence
        LevelPuzzleManager[] levelManagers = FindObjectsOfType<LevelPuzzleManager>();
        foreach (var levelManager in levelManagers)
        {
            // Enable persistence through reflection
            var persistenceField = levelManager.GetType().GetField("enablePersistence", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (persistenceField != null)
            {
                persistenceField.SetValue(levelManager, true);
            }
        }
        Debug.Log($"✓ Enabled persistence for {levelManagers.Length} level managers");
        
        Debug.Log("=== Persistence System Setup Complete ===");
        Debug.Log("Your game progress should now be saved automatically!");
    }

    [ContextMenu("Validate Checkpoint System")]
    public void ValidateCheckpointSystem()
    {
        Debug.Log("=== Validating Checkpoint System ===");
        
        // Check for required components
        CheckpointManager manager = FindObjectOfType<CheckpointManager>();
        DepthChecker depthChecker = FindObjectOfType<DepthChecker>();
        CheatCodeManager cheatManager = FindObjectOfType<CheatCodeManager>();
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        
        // Check for player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        // Check for checkpoints
        Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
        
        Debug.Log($"CheckpointManager: {(manager != null ? "✓ Found" : "✗ Missing")}");
        Debug.Log($"DepthChecker: {(depthChecker != null ? "✓ Found" : "✗ Missing")}");
        Debug.Log($"CheatCodeManager: {(cheatManager != null ? "✓ Found" : "✗ Missing")}");
        Debug.Log($"AudioManager: {(audioManager != null ? "✓ Found" : "✗ Missing")}");
        Debug.Log($"Player: {(player != null ? "✓ Found" : "✗ Missing")}");
        Debug.Log($"Checkpoints: {checkpoints.Length} found");
        
        if (checkpoints.Length > 0)
        {
            int startingCheckpoints = 0;
            foreach (var checkpoint in checkpoints)
            {
                if (checkpoint.IsStartingCheckpoint)
                    startingCheckpoints++;
            }
            
            Debug.Log($"Starting checkpoints: {startingCheckpoints}");
            
            if (startingCheckpoints == 0)
            {
                Debug.LogWarning("⚠ No starting checkpoint designated! Consider marking one checkpoint as starting.");
            }
            else if (startingCheckpoints > 1)
            {
                Debug.LogWarning("⚠ Multiple starting checkpoints found! Only one should be marked as starting.");
            }
        }
        else
        {
            Debug.LogWarning("⚠ No checkpoints found in scene! Add Checkpoint components to GameObjects.");
        }
        
        Debug.Log("=== Validation Complete ===");
    }

    [ContextMenu("Create Sample Checkpoint")]
    public void CreateSampleCheckpoint()
    {
        GameObject checkpointObj = new GameObject("SampleCheckpoint");
        checkpointObj.transform.position = Vector3.zero;
        
        Checkpoint checkpoint = checkpointObj.AddComponent<Checkpoint>();
        
        // Create visual indicators
        GameObject activeIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        activeIndicator.name = "ActiveIndicator";
        activeIndicator.transform.SetParent(checkpointObj.transform);
        activeIndicator.transform.localPosition = Vector3.zero;
        activeIndicator.transform.localScale = new Vector3(2f, 0.1f, 2f);
          // Set material color to green (use sharedMaterial in edit mode)
        var activeRenderer = activeIndicator.GetComponent<Renderer>();
        if (activeRenderer != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // In edit mode, create a new material to avoid leaks
                Material greenMaterial = new Material(activeRenderer.sharedMaterial);
                greenMaterial.color = Color.green;
                activeRenderer.sharedMaterial = greenMaterial;
            }
            else
#endif
            {
                activeRenderer.material.color = Color.green;
            }
        }
        
        GameObject inactiveIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        inactiveIndicator.name = "InactiveIndicator";
        inactiveIndicator.transform.SetParent(checkpointObj.transform);
        inactiveIndicator.transform.localPosition = Vector3.zero;
        inactiveIndicator.transform.localScale = new Vector3(2f, 0.1f, 2f);
        
        // Set material color to gray (use sharedMaterial in edit mode)
        var inactiveRenderer = inactiveIndicator.GetComponent<Renderer>();
        if (inactiveRenderer != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // In edit mode, create a new material to avoid leaks
                Material grayMaterial = new Material(inactiveRenderer.sharedMaterial);
                grayMaterial.color = Color.gray;
                inactiveRenderer.sharedMaterial = grayMaterial;
            }
            else
#endif
            {
                inactiveRenderer.material.color = Color.gray;
            }
        }Debug.Log("✓ Created sample checkpoint at origin");
        
#if UNITY_EDITOR
        Selection.activeGameObject = checkpointObj;
#endif
    }

    [ContextMenu("Test Save System")]
    public void TestSaveSystem()
    {
        Debug.Log("=== Testing Save System ===");
        
        // Test checkpoint system
        CheckpointManager manager = FindObjectOfType<CheckpointManager>();
        if (manager != null)
        {
            var currentCheckpoint = manager.GetCurrentCheckpoint();
            if (currentCheckpoint != null)
            {
                Debug.Log($"Current checkpoint: {currentCheckpoint.CheckpointId}");
                
                // Force save checkpoint data
                var saveMethod = manager.GetType().GetMethod("SaveCheckpointData", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (saveMethod != null)
                {
                    saveMethod.Invoke(manager, null);
                    Debug.Log("✓ Forced checkpoint save");
                }
            }
            else
            {
                Debug.LogWarning("⚠ No active checkpoint found");
            }
        }
        
        // Test player persistence
        PlayerPersistence playerPersistence = FindObjectOfType<PlayerPersistence>();
        if (playerPersistence != null)
        {
            playerPersistence.SavePlayerPosition();
            Debug.Log("✓ Saved player position");
        }
        else
        {
            Debug.LogWarning("⚠ PlayerPersistence component not found");
        }
        
        // Check PlayerPrefs
        string[] keys = {
            "EchoNomads_ActiveCheckpoint",
            "EchoNomads_Player_PosX",
            "EchoNomads_Player_PosY", 
            "EchoNomads_Player_PosZ"
        };
        
        foreach (string key in keys)
        {
            if (PlayerPrefs.HasKey(key))
            {
                Debug.Log($"✓ Found saved data: {key}");
            }
            else
            {
                Debug.Log($"⚠ Missing save data: {key}");
            }
        }
        
        Debug.Log("=== Save System Test Complete ===");
    }

    [ContextMenu("Force Save Game State")]
    public void ForceSaveGameState()
    {
        Debug.Log("=== Force Saving Game State ===");
        
        // Save checkpoint data
        CheckpointManager manager = FindObjectOfType<CheckpointManager>();
        if (manager != null)
        {
            var saveMethod = manager.GetType().GetMethod("SaveCheckpointData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            saveMethod?.Invoke(manager, null);
        }
        
        // Save player position
        PlayerPersistence playerPersistence = FindObjectOfType<PlayerPersistence>();
        if (playerPersistence != null)
        {
            playerPersistence.SavePlayerPosition();
        }
        
        // Save all puzzles
        ResonancePuzzle[] puzzles = FindObjectsOfType<ResonancePuzzle>();
        foreach (var puzzle in puzzles)
        {
            var saveMethod = puzzle.GetType().GetMethod("SavePuzzleState", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            saveMethod?.Invoke(puzzle, null);
        }
        
        // Save all level managers
        LevelPuzzleManager[] levelManagers = FindObjectsOfType<LevelPuzzleManager>();
        foreach (var levelManager in levelManagers)
        {
            var saveMethod = levelManager.GetType().GetMethod("SaveLevelState", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            saveMethod?.Invoke(levelManager, null);
        }
        
        PlayerPrefs.Save();
        Debug.Log("✓ Forced save complete - Game state should now be saved!");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CheckpointSystemSetup))]
public class CheckpointSystemSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10);
        
        CheckpointSystemSetup setup = (CheckpointSystemSetup)target;
        
        if (GUILayout.Button("Setup Checkpoint System", GUILayout.Height(30)))
        {
            setup.SetupCheckpointSystem();
        }
        
        if (GUILayout.Button("Validate System", GUILayout.Height(25)))
        {
            setup.ValidateCheckpointSystem();
        }
          if (GUILayout.Button("Create Sample Checkpoint", GUILayout.Height(25)))
        {
            setup.CreateSampleCheckpoint();
        }
        
        GUILayout.Space(5);
          if (GUILayout.Button("Setup Persistence System", GUILayout.Height(30)))
        {
            setup.SetupPersistenceSystem();
        }
          if (GUILayout.Button("Test Save System", GUILayout.Height(25)))
        {
            setup.TestSaveSystem();
        }
        
        if (GUILayout.Button("Force Save Game State", GUILayout.Height(25)))
        {
            setup.ForceSaveGameState();
        }
        
        GUILayout.Space(10);
          GUILayout.Label("Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Add this script to any GameObject in your scene", EditorStyles.wordWrappedLabel);
        GUILayout.Label("2. Click 'Setup Checkpoint System' to auto-create managers", EditorStyles.wordWrappedLabel);
        GUILayout.Label("3. Click 'Setup Persistence System' to enable save/load", EditorStyles.wordWrappedLabel);
        GUILayout.Label("4. Create checkpoints by adding Checkpoint components to GameObjects", EditorStyles.wordWrappedLabel);
        GUILayout.Label("5. Mark one checkpoint as 'Starting Checkpoint'", EditorStyles.wordWrappedLabel);
        GUILayout.Label("6. Use 'Test Save System' to verify persistence is working", EditorStyles.wordWrappedLabel);
        GUILayout.Label("7. Use 'Force Save Game State' to manually save progress", EditorStyles.wordWrappedLabel);
    }
}
#endif
