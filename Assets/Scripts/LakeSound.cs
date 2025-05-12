using UnityEngine;

public class LakeSound : MonoBehaviour
{
    public AudioSource waterAudioSource;
    private AudioSource[] allAudioSources;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Zatisni sve ostale zvukove
            allAudioSources = FindObjectsOfType<AudioSource>();

            foreach (AudioSource audio in allAudioSources)
            {
                if (audio != waterAudioSource && audio.isPlaying)
                {
                    audio.volume = 0f;
                }
            }

            // Pusti zvuk vode
            if (!waterAudioSource.isPlaying)
            {
                waterAudioSource.Play();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Vrati glasnoću ostalim zvukovima
            foreach (AudioSource audio in allAudioSources)
            {
                if (audio != waterAudioSource)
                {
                    audio.volume = 1f;
                }
            }

            // Zaustavi zvuk vode
            waterAudioSource.Stop();
        }
    }
}
