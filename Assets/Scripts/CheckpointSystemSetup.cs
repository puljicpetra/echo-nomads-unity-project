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
        
        GUILayout.Space(10);
        
        GUILayout.Label("Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Add this script to any GameObject in your scene", EditorStyles.wordWrappedLabel);
        GUILayout.Label("2. Click 'Setup Checkpoint System' to auto-create managers", EditorStyles.wordWrappedLabel);
        GUILayout.Label("3. Create checkpoints by adding Checkpoint components to GameObjects", EditorStyles.wordWrappedLabel);
        GUILayout.Label("4. Mark one checkpoint as 'Starting Checkpoint'", EditorStyles.wordWrappedLabel);
        GUILayout.Label("5. Use 'Validate System' to check everything is set up correctly", EditorStyles.wordWrappedLabel);
    }
}
#endif
