using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleBallSound : MonoBehaviour
{
    [Header("Sons")]
    public AudioClip[] racketSounds;
    public AudioClip[] bounceSounds;
    [Header("Configuracoes")]
    public float minVelocityToPlay = 0.2f;
    [Range(0.0f, 1.5f)]
    public float pitchVariation = 0.1f;
    [Header("Filtros")]
    public string floorTag = "Floor";

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1.0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(floorTag) || collision.gameObject.name.ToLower().Contains("floor"))
        {
            return;
        }
        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce < minVelocityToPlay) return;
        if (collision.gameObject.CompareTag("Racket"))
        {
            RacketHaptics haptics = collision.gameObject.GetComponent<RacketHaptics>();
            if (haptics != null)
            {
                float hapticStrength = Mathf.Clamp(impactForce / 8.0f, 0.1f, 1.0f);
                haptics.TriggerVibration(hapticStrength);
            }
            PlayRandomSound(racketSounds, impactForce);
        }
        else
        {
            PlayRandomSound(bounceSounds, impactForce);
        }
    }

    private void PlayRandomSound(AudioClip[] clips, float force)
    {
        if (clips == null || clips.Length == 0) return;
        // Semeado no experimento: mesma condicao, mesma sequencia de sons.
        int index = ExperimentRandom.Range(0, clips.Length);
        AudioClip clip = clips[index];
        _audioSource.pitch = 1.0f + ExperimentRandom.Range(-pitchVariation, pitchVariation);
        float volume = Mathf.Clamp(force / 8.0f, 0.2f, 1.0f);
        _audioSource.PlayOneShot(clip, volume);
    }
}