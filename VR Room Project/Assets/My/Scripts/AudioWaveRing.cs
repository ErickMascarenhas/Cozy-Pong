using UnityEngine;

public enum VisualStyle
{
    NeonWave = 0,
    SharpBars = 1,
    PulsingCircle = 2
}

public class AudioWaveRing : MonoBehaviour
{
    [Header("Referencias")]
    public AudioSource musicSource;
    [Header("Estilo Atual")]
    public VisualStyle currentStyle = VisualStyle.NeonWave;
    [Header("Configuracoes")]
    public float radius = 5f;
    public int pointsPerLine = 60;
    public float baseHeight = 1f;
    public float colorChangeSpeed = 0.1f;
    [Range(1f, 3f)] public float frequencyCompression = 1.3f;
    public int maxFrequencyBand = 80;
    [Header("Neon Wave")]
    public LineRenderer rightLine;
    public LineRenderer leftLine;
    public float waveHeight = 5f;
    public float smoothSpeed = 15f;
    [Header("Sharp Bars")]
    public GameObject barPrefab;
    public float maxBarHeight = 4f;
    public float barThickness = 0.1f;
    public float barSmoothSpeed = 25f;
    [Header("Pulsing Circle")]
    public LineRenderer circleLine;
    public int circlePoints = 120;
    public float pulseIntensity = 1.5f;
    public float circleRadius = 4f;
    public Vector3 circleOffset = new Vector3(0f, 1.5f, 4f);
    public float circleRotation = 90f;

    private float[] _leftSpectrum = new float[512];
    private float[] _rightSpectrum = new float[512];
    private float[] _smoothedLeftY;
    private float[] _smoothedRightY;
    private float[] _smoothedCircleY;
    private float _smoothedPulse;
    private GameObject[] _rightBars;
    private GameObject[] _leftBars;
    private Material[] _rightBarMaterials;
    private Material[] _leftBarMaterials;

    private void Start()
    {
        _smoothedLeftY = new float[pointsPerLine];
        _smoothedRightY = new float[pointsPerLine];
        _smoothedCircleY = new float[circlePoints];

        if (rightLine != null) { rightLine.positionCount = pointsPerLine; rightLine.useWorldSpace = false; }
        if (leftLine != null) { leftLine.positionCount = pointsPerLine; leftLine.useWorldSpace = false; }

        if (circleLine != null)
        {
            circleLine.positionCount = circlePoints;
            circleLine.useWorldSpace = false;
            circleLine.loop = true;
        }
        CreatePhysicalBars();
        UpdateVisualMode();
    }

    private void CreatePhysicalBars()
    {
        _rightBars = new GameObject[pointsPerLine];
        _leftBars = new GameObject[pointsPerLine];
        _rightBarMaterials = new Material[pointsPerLine];
        _leftBarMaterials = new Material[pointsPerLine];
        float rightAngleStep = 120f / (pointsPerLine - 1);
        float leftAngleStep = 120f / (pointsPerLine - 1);
        for (int i = 0; i < pointsPerLine; i++)
        {
            if (barPrefab != null)
            {
                float rAngle = -60f + (i * rightAngleStep);
                _rightBars[i] = InstantiateBar(rAngle, "RightBar_" + i, out _rightBarMaterials[i]);

                float lAngle = 120f + (i * leftAngleStep);
                _leftBars[i] = InstantiateBar(lAngle, "LeftBar_" + i, out _leftBarMaterials[i]);
            }
        }
    }

    private GameObject InstantiateBar(float angle, string name, out Material mat)
    {
        GameObject bar = Instantiate(barPrefab, transform);
        bar.name = name;
        float rad = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * radius;
        float z = Mathf.Sin(rad) * radius;
        bar.transform.localPosition = new Vector3(x, baseHeight, z);
        bar.transform.LookAt(transform.position);
        bar.transform.localEulerAngles = new Vector3(0, bar.transform.localEulerAngles.y, 0);
        mat = bar.GetComponent<Renderer>().material;
        bar.SetActive(false);
        return bar;
    }

    public void SetVisualStyle(int styleIndex)
    {
        currentStyle = (VisualStyle)styleIndex;
        UpdateVisualMode();
    }

    private void UpdateVisualMode()
    {
        if (rightLine != null) rightLine.gameObject.SetActive(currentStyle == VisualStyle.NeonWave);
        if (leftLine != null) leftLine.gameObject.SetActive(currentStyle == VisualStyle.NeonWave);

        if (circleLine != null) circleLine.gameObject.SetActive(currentStyle == VisualStyle.PulsingCircle);

        bool useBars = (currentStyle == VisualStyle.SharpBars);
        if (_rightBars != null && _rightBars.Length > 0 && _rightBars[0] != null)
        {
            for (int i = 0; i < pointsPerLine; i++)
            {
                if (_rightBars[i] != null) _rightBars[i].SetActive(useBars);
                if (_leftBars[i] != null) _leftBars[i].SetActive(useBars);
            }
        }
    }

