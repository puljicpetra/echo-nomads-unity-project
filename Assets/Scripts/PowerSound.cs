using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic; // Needed if you want to use LayerMask more efficiently, though not strictly required for GetComponent

// Make sure the SoundType enum is accessible, either in this file or another
// public enum SoundType { None, Sound1, Sound2, Sound3 }

public class PowerSound : MonoBehaviour
{
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction powerSound1Action;
    private InputAction powerSound2Action;
    private InputAction powerSound3Action;

    [Header("Sound Action Names (for AudioManager)")]
    [SerializeField] private string powerSound1Name = "PowerSound1";
    [SerializeField] private string powerSound2Name = "PowerSound2";
    [SerializeField] private string powerSound3Name = "PowerSound3";

    // --- New variables for Resonator Interaction ---
    [Header("Resonator Interaction")]
    [Tooltip("Radius around the player within which Resonators will be notified.")]
    [SerializeField] private float resonatorNotifyRadius = 6f;
    [Tooltip("Optional: Assign a LayerMask to only check for Resonators on specific layers for optimization.")]
    [SerializeField] private LayerMask resonatorLayer; // Assign in Inspector! Make sure Resonator prefabs are on this layer.

    // --- Removed attackRadius as it seemed unused in the original TriggerPowerAttack logic ---
    // [SerializeField] private float attackRadius = 50f; // Keep if needed for other attack logic

    [Header("Animation Settings")]
    [SerializeField] private float additionalDelay = 1f; // Keep if used elsewhere, otherwise remove if only for attack logic

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator component is missing on this GameObject.");

        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component is missing. Please add it to this GameObject.");
            enabled = false; // Disable script if core component is missing
            return;
        }

        var playerMap = playerInput.actions.FindActionMap("Player", throwIfNotFound: true);
        if (playerMap == null)
        {
            Debug.LogError("Action Map 'Player' not found!");
            enabled = false;
            return;
        }

        powerSound1Action = playerMap.FindAction("PowerSound1", throwIfNotFound: true);
        powerSound2Action = playerMap.FindAction("PowerSound2", throwIfNotFound: true);
        powerSound3Action = playerMap.FindAction("PowerSound3", throwIfNotFound: true);

        if (powerSound1Action == null || powerSound2Action == null || powerSound3Action == null)
        {
            Debug.LogError("One or more PowerSound actions not found in Player action map.");
            enabled = false;
            return;
        }

        // Subscribe during Awake or OnEnable
        powerSound1Action.performed += OnPowerSound1Performed;
        powerSound2Action.performed += OnPowerSound2Performed;
        powerSound3Action.performed += OnPowerSound3Performed;
    }

    void OnEnable()
    {
        // Enable actions if they were assigned
        powerSound1Action?.Enable();
        powerSound2Action?.Enable();
        powerSound3Action?.Enable();
    }

    void OnDisable()
    {
        // Unsubscribe and disable actions
        if (powerSound1Action != null)
        {
            powerSound1Action.performed -= OnPowerSound1Performed;
            powerSound1Action.Disable();
        }
        if (powerSound2Action != null)
        {
            powerSound2Action.performed -= OnPowerSound2Performed;
            powerSound2Action.Disable();
        }
        if (powerSound3Action != null)
        {
            powerSound3Action.performed -= OnPowerSound3Performed;
            powerSound3Action.Disable();
        }
    }

    // --- Input Action Callbacks ---

    private void OnPowerSound1Performed(InputAction.CallbackContext context)
    {
        // Pass the SoundType and the AudioManager sound name
        EmitAndNotifyResonators(SoundType.PowerSound1, powerSound1Name);
    }

    private void OnPowerSound2Performed(InputAction.CallbackContext context)
    {
        EmitAndNotifyResonators(SoundType.PowerSound2, powerSound2Name);
    }

    private void OnPowerSound3Performed(InputAction.CallbackContext context)
    {
        EmitAndNotifyResonators(SoundType.PowerSound3, powerSound3Name);
    }

    /// <summary>
    /// Central method to handle playing the sound, triggering animation,
    /// and notifying nearby resonators.
    /// </summary>
    /// <param name="emittedSoundType">The type of sound being emitted (for Resonators).</param>
    /// <param name="soundNameForManager">The name of the sound for the AudioManager.</param>
    private void EmitAndNotifyResonators(SoundType emittedSoundType, string soundNameForManager)
    {
        // --- 1. Animation Logic (Optional Check) ---
        if (animator != null)
        {
            // Check if character should be prevented from emitting sound while moving
            // float inputMagnitude = animator.GetFloat("InputMagnitude");
            // if (inputMagnitude > 0.5f)
            // {
            //     Debug.Log("Cannot emit power sound while moving significantly.");
            //     return; // Stop execution if moving
            // }
            // Always trigger animation if not moving or check is removed
            animator.SetTrigger("PowerSoundTrigger");
            Debug.Log($"Animation Triggered for {emittedSoundType}");
        }

        // --- 2. Play Sound via AudioManager ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(soundNameForManager);
            Debug.Log($"AudioManager playing: {soundNameForManager}");
        }
        else
        {
            Debug.LogWarning($"AudioManager instance not found when trying to play: {soundNameForManager}");
        }

        // --- 3. Notify Nearby Resonators ---
        // Find all colliders within the notification radius
        // Using the LayerMask makes this more efficient if Resonators are on their own layer
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, resonatorNotifyRadius, resonatorLayer, QueryTriggerInteraction.Collide); // QueryTriggerInteraction.Collide ensures we hit Trigger colliders

        int notifiedCount = 0;
        // Iterate through the colliders found
        foreach (var hitCollider in hitColliders)
        {
            // Try to get the Resonator component from the collider's GameObject
            Resonator resonator = hitCollider.GetComponent<Resonator>();
            if (resonator != null)
            {
                // If found, call the method to pass the player's input
                resonator.ReceivePlayerInput(emittedSoundType);
                notifiedCount++;
            }
        }

        if(notifiedCount > 0) {
             Debug.Log($"Notified {notifiedCount} resonators within radius {resonatorNotifyRadius} with sound {emittedSoundType}");
        } else {
            // Optional: Log if no resonators were found nearby
            // Debug.Log($"No resonators found nearby for sound {emittedSoundType}");
        }

        // --- 4. Optional: Other Attack Logic ---
        // If PowerSound also triggers some kind of 'attack' logic independent
        // of resonators, add it here.
        // Example: DealAreaDamage(transform.position, attackRadius, additionalDelay);
    }


    // --- Gizmo for visualizing the notification radius in the Editor ---
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, resonatorNotifyRadius);
    }
}