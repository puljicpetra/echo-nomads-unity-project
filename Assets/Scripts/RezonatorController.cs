using UnityEngine;

public class RezonatorController : MonoBehaviour
{
    public AudioSource audioSource;
    public float maxVolume = 1.5f;

    void Update()
    {
        if (Input.GetMouseButton(1)) // Drži desnu tipku miša
        {
            if (!audioSource.isPlaying)
            {
                audioSource.volume = maxVolume; 
                audioSource.Play();
            }
        }
        else if (Input.GetMouseButtonUp(1)) // Pusti desnu tipku miša
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
