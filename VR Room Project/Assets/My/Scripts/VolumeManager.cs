using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public Slider VolumeSlider;

    void Awake()
    {
        // Este componente controla AudioListener.volume, um segundo caminho de
        // volume paralelo ao AudioMixer do AudioSettingsManager. Ter dois
        // caminhos torna o nivel final imprevisivel, entao no experimento este
        // fica em ganho unitario e o mixer passa a ser o unico responsavel.
        if (ExperimentMode.IsActive)
        {
            AudioListener.volume = 1f;
            if (VolumeSlider != null)
            {
                VolumeSlider.SetValueWithoutNotify(1f);
                VolumeSlider.interactable = false;
            }
            return;
        }

        if (PlayerPrefs.HasKey("soundVolume")) LoadVolume();
        else
        {
            PlayerPrefs.SetFloat("soundVolume",  1f);
            LoadVolume();
        }
    }

    public void SetVolume()
    {
        if (ExperimentMode.IsActive) return;
        AudioListener.volume = VolumeSlider.value;
        SaveVolume();
    }

    public void SaveVolume()
    {
        PlayerPrefs.SetFloat("soundVolume", VolumeSlider.value);
    }

    public void LoadVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat("soundVolume");
        if (VolumeSlider != null) VolumeSlider.SetValueWithoutNotify(savedVolume);
        AudioListener.volume = savedVolume;
    }
}
