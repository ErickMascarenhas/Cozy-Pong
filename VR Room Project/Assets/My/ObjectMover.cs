using UnityEngine;
using UnityEngine.UI;

public class ObjectMover : MonoBehaviour
{
    public GameObject objectToMove; // camera
    public float baseMoveAmount = 0.1f; // quantidade de movimento

    public void MoveX(float direction) // 1 ou -1, pra mais ou pra menos
    {
        if (objectToMove != null)
        {
            objectToMove.transform.Translate(Vector3.right * baseMoveAmount * direction, Space.Self);
        }
    }

    public void MoveY(float direction) // 1 ou -1, pra mais ou pra menos
    {
        if (objectToMove != null)
        {
            objectToMove.transform.Translate(Vector3.up * baseMoveAmount * direction, Space.Self);
        }
    }

    public void MoveZ(float direction) // 1 ou -1, pra mais ou pra menos
    {
        if (objectToMove != null)
        {
            objectToMove.transform.Translate(Vector3.forward * baseMoveAmount * direction, Space.Self);
        }
    }

    public void ResetPosition()
    {
        if (objectToMove != null)
        {
            objectToMove.transform.localPosition = Vector3.zero;
        }
    }
}
