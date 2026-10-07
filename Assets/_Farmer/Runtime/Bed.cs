using System.Collections.Generic;
using UnityEngine;

namespace Farmer
{
    public sealed class Bed : MonoBehaviour
    {
        private static readonly HashSet<Bed> active = new HashSet<Bed>();
        private void OnEnable() => active.Add(this);
        private void OnDisable() => active.Remove(this);
        public static bool IsNear(Vector3 player)
        {
            foreach (var bed in active)
            {
                var collider = bed.GetComponent<Collider>();
                Vector3 nearest = collider != null ? collider.ClosestPoint(player) : bed.transform.position;
                Vector3 offset = nearest-player; offset.y=0;
                if (offset.sqrMagnitude <= 1.8f*1.8f) return true;
            }
            return false;
        }
    }
}
