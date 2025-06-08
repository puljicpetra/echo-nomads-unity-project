using UnityEngine;

/// <summary>
/// Editor utility to help set up fall death trigger zones in your level.
/// This replaces the complex global depth checking with simple, efficient trigger zones.
/// </summary>
public class FallDeathTriggerSetup : MonoBehaviour
{
    [Header("Auto Setup Options")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createDepthCheckerIfMissing = true;
    
    [Header("Default Trigger Settings")]
    [SerializeField] private Vector3 defaultTriggerSize = new Vector3(50f, 10f, 50f);
    [SerializeField] private Color defaultGizmoColor = Color.red;
    [SerializeField] private bool teleportToNearestCheckpoint = true;
    
    [Header("Audio Settings")]
    [SerializeField] private string defaultFallSoundName = "PlayerFallDeath";
    [SerializeField] private string defaultRespawnSoundName = "PlayerRespawn";
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugMode = false;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupFallDeathSystem();
        }
    }

    [ContextMenu("Setup Fall Death System")]
    public void SetupFallDeathSystem()
    {
        SetupDepthChecker();
        
        if (enableDebugMode)
        {
            Debug.Log("FallDeathTriggerSetup: Fall death system setup complete");
        }
    }

    void SetupDepthChecker()
    {
        DepthChecker existingChecker = FindObjectOfType<DepthChecker>();
        if (existingChecker == null && createDepthCheckerIfMissing)
        {
            GameObject checkerObj = new GameObject("DepthChecker");
            DepthChecker checker = checkerObj.AddComponent<DepthChecker>();
            
            Debug.Log("✓ Created DepthChecker for fall death trigger management");
        }
        else if (existingChecker != null)
        {
            Debug.Log("✓ DepthChecker already exists - fall death triggers will auto-register");
        }
        else
        {
            Debug.Log("⚠ DepthChecker not found and auto-creation is disabled");
        }
    }

    [ContextMenu("Create Fall Death Trigger Here")]
    public void CreateFallDeathTriggerHere()
    {
        CreateFallDeathTrigger(transform.position, defaultTriggerSize, $"FallDeathTrigger_{System.DateTime.Now.Ticks}");
    }

    [ContextMenu("Create Fall Death Trigger at Origin")]
    public void CreateFallDeathTriggerAtOrigin()
    {
        CreateFallDeathTrigger(Vector3.zero, defaultTriggerSize, "FallDeathTrigger_Origin");
    }

    public GameObject CreateFallDeathTrigger(Vector3 position, Vector3 size, string triggerName = "")
    {
        if (string.IsNullOrEmpty(triggerName))
        {
            triggerName = $"FallDeathTrigger_{Random.Range(1000, 9999)}";
        }

        // Create trigger object
        GameObject triggerObj = new GameObject(triggerName);
        triggerObj.transform.position = position;
        
        // Add and configure FallDeathTrigger component
        FallDeathTrigger trigger = triggerObj.AddComponent<FallDeathTrigger>();
        trigger.SetTriggerName(triggerName);
        trigger.SetTeleportToNearest(teleportToNearestCheckpoint);
        trigger.SetTriggerSize(size);
        
        // Configure BoxCollider (will be created automatically by FallDeathTrigger if needed)
        BoxCollider collider = triggerObj.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = triggerObj.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;
        collider.size = size;
        
        // Set layer (optional - you might want to create a dedicated "FallDeathTrigger" layer)
        triggerObj.layer = 0; // Default layer - consider creating a custom layer
        
        Debug.Log($"✓ Created FallDeathTrigger '{triggerName}' at {position} with size {size}");
        
        #if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = triggerObj;
        #endif
        
        return triggerObj;
    }

    [ContextMenu("Create Fall Death Trigger Below Scene")]
    public void CreateFallDeathTriggerBelowScene()
    {
        // Find the lowest point in the scene and create a trigger below it
        float lowestY = FindLowestPointInScene();
        Vector3 triggerPosition = new Vector3(0, lowestY - 20f, 0);
        Vector3 largeSize = new Vector3(200f, 10f, 200f); // Large trigger to catch falling players
        
        CreateFallDeathTrigger(triggerPosition, largeSize, "FallDeathTrigger_SceneBottom");
    }

    float FindLowestPointInScene()
    {
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        float lowestY = 0f;
        
        foreach (var renderer in allRenderers)
        {
            if (renderer.bounds.min.y < lowestY)
            {
                lowestY = renderer.bounds.min.y;
            }
        }
        
        if (enableDebugMode)
        {
            Debug.Log($"FallDeathTriggerSetup: Lowest point in scene found at Y = {lowestY}");
        }
        
        return lowestY;
    }

    [ContextMenu("Create Multiple Fall Death Triggers")]
    public void CreateMultipleFallDeathTriggers()
    {
        Vector3[] positions = {
            new Vector3(-50, -30, 0),
            new Vector3(0, -30, 0),
            new Vector3(50, -30, 0),
            new Vector3(0, -30, -50),
            new Vector3(0, -30, 50)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            CreateFallDeathTrigger(positions[i], defaultTriggerSize, $"FallDeathTrigger_{i + 1}");
        }
        
        Debug.Log($"✓ Created {positions.Length} fall death triggers in a grid pattern");
    }

    [ContextMenu("Find All Fall Death Triggers")]
    public void FindAllFallDeathTriggers()
    {
        FallDeathTrigger[] triggers = FindObjectsOfType<FallDeathTrigger>();
        
        Debug.Log($"=== Fall Death Triggers Found: {triggers.Length} ===");
        
        for (int i = 0; i < triggers.Length; i++)
        {
            var trigger = triggers[i];
            Debug.Log($"  [{i}] {trigger.GetTriggerName()} at {trigger.transform.position} - " +
                     $"Teleport to nearest: {trigger.IsTeleportToNearestEnabled()}");
        }
        
        if (triggers.Length == 0)
        {
            Debug.Log("No FallDeathTrigger components found in scene. Use 'Create Fall Death Trigger Here' to add some.");
        }
    }

    [ContextMenu("Validate Fall Death System")]
    public void ValidateFallDeathSystem()
    {
        Debug.Log("=== Fall Death System Validation ===");
        
        // Check DepthChecker
        DepthChecker depthChecker = FindObjectOfType<DepthChecker>();
        Debug.Log($"DepthChecker: {(depthChecker != null ? "✓ Found" : "✗ Missing")}");
        
        // Check CheckpointManager
        CheckpointManager checkpointManager = CheckpointManager.Instance;
        Debug.Log($"CheckpointManager: {(checkpointManager != null ? "✓ Found" : "✗ Missing")}");
        
        // Check AudioManager
        AudioManager audioManager = AudioManager.Instance;
        Debug.Log($"AudioManager: {(audioManager != null ? "✓ Found" : "✗ Missing")}");
        
        // Check Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log($"Player: {(player != null ? "✓ Found" : "✗ Missing")}");
        
        // Check Fall Death Triggers
        FallDeathTrigger[] triggers = FindObjectsOfType<FallDeathTrigger>();
        Debug.Log($"Fall Death Triggers: {triggers.Length} found");
        
        // Check Checkpoints
        Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
        Debug.Log($"Checkpoints: {checkpoints.Length} found");
        
        if (triggers.Length == 0)
        {
            Debug.LogWarning("⚠ No fall death triggers found! Create some using the context menu options.");
        }
        
        if (checkpoints.Length == 0)
        {
            Debug.LogWarning("⚠ No checkpoints found! Players need checkpoints to respawn at.");
        }
        
        if (checkpointManager == null)
        {
            Debug.LogWarning("⚠ No CheckpointManager found! Fall death triggers need this to find nearest checkpoints.");
        }
        
        Debug.Log("=== Validation Complete ===");
    }

    [ContextMenu("Convert Legacy DepthChecker")]
    public void ConvertLegacyDepthChecker()
    {
        DepthChecker depthChecker = FindObjectOfType<DepthChecker>();
        if (depthChecker != null)
        {
            // Disable legacy depth checking
            depthChecker.EnableDepthChecking(false);
            
            // Create a fall death trigger at the legacy death depth
            float legacyDepth = depthChecker.GetLegacyDeathDepth();
            Vector3 triggerPosition = new Vector3(0, legacyDepth - 5f, 0);
            Vector3 largeSize = new Vector3(500f, 10f, 500f); // Very large to cover whole level
            
            CreateFallDeathTrigger(triggerPosition, largeSize, "FallDeathTrigger_LegacyConversion");
            
            Debug.Log($"✓ Converted legacy DepthChecker (depth: {legacyDepth}) to FallDeathTrigger system");
            Debug.Log("⚠ Consider creating more specific triggers instead of one large one for better performance");
        }
        else
        {
            Debug.LogWarning("No DepthChecker found to convert");
        }
    }

    // Helper method to set default values on newly created triggers
    public void ConfigureTrigger(FallDeathTrigger trigger)
    {
        if (trigger == null) return;
        
        trigger.SetTeleportToNearest(teleportToNearestCheckpoint);
        
        // Set audio if available
        // Note: The trigger will handle audio internally, but you could customize it here
        
        if (enableDebugMode)
        {
            Debug.Log($"FallDeathTriggerSetup: Configured trigger '{trigger.GetTriggerName()}'");
        }
    }

    void OnDrawGizmos()
    {
        if (enableDebugMode)
        {
            // Draw setup position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
            
            // Draw default trigger size preview
            Gizmos.color = defaultGizmoColor;
            Gizmos.color = new Color(defaultGizmoColor.r, defaultGizmoColor.g, defaultGizmoColor.b, 0.3f);
            Gizmos.DrawCube(transform.position, defaultTriggerSize);
            
            Gizmos.color = defaultGizmoColor;
            Gizmos.DrawWireCube(transform.position, defaultTriggerSize);
        }
    }
}
