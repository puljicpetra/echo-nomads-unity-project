using UnityEngine;
using Invector.vCharacterController; // Make sure you have this using statement

public class CheatCodeManager : MonoBehaviour
{
    // Singleton instance
    public static CheatCodeManager Instance { get; private set; }    // Add more cheats as needed
    private string[] cheatCodes = { "superspeed", "normalspeed", "reset", "clearsave", "autosolve", "fly" };
    private int maxCheatLength = 15; // Longest cheat code length ("normalspeed" is 15)
    private string inputBuffer = "";// Reference to the player controller
    private vThirdPersonController playerController;    private float normalSprintSpeed = 6f;
    private float cheatSprintSpeed = 60f;
    private bool superSpeedActive = false;    // Fly cheat variables
    [Header("Fly Mode Settings")]
    [SerializeField] private bool flyModeActive = false;
    [SerializeField] private float flySpeed = 30f;
    [SerializeField] private float flyAcceleration = 8f;
    [SerializeField] private float flyDrag = 5f;
    [SerializeField] private float flyRotationSpeed = 5f;
    private float originalGravityScale;
    private bool originalUseGravity;

    // Add references for grounding system
    private AudioManager audioManager;
    private Rigidbody playerRigidbody;
    private float originalExtraGravity;
    private float originalGroundMaxDistance;
    private float superSpeedExtraGravity = -50f; // Increased downward force
    private float superSpeedGroundMaxDistance = 0.1f; // Reduced tolerance for being grounded

    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeReferences();
    }

    void OnEnable()
    {
        // Re-initialize references when the scene changes
        InitializeReferences();
    }    void InitializeReferences()
    {
        // Find the player controller in the scene
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<vThirdPersonController>();
            playerRigidbody = playerObj.GetComponent<Rigidbody>();
              // Assign normal sprint speed and store original physics values
            if (playerController != null)
            {
                normalSprintSpeed = playerController.freeSpeed.sprintSpeed;
                originalExtraGravity = playerController.extraGravity;
                originalGroundMaxDistance = playerController.groundMaxDistance;
            }
            
            // Store original rigidbody settings for fly mode
            if (playerRigidbody != null)
            {
                originalUseGravity = playerRigidbody.useGravity;
            }
        }
        
        // Find the AudioManager in the scene
        audioManager = FindObjectOfType<AudioManager>();
        
        // Enhanced AudioManager debugging
        if (audioManager != null)
        {
            Debug.Log("CheatCodeManager: AudioManager found successfully");
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("CheatCodeManager: AudioManager.Instance is null but AudioManager component found");
            }
        }
        else
        {
            Debug.LogWarning("CheatCodeManager: AudioManager not found in scene");
        }
        
        // Ensure cursor is properly configured for gameplay
        if (GameInitializer.Instance != null)
        {
            GameInitializer.Instance.EnsureGameplayCursor();
        }
        else if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetGameplayCursorState();
        }
    }void Update()
    {
        foreach (char c in Input.inputString)
        {
            if (char.IsLetter(c))
            {
                inputBuffer += char.ToLower(c);
                if (inputBuffer.Length > maxCheatLength)
                    inputBuffer = inputBuffer.Substring(inputBuffer.Length - maxCheatLength);
                CheckCheatCodes();
            }
        }// Apply additional grounding force when super speed is active
        if (superSpeedActive && playerController != null && playerRigidbody != null)
        {
            ApplySuperSpeedGrounding();
        }

        // Handle fly mode controls
        if (flyModeActive && playerController != null && playerRigidbody != null)
        {
            HandleFlyMode();
        }
    }    void CheckCheatCodes()
    {
        if (inputBuffer.EndsWith("superspeed") && !superSpeedActive)
        {
            ActivateSuperSpeed();
        }
        else if (inputBuffer.EndsWith("normalspeed") && superSpeedActive)
        {
            ActivateNormalSpeed();
        }
        else if (inputBuffer.EndsWith("reset"))
        {
            ActivateResetCheat();
        }
        else if (inputBuffer.EndsWith("clearsave"))
        {
            ActivateClearSaveCheat();
        }        else if (inputBuffer.EndsWith("autosolve"))
        {
            ActivateAutoSolveCheat();
        }        else if (inputBuffer.EndsWith("fly"))
        {
            Debug.Log("Fly cheat detected in input buffer!");
            ToggleFlyMode();
        }
        // Add more cheats here
    }void ActivateSuperSpeed()
    {
        if (playerController != null)
        {
            // Set sprintSpeed to cheat value
            var speed = playerController.freeSpeed;
            speed.sprintSpeed = cheatSprintSpeed;
            playerController.freeSpeed = speed;
            
            // Apply enhanced grounding physics
            playerController.extraGravity = superSpeedExtraGravity;
            playerController.groundMaxDistance = superSpeedGroundMaxDistance;
            
            superSpeedActive = true;
            Debug.Log("Super Speed Cheat Activated!");

            // Play cheat activated sound
            if (audioManager != null)
            {
                audioManager.Play("CheatActivated");
            }
        }
    }    void ActivateNormalSpeed()
    {
        if (playerController != null)
        {
            // Set sprintSpeed to normal value
            var speed = playerController.freeSpeed;
            speed.sprintSpeed = normalSprintSpeed;
            playerController.freeSpeed = speed;
            
            // Restore original grounding physics
            playerController.extraGravity = originalExtraGravity;
            playerController.groundMaxDistance = originalGroundMaxDistance;
            
            superSpeedActive = false;
            Debug.Log("Normal Speed Cheat Activated!");

            // Play cheat activated sound
            if (audioManager != null)
            {
                audioManager.Play("CheatActivated");
            }
        }
    }

    void ActivateResetCheat()
    {
        Debug.Log("Reset Cheat Activated - Teleporting to nearest checkpoint!");
        
        // Use CheckpointManager to teleport to nearest checkpoint
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.TeleportToNearestCheckpoint();
            
            // Play cheat activated sound
            if (audioManager != null)
            {
                audioManager.Play("CheatActivated");
            }
        }
        else
        {
            Debug.LogWarning("Reset Cheat: CheckpointManager not found! Make sure CheckpointManager is in the scene.");
            
            // Fallback: Try to find and use any checkpoint directly
            Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
            if (checkpoints.Length > 0)
            {
                // Find nearest checkpoint manually
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    Checkpoint nearest = null;
                    float shortestDistance = float.MaxValue;
                    
                    foreach (var checkpoint in checkpoints)
                    {
                        float distance = Vector3.Distance(playerObj.transform.position, checkpoint.transform.position);
                        if (distance < shortestDistance)
                        {
                            shortestDistance = distance;
                            nearest = checkpoint;
                        }
                    }
                    
                    if (nearest != null)
                    {
                        nearest.RespawnPlayerHere();
                        Debug.Log($"Reset Cheat: Teleported to checkpoint '{nearest.CheckpointId}' (fallback method)");
                        
                        // Play sound
                        if (audioManager != null)
                        {
                            audioManager.Play("CheatActivated");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Reset Cheat: No checkpoints found in scene!");            }
        }
    }

    void ActivateClearSaveCheat()
    {
        Debug.Log("Clear Save Cheat Activated - Clearing ALL PlayerPrefs and save data!");
        
        // Clear ALL PlayerPrefs (this will delete everything stored in Unity's PlayerPrefs)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save(); // Ensure changes are written to disk immediately
        
        // Also clear component-specific save data for completeness
        // Clear checkpoint save data
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.ClearSaveData();
        }
        
        // Clear player position save data
        var playerPersistence = FindObjectOfType<PlayerPersistence>();
        if (playerPersistence != null)
        {
            playerPersistence.ClearSaveData();
        }
        
        // Clear puzzle save data
        var puzzles = FindObjectsOfType<ResonancePuzzle>();
        foreach (var puzzle in puzzles)
        {
            if (puzzle != null)
            {
                puzzle.ClearSaveData();
            }
        }
        
        // Clear level save data
        var levelManagers = FindObjectsOfType<LevelPuzzleManager>();
        foreach (var levelManager in levelManagers)
        {
            if (levelManager != null)
            {
                levelManager.ClearSaveData();
            }
        }
        
        Debug.Log("Clear Save Cheat: ALL PlayerPrefs and save data cleared! Game will restart completely fresh on next load.");
        
        // Play cheat activated sound
        if (audioManager != null)
        {
            audioManager.Play("CheatActivated");
        }
    }    void ActivateAutoSolveCheat()
    {
        Debug.Log("AutoSolve Cheat Activated - Checking for resonators in player range!");
        
        // Find the player
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("AutoSolve Cheat: Player not found!");
            return;
        }
        
        Debug.Log($"AutoSolve Cheat: Player found at position {playerObj.transform.position}");
        
        // Find all resonators in the scene
        Resonator[] allResonators = FindObjectsOfType<Resonator>();
        Debug.Log($"AutoSolve Cheat: Found {allResonators.Length} resonators in scene");
        
        if (allResonators.Length == 0)
        {
            Debug.LogWarning("AutoSolve Cheat: No resonators found in scene!");
            return;
        }
        
        bool foundResonatorInRange = false;
        int activatedCount = 0;
          foreach (Resonator resonator in allResonators)
        {
            if (resonator == null) continue;
              // Check if player is within this resonator's activation radius
            float distance = Vector3.Distance(playerObj.transform.position, resonator.transform.position);
            
            // Get the activation radius from the resonator and account for scale
            float baseActivationRadius = resonator.activationRadius;
            
            // Get the maximum scale component to account for scaling
            Vector3 scale = resonator.transform.lossyScale;
            float maxScale = Mathf.Max(scale.x, scale.y, scale.z);
            float scaledActivationRadius = baseActivationRadius * maxScale;
            
            Debug.Log($"AutoSolve Cheat: Resonator '{resonator.gameObject.name}' at distance {distance:F2}, base radius {baseActivationRadius:F2}, scale {maxScale:F2}, scaled radius {scaledActivationRadius:F2}");
            
            if (distance <= scaledActivationRadius)
            {
                foundResonatorInRange = true;
                Debug.Log($"AutoSolve Cheat: Resonator '{resonator.gameObject.name}' is in range!");
                
                // Check if this resonator is already activated
                // Use reflection to access the private isActivated field
                var isActivatedField = resonator.GetType().GetField("isActivated", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (isActivatedField != null)
                {
                    bool isActivated = (bool)isActivatedField.GetValue(resonator);
                    
                    if (!isActivated)
                    {
                        Debug.Log($"AutoSolve Cheat: Attempting to activate resonator '{resonator.gameObject.name}'");
                        // Use reflection to call the private ActivateResonator method
                        var activateMethod = resonator.GetType().GetMethod("ActivateResonator",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        if (activateMethod != null)
                        {
                            activateMethod.Invoke(resonator, null);
                            activatedCount++;
                            Debug.Log($"AutoSolve Cheat: Activated resonator '{resonator.gameObject.name}'");
                        }
                        else
                        {
                            Debug.LogWarning($"AutoSolve Cheat: Could not find ActivateResonator method on '{resonator.gameObject.name}'");
                        }
                    }
                    else
                    {
                        Debug.Log($"AutoSolve Cheat: Resonator '{resonator.gameObject.name}' is already activated");
                    }
                }
                else
                {
                    Debug.LogWarning($"AutoSolve Cheat: Could not access isActivated field on '{resonator.gameObject.name}'");
                }
            }
        }
        
        if (!foundResonatorInRange)
        {
            Debug.LogWarning("AutoSolve Cheat: Player is not within range of any resonators!");
        }        else if (activatedCount > 0)
        {
            Debug.Log($"AutoSolve Cheat: Successfully activated {activatedCount} resonator(s)!");
              // Play cheat activated sound
            if (audioManager != null)
            {
                try
                {
                    // Additional validation for debugging
                    if (AudioManager.Instance == null)
                    {
                        Debug.LogWarning("AutoSolve Cheat: AudioManager.Instance is null!");
                    }
                    else
                    {
                        // Call validation method to diagnose issues
                        AudioManager.Instance.ValidateAudioManager();
                    }
                    
                    audioManager.Play("CheatActivated");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"AutoSolve Cheat: Could not play audio - {e.Message}");
                    Debug.LogWarning($"AudioManager state: audioManager={audioManager != null}, Instance={AudioManager.Instance != null}");
                }
            }
            else
            {
                Debug.LogWarning("AutoSolve Cheat: AudioManager not found, skipping sound effect");
            }
        }
        else
        {
            Debug.Log("AutoSolve Cheat: All resonators in range are already activated!");
        }
    }

    void ApplySuperSpeedGrounding()
    {
        // Apply additional downward force when moving fast on slopes
        if (playerController.isGrounded && playerController.input.sqrMagnitude > 0.1f)
        {
            float groundAngle = playerController.GroundAngle();
            
            // Apply stronger grounding force on slopes
            if (groundAngle > 5f && groundAngle <= playerController.slopeLimit)
            {
                float slopeMultiplier = Mathf.Clamp(groundAngle / playerController.slopeLimit, 0.2f, 2f);
                Vector3 groundNormal = playerController.groundHit.normal;
                
                // Apply force perpendicular to the slope to maintain contact
                Vector3 groundingForce = -groundNormal * (20f * slopeMultiplier * Time.deltaTime);
                playerRigidbody.AddForce(groundingForce, ForceMode.VelocityChange);
                
                // Limit vertical velocity to prevent launching
                if (playerRigidbody.linearVelocity.y > 2f)
                {
                    Vector3 velocity = playerRigidbody.linearVelocity;
                    velocity.y = Mathf.Clamp(velocity.y, -50f, 2f);
                    playerRigidbody.linearVelocity = velocity;
                }            }
        }
    }    void ToggleFlyMode()
    {
        if (playerController == null || playerRigidbody == null)
        {
            Debug.LogWarning("Fly Cheat: Player controller or rigidbody not found!");
            return;
        }

        flyModeActive = !flyModeActive;

        if (flyModeActive)
        {
            Debug.Log("Fly Mode Activated! Use WASD + Space/Shift to fly!");
            
            // Disable Invector's movement and gravity systems
            playerController.enabled = false;
              // Disable gravity and ground constraints
            playerRigidbody.useGravity = false;
            playerRigidbody.linearDamping = flyDrag; // Add some air resistance for better control
            
            // Store current velocity to prevent sudden stops
            Vector3 currentVelocity = playerRigidbody.linearVelocity;
            currentVelocity.y = 0f; // Remove any falling velocity
            playerRigidbody.linearVelocity = currentVelocity;
        }
        else
        {
            Debug.Log("Fly Mode Deactivated!");
            
            // Re-enable Invector controller
            playerController.enabled = true;
            
            // Restore normal physics
            playerRigidbody.useGravity = originalUseGravity;
            playerRigidbody.linearDamping = 0f; // Reset drag to default
        }

        // Play cheat activated sound
        if (audioManager != null)
        {
            audioManager.Play("CheatActivated");
        }
    }    void HandleFlyMode()
    {
        // Get the main camera for movement direction
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Fly Mode: Main camera not found!");
            return;
        }

        // Get input for movement
        Vector3 moveDirection = Vector3.zero;
        
        // Get camera forward and right vectors (projected on horizontal plane for WASD)
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        
        // Remove Y component for horizontal movement
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // WASD movement relative to camera
        if (Input.GetKey(KeyCode.W))
            moveDirection += cameraForward;
        if (Input.GetKey(KeyCode.S))
            moveDirection -= cameraForward;
        if (Input.GetKey(KeyCode.A))
            moveDirection -= cameraRight;
        if (Input.GetKey(KeyCode.D))
            moveDirection += cameraRight;
            
        // Up/down movement
        if (Input.GetKey(KeyCode.Space))
            moveDirection += Vector3.up;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            moveDirection -= Vector3.up;

        // Normalize movement direction to prevent faster diagonal movement
        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        // Apply fly speed with some smoothing
        Vector3 targetVelocity = moveDirection * flySpeed;
          // Use lerp for smoother movement
        Vector3 currentVelocity = playerRigidbody.linearVelocity;
        Vector3 newVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * flyAcceleration);
        
        playerRigidbody.linearVelocity = newVelocity;
        
        // Rotate player to face movement direction (only horizontal rotation)
        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 horizontalMovement = new Vector3(moveDirection.x, 0f, moveDirection.z);
            if (horizontalMovement.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(horizontalMovement);                playerController.transform.rotation = Quaternion.Slerp(
                    playerController.transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * flyRotationSpeed
                );
            }        }
        
        // Optional: Debug output only when first activating fly mode
        if (Input.anyKey && Time.frameCount % 60 == 0) // Log once per second when keys are pressed
        {
            Debug.Log($"Fly Mode Active - Velocity: {newVelocity.magnitude:F1}");
        }
    }
}