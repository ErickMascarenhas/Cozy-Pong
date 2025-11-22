using System.Diagnostics;
using UnityEngine;

public class BallDestroyer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball")) Destroy(other.gameObject);
    }
}