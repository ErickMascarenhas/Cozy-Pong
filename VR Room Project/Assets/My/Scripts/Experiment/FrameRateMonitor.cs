using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Amostra a taxa de quadros durante a exposicao.
///
/// Nao e vaidade tecnica: queda de taxa de quadros e causa conhecida de
/// cybersickness, e o Capitulo 4 precisa poder afirmar que a sessao de um
/// participante correu em condicoes comparaveis a dos demais. Se uma sessao
/// engasgou, isso tem de estar visivel no dado, e nao ser descoberto depois.
/// </summary>
public class FrameRateMonitor : MonoBehaviour
{
    private readonly List<float> _deltas = new List<float>(80000);
    private bool _sampling;
    private int _dropped;
    private float _expectedDelta = 1f / 72f;

    public int SampleCount { get { return _deltas.Count; } }
    public int DroppedFrames { get { return _dropped; } }

    public void BeginSampling()
    {
        _deltas.Clear();
        _dropped = 0;
        float rate = Screen.currentResolution.refreshRateRatio.value > 1f
            ? (float)Screen.currentResolution.refreshRateRatio.value
            : 72f;
        _expectedDelta = 1f / rate;
        _sampling = true;
    }

    public void EndSampling()
    {
        _sampling = false;
    }

    private void Update()
    {
        if (!_sampling) return;
        float dt = Time.unscaledDeltaTime;
        if (dt <= 0f) return;
        _deltas.Add(dt);
        // Um quadro que demora mais que o dobro do esperado significa que ao
        // menos um quadro nao foi apresentado a tempo.
        if (dt > _expectedDelta * 2f) _dropped++;
    }

    public float MedianFps { get { return FpsAtQuantile(0.5f); } }

    /// <summary>Percentil 5 da taxa de quadros: o pior desempenho tipico da sessao.</summary>
    public float Percentile05Fps { get { return FpsAtQuantile(0.95f); } }

    private float FpsAtQuantile(float deltaQuantile)
    {
        if (_deltas.Count == 0) return 0f;
        List<float> sorted = new List<float>(_deltas);
        sorted.Sort();
        int index = Mathf.Clamp(Mathf.RoundToInt(deltaQuantile * (sorted.Count - 1)), 0, sorted.Count - 1);
        float delta = sorted[index];
        return delta > 0f ? 1f / delta : 0f;
    }
}
