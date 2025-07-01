using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CutsceneTextController : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    public AudioSource audioSource;
    public AudioClip[] voiceoverClips;

    private string[] sentences = new string[]
    {
        "In an age before words, the world was a song.\nThe Echo Nomads listened to its soul.",
        "But a great Silence fell, consuming every voice.\nThe tribe was lost... but not all was forgotten.",
        "Today, a single sound breaks through the quiet.",
        "A call.",
        "Make the world sing again."
    };

    public float typingSpeed = 0.05f;

    void Start()
    {
        if (textDisplay == null)
        {
            Debug.LogError("Text Display (TextMeshProUGUI) is not assigned in the Inspector!");
            return;
        }
        if (audioSource == null)
        {
            Debug.LogError("Audio Source is not assigned in the Inspector!");
            return;
        }
        if (voiceoverClips.Length != sentences.Length)
        {
            Debug.LogWarning("WARNING: The number of sentences (" + sentences.Length + ") and the number of audio clips (" + voiceoverClips.Length + ") do not match!");
        }

        StartCoroutine(ShowSentences());
    }

    IEnumerator ShowSentences()
    {
        textDisplay.text = "";

        for (int i = 0; i < sentences.Length; i++)
        {
            string sentence = sentences[i];
            if (i < voiceoverClips.Length)
            {
                AudioClip clip = voiceoverClips[i];
                if (clip != null) { audioSource.PlayOneShot(clip); }

                textDisplay.text = sentence;
                textDisplay.maxVisibleCharacters = 0;
                while (textDisplay.maxVisibleCharacters < sentence.Length)
                {
                    textDisplay.maxVisibleCharacters++;
                    yield return new WaitForSeconds(typingSpeed);
                }

                if (clip != null)
                {
                    float waitTime = clip.length - (sentence.Length * typingSpeed) + 1.0f;
                    if (waitTime > 0) { yield return new WaitForSeconds(waitTime); }
                    else { yield return new WaitForSeconds(1.0f); }
                }
                else { yield return new WaitForSeconds(3.0f); }
            }
            textDisplay.text = "";
        }

        Debug.Log("Intro scene finished. Loading next level...");

        SceneManager.LoadScene("Desert scene");
    }
}