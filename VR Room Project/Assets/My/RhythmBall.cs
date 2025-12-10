using System;
using System.Diagnostics;
using UnityEngine;

public class RhythmBall : MonoBehaviour
{
    public NoteData data;
    public Rigidbody rb;
    public TrailRenderer trail;
    public Renderer ballRenderer;

    [Header("Configuracao visual")]
    public Material normalMat;
    public Material forbiddenMat;

    [Header("Fisica do Spin")]
    [Tooltip("Forca da curva")]
    public float magnusPower = 2.0f;

    private bool isForbidden = false;
    private bool applyMagnusEffect = false;

    public void Initialize(NoteData noteData)
    {
        data = noteData;
        rb = GetComponent<Rigidbody>();
        applyMagnusEffect = false;
        switch (data.noteType)
        {
            case NoteType.Spin:
                float spinDirection = 0f;
                if ((int)data.lane < 2) // se para esquerda, curva para direita
                {
                    spinDirection = -1f;
                }
                else if ((int)data.lane > 2) // se para direita, curva para esquerda
                {
                    spinDirection = 1f;
                }
                else
                {
                    spinDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1f : 1f; // se no meio, curva random
                }
                rb.AddTorque(Vector3.up * 100f * spinDirection, ForceMode.Impulse);
                trail.startColor = Color.cyan;
                applyMagnusEffect = true;
                break;
            case NoteType.Proibida:
                isForbidden = true;
                ballRenderer.material = forbiddenMat;
                trail.startColor = Color.red;
                break;
            default:
                ballRenderer.material = normalMat;
                break;
        }
    }

    void FixedUpdate()
    {
        if (applyMagnusEffect)
        {
            Vector3 magnusForce = Vector3.Cross(rb.angularVelocity, rb.linearVelocity) * magnusPower;
            rb.AddForce(magnusForce);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Racket"))
        {
            if (isForbidden)
            {
                UnityEngine.Debug.Log("ERRO! Rebateu bola proibida!");
            }
            else
            {
                UnityEngine.Debug.Log("HIT! Bola rebatida.");
            }
        }
    }
}