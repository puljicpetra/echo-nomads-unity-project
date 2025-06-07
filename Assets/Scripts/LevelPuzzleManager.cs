using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PuzzleRewardPair
{
    [Tooltip("The ResonancePuzzle that needs to be solved")]
    public ResonancePuzzle puzzle;
    
    [Tooltip("List of GameObjects that will be activated when this specific puzzle is solved (e.g., multiple lights, doors, platforms, etc.)")]
    public List<GameObject> rewardObjects = new List<GameObject>();
    
    [Tooltip("Optional: Sound to play when this specific puzzle is solved")]
    public string completionSoundName;
}

public class LevelPuzzleManager : MonoBehaviour
{
    [Header("Level Configuration")]
    [Tooltip("List of puzzle-reward pairs. Each puzzle can have its own reward object that activates when that specific puzzle is solved.")]
    public List<PuzzleRewardPair> puzzleRewardPairs;
    
    [Header("Level Completion Rewards")]
    [Tooltip("GameObject that will be made visible when ALL puzzles are solved (e.g., Bridge, Door, Platform, etc.)")]
    public GameObject levelCompletionRewardObject;
    
    [Header("Audio Settings")]
    [Tooltip("Name of the sound to play when ALL puzzles are completed (must exist in AudioManager)")]
    public string levelCompletionSoundName = "AllPuzzlesComplete";
    
    [Header("Persistence Settings")]
    [SerializeField] private string levelId;
    [SerializeField] private bool enablePersistence = true;

    // Internal state
    private int totalPuzzles;
    private int solvedPuzzles = 0;
    private bool levelCompleted = false;    void Start()
    {
        // Generate unique ID if not set
        if (string.IsNullOrEmpty(levelId))
        {
            levelId = $"level_{transform.GetInstanceID()}";
        }

        InitializePuzzleTracking();
        
        // Load saved state if persistence is enabled
        if (enablePersistence)
        {
            LoadLevelState();
        }
    }void InitializePuzzleTracking()
    {
        // Set total number of puzzles
        totalPuzzles = puzzleRewardPairs.Count;

        if (totalPuzzles == 0)
        {
            Debug.LogWarning("No puzzle-reward pairs assigned to LevelPuzzleManager: " + gameObject.name, this);
            return;
        }

        // Ensure level completion reward object is initially hidden
        if (levelCompletionRewardObject != null)
        {
            levelCompletionRewardObject.SetActive(false);
            Debug.Log($"Level completion reward object '{levelCompletionRewardObject.name}' initially set to invisible.");
        }

        // Initialize individual puzzle reward objects and subscribe to events
        foreach (PuzzleRewardPair pair in puzzleRewardPairs)
        {
            if (pair.puzzle == null)
            {
                Debug.LogError("Empty puzzle slot in puzzle-reward pairs for LevelPuzzleManager: " + gameObject.name, this);
                continue;
            }            // Ensure individual reward objects are initially hidden
            if (pair.rewardObjects != null && pair.rewardObjects.Count > 0)
            {
                foreach (GameObject rewardObj in pair.rewardObjects)
                {
                    if (rewardObj != null)
                    {
                        rewardObj.SetActive(false);
                        Debug.Log($"Puzzle reward object '{rewardObj.name}' for puzzle '{pair.puzzle.gameObject.name}' initially set to invisible.");
                    }
                }
            }

            // Set up event listener for when puzzle is solved
            pair.puzzle.OnPuzzleSolved += OnPuzzleSolved;
        }

        Debug.Log($"LevelPuzzleManager '{gameObject.name}' initialized. Total puzzles: {totalPuzzles}");
    }    /// <summary>
    /// Called when a ResonancePuzzle is solved
    /// </summary>
    /// <param name="solvedPuzzle">The puzzle that was just solved</param>
    public void OnPuzzleSolved(ResonancePuzzle solvedPuzzle)
    {
        if (levelCompleted) return; // Already completed, ignore

        // Find the corresponding reward pair for this puzzle
        PuzzleRewardPair solvedPair = null;
        foreach (PuzzleRewardPair pair in puzzleRewardPairs)
        {
            if (pair.puzzle == solvedPuzzle)
            {
                solvedPair = pair;
                break;
            }
        }        if (solvedPair != null)
        {
            // Activate all the individual puzzle's reward objects
            if (solvedPair.rewardObjects != null && solvedPair.rewardObjects.Count > 0)
            {
                foreach (GameObject rewardObj in solvedPair.rewardObjects)
                {
                    if (rewardObj != null)
                    {
                        rewardObj.SetActive(true);
                        Debug.Log($"Activated reward object '{rewardObj.name}' for solved puzzle '{solvedPuzzle.gameObject.name}'!");
                    }
                }
            }
            else
            {
                Debug.Log($"No reward objects assigned for puzzle '{solvedPuzzle.gameObject.name}'");
            }

            // Play the individual puzzle's completion sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(solvedPair.completionSoundName))
            {
                AudioManager.Instance.Play(solvedPair.completionSoundName);
                Debug.Log($"Playing puzzle completion sound: {solvedPair.completionSoundName}");
            }
        }

        solvedPuzzles++;
        Debug.Log($"Puzzle '{solvedPuzzle.gameObject.name}' solved! Progress: {solvedPuzzles}/{totalPuzzles}");

        // Check if all puzzles are now solved
        if (solvedPuzzles >= totalPuzzles)
        {
            CompleteLevelPuzzles();
        }
    }    void CompleteLevelPuzzles()
    {
        levelCompleted = true;
        Debug.LogWarning($"ALL PUZZLES COMPLETED IN LEVEL: {gameObject.name}!");

        // Save level completion state if persistence is enabled
        if (enablePersistence)
        {
            SaveLevelState();
        }

        // Play level completion sound
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(levelCompletionSoundName))
        {
            AudioManager.Instance.Play(levelCompletionSoundName);
            Debug.Log($"Playing level completion sound: {levelCompletionSoundName}");
        }
        else
        {
            Debug.LogWarning("AudioManager not found or level completion sound name not set!");
        }

