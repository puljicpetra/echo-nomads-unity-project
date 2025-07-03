using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles the specific transition from Temple Path scene to Final Cutscene.
/// This script can be attached to a trigger area in the temple scene to provide
/// a smooth fade-out transition to the final cutscene.
/// </summary>
public class TempleToFinalTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration = 2.0f;
    [SerializeField] private float delayBeforeTransition = 0.5f;
    [SerializeField] private bool playTransitionSound = true;
    [SerializeField] private string transitionSoundName = "SceneTransition";
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private bool hasTriggered = false;
    
    private void Start()
    {
        // Ensure we have a trigger collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            if (debugMode)
            {
                Debug.Log($"TempleToFinalTransition '{name}': Added BoxCollider");
            }
        }
        
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            if (debugMode)
            {
                Debug.Log($"TempleToFinalTransition '{name}': Set collider as trigger");
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            
            if (debugMode)
            {
                Debug.Log($"TempleToFinalTransition '{name}': Player triggered transition to Final Cutscene");
            }
            
            StartCoroutine(TransitionToFinalCutscene());
        }
    }
    
    private IEnumerator TransitionToFinalCutscene()
    {
        // Play transition sound
        if (playTransitionSound && !string.IsNullOrEmpty(transitionSoundName))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(transitionSoundName);
            }
        }
        
        // Wait for delay
        if (delayBeforeTransition > 0)
        {
            yield return new WaitForSeconds(delayBeforeTransition);
        }
        
        // Create fade overlay
        yield return StartCoroutine(CreateFadeOverlay());
        
        // Load final cutscene
        if (debugMode)
        {
            Debug.Log("TempleToFinalTransition: Loading FinalCutscene...");
        }
        
        SceneManager.LoadScene("FinalCutscene");
    }
    
    private IEnumerator CreateFadeOverlay()
    {
        if (debugMode)
        {
            Debug.Log($"TempleToFinalTransition: Creating fade overlay for {fadeOutDuration} seconds");
        }
        
        // Create a canvas for the fade overlay
        GameObject canvasGO = new GameObject("TransitionFadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Ensure it renders on top of everything
        
        CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create the fade overlay image
        GameObject fadeOverlay = new GameObject("FadeOverlay");
        fadeOverlay.transform.SetParent(canvasGO.transform, false);
        
        Image fadeImage = fadeOverlay.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Start transparent
        
        // Make it cover the entire screen
        RectTransform rectTransform = fadeOverlay.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Make sure the canvas persists through scene changes
        DontDestroyOnLoad(canvasGO);
        
        // Fade to black
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeOutDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        // Ensure it's fully black
        color.a = 1f;
        fadeImage.color = color;
        
        if (debugMode)
        {
            Debug.Log("TempleToFinalTransition: Fade to black completed");
        }
    }
    
    // Public method to trigger transition manually
    public void TriggerTransition()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(TransitionToFinalCutscene());
        }
    }
    
    private void OnDrawGizmos()
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
