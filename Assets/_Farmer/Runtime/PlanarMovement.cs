using UnityEngine;

namespace Farmer
{
    public static class PlanarMovement
    {
        public static Vector3 Direction(Vector2 input, Vector3 cameraForward, Vector3 cameraRight)
        {
            cameraForward.y = 0;
            cameraRight.y = 0;
            input = Vector2.ClampMagnitude(input, 1f);
            return cameraForward.normalized * input.y + cameraRight.normalized * input.x;
        }

        public static Vector3 ClampToGround(Vector3 position, float halfExtent)
        {
            position.x = Mathf.Clamp(position.x, -halfExtent, halfExtent);
            position.z = Mathf.Clamp(position.z, -halfExtent, halfExtent);
            return position;
        }
    }
}
