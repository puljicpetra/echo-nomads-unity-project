using UnityEngine;

/// <summary>
/// CursorManager handles cursor locking and visibility during gameplay.
/// This ensures the cursor stays within the game window during play mode
/// and provides appropriate cursor states for different game situations.
/// </summary>
public class CursorManager : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private CursorLockMode gameplayCursorLockMode = CursorLockMode.Confined;
    [SerializeField] private bool showCursorDuringGameplay = false;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    public static CursorManager Instance { get; private set; }
    
    private CursorLockMode originalLockMode;
    private bool originalCursorVisible;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Store original cursor settings
            originalLockMode = Cursor.lockState;
            originalCursorVisible = Cursor.visible;
            
            if (debugMode)
            {
                Debug.Log($"CursorManager: Original cursor state - Lock: {originalLockMode}, Visible: {originalCursorVisible}");
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Apply cursor settings for gameplay
        SetGameplayCursorState();
    }
    
    void Update()
    {
        // Allow escape key to toggle cursor lock for debugging purposes
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }
    
    /// <summary>
    /// Sets the cursor state appropriate for gameplay
    /// </summary>
    public void SetGameplayCursorState()
    {
        Cursor.lockState = gameplayCursorLockMode;
        Cursor.visible = showCursorDuringGameplay;
        
        if (debugMode)
        {
            Debug.Log($"CursorManager: Set gameplay cursor state - Lock: {gameplayCursorLockMode}, Visible: {showCursorDuringGameplay}");
        }
    }
    
    /// <summary>
    /// Sets the cursor to be free and visible (for UI interactions)
    /// </summary>
    public void SetFreeCursorState()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (debugMode)
        {
            Debug.Log("CursorManager: Set free cursor state - Lock: None, Visible: true");
        }
    }
    
    /// <summary>
    /// Locks the cursor to the center of the screen (for camera control)
    /// </summary>
    public void SetLockedCursorState()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (debugMode)
        {
            Debug.Log("CursorManager: Set locked cursor state - Lock: Locked, Visible: false");
        }
    }
    
    /// <summary>
    /// Confines cursor to the game window but keeps it visible
    /// </summary>
    public void SetConfinedCursorState()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        
        if (debugMode)
        {
            Debug.Log("CursorManager: Set confined cursor state - Lock: Confined, Visible: true");
        }
    }
    
    /// <summary>
    /// Toggles between confined and free cursor states
    /// </summary>
    public void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.None)
        {
            SetGameplayCursorState();
        }
        else
        {
            SetFreeCursorState();
        }
        
        if (debugMode)
        {
            Debug.Log($"CursorManager: Toggled cursor lock - Current state: Lock: {Cursor.lockState}, Visible: {Cursor.visible}");
        }
    }
    
    /// <summary>
    /// Restores the original cursor settings
    /// </summary>
    public void RestoreOriginalCursorState()
    {
        Cursor.lockState = originalLockMode;
        Cursor.visible = originalCursorVisible;
        
        if (debugMode)
        {
            Debug.Log($"CursorManager: Restored original cursor state - Lock: {originalLockMode}, Visible: {originalCursorVisible}");
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // When window gains focus, apply gameplay cursor settings
            SetGameplayCursorState();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // When game unpauses, apply gameplay cursor settings
            SetGameplayCursorState();
        }
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            // Restore original cursor state when the manager is destroyed
            RestoreOriginalCursorState();
        }
    }
}
