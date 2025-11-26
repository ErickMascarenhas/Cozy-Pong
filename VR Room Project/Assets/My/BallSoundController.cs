using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BallSoundController : MonoBehaviour
{
    [Tooltip("Tag para checar colisao")]
    public string racketTag = "Racket";
    [Tooltip("Sons de hit")]
    public AudioClip[] racketHitSounds;
    [Tooltip("Sons de bounce")]
    public AudioClip[] bounceSounds;
    [Tooltip("Velocidade minima para tocar o som (se nao toca enquanto rola)")]
    public float minVelocityToPlay = 0.5f;
    [Range(0.5f, 1.5f)]
    public float pitchVariation = 0.1f;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1.0f; // som 3d
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce < minVelocityToPlay) return;
        AudioClip[] clipsToUse;
        if (collision.gameObject.CompareTag(racketTag))  // se bater na raquete
        {
            clipsToUse = racketHitSounds;
            RacketHaptics haptics = collision.gameObject.GetComponent<RacketHaptics>(); // para feedback tatico
            if (haptics != null)
            {
                float hapticStrength = Mathf.Clamp(impactForce / 8.0f, 0.1f, 1.0f);
                haptics.TriggerVibration(hapticStrength);
            }
        }
        else // senao
        {
            clipsToUse = bounceSounds;
        }

        PlayRandomSound(clipsToUse, impactForce);
    }

    private void PlayRandomSound(AudioClip[] clips, float force)
    {
        if (clips.Length == 0) return;
        int index = UnityEngine.Random.Range(0, clips.Length);
        AudioClip clip = clips[index];
        _audioSource.pitch = 1.0f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
        float volume = Mathf.Clamp(force / 8.0f, 0.2f, 1.0f);
        _audioSource.PlayOneShot(clip, volume);
    }
}