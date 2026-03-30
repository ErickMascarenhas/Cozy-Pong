using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class RacketHaptics : MonoBehaviour
{
    [Tooltip("Componente com Haptic Impulse Player")]
    public HapticImpulsePlayer hapticPlayer;
    [Tooltip("Duracao")]
    public float defaultDuration = 0.1f;
    public void TriggerVibration(float strength)
    {
        if (hapticPlayer != null)
        {
            hapticPlayer.SendHapticImpulse(strength, defaultDuration);
        }
    }
}