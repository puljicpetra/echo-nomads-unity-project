using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic; // Needed if you want to use LayerMask more efficiently, though not strictly required for GetComponent
using UnityEngine.Audio; // Needed for AudioMixer
using System.Collections; // Needed for Coroutine

// Make sure the SoundType enum is accessible, either in this file or another
// public enum SoundType { None, Sound1, Sound2, Sound3, Sound4 }

public class PowerSound : MonoBehaviour
{
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction powerSound1Action;
    private InputAction powerSound2Action;
    private InputAction powerSound3Action;
    private InputAction powerSound4Action; // Added for PowerSound4
    private InputAction focusAction;

    [Header("Sound Action Names (for AudioManager)")]
    [SerializeField] private string powerSound1Name = "PowerSound1";
    [SerializeField] private string powerSound2Name = "PowerSound2";
    [SerializeField] private string powerSound3Name = "PowerSound3";
    [SerializeField] private string powerSound4Name = "PowerSound4"; // Added for PowerSound4

    [Header("Focus Settings")]
    [SerializeField] private string focusActionName = "Focus";
    [SerializeField] private float hushCheckRadius = 5f;
    [SerializeField] private string hushTag = "Hush";

    [Header("Resonator Interaction")]
    [Tooltip("Radius around the player within which Resonators will be notified.")]
    [SerializeField] private float resonatorNotifyRadius = 6f;
    [Tooltip("Optional: Assign a LayerMask to only check for Resonators on specific layers for optimization.")]
    [SerializeField] private LayerMask resonatorLayer; // Assign in Inspector! Make sure Resonator prefabs are on this layer.

    