    private void Update()
    {
        Color synchronizedColor = Color.HSVToRGB(Mathf.Repeat(Time.time * colorChangeSpeed, 1f), 1f, 1f);

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
        if (currentStyle == VisualStyle.NeonWave)
        {
            if (rightLine != null) { rightLine.startColor = synchronizedColor; rightLine.endColor = synchronizedColor; UpdateLine(rightLine, _rightSpectrum, _smoothedRightY, -60f, 60f); }
            if (leftLine != null) { leftLine.startColor = synchronizedColor; leftLine.endColor = synchronizedColor; UpdateLine(leftLine, _leftSpectrum, _smoothedLeftY, 120f, 240f); }
        }
        else if (currentStyle == VisualStyle.SharpBars)
        {
            UpdateBars(_rightBars, _rightBarMaterials, _rightSpectrum, _smoothedRightY, synchronizedColor);
            UpdateBars(_leftBars, _leftBarMaterials, _leftSpectrum, _smoothedLeftY, synchronizedColor);
        }
        else if (currentStyle == VisualStyle.PulsingCircle)
        {
            if (circleLine != null)
            {
                circleLine.startColor = synchronizedColor;
                circleLine.endColor = synchronizedColor;
                UpdateFullCircle(circleLine, _leftSpectrum);
            }
        }
    }

    private void UpdateLine(LineRenderer line, float[] spectrum, float[] smoothedY, float startAngle, float endAngle)
    {
        float angleStep = (endAngle - startAngle) / (pointsPerLine - 1);
        int centerIndex = pointsPerLine / 2;
        for (int i = 0; i < pointsPerLine; i++)
        {
            int distFromCenter = Mathf.Abs(i - centerIndex);
            int spectrumIndex = (int)Mathf.Pow(distFromCenter, frequencyCompression);
            if (spectrumIndex > maxFrequencyBand) spectrumIndex = maxFrequencyBand;
            float audioIntensity = spectrum[spectrumIndex] * waveHeight;
            float idleWave = Mathf.Sin(Time.time * 2f + i * 0.2f) * 0.1f;
            smoothedY[i] = Mathf.Lerp(smoothedY[i], audioIntensity + idleWave, Time.deltaTime * smoothSpeed);
            float currentAngle = startAngle + (i * angleStep);
            float rad = currentAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * radius;
            float z = Mathf.Sin(rad) * radius;
            line.SetPosition(i, new Vector3(x, baseHeight + smoothedY[i], z));
        }
    }

    private void UpdateBars(GameObject[] bars, Material[] mats, float[] spectrum, float[] smoothedY, Color syncColor)
    {
        if (bars == null || bars.Length == 0 || bars[0] == null) return;
        int centerIndex = pointsPerLine / 2;

        for (int i = 0; i < pointsPerLine; i++)
        {
            int distFromCenter = Mathf.Abs(i - centerIndex);
            int spectrumIndex = (int)Mathf.Pow(distFromCenter, frequencyCompression);
            if (spectrumIndex > maxFrequencyBand) spectrumIndex = maxFrequencyBand;

            float audioIntensity = spectrum[spectrumIndex] * maxBarHeight;
            float targetHeight = 0.1f + audioIntensity;
            smoothedY[i] = Mathf.Lerp(smoothedY[i], targetHeight, Time.deltaTime * barSmoothSpeed);

            bars[i].transform.localScale = new Vector3(barThickness, smoothedY[i], barThickness);
            bars[i].transform.localPosition = new Vector3(
                bars[i].transform.localPosition.x,
                baseHeight + (smoothedY[i] / 2f),
                bars[i].transform.localPosition.z
            );

            mats[i].color = syncColor;
        }
    }

    private void UpdateFullCircle(LineRenderer line, float[] spectrum)
    {
        float angleStep = 360f / circlePoints;
        float globalBassPulse = (spectrum[1] + spectrum[2] + spectrum[3]) * pulseIntensity;
        _smoothedPulse = Mathf.Lerp(_smoothedPulse, globalBassPulse, Time.deltaTime * smoothSpeed);
        float baseCircleRadius = circleRadius + _smoothedPulse;
        int halfPoints = circlePoints / 2;
        for (int i = 0; i < circlePoints; i++)
        {
            float rad = ((i * angleStep) + circleRotation) * Mathf.Deg2Rad;
            int mirrorI = i > halfPoints ? circlePoints - i : i;
            int spectrumIndex = (int)Mathf.Pow(mirrorI, frequencyCompression * 0.9f);
            if (spectrumIndex > maxFrequencyBand) spectrumIndex = maxFrequencyBand;
            float audioSpike = spectrum[spectrumIndex] * waveHeight;
            _smoothedCircleY[i] = Mathf.Lerp(_smoothedCircleY[i], audioSpike, Time.deltaTime * smoothSpeed);
            float finalPointRadius = baseCircleRadius + _smoothedCircleY[i];
            float x = Mathf.Cos(rad) * finalPointRadius;
            float y = Mathf.Sin(rad) * finalPointRadius;
            line.SetPosition(i, new Vector3(x + circleOffset.x, y + circleOffset.y, circleOffset.z));
        }
    }
}