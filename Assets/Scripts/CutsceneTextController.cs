using System.Collections;
using UnityEngine;
using TMPro;

public class CutsceneTextController : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;

    private string[] sentences = new string[]
    {
        "In an age before words, the world was a song.\nThe Echo Nomads listened to its soul.",
        "But a great Silence fell, consuming every voice.\nThe tribe was lost... but not all was forgotten.",
        "Today, a single sound breaks through the quiet.",
        "A call.",
        "Make the world sing again."
    };

    public float displayTime = 4.0f;
    public float delayBetweenSentences = 1.5f;

    public float typingSpeed = 0.05f;

    void Start()
    {
        if (textDisplay == null)
        {
            Debug.LogError("Text Display nije postavljen u Inspector prozoru!");
            return;
        }

        StartCoroutine(ShowSentences());
    }

    IEnumerator ShowSentences()
    {
        textDisplay.text = "";

        foreach (string sentence in sentences)
        {
            textDisplay.text = sentence;
            textDisplay.maxVisibleCharacters = 0;

            while (textDisplay.maxVisibleCharacters < sentence.Length)
            {
                textDisplay.maxVisibleCharacters++;
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(displayTime);

            textDisplay.text = "";
            yield return new WaitForSeconds(delayBetweenSentences);
        }

        Debug.Log("End.");
    }
}