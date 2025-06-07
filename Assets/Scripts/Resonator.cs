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
    [Tooltip("Redoslijed zvukova (Sound1, Sound2, Sound3, Sound4, ...) koji aktivira ovaj rezonator. Minimalno 1 zvuk. Koristi 'None' za kraće sekvence (npr., Sound1, Sound2, None, None).")]
    public List<SoundType> requiredSequence = new List<SoundType>() { SoundType.PowerSound1 }; // Uklonjena ograničenja na veličinu

    [Tooltip("Referenca na Puzzle Manager koji prati ovu zagonetku.")]
    public ResonancePuzzle puzzleManager;

    [Header("Audio Playback")]
    [Tooltip("Zvukovi koje rezonator reproducira za svoju sekvencu. Redoslijed mora odgovarati SoundType enumu (PowerSound1, PowerSound2, ...)")]
    public List<AudioClip> soundClips = new List<AudioClip>(); // Zamijenjeno s listom za podršku više zvukova
    [Tooltip("Zvuk koji se reproducira kada igrač unese pogrešnu sekvencu.")]
    public AudioClip incorrectSequenceClip; // Added field for incorrect sequence sound
    [Tooltip("Stanka između zvukova u sekvenci koju rezonator svira.")]
    public float sequencePlaybackDelay = 0.8f;
    [Tooltip("Stanka prije ponavljanja sekvence.")]
    public float sequenceRepeatDelay = 2.0f;
    [Tooltip("Zvuk koji se koristi za lociranje rezonatora iz daljine (3D zvuk).")]
    public AudioClip locatorClip; // New: locator sound
    [Tooltip("Glasnoća locator zvuka.")]
    [Range(0f, 1f)]
    public float locatorVolume = 0.7f;
    [Tooltip("Minimalna udaljenost za puni volumen locator zvuka.")]
    public float locatorMinDistance = 10f;
    [Tooltip("Maksimalna udaljenost na kojoj se locator zvuk čuje.")]
    public float locatorMaxDistance = 100f;

    [Header("Interaction")]
    [Tooltip("Radijus unutar kojeg igrač može aktivirati rezonator.")]
    public float activationRadius = 5f;

    [Tooltip("Radijus unutar kojeg se provjerava prisutnost Hush objekata.")]
    [SerializeField] private float hushCheckRadius = 5f;
    [Tooltip("Tag koji označava Hush objekte.")]
    [SerializeField] private string hushTag = "Hush";

    [Tooltip("Ime akcije za Focus.")]
    [SerializeField] private string focusActionName = "Focus";

    [Header("Visuals")] // Added header for visual elements
    [Tooltip("Referenca na Light komponentu na dječjem objektu 'ResonatorLight'.")]
    [SerializeField] private Light resonatorLight; // Added field for the light

    // Internal State
    private AudioSource audioSource;
    private AudioSource locatorAudioSource; // New: for locator sound
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

        // Pronađi svjetlo ako nije postavljeno u inspektoru
        if (resonatorLight == null)
        {
            Transform lightTransform = transform.Find("ResonatorLight");
            if (lightTransform != null)
            {
                resonatorLight = lightTransform.GetComponent<Light>();
                if (resonatorLight == null)
                {
                    Debug.LogWarning("Child object 'ResonatorLight' found, but it doesn't have a Light component.", this);
                }
            }
            else
            {
                Debug.LogWarning("Could not find child object named 'ResonatorLight'. Please create it and assign its Light component in the inspector.", this);
            }
        }

        // Osiguraj da je svjetlo isključeno na početku
        if (resonatorLight != null)
        {
            resonatorLight.enabled = false;
        }

        // Setup locator audio source if locatorClip is assigned
        if (locatorClip != null)
        {
            locatorAudioSource = gameObject.AddComponent<AudioSource>();
            locatorAudioSource.clip = locatorClip;
            locatorAudioSource.loop = true;
            locatorAudioSource.playOnAwake = false;
            locatorAudioSource.spatialBlend = 1f; // 3D sound
            locatorAudioSource.volume = locatorVolume;
            locatorAudioSource.minDistance = locatorMinDistance;
            locatorAudioSource.maxDistance = locatorMaxDistance;
            locatorAudioSource.dopplerLevel = 0f;
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

        // Uklonjena provjera maksimalne duljine (više nema ograničenja na 4)

        // Osiguraj da nema 'None' usred sekvence (nije podržano u ovoj logici)
        for (int i = 0; i < requiredSequence.Count; i++) {
            if (requiredSequence[i] == SoundType.None) {
                Debug.LogError("SoundType.None found in the middle of the sequence for " + gameObject.name + ". This is not supported. Replace it with Sound1, Sound2, Sound3, ...", this);
                enabled = false; // Najsigurnije je onemogućiti dok se ne popravi
                return;
            }
            // Validate that only allowed SoundTypes are used
            if ((int)requiredSequence[i] < (int)SoundType.PowerSound1 ||
                (int)requiredSequence[i] > (int)SoundType.PowerSound1 + soundClips.Count - 1)
            {
                Debug.LogError("Invalid SoundType in sequence for " + gameObject.name + ". Only PowerSound1 and up to the number of soundClips are allowed.", this);
                enabled = false;
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

    void Update()
    {
        // Stop playback if Focus is not held or Hush is nearby
        if (!isActivated && sequencePlaybackCoroutine != null)
        {
            bool shouldPlay = (focusAction != null && focusAction.IsPressed() && !IsHushNearby());
            if (!shouldPlay)
            {
                StopSequencePlayback();
            }
            else if (sequencePlaybackCoroutine == null)
            {
                StartPlayingSequenceLoop();
            }
        }
        // Optionally, restart playback if focus is pressed again and coroutine is null
        if (!isActivated && sequencePlaybackCoroutine == null && focusAction != null && focusAction.IsPressed() && !IsHushNearby())
        {
            StartPlayingSequenceLoop();
        }

        // Locator sound logic (independent of sequence)
        if (!isActivated && locatorAudioSource != null)
        {
            bool shouldPlayLocator = false;
            if (focusAction != null && focusAction.IsPressed() && !IsHushNearby())
            {
                // Find player GameObject by tag
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    float dist = Vector3.Distance(transform.position, playerObj.transform.position);
                    if (dist > locatorMinDistance)
                    {
                        shouldPlayLocator = true;
                    }
                }
            }
            if (shouldPlayLocator)
            {
                if (!locatorAudioSource.isPlaying)
                    locatorAudioSource.Play();
            }
            else
            {
                if (locatorAudioSource.isPlaying)
                    locatorAudioSource.Stop();
            }
        }
        else if (locatorAudioSource != null && locatorAudioSource.isPlaying)
        {
            locatorAudioSource.Stop();
        }
    }

    void StopSequencePlayback()
    {
        if (sequencePlaybackCoroutine != null)
        {
            StopCoroutine(sequencePlaybackCoroutine);
            sequencePlaybackCoroutine = null;
            audioSource.Stop();
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
        // No longer stop locator sound here (independent)
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

                    // Check again before each sound in case focus/hush changed
                    if (!(focusAction != null && focusAction.IsPressed() && !IsHushNearby()))
                    {
                        yield break;
                    }

                    SoundType sound = requiredSequence[i]; // Uzmi zvuk iz validirane liste
                    AudioClip clipToPlay = GetClipForSoundType(sound);
                    if (clipToPlay != null) // Ne bi trebalo biti null zbog validacije, ali za sigurnost
                    {
                        Debug.Log($"Playing sequence sound: {sound} (clip: {clipToPlay.name}) on {gameObject.name}");
                        audioSource.PlayOneShot(clipToPlay);
                        // Wait until the clip finishes playing
                        yield return new WaitUntil(() => !audioSource.isPlaying);
                        yield return new WaitForSeconds(sequencePlaybackDelay);
                    }
                    else
                    {
                        Debug.LogWarning("Unexpected null clip for sound type " + sound + " in " + gameObject.name, this);
                        yield return new WaitForSeconds(sequencePlaybackDelay);
                    }
                }
            }
            else
            {
                // If focus is lost or hush appears, stop playback
                yield break;
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
        int index = (int)sound - (int)SoundType.PowerSound1;
        if (index >= 0 && index < soundClips.Count)
            return soundClips[index];
        return null;
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
            // Play the incorrect sequence sound if assigned
            if (incorrectSequenceClip != null)
            {
                audioSource.PlayOneShot(incorrectSequenceClip);
            }
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

            // Aktiviraj svjetlo ako postoji
            if (resonatorLight != null)
            {
                resonatorLight.enabled = true;
                Debug.Log("ResonatorLight activated for " + gameObject.name);
            }

            // --- Resonator stops transmitting any sound here ---
            if (puzzleManager != null)
            {
                puzzleManager.NotifyResonatorActivated(this);
            } else {
                 Debug.LogWarning("Activated resonator " + gameObject.name + " has no Puzzle Manager assigned!");
            }
             GetComponent<SphereCollider>().enabled = false;

            // Stop locator sound on activation
            if (locatorAudioSource != null && locatorAudioSource.isPlaying)
                locatorAudioSource.Stop();
        }
    }

    // Visualize radii in the Unity Editor
    void OnDrawGizmosSelected()
    {
        // Draw activation radius (green)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRadius);

        // Draw hush check radius (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hushCheckRadius);

        // Draw locator sound range (cyan)
        if (locatorMaxDistance > 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, locatorMaxDistance);
            
            // Draw minimum distance for locator (blue)
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, locatorMinDistance);
        }
    }
}