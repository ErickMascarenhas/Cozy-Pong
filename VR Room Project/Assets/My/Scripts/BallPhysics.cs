using UnityEngine;

public static class BallPhysics
{
    public static void CalculateTrajectory(Vector3 start, Vector3 end, float time, float arcHeight, out Vector3 velocity, out float gravity)
    {
        Vector3 displacement = end - start;
        Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);
        Vector3 velocityXZ = displacementXZ / time;
        gravity = (8.0f * arcHeight) / (time * time);
        float velocityY = (displacement.y + (0.5f * gravity * time * time)) / time;
        velocity = velocityXZ + Vector3.up * velocityY;
    }
}