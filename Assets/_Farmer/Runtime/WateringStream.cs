using UnityEngine;
using UnityEngine.Rendering;

namespace Farmer
{
    // Small reusable shower strands; visual only, with no collision or watering transactions.
    internal sealed class WateringStream
    {
        private readonly GameObject root;
        private readonly Material material;
        private readonly LineRenderer[] lines = new LineRenderer[7];
        public bool Visible { get; private set; }
        public WateringStream(Transform parent)
        {
            root = new GameObject("Watering Stream"); root.transform.SetParent(parent, false);
            // Already referenced by the scene's selection outline, including in release builds.
            material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            material.SetColor("_BaseColor", Color.white);
            for (int i = 0; i < lines.Length; i++)
            {
                var obj = new GameObject("Water Strand " + i); obj.transform.SetParent(root.transform, false);
                var line = obj.AddComponent<LineRenderer>(); lines[i] = line;
                line.sharedMaterial = material; line.positionCount = 9; line.useWorldSpace = true;
                line.shadowCastingMode = ShadowCastingMode.Off; line.receiveShadows = false;
                line.numCapVertices = 2; line.enabled = false;
                line.startColor = new Color(.64f, .86f, .94f); line.endColor = new Color(.78f, .92f, .97f);
            }
        }
        public void Draw(Vector3 spout, Vector3 target, float strength)
        {
            Visible = strength > .015f;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i]; line.enabled = Visible;
                if (!Visible) continue;
                float angle = i * 2.39996f;
                Vector3 spread = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (.04f + i * .015f);
                Vector3 end = target + spread;
                line.startWidth = .011f * Mathf.Sqrt(strength); line.endWidth = .005f * Mathf.Sqrt(strength);
                for (int j = 0; j < line.positionCount; j++)
                {
                    float t = (float)j / (line.positionCount - 1);
                    // Slight strand motion keeps the flow from looking like a rigid beam.
                    float ripple = Mathf.Sin(Time.time * 20 - t * 14 + i) * .012f * t;
                    Vector3 point = Vector3.Lerp(spout + spread * .09f, end, t);
                    point.y += .18f * Mathf.Sin(Mathf.PI * t);
                    point.x += ripple; point.z += ripple * .4f;
                    line.SetPosition(j, point);
                }
            }
        }
        public void Hide() { Visible = false; foreach (var line in lines) line.enabled = false; }
        public void Dispose() { Object.Destroy(root); Object.Destroy(material); }
    }
}
