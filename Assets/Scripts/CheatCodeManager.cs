using UnityEngine;
using Invector.vCharacterController; // Make sure you have this using statement

public class CheatCodeManager : MonoBehaviour
{
    // Singleton instance
    public static CheatCodeManager Instance { get; private set; }    // Add more cheats as needed
    private string[] cheatCodes = { "superspeed", "normalspeed", "reset", "clearsave", "autosolve" };
    private int maxCheatLength = 15; // Longest cheat code length ("normalspeed" is 15)
    private string inputBuffer = "";// Reference to the player controller
    private vThirdPersonController playerController;
    private float normalSprintSpeed = 6f;
    private float cheatSprintSpeed = 60f;
    private bool superSpeedActive = false;

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
        }        // Find the AudioManager in the scene
        audioManager = FindObjectOfType<AudioManager>();
        
        // Enhanced AudioManager debugging
        if (audioManager != null)
        {
            Debug.Log("CheatCodeManager: AudioManager found successfully");
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("CheatCodeManager: AudioManager found but Instance is null!");
            }
        }
        else
        {
            Debug.LogWarning("CheatCodeManager: AudioManager not found in scene!");
            // Try to find it by name as fallback
            var audioManagerObj = GameObject.Find("AudioManager");
            if (audioManagerObj != null)
            {
                audioManager = audioManagerObj.GetComponent<AudioManager>();
                Debug.Log($"CheatCodeManager: Found AudioManager by name, component: {audioManager != null}");
            }
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
        }

        // Apply additional grounding force when super speed is active
        if (superSpeedActive && playerController != null && playerRigidbody != null)
        {
            ApplySuperSpeedGrounding();
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
        }
        else if (inputBuffer.EndsWith("autosolve"))
        {
            ActivateAutoSolveCheat();
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
        Debug.Log("Clear Save Cheat Activated - Clearing all save data!");
        
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
        
        Debug.Log("Clear Save Cheat: All save data cleared! Game will restart fresh on next load.");
        
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
                }
            }
        }
    }
}