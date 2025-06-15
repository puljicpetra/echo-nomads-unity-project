using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    void Start()
    {
        if (volumeSlider == null)
        {
            Debug.LogError("Volume Slider nije postavljen u Inspector prozoru!");
            return;
        }

        volumeSlider.minValue = 0;
        volumeSlider.maxValue = 100;

        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);

        float savedVolume = PlayerPrefs.GetFloat("MasterVolumeValue", 100f);
        volumeSlider.value = savedVolume;
        OnVolumeSliderChanged(savedVolume);
    }

    public void OnVolumeSliderChanged(float value)
    {
        float normalizedVolume = value / 100.0f;
        AudioListener.volume = normalizedVolume;
        PlayerPrefs.SetFloat("MasterVolumeValue", value);
    }
}