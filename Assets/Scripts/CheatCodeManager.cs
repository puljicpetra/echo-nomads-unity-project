using UnityEngine;
using Invector.vCharacterController; // Make sure you have this using statement

public class CheatCodeManager : MonoBehaviour
{
    // Add more cheats as needed
    private string[] cheatCodes = { "superspeed", "normalspeed" };
    private int maxCheatLength = 15; // Longest cheat code length ("normalspeed" is 15)
    private string inputBuffer = "";

    // Reference to the player controller
    private vThirdPersonController playerController;
    private float normalSprintSpeed = 6f;
    private float cheatSprintSpeed = 60f;
    private bool superSpeedActive = false;

    // Add a reference to the AudioManager
    private AudioManager audioManager;

    void Start()
    {
        // Find the player controller in the scene
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<vThirdPersonController>();
            // Assign normal sprint speed
            normalSprintSpeed = playerController.freeSpeed.sprintSpeed;
        }

        // Find the AudioManager in the scene
        audioManager = FindObjectOfType<AudioManager>();
    }

    void Update()
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
    }

    void CheckCheatCodes()
    {
        if (inputBuffer.EndsWith("superspeed") && !superSpeedActive)
        {
            ActivateSuperSpeed();
        }
        else if (inputBuffer.EndsWith("normalspeed") && superSpeedActive)
        {
            ActivateNormalSpeed();
        }
        // Add more cheats here
    }

    void ActivateSuperSpeed()
    {
        if (playerController != null)
        {
            // Set sprintSpeed to cheat value
            var speed = playerController.freeSpeed;
            speed.sprintSpeed = cheatSprintSpeed;
            playerController.freeSpeed = speed;
            superSpeedActive = true;
            Debug.Log("Super Speed Cheat Activated!");

            // Play cheat activated sound
            if (audioManager != null)
            {
                audioManager.Play("CheatActivated");
            }
        }
    }

    void ActivateNormalSpeed()
    {
        if (playerController != null)
        {
            // Set sprintSpeed to normal value
            var speed = playerController.freeSpeed;
            speed.sprintSpeed = normalSprintSpeed;
            playerController.freeSpeed = speed;
            superSpeedActive = false;
            Debug.Log("Normal Speed Cheat Activated!");

            // Play cheat activated sound
            if (audioManager != null)
            {
                audioManager.Play("CheatActivated");
            }
        }
    }
}