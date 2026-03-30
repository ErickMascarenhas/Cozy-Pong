using UnityEngine;

public class LaneManager : MonoBehaviour
{
    public static LaneManager Instance;
    [Header("Mesa do jogador")]
    public Transform[] playerLaneTargets;
    [Header("Pontos de hit pro jogador no ar")]
    public Transform[] playerAirTargets;
    [Header("Mesa inimiga")]
    public Transform[] opponentTableTargets;
    [Header("Pontos de hit pro inimigo no ar")]
    public Transform[] opponentAirTargets;
    void Awake() { Instance = this; }
    public Vector3 GetPlayerLanePos(int lane) => playerLaneTargets[Mathf.Clamp(lane, 0, 4)].position;
    public Vector3 GetPlayerAirPos(int lane) => playerAirTargets[Mathf.Clamp(lane, 0, 4)].position;
    public Vector3 GetOpponentTablePos(int lane) => opponentTableTargets[Mathf.Clamp(lane, 0, 4)].position;
    public Vector3 GetOpponentAirPos(int lane) => opponentAirTargets[Mathf.Clamp(lane, 0, 4)].position;
}