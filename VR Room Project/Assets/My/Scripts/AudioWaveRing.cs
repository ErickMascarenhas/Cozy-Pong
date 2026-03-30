using UnityEngine;

public class AudioWaveRing : MonoBehaviour
{
    [Header("Referencias")]
    public AudioSource musicSource;
    public LineRenderer rightLine;
    public LineRenderer leftLine;
    [Header("Configuracoes")]
    public float radius = 5f;
    public int pointsPerLine = 60;
    public float waveHeight = 5f;
    public float smoothSpeed = 10f;
    public float baseHeight = 1f;
    [Header("Cores")]
    public float colorChangeSpeed = 0.1f;
    private float[] _leftSpectrum = new float[512];
    private float[] _rightSpectrum = new float[512];
    private float[] _smoothedLeftY;
    private float[] _smoothedRightY;

    private void Start()
    {
        _smoothedLeftY = new float[pointsPerLine];
        _smoothedRightY = new float[pointsPerLine];

        if (rightLine != null)
        {
            rightLine.positionCount = pointsPerLine;
            rightLine.useWorldSpace = false;
        }

        if (leftLine != null)
        {
            leftLine.positionCount = pointsPerLine;
            leftLine.useWorldSpace = false;
        }
    }

    private void Update()
    {
        Color synchronizedColor = Color.HSVToRGB(Mathf.Repeat(Time.time * colorChangeSpeed, 1f), 1f, 1f);

        if (rightLine != null)
        {
            rightLine.startColor = synchronizedColor;
            rightLine.endColor = synchronizedColor;
        }
        if (leftLine != null)
        {
            leftLine.startColor = synchronizedColor;
            leftLine.endColor = synchronizedColor;
        }

        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.GetSpectrumData(_leftSpectrum, 0, FFTWindow.BlackmanHarris);
            musicSource.GetSpectrumData(_rightSpectrum, 1, FFTWindow.BlackmanHarris);
        }
        else
        {
            System.Array.Clear(_leftSpectrum, 0, _leftSpectrum.Length);
            System.Array.Clear(_rightSpectrum, 0, _rightSpectrum.Length);
        }
        if (rightLine != null) UpdateLine(rightLine, _rightSpectrum, _smoothedRightY, -60f, 60f);
        if (leftLine != null) UpdateLine(leftLine, _leftSpectrum, _smoothedLeftY, 120f, 240f);
    }

    private void UpdateLine(LineRenderer line, float[] spectrum, float[] smoothedY, float startAngle, float endAngle)
    {
        float angleStep = (endAngle - startAngle) / (pointsPerLine - 1);
        int centerIndex = pointsPerLine / 2;

        for (int i = 0; i < pointsPerLine; i++)
        {
            float currentAngle = startAngle + (i * angleStep);
            float rad = currentAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * radius;
            float z = Mathf.Sin(rad) * radius;
            int distFromCenter = Mathf.Abs(i - centerIndex);
            int spectrumIndex = distFromCenter * 3;
            float audioIntensity = spectrum[spectrumIndex] * waveHeight;
            float idleWave = Mathf.Sin(Time.time * 2f + i * 0.2f) * 0.1f;
            float targetY = audioIntensity + idleWave;
            smoothedY[i] = Mathf.Lerp(smoothedY[i], targetY, Time.deltaTime * smoothSpeed);
            line.SetPosition(i, new Vector3(x, baseHeight + smoothedY[i], z));
        }
    }
}