        // Make level completion reward object visible
        if (levelCompletionRewardObject != null)
        {
            levelCompletionRewardObject.SetActive(true);
            Debug.Log($"Level completion reward object '{levelCompletionRewardObject.name}' is now visible!");
        }
        else
        {
            Debug.LogWarning("Level completion reward object not assigned in LevelPuzzleManager!");
        }

        // --- ADD MORE COMPLETION LOGIC HERE ---
        // e.g., trigger cutscenes, unlock new areas, save progress, etc.
    }    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        foreach (PuzzleRewardPair pair in puzzleRewardPairs)
        {
            if (pair.puzzle != null)
            {
                pair.puzzle.OnPuzzleSolved -= OnPuzzleSolved;
            }
        }
    }    // Optional: Method to manually check puzzle states (for debugging)
    [ContextMenu("Check Puzzle States")]
    void CheckPuzzleStates()
    {
        int currentSolved = 0;
        foreach (PuzzleRewardPair pair in puzzleRewardPairs)
        {
            if (pair.puzzle != null && pair.puzzle.IsSolved)
            {
                currentSolved++;
            }
        }
        Debug.Log($"Current puzzle states: {currentSolved}/{totalPuzzles} solved");
    }

    // === PERSISTENCE METHODS ===
    
    void SaveLevelState()
    {
        if (!enablePersistence) return;

        try
        {
            string savePrefix = CheckpointManager.Instance != null ? 
                CheckpointManager.Instance.GetComponent<CheckpointManager>().gameObject.name + "_" : 
                "EchoNomads_";

            PlayerPrefs.SetInt($"{savePrefix}Level_{levelId}_Completed", levelCompleted ? 1 : 0);
            PlayerPrefs.SetInt($"{savePrefix}Level_{levelId}_SolvedPuzzles", solvedPuzzles);
            PlayerPrefs.SetString($"{savePrefix}Level_{levelId}_SaveTime", System.DateTime.Now.ToBinary().ToString());
            PlayerPrefs.Save();

            Debug.Log($"LevelPuzzleManager: Saved state for level '{levelId}' - Completed: {levelCompleted}, Solved: {solvedPuzzles}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LevelPuzzleManager: Failed to save level state - {e.Message}");
        }
    }

    void LoadLevelState()
    {
        if (!enablePersistence) return;

        try
        {
            string savePrefix = CheckpointManager.Instance != null ? 
                CheckpointManager.Instance.GetComponent<CheckpointManager>().gameObject.name + "_" : 
                "EchoNomads_";

            if (PlayerPrefs.HasKey($"{savePrefix}Level_{levelId}_Completed"))
            {
                bool wasCompleted = PlayerPrefs.GetInt($"{savePrefix}Level_{levelId}_Completed") == 1;
                int savedSolvedCount = PlayerPrefs.GetInt($"{savePrefix}Level_{levelId}_SolvedPuzzles", 0);

                if (wasCompleted)
                {
                    levelCompleted = true;
                    solvedPuzzles = savedSolvedCount;

                    // Activate level completion reward object
                    if (levelCompletionRewardObject != null)
                    {
                        levelCompletionRewardObject.SetActive(true);
                    }

                    // Activate all individual puzzle rewards
                    foreach (PuzzleRewardPair pair in puzzleRewardPairs)
                    {
                        if (pair.puzzle != null && pair.puzzle.IsSolved)
                        {
                            if (pair.rewardObjects != null && pair.rewardObjects.Count > 0)
                            {
                                foreach (GameObject rewardObj in pair.rewardObjects)
                                {
                                    if (rewardObj != null)
                                    {
                                        rewardObj.SetActive(true);
                                    }
                                }
                            }
                        }
                    }

                    Debug.Log($"LevelPuzzleManager: Loaded completed level state for '{levelId}'");
                }
                else if (savedSolvedCount > 0)
                {
                    solvedPuzzles = savedSolvedCount;
                    
                    // Activate rewards for individual solved puzzles
                    foreach (PuzzleRewardPair pair in puzzleRewardPairs)
                    {
                        if (pair.puzzle != null && pair.puzzle.IsSolved)
                        {
                            if (pair.rewardObjects != null && pair.rewardObjects.Count > 0)
                            {
                                foreach (GameObject rewardObj in pair.rewardObjects)
                                {
                                    if (rewardObj != null)
                                    {
                                        rewardObj.SetActive(true);
                                    }
                                }
                            }
                        }
                    }

                    Debug.Log($"LevelPuzzleManager: Loaded partial level state for '{levelId}' - Solved: {solvedPuzzles}/{totalPuzzles}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LevelPuzzleManager: Failed to load level state - {e.Message}");
        }
    }

    public void ClearSaveData()
    {
        if (!enablePersistence) return;

        try
        {
            string savePrefix = CheckpointManager.Instance != null ? 
                CheckpointManager.Instance.GetComponent<CheckpointManager>().gameObject.name + "_" : 
                "EchoNomads_";

            PlayerPrefs.DeleteKey($"{savePrefix}Level_{levelId}_Completed");
            PlayerPrefs.DeleteKey($"{savePrefix}Level_{levelId}_SolvedPuzzles");
            PlayerPrefs.DeleteKey($"{savePrefix}Level_{levelId}_SaveTime");
            PlayerPrefs.Save();

            Debug.Log($"LevelPuzzleManager: Cleared save data for level '{levelId}'");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LevelPuzzleManager: Failed to clear save data - {e.Message}");
        }
    }
}
