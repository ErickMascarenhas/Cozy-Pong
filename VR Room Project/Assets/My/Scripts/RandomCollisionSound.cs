using System;
using System.Diagnostics;
using UnityEngine;

public class RandomCollisionSound : MonoBehaviour
{
    public AudioClip[] collisionSounds;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            if (collisionSounds.Length > 0 && audioSource != null)
            {
                int randomIndex = UnityEngine.Random.Range(0, collisionSounds.Length);
                AudioClip randomClip = collisionSounds[randomIndex];
                audioSource.PlayOneShot(randomClip);
            }
        }
    }
}