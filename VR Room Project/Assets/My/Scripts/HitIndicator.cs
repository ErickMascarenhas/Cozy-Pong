using System;
using UnityEngine;
using UnityEngine.UI;

public class HitIndicator : MonoBehaviour
{
    [Header("UI")]
    public Image fillImage;
    public Transform arrowPivot;
    private float _totalTime;
    private float _currentTime;
    private bool _active = false;
    private bool _isPaused = false;
    private Action _onExpiredCallback;

    public void Initialize(float duration, Vector3 opponentCenterPos, Action onExpired)
    {
        _totalTime = duration;
        _currentTime = 0;
        _active = true;
        _isPaused = false;
        _onExpiredCallback = onExpired;
        if (fillImage != null) fillImage.fillAmount = 0;
        if (arrowPivot != null)
        {
            float myX = transform.position.x;
            float targetX = opponentCenterPos.x;
            float tolerance = 0.2f;
            float zRotation = 0f;
            if (Mathf.Abs(targetX - myX) <= tolerance) zRotation = 90f;
            else if (targetX < myX) zRotation = 180f;
            else zRotation = 0f;
            arrowPivot.localEulerAngles = new Vector3(0, 0, zRotation);
        }
    }

    void Update()
    {
        if (!_active || _isPaused) return;
        _currentTime += Time.deltaTime;
        if (fillImage != null) fillImage.fillAmount = _currentTime / _totalTime;
        if (_currentTime >= _totalTime + 0.15f)
        {
            var callback = _onExpiredCallback;
            _onExpiredCallback = null;
            if (callback != null)
            {
                try
                {
                    callback.Invoke();
                }
                catch (System.Exception)
                {
                }
            }
            Destroy(gameObject);
        }
    }

    public void PauseIndicator()
    {
        _isPaused = true;
    }

    public void ResumeIndicator()
    {
        _isPaused = false;
    }

}