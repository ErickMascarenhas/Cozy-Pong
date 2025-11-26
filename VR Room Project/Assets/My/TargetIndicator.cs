using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class TargetIndicator : MonoBehaviour
{
    [Header("UI do alvo")]
    public UnityEngine.UI.Image fillImage; // imagem de alvo
    public Transform arrowPivot; // seta indicando direcao
    private float _totalTime;
    private float _currentTime;
    private bool _active = false;

    public void Initialize(float duration, Vector3 lookAtTarget)
    {
        _totalTime = duration;
        _currentTime = 0;
        _active = true;
        fillImage.fillAmount = 0;
        if (arrowPivot != null)
        {
            Vector3 targetFlat = new Vector3(lookAtTarget.x, arrowPivot.position.y, lookAtTarget.z);
            arrowPivot.LookAt(targetFlat);
        }
    }

    void Update()
    {
        if (!_active) return;
        _currentTime += Time.deltaTime;
        float progress = _currentTime / _totalTime;
        if (fillImage != null) fillImage.fillAmount = progress;
        if (_currentTime >= _totalTime + 0.5f)
        {
            Destroy(gameObject);
        }
    }
}