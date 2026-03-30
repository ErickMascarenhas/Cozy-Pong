using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public Slider VolumeSlider;
    void Awake()
    {
        if (PlayerPrefs.HasKey("soundVolume")) LoadVolume();
        else
        {
            PlayerPrefs.SetFloat("soundVolume",  1f);
            LoadVolume();
        }
    }

    public void SetVolume()
    {
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
