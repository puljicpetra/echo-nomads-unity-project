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
        // SceneManager.LoadScene("Desert scene");
    }
}