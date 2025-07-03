using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CutsceneLoadingController : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;

    void Start()
    {
        if (textDisplay == null)
        {
            Debug.LogError("Text Display (TextMeshProUGUI) is not assigned in the Inspector!");
            return;
        }

        textDisplay.text = "Loading...";
        
        // Get the previous scene name
        string previousScene = PlayerPrefs.GetString("PreviousScene", "");
        Debug.Log("Previous scene from PlayerPrefs: '" + previousScene + "'");
        
        // If no previous scene is set, try to determine it from current context
        if (string.IsNullOrEmpty(previousScene))
        {
            previousScene = DeterminePreviousScene();
            Debug.Log("Determined previous scene: '" + previousScene + "'");
        }
        
        string nextScene = GetNextScene(previousScene);
        
        if (!string.IsNullOrEmpty(nextScene))
        {
            Debug.Log("Loading next scene: " + nextScene);
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.LogWarning("No next scene found for previous scene: '" + previousScene + "'");
            // Fallback to Desert scene if nothing else works
            Debug.Log("Falling back to Desert scene");
            SceneManager.LoadScene("Desert scene");
        }
    }

    private string DeterminePreviousScene()
    {
        // Check if player has made progress in any scenes
        // This helps determine where they should go next
        
        // Check for player progress in various scenes
        string playerScene = PlayerPrefs.GetString("EchoNomads_Player_Scene", "");
        if (!string.IsNullOrEmpty(playerScene))
        {
            Debug.Log("Found player scene: " + playerScene);
            return playerScene;
        }
        
        // Check for checkpoint data that might indicate progress
        string activeCheckpointScene = PlayerPrefs.GetString("EchoNomads_ActiveCheckpointScene", "");
        if (!string.IsNullOrEmpty(activeCheckpointScene))
        {
            Debug.Log("Found active checkpoint scene: " + activeCheckpointScene);
            return activeCheckpointScene;
        }
        
        // Default to starting scene progression
        Debug.Log("No previous scene found, defaulting to intro progression");
        return "IntroCutscene";
    }

    private string GetNextScene(string previousScene)
    {
        // Trim whitespace and make case-insensitive comparison
        string cleanPreviousScene = previousScene?.Trim().ToLowerInvariant() ?? "";
        
        switch (cleanPreviousScene)
        {
            case "introcutscene":
            case "intro cutscene":
                return "Desert scene";
            case "desert scene":
                return "Night Desert scene";
            case "night desert scene":
                return "Temple Path scene";
            case "temple path scene":
                return "FinalCutscene"; // or whatever the next scene should be
            default:
                Debug.LogWarning("Unknown previous scene: '" + previousScene + "' (cleaned: '" + cleanPreviousScene + "')");
                return "";
        }
    }
}