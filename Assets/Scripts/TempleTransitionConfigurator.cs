using UnityEngine;

/// <summary>
/// Helper script to configure the temple scene transition settings.
/// Attach this to a GameObject in the Temple Path scene to automatically
/// configure the existing SceneTransitionTrigger with better fade settings.
/// </summary>
public class TempleTransitionConfigurator : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float improvedFadeTime = 2.0f;
    [SerializeField] private float improvedTransitionDelay = 0.3f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private void Start()
    {
        // Find the existing SceneTransitionTrigger in the scene
        SceneTransitionTrigger[] triggers = FindObjectsOfType<SceneTransitionTrigger>();
        
        foreach (var trigger in triggers)
        {
            // Look for the trigger that transitions to FinalCutscene
            if (trigger.GetTargetSceneName().Contains("FinalCutscene") || 
                trigger.GetTargetSceneName().Contains("Index: 6"))
            {
                // Use reflection to set private fields for better fade experience
                var triggerType = typeof(SceneTransitionTrigger);
                
                // Set fade time
                var fadeTimeField = triggerType.GetField("fadeTime", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fadeTimeField != null)
                {
                    fadeTimeField.SetValue(trigger, improvedFadeTime);
                    if (debugMode)
                    {
                        Debug.Log($"Set fade time to {improvedFadeTime} seconds for trigger: {trigger.name}");
                    }
                }
                
                // Set transition delay
                var delayField = triggerType.GetField("transitionDelay",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (delayField != null)
                {
                    delayField.SetValue(trigger, improvedTransitionDelay);
                    if (debugMode)
                    {
                        Debug.Log($"Set transition delay to {improvedTransitionDelay} seconds for trigger: {trigger.name}");
                    }
                }
                
                // Ensure fade screen is enabled
                var fadeScreenField = triggerType.GetField("fadeScreen",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fadeScreenField != null)
                {
                    fadeScreenField.SetValue(trigger, true);
                    if (debugMode)
                    {
                        Debug.Log($"Enabled fade screen for trigger: {trigger.name}");
                    }
                }
                
                if (debugMode)
                {
                    Debug.Log($"Successfully configured temple transition trigger: {trigger.name}");
                }
                
                break; // We found and configured the right trigger
            }
        }
        
        // Remove this configurator after setup
        Destroy(gameObject);
    }
}
