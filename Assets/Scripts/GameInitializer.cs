using UnityEngine;

/// <summary>
/// GameInitializer ensures essential game systems are set up when the game starts.
/// This script handles the initialization of cursor management and other core systems.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("System Settings")]
    [SerializeField] private bool enableCursorManagement = true;
    [SerializeField] private bool debugMode = true;
    
    public static GameInitializer Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (debugMode)
            {
                Debug.Log("GameInitializer: Initializing core game systems...");
            }
            
            InitializeSystems();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize all core game systems
    /// </summary>
    private void InitializeSystems()
    {
        if (enableCursorManagement)
        {
            InitializeCursorManager();
        }
        
        if (debugMode)
        {
            Debug.Log("GameInitializer: Core systems initialization completed");
        }
    }
    
    /// <summary>
    /// Initialize the cursor management system
    /// </summary>
    private void InitializeCursorManager()
    {
        // Check if CursorManager already exists
        if (CursorManager.Instance == null)
        {
            // Create CursorManager if it doesn't exist
            GameObject cursorManagerGO = new GameObject("CursorManager");
            cursorManagerGO.AddComponent<CursorManager>();
            
            if (debugMode)
            {
                Debug.Log("GameInitializer: Created CursorManager");
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.Log("GameInitializer: CursorManager already exists");
            }
        }
    }
    
    /// <summary>
    /// Call this method to manually ensure cursor is properly configured for gameplay
    /// </summary>
    public void EnsureGameplayCursor()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetGameplayCursorState();
        }
        else
        {
            // Fallback cursor settings
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            
            if (debugMode)
            {
                Debug.Log("GameInitializer: Applied fallback cursor settings");
            }
        }
    }
    
    /// <summary>
    /// Call this method to free the cursor for UI interactions
    /// </summary>
    public void EnsureFreeCursor()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetFreeCursorState();
        }
        else
        {
            // Fallback cursor settings
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (debugMode)
            {
                Debug.Log("GameInitializer: Applied fallback free cursor settings");
            }
        }
    }
}
