using UnityEngine;

namespace Farmer
{
    // Add to terrain surfaces in any region. No farm/zone identifier is required.
    public sealed class WorldGround : MonoBehaviour
    {
        public bool cultivable = true;
        public bool buildable = true;
        public static bool SupportsCell(int x, int z, bool farming)
        {
            // Entire one-metre footprint must have flat ground at the current ground layer.
            foreach (var offset in new[] { new Vector2(.05f,.05f), new Vector2(.95f,.05f), new Vector2(.05f,.95f), new Vector2(.95f,.95f) })
            {
                bool supported = false;
                foreach (var hit in Physics.RaycastAll(new Vector3(x+offset.x, .2f, z+offset.y), Vector3.down, .4f, ~0, QueryTriggerInteraction.Ignore))
                {
                    var ground = hit.collider.GetComponent<WorldGround>();
                    if (ground != null && (farming ? ground.cultivable : ground.buildable) && hit.normal.y > .98f && Mathf.Abs(hit.point.y) < .02f)
                    { supported = true; break; }
                }
                if (!supported) return false;
            }
            return true;
        }
        public static bool Obstructed(int x, int z, Transform ignoredPlayer = null)
        {
            foreach (var collider in Physics.OverlapBox(new Vector3(x+.5f,.5f,z+.5f), Vector3.one*.47f, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
                if (ignoredPlayer == null || !collider.transform.IsChildOf(ignoredPlayer)) return true;
            return false;
        }
    }
}
