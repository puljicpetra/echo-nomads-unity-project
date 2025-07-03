using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinalCutsceneController : MonoBehaviour
{
    [Header("UI Elements")]
    public Image fadePanel;
    public TextMeshProUGUI finalDisplayText;

    [Header("Timing Settings")]
    public float fadeToBlackDuration = 3.0f;
    public float pauseInBlack = 2.0f;
    public float typingSpeed = 0.1f;
    public float delayBetweenLines = 3.0f;

    [Header("Audio (Optional)")]
    public AudioClip finalMelody;
    public AudioClip breathingSound;
    public AudioClip[] voiceoverClips;

    [Header("Studio Logo")]
    public TextMeshProUGUI studioLogoText;
    public float logoFadeInDuration = 2.0f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private AudioSource audioSource;
    private bool hasSequenceStarted = false;

    void Start()
    {
        if (debugMode) Debug.Log("FinalCutsceneController: Starting initialization");
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (finalDisplayText != null) finalDisplayText.gameObject.SetActive(false);
        if (studioLogoText != null) studioLogoText.gameObject.SetActive(false);

        if (debugMode)
        {
            Debug.Log($"FinalCutsceneController: fadePanel assigned: {fadePanel != null}");
            Debug.Log($"FinalCutsceneController: finalDisplayText assigned: {finalDisplayText != null}");
            Debug.Log($"FinalCutsceneController: studioLogoText assigned: {studioLogoText != null}");
        }

        // Check if there's a fade canvas from scene transition and clean it up
        StartCoroutine(HandleSceneTransitionAndStart());
    }

    private IEnumerator HandleSceneTransitionAndStart()
    {
        // Wait a moment for any existing fade overlays to settle
        yield return new WaitForSeconds(0.1f);
        
        // Look for transition fade canvases and handle them appropriately
        GameObject transitionFadeCanvas = GameObject.Find("TransitionFadeCanvas");
        GameObject fadeCanvas = GameObject.Find("FadeCanvas");
        
        bool foundTransitionCanvas = false;
        
        if (transitionFadeCanvas != null)
        {
            Debug.Log("Found transition fade canvas from temple scene");
            foundTransitionCanvas = true;
        }
        else if (fadeCanvas != null)
        {
            Debug.Log("Found general fade canvas from scene transition");
            foundTransitionCanvas = true;
        }
        
        // If we found a transition canvas, we're starting with a black screen
        // so we should skip our own initial fade to black
        if (foundTransitionCanvas)
        {
            Debug.Log("Starting with black screen from transition - will use existing fade");
            // Start the sequence but skip the initial fade to black
            StartEndSequenceWithoutInitialFade();
            
            // Clean up transition canvases after a delay
            yield return new WaitForSeconds(1.0f);
            if (transitionFadeCanvas != null) Destroy(transitionFadeCanvas);
            if (fadeCanvas != null) Destroy(fadeCanvas);
        }
        else
        {
            Debug.Log("No transition canvas found - starting normal sequence");
            // Start the normal sequence with fade to black
            StartEndSequence();
        }
    }

    public void StartEndSequence()
    {
        if (hasSequenceStarted) return;
        hasSequenceStarted = true;
        StartCoroutine(EndSequenceCoroutine());
    }

    private IEnumerator EndSequenceCoroutine()
    {
        yield return FadeToBlack();
        yield return ShowFinalText();

        if (finalMelody != null)
        {
            audioSource.PlayOneShot(finalMelody);
            yield return new WaitForSeconds(finalMelody.length);
        }
        else
        {
            yield return new WaitForSeconds(2.0f);
        }

        if (breathingSound != null)
        {
            audioSource.PlayOneShot(breathingSound);
            yield return new WaitForSeconds(breathingSound.length > 2.0f ? 2.0f : breathingSound.length);
        }

        yield return FadeInStudioLogo();

        Debug.Log("End sequence fully finished.");
    }

    public void StartEndSequenceWithoutInitialFade()
    {
        if (hasSequenceStarted) return;
        hasSequenceStarted = true;
        StartCoroutine(EndSequenceCoroutineWithoutInitialFade());
    }

    private IEnumerator EndSequenceCoroutineWithoutInitialFade()
    {
        // Skip the initial fade to black since we're already starting with black screen
        // Ensure our fadePanel is black to maintain consistency
        if (fadePanel != null) 
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = Color.black;
        }
        
        // Wait a moment for the black screen to settle
        yield return new WaitForSeconds(pauseInBlack);
        
        // Continue with the rest of the sequence
        yield return ShowFinalText();

        if (finalMelody != null)
        {
            audioSource.PlayOneShot(finalMelody);
            yield return new WaitForSeconds(finalMelody.length);
        }
        else
        {
            yield return new WaitForSeconds(2.0f);
        }

        if (breathingSound != null)
        {
            audioSource.PlayOneShot(breathingSound);
            yield return new WaitForSeconds(breathingSound.length > 2.0f ? 2.0f : breathingSound.length);
        }

        yield return FadeInStudioLogo();

        Debug.Log("End sequence fully finished.");
    }

    private IEnumerator FadeToBlack()
    {
        Debug.Log("Fading to black...");
        
        // If fadePanel is not assigned, create our own fade overlay
        if (fadePanel == null)
        {
            Debug.LogWarning("FadePanel not assigned, creating temporary fade overlay");
            yield return StartCoroutine(CreateTemporaryFadeOverlay());
        }
        else
        {
            // Use the assigned fade panel
            fadePanel.gameObject.SetActive(true);
            float timer = 0f;
            fadePanel.color = new Color(0, 0, 0, 0);

            while (timer < fadeToBlackDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / fadeToBlackDuration);
                fadePanel.color = new Color(0, 0, 0, progress);
                yield return null;
            }

            fadePanel.color = Color.black;
        }
        
        Debug.Log("Pausing in black...");
        yield return new WaitForSeconds(pauseInBlack);
    }
    
    private IEnumerator CreateTemporaryFadeOverlay()
    {
        // Create a canvas for the fade overlay
        GameObject canvasGO = new GameObject("CutsceneFadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9998; // Just below transition canvases
        
        CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create the fade overlay image
        GameObject fadeOverlay = new GameObject("CutsceneFadeOverlay");
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
        
        // Fade to black
        float timer = 0f;
        Color color = fadeImage.color;
        
        while (timer < fadeToBlackDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Clamp01(timer / fadeToBlackDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        // Ensure it's fully black
        color.a = 1f;
        fadeImage.color = color;
        
        // Store reference to the temporary fade panel for later use
        fadePanel = fadeImage;
        
        Debug.Log("Created temporary fade overlay for cutscene");
    }

    private IEnumerator ShowFinalText()
    {
        string[] finalLines = new string[]
        {
            "The sounds were never lost.",
            "They were simply... unheard.",
            "You are the final echo."
        };

        if (finalDisplayText == null)
        {
            Debug.LogWarning("Final Display Text is not assigned in the Inspector. Skipping text sequence.");
            yield break;
        }

        finalDisplayText.gameObject.SetActive(true);

        for (int i = 0; i < finalLines.Length; i++)
        {
            if (voiceoverClips.Length > i && voiceoverClips[i] != null)
            {
                audioSource.PlayOneShot(voiceoverClips[i]);
            }

            finalDisplayText.text = "";
            foreach (char letter in finalLines[i].ToCharArray())
            {
                finalDisplayText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(delayBetweenLines);
        }

        yield return new WaitForSeconds(delayBetweenLines / 2);
        finalDisplayText.text = "";
    }

    private IEnumerator FadeInStudioLogo()
    {
        if (studioLogoText == null)
        {
            Debug.LogWarning("Studio Logo Text is not assigned. Skipping logo fade-in.");
            yield break;
        }

        Debug.Log("Fading in studio logo...");
        studioLogoText.gameObject.SetActive(true);

        float timer = 0f;
        Color startColor = new Color(studioLogoText.color.r, studioLogoText.color.g, studioLogoText.color.b, 0);
        studioLogoText.color = startColor;

        while (timer < logoFadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / logoFadeInDuration);
            studioLogoText.color = new Color(startColor.r, startColor.g, startColor.b, progress);
            yield return null;
        }

        studioLogoText.color = new Color(startColor.r, startColor.g, startColor.b, 1);
    }
}