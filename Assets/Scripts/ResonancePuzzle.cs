using UnityEngine;
using System.Collections.Generic; // Potrebno za List<>
using System; // For Action delegate

public class ResonancePuzzle : MonoBehaviour
{
    [Header("Puzzle Components")]
    [Tooltip("Lista svih rezonatora koji su dio ove zagonetke. Dodaj ih ručno u Inspector.")]
    public List<Resonator> resonatorsInPuzzle;
    
    [Header("Persistence Settings")]
    [SerializeField] private string puzzleId;
    [SerializeField] private bool enablePersistence = true;

    // Internal State
    private int requiredActivations;
    private int currentActivations = 0;
    private bool isSolved = false;

    // Public property to check if puzzle is solved
    public bool IsSolved => isSolved;
    public string PuzzleId => puzzleId;

    // Event that fires when this puzzle is solved
    public event Action<ResonancePuzzle> OnPuzzleSolved;    void Start()
    {
        // Generate unique ID if not set
        if (string.IsNullOrEmpty(puzzleId))
        {
            puzzleId = $"puzzle_{transform.GetInstanceID()}";
        }

        // Load saved state if persistence is enabled
        if (enablePersistence)
        {
            LoadPuzzleState();
        }

        // Postavi potreban broj aktivacija na temelju dodanih rezonatora
        requiredActivations = resonatorsInPuzzle.Count;

        // Provjeri jesu li svi rezonatori ispravno povezani s ovim managerom
        foreach (Resonator res in resonatorsInPuzzle)
        {
            if (res == null)
            {
                Debug.LogError("Prazan slot u listi rezonatora za zagonetku: " + gameObject.name, this);
                continue;
            }
            if (res.puzzleManager == null)
            {
                Debug.LogWarning("Rezonator " + res.gameObject.name + " nema postavljen Puzzle Manager, postavljam na ovaj: " + gameObject.name);
                res.puzzleManager = this;
            }
            else if (res.puzzleManager != this)
            {
                 Debug.LogWarning("Rezonator " + res.gameObject.name + " je povezan s drugim Puzzle Managerom (" + res.puzzleManager.gameObject.name + "). Provjeri postavke.");
            }
        }


        if (requiredActivations == 0)
        {
            Debug.LogWarning("Nema rezonatora dodijeljenih zagonetki: " + gameObject.name, this);
        }
         Debug.Log("Puzzle " + gameObject.name + " initialized. Required activations: " + requiredActivations);
    }

    /// <summary>
    /// Metoda koju poziva Resonator kada je uspješno aktiviran.
    /// </summary>
    public void NotifyResonatorActivated(Resonator activatedResonator)
    {
        if (isSolved) return; // Ako je zagonetka već riješena, ne radi ništa

        currentActivations++;
        Debug.Log("Resonator activated for puzzle " + gameObject.name + ". Total activated: " + currentActivations + "/" + requiredActivations);

        // Provjeri jesu li svi rezonatori aktivirani
        if (currentActivations >= requiredActivations)
        {
            SolvePuzzle();
        }
    }    void SolvePuzzle()
    {
        isSolved = true;
        Debug.LogWarning("PUZZLE SOLVED: " + gameObject.name + "!"); // Upozorenje da se lakše vidi u konzoli

        // Save puzzle state if persistence is enabled
        if (enablePersistence)
        {
            SavePuzzleState();
        }

        // Fire the puzzle solved event
        OnPuzzleSolved?.Invoke(this);

        // --- OVDJE IDE LOGIKA NAKON RJEŠAVANJA ZAGONETKE ---
        // Npr. Otvori vrata, pokreni animaciju, pusti posebnu melodiju,
        // aktiviraj sljedeću fazu igre, itd.
        // GameObject doorToOpen = GameObject.Find("PuzzleDoor");
        // if (doorToOpen != null) doorToOpen.SetActive(false); // Primjer otvaranja vrata        // Prevent further activation of resonators
        foreach (var res in resonatorsInPuzzle)
        {
            if (res != null)
            {
                res.enabled = false;
            }
        }
    }

    // === PERSISTENCE METHODS ===
    
    void SavePuzzleState()
    {
        if (!enablePersistence) return;

        try
        {
            string savePrefix = CheckpointManager.Instance != null ? 
                CheckpointManager.Instance.GetComponent<CheckpointManager>().gameObject.name + "_" : 
                "EchoNomads_";

            PlayerPrefs.SetInt($"{savePrefix}Puzzle_{puzzleId}_Solved", isSolved ? 1 : 0);
            PlayerPrefs.SetInt($"{savePrefix}Puzzle_{puzzleId}_Activations", currentActivations);
            PlayerPrefs.SetString($"{savePrefix}Puzzle_{puzzleId}_SaveTime", System.DateTime.Now.ToBinary().ToString());
            PlayerPrefs.Save();

            Debug.Log($"ResonancePuzzle: Saved state for puzzle '{puzzleId}' - Solved: {isSolved}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ResonancePuzzle: Failed to save puzzle state - {e.Message}");
        }
    }

    void LoadPuzzleState()
    {
        if (!enablePersistence) return;

        try
        {
            string savePrefix = CheckpointManager.Instance != null ? 
                CheckpointManager.Instance.GetComponent<CheckpointManager>().gameObject.name + "_" : 
                "EchoNomads_";

            if (PlayerPrefs.HasKey($"{savePrefix}Puzzle_{puzzleId}_Solved"))
            {
                bool wasSolved = PlayerPrefs.GetInt($"{savePrefix}Puzzle_{puzzleId}_Solved") == 1;
                int savedActivations = PlayerPrefs.GetInt($"{savePrefix}Puzzle_{puzzleId}_Activations", 0);                if (wasSolved)
                {
                    isSolved = true;
                    currentActivations = savedActivations;
                    
                    // Activate all resonators and turn on their lights since puzzle is solved
                    foreach (var res in resonatorsInPuzzle)
                    {
                        if (res != null)
                        {
                            // Set resonator as activated from save
                            res.SetActivatedFromSave();
                            
                            // Disable the resonator script since puzzle is solved
                            res.enabled = false;
                        }
                    }

                    Debug.Log($"ResonancePuzzle: Loaded saved state for puzzle '{puzzleId}' - Already solved, lights activated");
                }
                else if (savedActivations > 0)
                {
                    currentActivations = savedActivations;
                    Debug.Log($"ResonancePuzzle: Loaded saved state for puzzle '{puzzleId}' - Activations: {currentActivations}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ResonancePuzzle: Failed to load puzzle state - {e.Message}");
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

            PlayerPrefs.DeleteKey($"{savePrefix}Puzzle_{puzzleId}_Solved");
            PlayerPrefs.DeleteKey($"{savePrefix}Puzzle_{puzzleId}_Activations");
            PlayerPrefs.DeleteKey($"{savePrefix}Puzzle_{puzzleId}_SaveTime");
            PlayerPrefs.Save();

            Debug.Log($"ResonancePuzzle: Cleared save data for puzzle '{puzzleId}'");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ResonancePuzzle: Failed to clear save data - {e.Message}");
        }
    }
}