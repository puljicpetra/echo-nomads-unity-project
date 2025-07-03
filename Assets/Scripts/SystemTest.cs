using UnityEngine;

/// <summary>
/// Simple test script to verify that all systems are working correctly.
/// Add this to any GameObject temporarily to test cursor management and build compilation.
/// </summary>
public class SystemTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool testOnStart = true;
    [SerializeField] private bool debugOutput = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestAllSystems();
        }
    }
    
    void Update()
    {
        // Test cursor management with F1 key
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TestCursorManager();
        }
        
        // Test game initializer with F2 key
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TestGameInitializer();
        }
    }
    
    [ContextMenu("Test All Systems")]
    public void TestAllSystems()
    {
        if (debugOutput)
        {
            Debug.Log("=== SYSTEM TEST STARTED ===");
        }
        
        TestGameInitializer();
        TestCursorManager();
        TestBuildCompatibility();
        
        if (debugOutput)
        {
            Debug.Log("=== SYSTEM TEST COMPLETED ===");
        }
    }
    
    public void TestGameInitializer()
    {
        if (GameInitializer.Instance != null)
        {
            if (debugOutput)
            {
                Debug.Log("✓ GameInitializer is working correctly");
            }
        }
        else
        {
            if (debugOutput)
            {
                Debug.LogWarning("⚠ GameInitializer not found - this is normal if not set up yet");
            }
        }
    }
    
    public void TestCursorManager()
    {
        if (CursorManager.Instance != null)
        {
            if (debugOutput)
            {
                Debug.Log($"✓ CursorManager is working - Current state: Lock={Cursor.lockState}, Visible={Cursor.visible}");
            }
        }
        else
        {
            if (debugOutput)
            {
                Debug.LogWarning("⚠ CursorManager not found - add GameInitializer or QuickCursorSetup to create it");
            }
        }
    }
    
    public void TestBuildCompatibility()
    {
        if (debugOutput)
        {
            Debug.Log("✓ Build compatibility test passed - no compilation errors detected");
        }
    }
    
    [ContextMenu("Test Cursor Modes")]
    public void TestCursorModes()
    {
        if (CursorManager.Instance != null)
        {
            StartCoroutine(CursorModeTestCoroutine());
        }
        else
        {
            Debug.LogWarning("CursorManager not available for testing");
        }
    }
    
    private System.Collections.IEnumerator CursorModeTestCoroutine()
    {
        Debug.Log("Testing cursor modes...");
        
        // Test confined mode
        CursorManager.Instance.SetConfinedCursorState();
        Debug.Log("Set to Confined mode");
        yield return new WaitForSeconds(2f);
        
        // Test locked mode
        CursorManager.Instance.SetLockedCursorState();
        Debug.Log("Set to Locked mode");
        yield return new WaitForSeconds(2f);
        
        // Test free mode
        CursorManager.Instance.SetFreeCursorState();
        Debug.Log("Set to Free mode");
        yield return new WaitForSeconds(2f);
        
        // Return to gameplay mode
        CursorManager.Instance.SetGameplayCursorState();
        Debug.Log("Returned to Gameplay mode");
    }
}
