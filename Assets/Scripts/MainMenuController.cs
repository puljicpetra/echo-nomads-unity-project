using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// MainMenuController handles the main menu functionality including:
/// - Starting the game with checkpoint progress detection
/// - Skipping intro cutscene if player has already made progress
/// - Loading the appropriate scene based on save data
/// - Audio and visual fade transitions
/// - Game progress management (clearing save data)
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 3.0f;
    
    [Header("Audio Fade Settings")]
    public AudioMixer mainAudioMixer;
    public string masterVolumeParameter = "BackgroundVolume";
    public float audioFadeDuration = 2.5f;
    public float minVolumeDB = -80f;
    
    [Header("UI Feedback")]
    public GameObject continuingGameText; // Optional UI text to show "Continuing where you left off..."
    
    private GameObject fadeOverlay;

    public void StartGame()
    {
        // Check if player has passed first checkpoint and should skip intro
        if (HasPassedFirstCheckpoint())
        {
            StartCoroutine(FadeToGameScene());
        }
        else
        {
            StartCoroutine(FadeToIntroScene());
        }
    }

    private IEnumerator FadeToIntroScene()
    {
        Debug.Log("Starting fade to intro scene...");
        
        // Start fading out audio (only if mixer is assigned)
        if (mainAudioMixer != null)
        {
            StartCoroutine(GraduallyQuietAudio());
        }
        else
        {
            Debug.LogWarning("Audio mixer not assigned - skipping audio fade. Please assign the AudioMixer in the inspector for audio fade effect.");
        }

        // Create a fade overlay that covers the entire screen
        CreateFadeOverlay();

        Debug.Log("Starting visual fade to black...");
        // Fade to black
        float elapsedTime = 0f;
        Image fadeImage = fadeOverlay.GetComponent<Image>();
        Color color = fadeImage.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Ensure it's fully black
        color.a = 1f;
        fadeImage.color = color;
        Debug.Log("Visual fade completed.");

        // Wait for the longer of the two fades to complete (only if audio mixer is assigned)
        if (mainAudioMixer != null)
        {
            float maxFadeDuration = Mathf.Max(fadeDuration, audioFadeDuration);
            float waitTime = maxFadeDuration - fadeDuration; // How much longer to wait after visual fade
            if (waitTime > 0)
            {
                Debug.Log($"Waiting additional {waitTime} seconds for audio fade to complete...");
                yield return new WaitForSeconds(waitTime);
            }
        }

        // Destroy audio manager after audio has been faded out
        GameObject audioManager = GameObject.Find("AudioManager");
        if (audioManager != null)
        {
            Debug.Log("Destroying AudioManager...");
            Destroy(audioManager);
        }
        else
        {
            Debug.Log("AudioManager not found, skipping destruction.");
        }

        // Small delay before scene transition
        yield return new WaitForSeconds(0.2f);
        
        Debug.Log("Loading IntroCutscene...");
        
        // Create a singleton fade manager that will handle the fade-in in the new scene
        CreateFadeManager();
        
        SceneManager.LoadScene("IntroCutscene");
    }
    
    private void CreateFadeManager()
    {
        // Create a persistent game object that will handle the fade-in
        GameObject fadeManager = new GameObject("FadeManager");
        FadeInManager fadeInScript = fadeManager.AddComponent<FadeInManager>();
        fadeInScript.fadeOverlay = fadeOverlay;
        DontDestroyOnLoad(fadeManager);
    }

    private IEnumerator GraduallyQuietAudio()
    {
        if (mainAudioMixer == null)
        {
            Debug.LogWarning("Main audio mixer is not assigned!");
            yield break;
        }

        Debug.Log("Starting audio fade...");

        // Get current volume to start the transition smoothly
        bool hasParameter = mainAudioMixer.GetFloat(masterVolumeParameter, out float currentVolumeDB);
        if (!hasParameter)
        {
            Debug.LogError($"Audio mixer parameter '{masterVolumeParameter}' not found!");
            yield break;
        }

        float startVolumeDB = currentVolumeDB;
        Debug.Log($"Starting audio fade from {startVolumeDB}dB to {minVolumeDB}dB over {audioFadeDuration} seconds");

        float elapsedTime = 0f;

        while (elapsedTime < audioFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / audioFadeDuration;
            
            // Smooth fade from current volume to minimum
            currentVolumeDB = Mathf.Lerp(startVolumeDB, minVolumeDB, progress);
            mainAudioMixer.SetFloat(masterVolumeParameter, currentVolumeDB);
            
            yield return null;
        }

        // Ensure final value is set exactly
        mainAudioMixer.SetFloat(masterVolumeParameter, minVolumeDB);
        Debug.Log("Audio fade completed.");
    }

    private void CreateFadeOverlay()
    {
        // Create a canvas for the fade overlay
        GameObject canvasGO = new GameObject("FadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Ensure it renders on top of everything
        
        CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create the fade overlay image
        fadeOverlay = new GameObject("FadeOverlay");
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
        
        // Make sure the canvas persists through scene changes temporarily
        DontDestroyOnLoad(canvasGO);
    }

    public void QuitGame()
    {
        Debug.Log("Quit button was pressed! The game would close now.");

        Application.Quit();
    }

    private bool HasPassedFirstCheckpoint()
    {
        // Check if persistence system has any saved checkpoint data
        string saveDataPrefix = "EchoNomads_";
        
        // Check if there's any saved active checkpoint
        bool hasActiveCheckpoint = PlayerPrefs.HasKey($"{saveDataPrefix}ActiveCheckpoint") && 
                                  !string.IsNullOrEmpty(PlayerPrefs.GetString($"{saveDataPrefix}ActiveCheckpoint", ""));
        
        // Check if there's any saved last activated checkpoint
        bool hasLastActivatedCheckpoint = PlayerPrefs.HasKey($"{saveDataPrefix}LastActivatedCheckpoint") && 
                                         !string.IsNullOrEmpty(PlayerPrefs.GetString($"{saveDataPrefix}LastActivatedCheckpoint", ""));
        
        // Check if there are any discovered checkpoints
        bool hasDiscoveredCheckpoints = PlayerPrefs.HasKey($"{saveDataPrefix}DiscoveredCheckpoints") && 
                                       !string.IsNullOrEmpty(PlayerPrefs.GetString($"{saveDataPrefix}DiscoveredCheckpoints", ""));
        
        // Check if there's a saved player position indicating progress
        bool hasSavedPosition = PlayerPrefs.HasKey($"{saveDataPrefix}Player_SaveTime");
        
        // If any of these conditions are true, the player has made progress
        bool hasProgress = hasActiveCheckpoint || hasLastActivatedCheckpoint || hasDiscoveredCheckpoints || hasSavedPosition;
        
        if (hasProgress)
        {
            Debug.Log("MainMenuController: Player progress detected - skipping intro cutscene");
            
            // Log details for debugging
            if (hasActiveCheckpoint)
            {
                string activeCheckpoint = PlayerPrefs.GetString($"{saveDataPrefix}ActiveCheckpoint", "");
                string activeScene = PlayerPrefs.GetString($"{saveDataPrefix}ActiveCheckpointScene", "");
                Debug.Log($"Active checkpoint found: {activeCheckpoint} in scene {activeScene}");
            }
            
            if (hasSavedPosition)
            {
                string savedScene = PlayerPrefs.GetString($"{saveDataPrefix}Player_Scene", "");
                Debug.Log($"Saved player position found in scene: {savedScene}");
            }
        }
        else
        {
            Debug.Log("MainMenuController: No player progress detected - showing intro cutscene");
        }
        
        return hasProgress;
    }

    private string DetermineGameScene()
    {
        string saveDataPrefix = "EchoNomads_";
        
        // First priority: Check if there's a saved player position with scene info
        if (PlayerPrefs.HasKey($"{saveDataPrefix}Player_Scene"))
        {
            string savedScene = PlayerPrefs.GetString($"{saveDataPrefix}Player_Scene", "");
            if (!string.IsNullOrEmpty(savedScene))
            {
                Debug.Log($"MainMenuController: Loading saved scene: {savedScene}");
                return savedScene;
            }
        }
        
        // Second priority: Check if there's an active checkpoint with scene info
        if (PlayerPrefs.HasKey($"{saveDataPrefix}ActiveCheckpointScene"))
        {
            string checkpointScene = PlayerPrefs.GetString($"{saveDataPrefix}ActiveCheckpointScene", "");
            if (!string.IsNullOrEmpty(checkpointScene))
            {
                Debug.Log($"MainMenuController: Loading active checkpoint scene: {checkpointScene}");
                return checkpointScene;
            }
        }
        
        // Default fallback: Load the first game scene (Desert scene)
        string defaultScene = "Desert scene";
        Debug.Log($"MainMenuController: No specific scene found, loading default: {defaultScene}");
        return defaultScene;
    }

    private IEnumerator FadeToGameScene()
    {
        string targetScene = DetermineGameScene();
        Debug.Log($"Starting fade to game scene: {targetScene}");
        
        // Show "Continuing where you left off..." message if UI is available
        if (continuingGameText != null)
        {
            continuingGameText.SetActive(true);
            yield return new WaitForSeconds(1.5f); // Show message for 1.5 seconds
        }
        
        // Start fading out audio (only if mixer is assigned)
        if (mainAudioMixer != null)
        {
            StartCoroutine(GraduallyQuietAudio());
        }
        else
        {
            Debug.LogWarning("Audio mixer not assigned - skipping audio fade. Please assign the AudioMixer in the inspector for audio fade effect.");
        }

        // Create a fade overlay that covers the entire screen
        CreateFadeOverlay();

        Debug.Log("Starting visual fade to black...");
        // Fade to black
        float elapsedTime = 0f;
        Image fadeImage = fadeOverlay.GetComponent<Image>();
        Color color = fadeImage.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Ensure it's fully black
        color.a = 1f;
        fadeImage.color = color;
        Debug.Log("Visual fade completed.");

        // Wait for the longer of the two fades to complete (only if audio mixer is assigned)
        if (mainAudioMixer != null)
        {
            float maxFadeDuration = Mathf.Max(fadeDuration, audioFadeDuration);
            float waitTime = maxFadeDuration - fadeDuration; // How much longer to wait after visual fade
            if (waitTime > 0)
            {
                Debug.Log($"Waiting additional {waitTime} seconds for audio fade to complete...");
                yield return new WaitForSeconds(waitTime);
            }
        }

        // Destroy audio manager after audio has been faded out
        GameObject audioManager = GameObject.Find("AudioManager");
        if (audioManager != null)
        {
            Debug.Log("Destroying AudioManager...");
            Destroy(audioManager);
        }
        else
        {
            Debug.Log("AudioManager not found, skipping destruction.");
        }

        // Small delay before scene transition
        yield return new WaitForSeconds(0.2f);
        
        Debug.Log($"Loading {targetScene}...");
        
        // Create a singleton fade manager that will handle the fade-in in the new scene
        CreateFadeManager();
        
        SceneManager.LoadScene(targetScene);
    }

    // Method to clear all game progress - useful for testing or manual reset
    public void ClearGameProgress()
    {
        Debug.Log("MainMenuController: Clearing all game progress...");
        
        string saveDataPrefix = "EchoNomads_";
        
        // Clear checkpoint data
        PlayerPrefs.DeleteKey($"{saveDataPrefix}ActiveCheckpoint");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}ActiveCheckpointScene");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}LastActivatedCheckpoint");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}LastActivatedCheckpointScene");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}DiscoveredCheckpoints");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}ActivatedCheckpoints");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}UseNearestAsActive");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}SaveTime");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}PendingActiveCheckpoint");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}PendingLastActivatedCheckpoint");
        
        // Clear player position data
        PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_PosX");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_PosY");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_PosZ");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_RotX");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_RotY");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_RotZ");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_Scene");
        PlayerPrefs.DeleteKey($"{saveDataPrefix}Player_SaveTime");
        
        PlayerPrefs.Save();
        
        Debug.Log("MainMenuController: All game progress cleared. Next game start will show intro cutscene.");
    }

    // Debug method to test checkpoint detection (can be called from inspector or debug menu)
    [ContextMenu("Test Checkpoint Detection")]
    public void TestCheckpointDetection()
    {
        bool hasProgress = HasPassedFirstCheckpoint();
        string targetScene = DetermineGameScene();
        
        Debug.Log($"=== CHECKPOINT DETECTION TEST ===");
        Debug.Log($"Has player progress: {hasProgress}");
        Debug.Log($"Target scene: {targetScene}");
        Debug.Log($"Would skip intro: {hasProgress}");
        Debug.Log($"================================");
    }
}

// Separate component to handle fade-in after scene transition
public class FadeInManager : MonoBehaviour
{
    public GameObject fadeOverlay;
    
    void Start()
    {
        StartCoroutine(HandleFadeIn());
    }
    
    private IEnumerator HandleFadeIn()
    {
        // Wait a moment for the scene to fully initialize
        yield return new WaitForSeconds(0.5f);
        
        // Fade back from black to transparent over 1 second
        if (fadeOverlay != null)
        {
            Image fadeImage = fadeOverlay.GetComponent<Image>();
            Color color = fadeImage.color;
            
            float fadeBackDuration = 1.0f;
            float elapsedTime = 0f;
            
            Debug.Log("Fading back from black...");
            while (elapsedTime < fadeBackDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeBackDuration);
                fadeImage.color = color;
                yield return null;
            }
            
            // Ensure it's fully transparent
            color.a = 0f;
            fadeImage.color = color;
            
            // Destroy the fade overlay and this manager
            Debug.Log("Destroying fade overlay and fade manager");
            Destroy(fadeOverlay.transform.parent.gameObject);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Fade overlay not found, destroying fade manager");
            Destroy(gameObject);
        }
    }
}