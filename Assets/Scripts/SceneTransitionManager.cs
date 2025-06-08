using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Singleton manager for handling scene transitions with fade effects.
/// Place this on a GameObject that should persist between scenes.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    
    [Header("Fade Settings")]
    [SerializeField] private Canvas fadeCanvas;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private Color fadeColor = Color.black;
    
    [Header("Loading Settings")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private Text loadingText;
    
    [Header("Audio")]
    [SerializeField] private string transitionStartSound = "TransitionStart";
    [SerializeField] private string transitionEndSound = "TransitionEnd";
    
    private bool isTransitioning = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupFadeCanvas();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void SetupFadeCanvas()
    {
        if (fadeCanvas == null)
        {
            // Create fade canvas if not assigned
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);
            
            fadeCanvas = canvasObj.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 1000; // High sorting order to appear on top
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create fade image
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvasObj.transform);
            
            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = fadeColor;
            
            // Set image to fill screen
            RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }
        
        // Start with transparent fade
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }
        
        // Hide loading panel initially
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
    
    public void TransitionToScene(string sceneName, bool showLoading = false)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionCoroutine(sceneName, showLoading));
        }
    }
    
    public void TransitionToScene(int sceneIndex, bool showLoading = false)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionCoroutine(sceneIndex, showLoading));
        }
    }
    
    IEnumerator TransitionCoroutine(object sceneIdentifier, bool showLoading)
    {
        isTransitioning = true;
        
        // Play transition start sound
        if (!string.IsNullOrEmpty(transitionStartSound) && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(transitionStartSound);
        }
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Show loading panel if requested
        if (showLoading && loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        
        // Load the scene
        AsyncOperation asyncLoad;
        if (sceneIdentifier is string sceneName)
        {
            asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        }
        else if (sceneIdentifier is int sceneIndex)
        {
            asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        }
        else
        {
            Debug.LogError("SceneTransitionManager: Invalid scene identifier");
            isTransitioning = false;
            yield break;
        }
        
        // Update loading bar
        while (!asyncLoad.isDone)
        {
            if (showLoading)
            {
                UpdateLoadingUI(asyncLoad.progress);
            }
            yield return null;
        }
        
        // Hide loading panel
        if (showLoading && loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        
        // Wait a brief moment
        yield return new WaitForSeconds(0.1f);
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        // Play transition end sound
        if (!string.IsNullOrEmpty(transitionEndSound) && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(transitionEndSound);
        }
        
        isTransitioning = false;
    }
    
    IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;
        
        Color color = fadeImage.color;
        float startAlpha = color.a;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeSpeed);
            
            color.a = alpha;
            fadeImage.color = color;
            
            yield return null;
        }
        
        // Ensure fully opaque
        color.a = 1f;
        fadeImage.color = color;
    }
    
    IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;
        
        Color color = fadeImage.color;
        float startAlpha = color.a;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeSpeed);
            
            color.a = alpha;
            fadeImage.color = color;
            
            yield return null;
        }
        
        // Ensure fully transparent
        color.a = 0f;
        fadeImage.color = color;
    }
    
    void UpdateLoadingUI(float progress)
    {
        if (loadingBar != null)
        {
            loadingBar.value = progress;
        }
        
        if (loadingText != null)
        {
            loadingText.text = $"Loading... {progress * 100f:F0}%";
        }
    }
    
    // Public getters
    public bool IsTransitioning() => isTransitioning;
    
    public void SetFadeColor(Color color)
    {
        fadeColor = color;
        if (fadeImage != null)
        {
            Color currentColor = fadeImage.color;
            currentColor.r = color.r;
            currentColor.g = color.g;
            currentColor.b = color.b;
            fadeImage.color = currentColor;
        }
    }
    
    public void SetFadeSpeed(float speed)
    {
        fadeSpeed = Mathf.Max(0.1f, speed);
    }
    
    // Manual fade control
    public void FadeOutManual()
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeOut());
        }
    }
    
    public void FadeInManual()
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeIn());
        }
    }
}
