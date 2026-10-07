using System.Collections;
using UnityEngine;

namespace Farmer
{
    public sealed class FarmPresentation : MonoBehaviour
    {
        [SerializeField] private FarmGame game;
        [SerializeField] private Renderer[] soil;
        private GameObject[] plants;
        private int[] shownStages;
        private string[] shownCrops;
        private Color[] dryColors;
        private MaterialPropertyBlock block;
        private AudioSource audioSource;
        private AudioClip harvestSound;

        public void Configure(FarmGame source, Renderer[] cells) { game = source; soil = cells; }
        private void Start()
        {
            block = new MaterialPropertyBlock();
            plants = new GameObject[soil.Length]; shownStages = new int[soil.Length]; shownCrops = new string[soil.Length];
            dryColors = new Color[soil.Length];
            for (int i = 0; i < soil.Length; i++) { shownStages[i] = -1; dryColors[i] = soil[i].sharedMaterial.GetColor("_BaseColor"); }
            audioSource = gameObject.AddComponent<AudioSource>(); audioSource.playOnAwake = false;
            const int rate = 22050;
            var samples = new float[rate / 4];
            for (int i = 0; i < samples.Length; i++)
            {
                float time = (float)i / rate;
                float envelope = Mathf.Sin(Mathf.PI * i / samples.Length) * Mathf.Exp(-time * 10f);
                samples[i] = envelope * (Mathf.Sin(2 * Mathf.PI * 880 * time) + 0.3f * Mathf.Sin(2 * Mathf.PI * 1320 * time)) * 0.3f;
            }
            harvestSound = AudioClip.Create("Harvest Chime", samples.Length, 1, rate, false);
            harvestSound.SetData(samples, 0);
            game.Changed += Refresh; game.Harvested += OnHarvest; Refresh();
        }
        private void OnDestroy()
        {
            if (game != null) { game.Changed -= Refresh; game.Harvested -= OnHarvest; }
            if (harvestSound != null) Destroy(harvestSound);
        }
        private void OnHarvest(int index, string label)
        {
            audioSource.PlayOneShot(harvestSound, 0.5f);
            StartCoroutine(FloatHarvest(index, label));
        }
        private IEnumerator FloatHarvest(int index, string label)
        {
            var obj = new GameObject("Harvest Feedback"); obj.transform.SetParent(transform);
            Vector3 start = game.Selection.Layout.Center(new Vector2Int(index % game.Model.Width, index / game.Model.Width), 1.7f);
            obj.transform.rotation = Camera.main.transform.rotation;
            var text = obj.AddComponent<TextMesh>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            obj.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            text.text = label; text.fontSize = 64; text.characterSize = 0.06f; text.anchor = TextAnchor.MiddleCenter;
            for (float elapsed = 0; elapsed < 1.2f; elapsed += Time.deltaTime)
            {
                obj.transform.position = start + Vector3.up * elapsed * 0.6f;
                text.color = new Color(1f, 0.9f, 0.5f, 1f - elapsed / 1.2f);
                yield return null;
            }
            Destroy(obj);
        }
        private void Refresh()
        {
            for (int i = 0; i < soil.Length; i++)
            {
                var plot = game.Model.Plot(i);
                block.SetColor("_BaseColor", plot.watered ? new Color(0.16f, 0.105f, 0.075f) : dryColors[i]);
                soil[i].SetPropertyBlock(block);
                int stage = game.Model.Stage(i);
                if (stage == shownStages[i] && plot.cropId == shownCrops[i]) continue;
                if (plants[i] != null) Destroy(plants[i]);
                plants[i] = null; shownStages[i] = stage; shownCrops[i] = plot.cropId;
                if (stage < 0) continue;
                var definition = game.Definition(plot.cropId);
                plants[i] = Instantiate(definition.growthStages[stage], transform);
                plants[i].name = $"{plot.cropId} {i} stage {stage}";
                plants[i].transform.position = game.Selection.Layout.Center(new Vector2Int(i % game.Model.Width, i / game.Model.Width), 0.06f);
            }
        }
    }
}
