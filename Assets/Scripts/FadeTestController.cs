using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Simple test script to verify fade-to-black functionality.
/// Attach this to any GameObject to test if the fade system is working.
/// Press Space to trigger a test fade.
/// </summary>
public class FadeTestController : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private float testFadeDuration = 2.0f;
    [SerializeField] private KeyCode testKey = KeyCode.Space;
    
    private bool isTesting = false;
    
    void Update()
    {
        if (Input.GetKeyDown(testKey) && !isTesting)
        {
            Debug.Log("FadeTestController: Starting test fade");
            StartCoroutine(TestFade());
        }
    }
    
    private IEnumerator TestFade()
    {
        isTesting = true;
        
        // Create a test fade canvas
        GameObject canvasGO = new GameObject("TestFadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000; // Very high to ensure it's on top
        
        CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create the fade overlay image
        GameObject fadeOverlay = new GameObject("TestFadeOverlay");
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
        
        Debug.Log("FadeTestController: Starting fade to black");
        
        // Fade to black
        float timer = 0f;
        Color color = fadeImage.color;
        
        while (timer < testFadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Clamp01(timer / testFadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        // Ensure it's fully black
        color.a = 1f;
        fadeImage.color = color;
        
        Debug.Log("FadeTestController: Fade to black completed. Waiting 2 seconds...");
        yield return new WaitForSeconds(2.0f);
        
        Debug.Log("FadeTestController: Fading back to transparent");
        
        // Fade back to transparent
        timer = 0f;
        while (timer < testFadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / testFadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        // Clean up
        color.a = 0f;
        fadeImage.color = color;
        Destroy(canvasGO);
        
        Debug.Log("FadeTestController: Test completed");
        isTesting = false;
    }
}
