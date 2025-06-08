using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple component to trigger scene transitions when player touches a trigger.
/// Attach to any GameObject with a trigger collider (like a plane).
/// </summary>
public class SimpleSceneTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "NextScene";
    [SerializeField] private float transitionDelay = 1f;
    
    [Header("Audio")]
    [SerializeField] private string transitionSoundName = "SceneTransition";
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private bool hasTriggered = false;
    
    void Start()
    {
        // Ensure we have a trigger collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            if (debugMode)
            {
                Debug.Log($"SimpleSceneTransition '{name}': Added BoxCollider");
            }
        }
        
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            if (debugMode)
            {
                Debug.Log($"SimpleSceneTransition '{name}': Set collider as trigger");
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            
            if (debugMode)
            {
                Debug.Log($"SimpleSceneTransition '{name}': Player touched trigger, transitioning to '{targetSceneName}' in {transitionDelay} seconds");
            }
            
            // Play transition sound
            if (!string.IsNullOrEmpty(transitionSoundName) && AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(transitionSoundName);
            }
            
            // Load the scene after delay
            Invoke(nameof(LoadTargetScene), transitionDelay);
        }
    }
    
    void LoadTargetScene()
    {
        if (debugMode)
        {
            Debug.Log($"SimpleSceneTransition '{name}': Loading scene '{targetSceneName}'");
        }
        
        SceneManager.LoadScene(targetSceneName);
    }
    
    // Public method to trigger transition manually
    public void TriggerTransition()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            Invoke(nameof(LoadTargetScene), transitionDelay);
        }
    }
    
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
    }
    
    void OnDrawGizmos()
    {
        if (debugMode)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = hasTriggered ? Color.red : Color.cyan;
                
                if (col is BoxCollider box)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
                }
            }
        }
    }
}
