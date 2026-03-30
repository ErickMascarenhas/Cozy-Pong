using UnityEngine;

[RequireComponent(typeof(Light))]
public class AudioReactLight : MonoBehaviour
{
    [Header("Referências")]
    public AudioSource musicSource;
    [Header("Configuracoes")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 3.0f;
    public float sensitivity = 15f;
    public float smoothSpeed = 12f;
    [Header("Cores")]
    public float colorChangeSpeed = 0.1f;
    private Light _light;
    private float[] _spectrum = new float[512];

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    private void Update()
    {
        _light.color = Color.HSVToRGB(Mathf.Repeat(Time.time * colorChangeSpeed, 1f), 1f, 1f);
        float targetIntensity = minIntensity;
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
            float bassSum = 0f;
            for (int i = 0; i < 10; i++) bassSum += _spectrum[i];
            targetIntensity = minIntensity + (bassSum * sensitivity);
            targetIntensity = Mathf.Clamp(targetIntensity, minIntensity, maxIntensity);
        }
        _light.intensity = Mathf.Lerp(_light.intensity, targetIntensity, Time.deltaTime * smoothSpeed);
    }
}