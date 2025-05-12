using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSwimmingSound : MonoBehaviour
{
    [Tooltip("Audio isjecak za plivanje.")]
    public AudioClip swimmingSoundClip;

    [Tooltip("Tag objekata koji predstavljaju vodu.")]
    public string waterTag = "Water";

    private AudioSource audioSource;
    private bool isSwimming = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (swimmingSoundClip == null)
        {
            Debug.LogError("Nije dodijeljen 'Swimming Sound Clip' na PlayerSwimmingSound skripti!", this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(waterTag) && !isSwimming)
        {
            isSwimming = true;
            PlaySwimmingSound();
            Debug.Log("Player entered water.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(waterTag) && isSwimming)
        {
            isSwimming = false;
            StopSwimmingSound();
            Debug.Log("Player exited water.");
        }
    }

    void PlaySwimmingSound()
    {
        if (swimmingSoundClip != null)
        {
            audioSource.clip = swimmingSoundClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void StopSwimmingSound()
    {
        if (audioSource.isPlaying && audioSource.clip == swimmingSoundClip)
        {
            audioSource.Stop();
        }
    }

    void OnDisable()
    {
        if (isSwimming)
        {
            StopSwimmingSound();
            isSwimming = false;
        }
    }
}