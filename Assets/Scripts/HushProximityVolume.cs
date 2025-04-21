using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic; // Needed for List
using System.Linq; // Needed for Linq functions like OrderBy
using System.Collections; // Needed for Coroutine

public class HushProximityVolume : MonoBehaviour
{
    // No specific hushTarget needed here anymore
    public AudioMixer audioMixer;
    public string volumeParameterName = "BackgroundVolume";
    public string hushTag = "Hush"; // Tag for identifying Hush objects

    public float maxEffectDistance = 20f;
    public float minEffectDistance = 5f;
    public float minVolumeDB = -40f;
    public float maxVolumeDB = 0f;

    private float checkRadius = 30f; // How far around the player to check for Hush objects (should be >= maxEffectDistance)
    private List<Transform> nearbyHushObjects = new List<Transform>(); // Reusable list
    private Coroutine volumeResetCoroutine; // To track the gradual volume reset coroutine

    void Update()
    {
        if (audioMixer == null) return;

        // Find the *closest* active Hush object within checkRadius
        Transform closestHush = FindClosestHush();

        float targetVolumeDB = maxVolumeDB; // Default to normal volume

        if (closestHush != null)
        {
            float distance = Vector3.Distance(transform.position, closestHush.position);

            // Calculate volume based on distance to the closest Hush
            if (distance <= minEffectDistance)
            {
                targetVolumeDB = minVolumeDB;
            }
            else if (distance < maxEffectDistance) // Note: less than, not >=
            {
                float lerpFactor = Mathf.InverseLerp(maxEffectDistance, minEffectDistance, distance);
                targetVolumeDB = Mathf.Lerp(maxVolumeDB, minVolumeDB, lerpFactor);
            }

            // Stop any ongoing volume reset coroutine if a Hush is found
            if (volumeResetCoroutine != null)
            {
                StopCoroutine(volumeResetCoroutine);
                volumeResetCoroutine = null;
            }
        }
        else
        {
            // If no Hush is found, start gradually increasing the volume
            if (volumeResetCoroutine == null)
            {
                volumeResetCoroutine = StartCoroutine(GraduallyIncreaseVolume());
            }
        }

        // Set the exposed parameter on the Audio Mixer
        audioMixer.SetFloat(volumeParameterName, targetVolumeDB);
    }

    private IEnumerator GraduallyIncreaseVolume()
    {
        float currentVolumeDB;
        audioMixer.GetFloat(volumeParameterName, out currentVolumeDB);

        while (currentVolumeDB < maxVolumeDB)
        {
            currentVolumeDB = Mathf.MoveTowards(currentVolumeDB, maxVolumeDB, Time.deltaTime * 10f); // Adjust speed as needed
            audioMixer.SetFloat(volumeParameterName, currentVolumeDB);
            yield return null;
        }

        volumeResetCoroutine = null; // Reset the coroutine reference when done
    }

    Transform FindClosestHush()
    {
        // Simple overlap sphere check (can be optimized if needed)
        nearbyHushObjects.Clear(); // Clear list before reuse
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, checkRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(hushTag))
            {
                // Check if the Hush object is active in the hierarchy
                if (hitCollider.gameObject.activeInHierarchy)
                {
                    nearbyHushObjects.Add(hitCollider.transform);
                }
            }
        }

        // Find the closest transform from the list
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (Transform hushTransform in nearbyHushObjects)
        {
            float dist = Vector3.Distance(transform.position, hushTransform.position);
            if (dist < minDist)
            {
                closest = hushTransform;
                minDist = dist;
            }
        }
        return closest;

        // Alternative using Linq (potentially less performant if called every frame with many objects):
        // return nearbyHushObjects
        //       .OrderBy(t => Vector3.Distance(transform.position, t.position))
        //       .FirstOrDefault();
    }

    // Optional: Reset volume when script is disabled or destroyed
    void OnDisable()
    {
        if (audioMixer != null)
        {
            // Use clearFloat to be safer if other things might set this param
            // audioMixer.ClearFloat(volumeParameterName);
            // Or just set back to default
            audioMixer.SetFloat(volumeParameterName, maxVolumeDB);
        }
    }

    void OnDestroy()
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat(volumeParameterName, maxVolumeDB);
        }

        // Ensure the volume is set to the max value at the end
        audioMixer.SetFloat(volumeParameterName, maxVolumeDB);
    }
}