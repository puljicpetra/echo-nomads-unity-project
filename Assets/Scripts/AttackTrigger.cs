using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AttackTrigger : MonoBehaviour
{
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction attackAction;
    private bool isAttacking = false; // Track if attack animation is in progress

    [SerializeField]
    private string attackSoundName = "PlayerAttack"; // Name of the attack sound in AudioManager

    [SerializeField]
    private float attackRadius = 50f; // Radius within which Hush objects will be destroyed

    [SerializeField]
    private float additionalDelay = 1f; // Additional delay before deactivating Hush objects

    void Awake()
    {
        // Get the animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing on this GameObject.");
        }

        // Get the PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component is missing. Please add it to this GameObject.");
            return;
        }

        // Get the attack action from the Player action map
        attackAction = playerInput.actions.FindActionMap("Player").FindAction("Attack");
        if (attackAction == null)
        {
            Debug.LogError("Attack action not found in Player action map.");
            return;
        }

        // Subscribe to the attack action
        attackAction.performed += OnAttackPerformed;

        // Ensure AudioManager exists in the scene
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager instance not found in the scene!");
        }
    }

    void OnEnable()
    {
        // Enable the attack action when this component is enabled
        attackAction?.Enable();
    }

    void OnDisable()
    {
        // Disable the attack action when this component is disabled
        if (attackAction != null)
        {
            attackAction.performed -= OnAttackPerformed;
            attackAction.Disable();
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        // Ignore input if attack animation is in progress or attack sound is playing
        if (isAttacking || (AudioManager.Instance != null && AudioManager.Instance.IsPlaying(attackSoundName)))
        {
            Debug.Log("Attack input ignored: attack animation or attack sound is still in progress.");
            return;
        }

        // Check InputMagnitude parameter before proceeding
        if (animator != null)
        {
            float inputMagnitude = animator.GetFloat("InputMagnitude");
            if (inputMagnitude > 0.5f)
            {
                // Do nothing if InputMagnitude is greater than 0.5
                return;
            }
            animator.SetTrigger("AttackTrigger");
            Debug.Log("Attack triggered!");
        }

        isAttacking = true;
        // Option 1: Use animation event to call EndAttackAnimation() at the end of the animation
        // Option 2: Use a coroutine with a fixed duration (fallback if no animation event)
        StartCoroutine(ResetAttackFlagCoroutine());

        // Play the attack sound using AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(attackSoundName);
        }

        // Start the coroutine to handle delayed deactivation
        StartCoroutine(DeactivateHushObjectsWithDelay());
    }

    // Call this from an animation event at the end of the attack animation if possible
    public void EndAttackAnimation()
    {
        isAttacking = false;
        Debug.Log("Attack animation ended, ready for next attack.");
    }

    private IEnumerator ResetAttackFlagCoroutine()
    {
        // Fallback: Wait for the length of the attack animation (adjust as needed)
        float attackAnimLength = 1.0f;
        if (animator != null)
        {
            // Try to get the length of the current attack animation state
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Attack")) // Replace "Attack" with your actual attack state name
            {
                attackAnimLength = stateInfo.length;
            }
        }
        yield return new WaitForSeconds(attackAnimLength);
        isAttacking = false;
        Debug.Log("Attack animation ended (coroutine), ready for next attack.");
    }

    private IEnumerator DeactivateHushObjectsWithDelay()
    {
        // Wait for 1 second (or any desired delay)
        yield return new WaitForSeconds(2f);

        // Deactivate all objects with the "Hush" tag within the attack radius
        GameObject[] hushObjects = GameObject.FindGameObjectsWithTag("Hush");
        foreach (GameObject hushObject in hushObjects)
        {
            if (Vector3.Distance(transform.position, hushObject.transform.position) <= attackRadius)
            {
                // Play the vanish sound
                AudioManager.Instance.Play("hushVanish");

                // Directly access and manipulate the DustExplosion and HeatDistortion particle systems
                ParticleSystem dustExplosion = hushObject.transform.Find("DustExplosion")?.GetComponent<ParticleSystem>();
                ParticleSystem heatDistortion = hushObject.transform.Find("HeatDistortion")?.GetComponent<ParticleSystem>();

                if (dustExplosion != null)
                {
                    dustExplosion.Play();
                }

                if (heatDistortion != null)
                {
                    // Stop the particle system instead of trying to use SetActive
                    heatDistortion.Stop();
                    heatDistortion.Clear();
                }

                // Wait for the additional delay
                yield return new WaitForSeconds(additionalDelay);

                // Deactivate the Hush object
                hushObject.SetActive(false);
            }
        }
    }
}