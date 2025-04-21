using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AttackTrigger : MonoBehaviour
{
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction attackAction;

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

        // Play the attack sound using AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(attackSoundName);
        }

        // Start the coroutine to handle delayed deactivation
        StartCoroutine(DeactivateHushObjectsWithDelay());
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