using UnityEngine;

[RequireComponent(typeof(Light))]
public class AudioReactLight : MonoBehaviour
{
    [Header("Referencias")]
    public AudioSource musicSource;

    [Header("Cor")]
    [Tooltip("Cicla o matiz continuamente. Desligado no experimento: luz saturada e " +
             "pulsante contradiz a ambientacao relaxante e e o estimulo associado a " +
             "crise fotossensivel declarada no TCLE.")]
    public bool cycleHue = false;
    [Tooltip("Cor usada quando o ciclo de matiz esta desligado.")]
    public Color fixedColor = new Color(1f, 0.85f, 0.65f);
    public float colorChangeSpeed = 0.1f;

    [Header("Configuracoes")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 3.0f;
    public float sensitivity = 15f;
    public float smoothSpeed = 12f;

    private Light _light;
    private float[] _spectrum = new float[512];

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    private void Update()
    {
        _light.color = cycleHue
            ? Color.HSVToRGB(Mathf.Repeat(Time.time * colorChangeSpeed, 1f), 1f, 1f)
            : fixedColor;

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
