using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleBallSound : MonoBehaviour
{
    public AudioClip[] hitSounds;
    public float minVelocityToPlay = 0.2f;
    [Range(0.0f, 1.5f)]
    public float pitchVariation = 0.1f;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1.0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce < minVelocityToPlay) return;
        if (collision.gameObject.CompareTag("Racket"))  // se bater na raquete
        {
            RacketHaptics haptics = collision.gameObject.GetComponent<RacketHaptics>(); // feedback tatico
            if (haptics != null)
            {
                float hapticStrength = Mathf.Clamp(impactForce / 8.0f, 0.1f, 1.0f);
                haptics.TriggerVibration(hapticStrength);
            }
        }
        PlayRandomSound(hitSounds, impactForce);
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