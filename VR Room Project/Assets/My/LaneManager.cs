using UnityEngine;

public class LaneManager : MonoBehaviour
{
    public static LaneManager Instance;
    [Header("Pontos de impacto na mesa")]
    public Transform[] laneTargets; // 5 pontos na mesa do jogador

    void Awake()
    {
        Instance = this;
    }

    public Vector3 GetLanePosition(Lane lane)
    {
        int index = (int)lane;
        if (index >= 0 && index < laneTargets.Length)
        {
            return laneTargets[index].position;
        }
        return transform.position;
    }
}