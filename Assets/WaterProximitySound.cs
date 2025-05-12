using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WaterProximitySound : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Transform igraca. Ako ostane prazno, trazit ce objekt s tagom 'Player'.")]
    public Transform playerTransform;

    [Tooltip("Audio isjecak koji ce svirati (zvuk vode).")]
    public AudioClip waterSoundClip;

    [Header("Sound Settings")]
    [Tooltip("Maksimalna udaljenost na kojoj se zvuk pocinje cuti.")]
    public float maxDistance = 20f;

    [Tooltip("Udaljenost na kojoj je zvuk najglasniji.")]
    public float minDistance = 1f;

    [Tooltip("Maksimalna glasnoca zvuka.")]
    [Range(0f, 1f)]
    public float maxVolume = 0.8f;

    private AudioSource audioSource;
    private bool playerFound = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
                playerFound = true;
                Debug.Log("Player object found via tag on: " + gameObject.name);
            }
            else
            {
                Debug.LogError("Player Transform nije dodijeljen i objekt s tagom 'Player' nije pronadjen! Zvuk nece raditi.", this);
                enabled = false;
                return;
            }
        }
        else
        {
            playerFound = true;
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;

        if (waterSoundClip != null)
        {
            audioSource.clip = waterSoundClip;
        }

        if (audioSource.clip == null)
        {
            Debug.LogError("AudioSource na objektu '" + gameObject.name + "' nema dodijeljen AudioClip!", this);
            enabled = false;
            return;
        }

        audioSource.Play();
    }

    void Update()
    {
        if (!playerFound || playerTransform == null)
        {
            if (audioSource.volume > 0) audioSource.volume = 0f;
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= maxDistance)
        {
            float volumeFactor = Mathf.InverseLerp(maxDistance, minDistance, distance);

            audioSource.volume = Mathf.Clamp01(volumeFactor) * maxVolume;
        }
        else
        {
            audioSource.volume = 0f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }
}