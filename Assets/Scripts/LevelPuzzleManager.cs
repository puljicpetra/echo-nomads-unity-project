using UnityEngine;
using System.Collections.Generic;

public class LevelPuzzleManager : MonoBehaviour
{
    [Header("Level Configuration")]
    [Tooltip("List of all ResonancePuzzle objects in this level. Add them manually in Inspector.")]
    public List<ResonancePuzzle> puzzlesInLevel;
      [Header("Completion Rewards")]
    [Tooltip("GameObject that will be made visible when all puzzles are solved (e.g., Bridge, Door, Platform, etc.)")]
    public GameObject rewardObject;
    
    [Header("Audio Settings")]
    [Tooltip("Name of the sound to play when all puzzles are completed (must exist in AudioManager)")]
    public string completionSoundName = "AllPuzzlesComplete";

    // Internal state
    private int totalPuzzles;
    private int solvedPuzzles = 0;
    private bool levelCompleted = false;

    void Start()
    {
        InitializePuzzleTracking();
    }

    void InitializePuzzleTracking()
    {
        // Set total number of puzzles
        totalPuzzles = puzzlesInLevel.Count;

        if (totalPuzzles == 0)
        {
            Debug.LogWarning("No puzzles assigned to LevelPuzzleManager: " + gameObject.name, this);
            return;
        }        // Ensure reward object is initially hidden
        if (rewardObject != null)
        {
            rewardObject.SetActive(false);
            Debug.Log($"Reward object '{rewardObject.name}' initially set to invisible.");
        }

        // Subscribe to puzzle completion events
        foreach (ResonancePuzzle puzzle in puzzlesInLevel)
        {
            if (puzzle == null)
            {
                Debug.LogError("Empty slot in puzzles list for LevelPuzzleManager: " + gameObject.name, this);
                continue;
            }

            // Set up event listener for when puzzle is solved
            puzzle.OnPuzzleSolved += OnPuzzleSolved;
        }

        Debug.Log($"LevelPuzzleManager '{gameObject.name}' initialized. Total puzzles: {totalPuzzles}");
    }

    /// <summary>
    /// Called when a ResonancePuzzle is solved
    /// </summary>
    /// <param name="solvedPuzzle">The puzzle that was just solved</param>
    public void OnPuzzleSolved(ResonancePuzzle solvedPuzzle)
    {
        if (levelCompleted) return; // Already completed, ignore

        solvedPuzzles++;
        Debug.Log($"Puzzle '{solvedPuzzle.gameObject.name}' solved! Progress: {solvedPuzzles}/{totalPuzzles}");

        // Check if all puzzles are now solved
        if (solvedPuzzles >= totalPuzzles)
        {
            CompleteLevelPuzzles();
        }
    }

    void CompleteLevelPuzzles()
    {
        levelCompleted = true;
        Debug.LogWarning($"ALL PUZZLES COMPLETED IN LEVEL: {gameObject.name}!");

        // Play completion sound
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(completionSoundName))
        {
            AudioManager.Instance.Play(completionSoundName);
            Debug.Log($"Playing completion sound: {completionSoundName}");
        }
        else
        {
            Debug.LogWarning("AudioManager not found or completion sound name not set!");
        }        // Make reward object visible
        if (rewardObject != null)
        {
            rewardObject.SetActive(true);
            Debug.Log($"Reward object '{rewardObject.name}' is now visible!");
        }
        else
        {
            Debug.LogWarning("Reward object not assigned in LevelPuzzleManager!");
        }

        // --- ADD MORE COMPLETION LOGIC HERE ---
        // e.g., trigger cutscenes, unlock new areas, save progress, etc.
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        foreach (ResonancePuzzle puzzle in puzzlesInLevel)
        {
            if (puzzle != null)
            {
                puzzle.OnPuzzleSolved -= OnPuzzleSolved;
            }
        }
    }

    // Optional: Method to manually check puzzle states (for debugging)
    [ContextMenu("Check Puzzle States")]
    void CheckPuzzleStates()
    {
        int currentSolved = 0;
        foreach (ResonancePuzzle puzzle in puzzlesInLevel)
        {
            if (puzzle != null && puzzle.IsSolved)
            {
                currentSolved++;
            }
        }
        Debug.Log($"Current puzzle states: {currentSolved}/{totalPuzzles} solved");
    }
}
