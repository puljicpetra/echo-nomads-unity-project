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

    private AudioSource audioSource;
    private bool hasSequenceStarted = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (finalDisplayText != null) finalDisplayText.gameObject.SetActive(false);
        if (studioLogoText != null) studioLogoText.gameObject.SetActive(false);

        StartEndSequence();
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

    private IEnumerator FadeToBlack()
    {
        Debug.Log("Fading to black...");
        float timer = 0f;
        if (fadePanel != null) fadePanel.color = new Color(0, 0, 0, 0);

        while (timer < fadeToBlackDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / fadeToBlackDuration);
            if (fadePanel != null) fadePanel.color = new Color(0, 0, 0, progress);
            yield return null;
        }

        if (fadePanel != null) fadePanel.color = Color.black;
        Debug.Log("Pausing in black...");
        yield return new WaitForSeconds(pauseInBlack);
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