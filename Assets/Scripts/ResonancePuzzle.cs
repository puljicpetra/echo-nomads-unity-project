using UnityEngine;
using System.Collections.Generic; // Potrebno za List<>
using System; // For Action delegate

public class ResonancePuzzle : MonoBehaviour
{
    [Header("Puzzle Components")]
    [Tooltip("Lista svih rezonatora koji su dio ove zagonetke. Dodaj ih ručno u Inspector.")]
    public List<Resonator> resonatorsInPuzzle;

    // Internal State
    private int requiredActivations;
    private int currentActivations = 0;
    private bool isSolved = false;

    // Public property to check if puzzle is solved
    public bool IsSolved => isSolved;

    // Event that fires when this puzzle is solved
    public event Action<ResonancePuzzle> OnPuzzleSolved;

    void Start()
    {
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

        // Fire the puzzle solved event
        OnPuzzleSolved?.Invoke(this);

        // --- OVDJE IDE LOGIKA NAKON RJEŠAVANJA ZAGONETKE ---
        // Npr. Otvori vrata, pokreni animaciju, pusti posebnu melodiju,
        // aktiviraj sljedeću fazu igre, itd.
        // GameObject doorToOpen = GameObject.Find("PuzzleDoor");
        // if (doorToOpen != null) doorToOpen.SetActive(false); // Primjer otvaranja vrata

        // Prevent further activation of resonators
        foreach (var res in resonatorsInPuzzle)
        {
            if (res != null)
            {
                res.enabled = false;
            }
        }
    }
}