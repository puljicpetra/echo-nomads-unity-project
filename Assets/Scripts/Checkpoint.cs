using UnityEngine;
using Invector.vCharacterController;

[System.Serializable]
public class CheckpointData
{
    public Vector3 position;
    public Quaternion rotation;
    public string sceneName;
    public string checkpointId;
    public float timestamp;

    public CheckpointData(Vector3 pos, Quaternion rot, string scene, string id)
    {
        position = pos;
        rotation = rot;
        sceneName = scene;
        checkpointId = id;
        timestamp = Time.time;
    }
}

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private string checkpointId;
    [SerializeField] private bool isStartingCheckpoint = false;
    [SerializeField] private float activationRadius = 2f;
    [SerializeField] private LayerMask playerLayer = 1 << 0; // Default layer
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject activeIndicator;
    [SerializeField] private GameObject inactiveIndicator;
    [SerializeField] private ParticleSystem activationEffect;
    
    [Header("Audio")]
    [SerializeField] private string activationSoundName = "CheckpointActivated";    private bool isActivated = false;
    private bool hasBeenTriggered = false;
    private bool isDiscovered = false; // For persistence - separate from hasBeenTriggered

    public string CheckpointId => checkpointId;
    public bool IsActivated => isActivated;
    public bool IsStartingCheckpoint => isStartingCheckpoint;
    public bool IsDiscovered => isDiscovered;

    private void Start()
    {
        // Generate unique ID if not set
        if (string.IsNullOrEmpty(checkpointId))
        {
            checkpointId = $"checkpoint_{transform.GetInstanceID()}";
        }

        // Register with CheckpointManager
        CheckpointManager.Instance?.RegisterCheckpoint(this);

        // Set up initial visual state
        UpdateVisualState();
        
        // If this is a starting checkpoint, activate it immediately
        if (isStartingCheckpoint && !hasBeenTriggered)
        {
            ActivateCheckpoint(silent: true);
        }
    }

    private void Update()
    {
        if (!hasBeenTriggered)
        {
            CheckForPlayerActivation();
        }
    }

    private void CheckForPlayerActivation()
    {
        // Check for player within activation radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, activationRadius, playerLayer);
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                ActivateCheckpoint();
                break;
            }
        }
    }    public void ActivateCheckpoint(bool silent = false)
    {
        if (hasBeenTriggered) return;

        hasBeenTriggered = true;
        isActivated = true;
        isDiscovered = true; // Mark as discovered when activated

        // Save this checkpoint
        CheckpointManager.Instance?.SaveCheckpoint(this);

        // Visual feedback
        UpdateVisualState();
        
        // Particle effect
        if (activationEffect != null)
        {
            activationEffect.Play();
        }

        // Audio feedback
        if (!silent && AudioManager.Instance != null && !string.IsNullOrEmpty(activationSoundName))
        {
            AudioManager.Instance.Play(activationSoundName);
        }

        Debug.Log($"Checkpoint '{checkpointId}' activated!");
    }

    public void SetAsCurrentCheckpoint()
    {
        isActivated = true;
        UpdateVisualState();
    }

    public void DeactivateAsCurrentCheckpoint()
    {
        // Keep it triggered but not the current active one
        // This allows visual distinction between triggered and current checkpoints
        UpdateVisualState();
    }    public void MarkAsDiscovered()
    {
        // Mark as discovered without playing effects (used in nearest-active mode or loading from save)
        isDiscovered = true;
        hasBeenTriggered = true;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (activeIndicator != null)
        {
            activeIndicator.SetActive(isActivated);
        }
        
        if (inactiveIndicator != null)
        {
            inactiveIndicator.SetActive(!isActivated);
        }
    }

    public CheckpointData GetCheckpointData()
    {
        return new CheckpointData(
            transform.position,
            transform.rotation,
            gameObject.scene.name,
            checkpointId
        );
    }

    // Method to respawn player at this checkpoint
    public void RespawnPlayerHere()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            // Disable player controller temporarily to prevent issues during teleportation
            var playerController = playerObj.GetComponent<vThirdPersonController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // Disable rigidbody temporarily
            var playerRigidbody = playerObj.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = true;
            }

            // Teleport player
            Vector3 spawnPosition = transform.position + Vector3.up * 0.5f; // Slight offset above checkpoint
            playerObj.transform.position = spawnPosition;
            playerObj.transform.rotation = transform.rotation;

            // Re-enable components after a small delay
            StartCoroutine(ReenablePlayerComponents(playerController, playerRigidbody));
        }
    }

    private System.Collections.IEnumerator ReenablePlayerComponents(
        vThirdPersonController controller, 
        Rigidbody rb)
    {
        yield return new WaitForFixedUpdate(); // Wait one physics frame
        
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero; // Clear any velocity
            rb.angularVelocity = Vector3.zero;
        }
        
        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw activation radius
        Gizmos.color = isActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
        
        // Draw checkpoint marker
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
    }

    private void OnDestroy()
    {
        // Unregister from CheckpointManager
        CheckpointManager.Instance?.UnregisterCheckpoint(this);
    }
}
