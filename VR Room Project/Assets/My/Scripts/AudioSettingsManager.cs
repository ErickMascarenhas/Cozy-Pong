using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixer mainMixer;
    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        // Durante o experimento o volume e fixado pela condicao e nao pode ser
        // alterado: a trilha de C1 e a de C3 precisam chegar ao participante no
        // mesmo nivel, e o nivel nao pode variar entre participantes.
        if (ExperimentMode.IsActive)
        {
            if (masterSlider != null) masterSlider.interactable = false;
            if (musicSlider != null) musicSlider.interactable = false;
            if (sfxSlider != null) sfxSlider.interactable = false;
            return;
        }

        if (masterSlider != null) masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 0.75f);
        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.75f);
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        SetMasterVolume(masterSlider.value);
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
    }

    public void SetMasterVolume(float sliderValue)
    {
        float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        mainMixer.SetFloat("MasterVolume", db);
        PlayerPrefs.SetFloat("MasterVol", sliderValue);
    }

    public void SetMusicVolume(float sliderValue)
    {
        float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        mainMixer.SetFloat("MusicVolume", db);
        PlayerPrefs.SetFloat("MusicVol", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        mainMixer.SetFloat("SFXVolume", db);
        PlayerPrefs.SetFloat("SFXVol", sliderValue);
    }
}