using UnityEngine;

/// <summary>
/// Simple helper script to quickly set up cursor management in any scene.
/// Add this to an empty GameObject and it will automatically create the necessary cursor management systems.
/// </summary>
public class QuickCursorSetup : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool setupOnAwake = true;
    [SerializeField] private CursorLockMode preferredCursorMode = CursorLockMode.Confined;
    [SerializeField] private bool showCursorDuringPlay = false;
    
    void Awake()
    {
        if (setupOnAwake)
        {
            SetupCursorManagement();
        }
    }
    
    [ContextMenu("Setup Cursor Management")]
    public void SetupCursorManagement()
    {
        Debug.Log("QuickCursorSetup: Setting up cursor management...");
        
        // Ensure GameInitializer exists
        if (GameInitializer.Instance == null)
        {
            GameObject initializerGO = new GameObject("GameInitializer");
            initializerGO.AddComponent<GameInitializer>();
            Debug.Log("QuickCursorSetup: Created GameInitializer");
        }
        
        // Configure cursor settings
        if (CursorManager.Instance != null)
        {
            // Apply the preferred settings
            switch (preferredCursorMode)
            {
                case CursorLockMode.Confined:
                    CursorManager.Instance.SetConfinedCursorState();
                    break;
                case CursorLockMode.Locked:
                    CursorManager.Instance.SetLockedCursorState();
                    break;
                case CursorLockMode.None:
                    CursorManager.Instance.SetFreeCursorState();
                    break;
            }
            
            // Override visibility if needed
            if (showCursorDuringPlay)
            {
                Cursor.visible = true;
            }
            
            Debug.Log($"QuickCursorSetup: Applied cursor settings - Mode: {preferredCursorMode}, Visible: {showCursorDuringPlay}");
        }
        else
        {
            // Wait a frame and try again
            StartCoroutine(WaitAndSetupCursor());
        }
        
        // Remove this component after setup (it's no longer needed)
        Destroy(this);
    }
    
    private System.Collections.IEnumerator WaitAndSetupCursor()
    {
        yield return null; // Wait one frame
        
        if (CursorManager.Instance != null)
        {
            switch (preferredCursorMode)
            {
                case CursorLockMode.Confined:
                    CursorManager.Instance.SetConfinedCursorState();
                    break;
                case CursorLockMode.Locked:
                    CursorManager.Instance.SetLockedCursorState();
                    break;
                case CursorLockMode.None:
                    CursorManager.Instance.SetFreeCursorState();
                    break;
            }
            
            if (showCursorDuringPlay)
            {
                Cursor.visible = true;
            }
            
            Debug.Log($"QuickCursorSetup: Applied cursor settings (delayed) - Mode: {preferredCursorMode}, Visible: {showCursorDuringPlay}");
        }
        else
        {
            Debug.LogWarning("QuickCursorSetup: CursorManager still not available after waiting");
        }
    }
}
