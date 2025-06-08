using UnityEngine;
using UnityEngine.Audio; // Add this to use AudioMixer
using System;
using System.Collections.Generic;


public class AudioManager : MonoBehaviour
{
    // Singleton instance
    public static AudioManager Instance { get; private set; }

    // Array to hold sound data
    public Sound[] sounds;

    // Dictionary for quick lookup
    private Dictionary<string, Sound> soundDictionary;

    // Reference to the BackgroundSounds Mixer Group
    public AudioMixerGroup backgroundSoundsMixerGroup;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate
            return;
        }

        // Initialize the dictionary
        soundDictionary = new Dictionary<string, Sound>();        // Set up AudioSource components for each sound
        foreach (Sound sound in sounds)
        {
            if (sound == null)
            {
                Debug.LogError("AudioManager: Found null sound in sounds array!");
                continue;
            }
            
            if (sound.clip == null)
            {
                Debug.LogError($"AudioManager: Sound '{sound.name}' has no AudioClip assigned!");
                continue;
            }
            
            try
            {
                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = sound.clip;
                sound.source.volume = sound.volume;
                sound.source.pitch = sound.pitch;
                sound.source.loop = sound.loop;

                // Only assign mixerGroup, fallback to background if null
                sound.source.outputAudioMixerGroup = sound.mixerGroup != null
                    ? sound.mixerGroup
                    : backgroundSoundsMixerGroup;

                soundDictionary.Add(sound.name, sound);
                Debug.Log($"AudioManager: Successfully initialized sound '{sound.name}'");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AudioManager: Failed to initialize sound '{sound.name}': {e.Message}");
            }
        }
    }

    // New Start method to play sounds marked as playOnStart
    private void Start()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.playOnStart)
            {
                Play(sound.name);
            }
        }
    }    // Play a sound by name
    public void Play(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            if (sound != null && sound.source != null)
            {
                sound.source.Play();
                Debug.Log($"Playing sound: {soundName}");
            }
            else
            {
                Debug.LogError($"Sound '{soundName}' or its AudioSource is null! AudioManager may not be properly initialized.");
            }
        }
        else
        {
            Debug.LogWarning($"Sound {soundName} not found!");
        }
    }

    // Stop a sound by name
    public void Stop(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            sound.source.Stop();
        }
        else
        {
            Debug.LogWarning($"Sound {soundName} not found!");
        }
    }

    // Adjust volume for a specific sound
    public void SetVolume(string soundName, float volume)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            sound.volume = Mathf.Clamp01(volume);
            sound.source.volume = sound.volume;
        }
        else
        {
            Debug.LogWarning($"Sound {soundName} not found!");
        }
    }

    // Adjust global volume
    public void SetGlobalVolume(float volume)
    {
        foreach (Sound sound in sounds)
        {
            sound.volume = Mathf.Clamp01(volume);
            sound.source.volume = sound.volume;
        }
    }

    // Returns true if the sound is currently playing
    public bool IsPlaying(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            return sound.source.isPlaying;
        }
        else
        {
            Debug.LogWarning($"Sound {soundName} not found!");
            return false;
        }
    }

    // Validation method to check AudioManager state
    public void ValidateAudioManager()
    {
        Debug.Log($"AudioManager Validation:");
        Debug.Log($"- Instance: {(Instance != null ? "OK" : "NULL")}");
        Debug.Log($"- Sounds array: {(sounds != null ? sounds.Length.ToString() : "NULL")} sounds");
        Debug.Log($"- Sound dictionary: {(soundDictionary != null ? soundDictionary.Count.ToString() : "NULL")} entries");
        
        if (sounds != null)
        {
            foreach (Sound sound in sounds)
            {
                if (sound != null)
                {
                    Debug.Log($"- Sound '{sound.name}': Source={sound.source != null}, Clip={sound.clip != null}");
                }
            }
        }
    }
}

// Serializable class to hold sound data
[System.Serializable]
public class Sound
{
    public string name; // Name of the sound (used to reference it)
    public AudioClip clip; // The audio file
    [Range(0f, 1f)]
    public float volume = 1f; // Volume level
    [Range(0.1f, 3f)]
    public float pitch = 1f; // Pitch level
    public bool loop; // Should the sound loop?
    public bool playOnStart; // New field: Should this sound play when the game starts?

    public AudioMixerGroup mixerGroup; // New: assign per sound in Inspector

    [HideInInspector]
    public AudioSource source; // Reference to the AudioSource component
}