    [Header("Focus Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string volumeParameterName = "BackgroundVolume"; // Match the exposed parameter name in your Audio Mixer
    [SerializeField] private float focusVolumeDB = -20f; // Target volume in dB when focusing
    [SerializeField] private float defaultVolumeDB = 0f; // Default volume in dB (used if no other script controls it)
    [SerializeField] private float volumeTransitionSpeed = 10f; // Speed to change volume (dB per second)

    private float volumeBeforeFocus; // Stores the volume level before focus started
    private Coroutine volumeChangeCoroutine; // Reference to the active volume changing coroutine

    private bool isPowerSoundPlaying = false; // Track if a PowerSound is playing

    // Public static property to indicate focus state
    public static bool IsFocusing { get; private set; } = false;

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
        powerSound4Action = playerMap.FindAction("PowerSound4", throwIfNotFound: true); // Added for PowerSound4
        focusAction = playerInput.actions.FindAction(focusActionName, false);
        if (focusAction == null)
        {
            Debug.LogError($"Focus action '{focusActionName}' not found in Player action map.");
            // Don't disable the whole script, maybe only focus audio is broken
        }

        if (audioMixer == null)
        {
            Debug.LogWarning("AudioMixer is not assigned in the Inspector. Focus audio effect will not work.");
        }

        if (powerSound1Action == null || powerSound2Action == null || powerSound3Action == null || powerSound4Action == null)
        {
            Debug.LogError("One or more PowerSound actions not found in Player action map.");
            enabled = false;
            return;
        }

        // Subscribe during Awake or OnEnable
        powerSound1Action.performed += OnPowerSound1Performed;
        powerSound2Action.performed += OnPowerSound2Performed;
        powerSound3Action.performed += OnPowerSound3Performed;
        powerSound4Action.performed += OnPowerSound4Performed; // Added for PowerSound4
        // Subscribe to the focus action's started and canceled events
        if (focusAction != null)
        {
            focusAction.started += OnFocusStarted;
            focusAction.canceled += OnFocusCanceled;
        }
    }

    void OnEnable()
    {
        // Enable actions if they were assigned
        powerSound1Action?.Enable();
        powerSound2Action?.Enable();
        powerSound3Action?.Enable();
        powerSound4Action?.Enable(); // Added for PowerSound4
        focusAction?.Enable();

        // Subscribe to the focus action's started and canceled events only if the action exists
        if (focusAction != null)
        {
            focusAction.started += OnFocusStarted;
            focusAction.canceled += OnFocusCanceled;
            focusAction.Enable(); // Enable the action here
        }
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
        if (powerSound4Action != null)
        {
            powerSound4Action.performed -= OnPowerSound4Performed; // Added for PowerSound4
            powerSound4Action.Disable();
        }
        if (focusAction != null)
        {
            // Unsubscribe from the focus action events
            focusAction.started -= OnFocusStarted;
            focusAction.canceled -= OnFocusCanceled;
            focusAction.Disable();

            // Ensure focus state is reset if disabled while focusing
            IsFocusing = false;

            // If disabled while focusing, attempt to restore volume
            if (volumeChangeCoroutine != null)
            {
                StopCoroutine(volumeChangeCoroutine);
                volumeChangeCoroutine = null;
                if (audioMixer != null)
                {
                    // Instantly restore the volume it had before focusing
                    audioMixer.SetFloat(volumeParameterName, volumeBeforeFocus);
                    Debug.Log($"PowerSound disabled during focus, restoring volume to {volumeBeforeFocus} dB.");
                }
            }
        }
    }

    // --- Input Action Callbacks ---

    private void OnPowerSound1Performed(InputAction.CallbackContext context)
    {
        TryEmitPowerSound(SoundType.PowerSound1, powerSound1Name);
    }

    private void OnPowerSound2Performed(InputAction.CallbackContext context)
    {
        TryEmitPowerSound(SoundType.PowerSound2, powerSound2Name);
    }

    private void OnPowerSound3Performed(InputAction.CallbackContext context)
    {
        TryEmitPowerSound(SoundType.PowerSound3, powerSound3Name);
    }

    private void OnPowerSound4Performed(InputAction.CallbackContext context) // Added for PowerSound4
    {
        TryEmitPowerSound(SoundType.PowerSound4, powerSound4Name);
    }

    // Helper method to check and emit sound if not already playing
    private void TryEmitPowerSound(SoundType soundType, string soundName)
    {
        if (isPowerSoundPlaying)
        {
            Debug.Log($"PowerSound input ignored: {soundType} is requested but another PowerSound is still playing.");
            return;
        }
        isPowerSoundPlaying = true;
        EmitAndNotifyResonators(soundType, soundName);
        StartCoroutine(PowerSoundPlayingCoroutine(soundName));
    }

    // Coroutine to reset isPowerSoundPlaying when the sound finishes
    private IEnumerator PowerSoundPlayingCoroutine(string soundName)
    {
        // Wait until the AudioManager reports the sound is no longer playing
        if (AudioManager.Instance != null)
        {
            while (AudioManager.Instance.IsPlaying(soundName))
            {
                yield return null;
            }
        }
        else
        {
            // Fallback: wait a short time if AudioManager is missing
            yield return new WaitForSeconds(0.5f);
        }
        isPowerSoundPlaying = false;
    }

    // Called when the Focus action starts (button pressed)
    private void OnFocusStarted(InputAction.CallbackContext context)
    {
        IsFocusing = true; // Set focus state

        if (animator != null)
        {
            animator.SetBool("IsListening", true);
            Debug.Log("Focus started, IsListening set to true.");
        }
        else
        {
            Debug.LogWarning("Animator component not found, cannot set IsListening.");
        }

        // --- Audio Mixer Volume Change ---
        if (audioMixer != null)
        {
            // Stop any previous volume change
            if (volumeChangeCoroutine != null)
            {
                StopCoroutine(volumeChangeCoroutine);
            }
            // Store the current volume before changing it
            if (!audioMixer.GetFloat(volumeParameterName, out volumeBeforeFocus))
            {
                // Fallback if parameter doesn't exist or isn't set
                volumeBeforeFocus = defaultVolumeDB;
                Debug.LogWarning($"Could not get current value for '{volumeParameterName}'. Using default {defaultVolumeDB} dB.");
            }
            Debug.Log($"Focus started. Stored volume: {volumeBeforeFocus} dB. Transitioning to {focusVolumeDB} dB.");
            // Start transitioning to the focus volume
            volumeChangeCoroutine = StartCoroutine(ChangeVolumeCoroutine(focusVolumeDB));
        }
    }

    // Called when the Focus action is canceled (button released)
    private void OnFocusCanceled(InputAction.CallbackContext context)
    {
        IsFocusing = false; // Reset focus state

        if (animator != null)
        {
            animator.SetBool("IsListening", false);
            Debug.Log("Focus canceled, IsListening set to false.");
        }
        // No warning needed here if animator is null, as the state should reset anyway

        // --- Audio Mixer Volume Change ---
        if (audioMixer != null)
        {
            // Stop any previous volume change (e.g., if focus started again quickly)
            if (volumeChangeCoroutine != null)
            {
                StopCoroutine(volumeChangeCoroutine);
            }
            Debug.Log($"Focus canceled. Transitioning back to {volumeBeforeFocus} dB.");
            // Start transitioning back to the volume level before focus started
            volumeChangeCoroutine = StartCoroutine(ChangeVolumeCoroutine(volumeBeforeFocus));
        }
    }

    /// <summary>
    /// Coroutine to gradually change the audio mixer volume parameter.
    /// </summary>
    /// <param name="targetVolumeDB">The target volume in decibels.</param>
    private IEnumerator ChangeVolumeCoroutine(float targetVolumeDB)
    {
        if (audioMixer == null) yield break; // Safety check

        float currentVolumeDB;
        if (!audioMixer.GetFloat(volumeParameterName, out currentVolumeDB))
        {
            Debug.LogError($"AudioMixer parameter '{volumeParameterName}' not found during coroutine start.");
            volumeChangeCoroutine = null;
            yield break; // Exit if parameter is invalid
        }

        // Continue looping as long as the current volume is not approximately equal to the target
        while (!Mathf.Approximately(currentVolumeDB, targetVolumeDB))
        {
            currentVolumeDB = Mathf.MoveTowards(currentVolumeDB, targetVolumeDB, volumeTransitionSpeed * Time.deltaTime);
            audioMixer.SetFloat(volumeParameterName, currentVolumeDB);
            yield return null; // Wait for the next frame
        }

        // Ensure the final value is set exactly
        audioMixer.SetFloat(volumeParameterName, targetVolumeDB);
        volumeChangeCoroutine = null; // Mark coroutine as finished
        Debug.Log($"Volume transition finished. Current volume: {targetVolumeDB} dB.");
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
                // Check for Hush objects near the resonator
                Collider[] hushNearby = Physics.OverlapSphere(resonator.transform.position, hushCheckRadius);
                bool hushFound = false;
                foreach (var hushCol in hushNearby)
                {
                    if (hushCol.CompareTag(hushTag) && hushCol.gameObject.activeInHierarchy)
                    {
                        hushFound = true;
                        break;
                    }
                }
                if (!hushFound)
                {
                    resonator.ReceivePlayerInput(emittedSoundType);
                    notifiedCount++;
                }
            }
        }

        if (notifiedCount > 0)
        {
            Debug.Log($"Notified {notifiedCount} resonators within radius {resonatorNotifyRadius} with sound {emittedSoundType}");
        }
        else
        {
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