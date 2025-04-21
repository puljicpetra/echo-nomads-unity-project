using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Potrebno za List<>
using System.Linq; // Za korištenje Linq metoda poput Count
using UnityEngine.InputSystem; // For PlayerInput and InputAction

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SphereCollider))]
public class Resonator : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("Redoslijed zvukova (Sound1, Sound2, Sound3) koji aktivira ovaj rezonator. Minimalno 1, maksimalno 4 zvuka. Koristi 'None' za kraće sekvence (npr., Sound1, Sound2, None, None).")]
    public List<SoundType> requiredSequence = new List<SoundType>(4) { SoundType.PowerSound1, SoundType.None, SoundType.None, SoundType.None }; // Promijenjen default i veličina

    [Tooltip("Referenca na Puzzle Manager koji prati ovu zagonetku.")]
    public ResonancePuzzle puzzleManager;

    [Header("Audio Playback")]
    [Tooltip("Zvukovi koje rezonator reproducira za svoju sekvencu.")]
    public AudioClip sound1Clip;
    public AudioClip sound2Clip;
    public AudioClip sound3Clip;
    [Tooltip("Stanka između zvukova u sekvenci koju rezonator svira.")]
    public float sequencePlaybackDelay = 0.8f;
    [Tooltip("Stanka prije ponavljanja sekvence.")]
    public float sequenceRepeatDelay = 2.0f;

    [Header("Interaction")]
    [Tooltip("Radijus unutar kojeg igrač može aktivirati rezonator.")]
    public float activationRadius = 5f;

    [Tooltip("Radijus unutar kojeg se provjerava prisutnost Hush objekata.")]
    [SerializeField] private float hushCheckRadius = 5f;
    [Tooltip("Tag koji označava Hush objekte.")]
    [SerializeField] private string hushTag = "Hush";

    [Tooltip("Ime akcije za Focus.")]
    [SerializeField] private string focusActionName = "Focus";

    // Internal State
    private AudioSource audioSource;
    private List<SoundType> playerInputSequence = new List<SoundType>();
    private bool isActivated = false;
    private bool playerIsInRange = false;
    private Coroutine sequencePlaybackCoroutine;
    private int actualSequenceLength; // Pohranjujemo stvarnu dužinu sekvence (bez None na kraju)
    private PlayerInput playerInput; // For Focus
    private InputAction focusAction;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        SphereCollider triggerCollider = GetComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = activationRadius;

        // Validacija i određivanje stvarne dužine sekvence
        ValidateAndSetSequenceLength();

        // Pronađi PlayerInput i akciju za Focus
        playerInput = Object.FindFirstObjectByType<PlayerInput>(); // Updated to use the recommended method
        if (playerInput != null)
        {
            focusAction = playerInput.actions.FindAction(focusActionName, false);
        }
        else
        {
            Debug.LogWarning("No PlayerInput found in scene for Resonator focus check.");
        }
    }

    void ValidateAndSetSequenceLength()
    {
        // Ukloni sve 'None' s kraja liste radi lakše usporedbe kasnije
        while (requiredSequence.Count > 0 && requiredSequence[requiredSequence.Count - 1] == SoundType.None)
        {
            requiredSequence.RemoveAt(requiredSequence.Count - 1);
        }

        // Osiguraj da imamo barem jedan element ako je lista ispražnjena
        if (requiredSequence.Count == 0) {
            requiredSequence.Add(SoundType.PowerSound1); // Dodaj default ako je prazna
             Debug.LogWarning("Required sequence for " + gameObject.name + " was empty or only None. Setting to default [Sound1].", this);
        }

        // Osiguraj da nije duža od 4 (iako je inicijalizirana s 4, ali za svaki slučaj)
        if (requiredSequence.Count > 4)
        {
             requiredSequence = requiredSequence.GetRange(0, 4); // Uzmi samo prva 4
             Debug.LogWarning("Required sequence for " + gameObject.name + " was longer than 4. Truncating.", this);
        }

         // Osiguraj da nema 'None' usred sekvence (nije podržano u ovoj logici)
         for (int i = 0; i < requiredSequence.Count; i++) {
             if (requiredSequence[i] == SoundType.None) {
                 Debug.LogError("SoundType.None found in the middle of the sequence for " + gameObject.name + ". This is not supported. Replace it with Sound1, Sound2, or Sound3.", this);
                 // Ovdje možeš ili zaustaviti ili zamijeniti s defaultom, npr.:
                 // requiredSequence[i] = SoundType.PowerSound1;
                 enabled = false; // Najsigurnije je onemogućiti dok se ne popravi
                 return;
             }
         }

        actualSequenceLength = requiredSequence.Count;
         Debug.Log("Validated sequence for " + gameObject.name + ". Actual length: " + actualSequenceLength);
    }


    void Start()
    {
        if (puzzleManager == null)
        {
            puzzleManager = GetComponentInParent<ResonancePuzzle>();
        }
        if (puzzleManager == null)
        {
            Debug.LogError("Resonator " + gameObject.name + " nije povezan s ResonancePuzzle managerom!", this);
            enabled = false;
            return;
        }
        if (!isActivated)
        {
           StartPlayingSequenceLoop();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            playerIsInRange = true;
            playerInputSequence.Clear();
            Debug.Log("Player entered range of Resonator: " + gameObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInRange = false;
            playerInputSequence.Clear();
             Debug.Log("Player exited range of Resonator: " + gameObject.name);
        }
    }

    void StartPlayingSequenceLoop()
    {
        if (sequencePlaybackCoroutine != null) StopCoroutine(sequencePlaybackCoroutine);
        sequencePlaybackCoroutine = StartCoroutine(PlaySequenceLoopRoutine());
    }

    IEnumerator PlaySequenceLoopRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        while (!isActivated)
        {
            // Only play sequence if Focus is held and no Hush is near
            if (focusAction != null && focusAction.IsPressed() && !IsHushNearby())
            {
                for(int i = 0; i < actualSequenceLength; i++)
                {
                    if(isActivated) yield break;

                    SoundType sound = requiredSequence[i]; // Uzmi zvuk iz validirane liste
                    AudioClip clipToPlay = GetClipForSoundType(sound);
                    if (clipToPlay != null) // Ne bi trebalo biti null zbog validacije, ali za sigurnost
                    {
                        audioSource.PlayOneShot(clipToPlay);
                        yield return new WaitForSeconds(sequencePlaybackDelay);
                    }
                     else
                    {
                         Debug.LogWarning("Unexpected null clip for sound type " + sound + " in " + gameObject.name, this);
                        yield return new WaitForSeconds(sequencePlaybackDelay);
                    }
                }
            }
            yield return new WaitForSeconds(sequenceRepeatDelay);
        }
    }

    private bool IsHushNearby()
    {
        Collider[] hushNearby = Physics.OverlapSphere(transform.position, hushCheckRadius);
        foreach (var hushCol in hushNearby)
        {
            if (hushCol.CompareTag(hushTag) && hushCol.gameObject.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }

    AudioClip GetClipForSoundType(SoundType sound)
    {
        switch (sound)
        {
            case SoundType.PowerSound1: return sound1Clip;
            case SoundType.PowerSound2: return sound2Clip;
            case SoundType.PowerSound3: return sound3Clip;
            default: return null; // Ne bi se smjelo dogoditi nakon validacije
        }
    }

    public void ReceivePlayerInput(SoundType playedSound)
    {
        if (!isActivated && playerIsInRange)
        {
             // Ignoriraj unos ako je None (npr. ako igrač ima "prazan" gumb)
            if (playedSound == SoundType.None) return;

            Debug.Log("Resonator " + gameObject.name + " received input: " + playedSound);
            playerInputSequence.Add(playedSound);

            // Provjeri je li unesena sekvenca duža od potrebne stvarne dužine
            if (playerInputSequence.Count > actualSequenceLength)
            {
                 playerInputSequence.RemoveAt(0);
            }

            // Provjeri podudaranje SAMO ako je uneseno točno onoliko zvukova koliko je potrebno
            if (playerInputSequence.Count == actualSequenceLength)
            {
                CheckSequenceMatch();
            }
        }
    }

    void CheckSequenceMatch()
    {
        bool match = true;
        // Usporedi samo do stvarne dužine sekvence
        for (int i = 0; i < actualSequenceLength; i++)
        {
            // Ne bi trebalo biti None u requiredSequence[i] zbog validacije
            if (requiredSequence[i] != playerInputSequence[i])
            {
                match = false;
                break;
            }
        }

        if (match)
        {
            ActivateResonator();
        }
        else
        {
            Debug.Log("Incorrect sequence for " + gameObject.name + ". Resetting input.");
            // Kada igrač pogriješi, ne mora nužno resetirati cijeli unos.
            // Možda je bolje samo čekati da unese sljedeći zvuk, pa će se
            // najstariji unos automatski ukloniti ako prekorači dužinu.
            // Ostavljam Clear() za sada radi jednostavnosti, ali ovo je točka za razmatranje.
            playerInputSequence.Clear();
        }
    }

    void ActivateResonator()
    {
        if (!isActivated)
        {
            isActivated = true;
            playerIsInRange = false;
            if (sequencePlaybackCoroutine != null) StopCoroutine(sequencePlaybackCoroutine);
            audioSource.Stop();
            Debug.Log("Resonator " + gameObject.name + " ACTIVATED!");
            // --- Resonator stops transmitting any sound here ---
            if (puzzleManager != null)
            {
                puzzleManager.NotifyResonatorActivated(this);
            } else {
                 Debug.LogWarning("Activated resonator " + gameObject.name + " has no Puzzle Manager assigned!");
            }
             GetComponent<SphereCollider>().enabled = false;
        }
    }
}