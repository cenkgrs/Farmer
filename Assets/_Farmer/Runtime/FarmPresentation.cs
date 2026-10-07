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
        private AudioSource wateringAudio;
        private AudioClip wateringSound;
        private const float WateringVolume = 0.064f; // 20% of the previous 0.32 source gain.
        private const float WateringReleaseSeconds = 0.7f;
        private bool pouringAudio;
        private float releaseElapsed, releaseVolume;
        private readonly GameObject[] heldItems = new GameObject[3];
        private readonly Material[] toolMaterials = new Material[3];

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
            wateringSound = CreateWateringSound();
            wateringAudio = gameObject.AddComponent<AudioSource>();
            wateringAudio.playOnAwake = false; wateringAudio.loop = true;
            wateringAudio.clip = wateringSound; wateringAudio.volume = 0;
            game.WateringChanged += OnWatering;
            OnWatering(game.WateringActive);
            CreateHeldItems();
            game.Changed += Refresh; game.Harvested += OnHarvest; Refresh();
        }
        private void OnDestroy()
        {
            if (game != null) { game.Changed -= Refresh; game.Harvested -= OnHarvest; game.WateringChanged -= OnWatering; }
            if (harvestSound != null) Destroy(harvestSound);
            if (wateringSound != null) Destroy(wateringSound);
            foreach (var material in toolMaterials) if (material != null) Destroy(material);
            foreach (var item in heldItems) if (item != null) Destroy(item);
        }
        private void Update()
        {
            if (wateringAudio == null || !wateringAudio.isPlaying) return;
            if (!game.isActiveAndEnabled) { StopWateringAudio(); return; }
            if (pouringAudio)
                wateringAudio.volume = Mathf.MoveTowards(wateringAudio.volume, WateringVolume, Time.unscaledDeltaTime * WateringVolume / 0.12f);
            else
            {
                releaseElapsed += Time.unscaledDeltaTime;
                float remaining = 1f - Mathf.Clamp01(releaseElapsed / WateringReleaseSeconds);
                wateringAudio.volume = releaseVolume * remaining * remaining;
                if (remaining <= 0) StopWateringAudio();
            }
        }
        private void OnDisable() => StopWateringAudio();
        private void OnApplicationFocus(bool focused) { if (!focused) StopWateringAudio(); }
        private void OnApplicationPause(bool paused) { if (paused) StopWateringAudio(); }
        private void StopWateringAudio()
        {
            pouringAudio = false;
            if (wateringAudio == null) return;
            wateringAudio.Stop(); wateringAudio.volume = 0;
        }
        private void OnWatering(bool active)
        {
            if (!isActiveAndEnabled || !game.isActiveAndEnabled || !Application.isFocused)
            { StopWateringAudio(); return; }
            pouringAudio = active;
            if (active)
            {
                // Re-gripping during the tail reuses the playing source without a restart or overlap.
                if (!wateringAudio.isPlaying) { wateringAudio.volume = 0; wateringAudio.Play(); }
            }
            else
            {
                releaseElapsed = 0;
                releaseVolume = wateringAudio.volume;
            }
        }
        private static AudioClip CreateWateringSound()
        {
            // Soft close-up trickle: overlapping rounded droplets, with almost no broadband hiss.
            // Crossfade the end into pre-roll for a seamless four-second loop.
            const int rate = 22050, length = rate * 4, blend = rate / 8;
            var raw = new float[length + blend];
            var random = new System.Random(7319);
            float low = 0, smooth = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                low += 0.08f * ((float)random.NextDouble() * 2 - 1 - low);
                smooth += 0.08f * (low - smooth);
                raw[i] = smooth * 0.06f;
            }
            for (int start = 0; start < raw.Length; start += random.Next(rate / 60, rate / 22))
            {
                float frequency = 550 + (float)random.NextDouble() * 650;
                float amplitude = 0.13f + (float)random.NextDouble() * 0.10f;
                for (int j = 0; j < rate / 9 && start + j < raw.Length; j++)
                {
                    float time = (float)j / rate;
                    float envelope = (1f - Mathf.Exp(-time * 400f)) * Mathf.Exp(-time * 65f);
                    float phase = 2 * Mathf.PI * frequency * (time + time * time * 1.4f);
                    raw[start + j] += amplitude * envelope * (Mathf.Sin(phase) + 0.18f * Mathf.Sin(phase * 1.71f));
                }
            }
            var samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                samples[i] = raw[i + blend];
                if (i >= length - blend)
                {
                    int j = i - (length - blend);
                    samples[i] = Mathf.Lerp(samples[i], raw[j], (float)j / (blend - 1));
                }
            }
            var clip = AudioClip.Create("Watering Pour", length, 1, rate, false);
            clip.SetData(samples, 0); return clip;
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
        private void CreateHeldItems()
        {
            Color[] colors = { new Color(0.86f, 0.67f, 0.35f), new Color(0.22f, 0.64f, 0.73f), new Color(0.78f, 0.82f, 0.80f) };
            for (int i = 0; i < 3; i++)
            {
                toolMaterials[i] = new Material(soil[0].sharedMaterial);
                toolMaterials[i].SetColor("_BaseColor", colors[i]);
                heldItems[i] = new GameObject("Held " + (FarmItem)i);
                heldItems[i].transform.SetParent(game.Player.GetComponent<PlayerMotor>().Visual, false);
                heldItems[i].transform.localPosition = new Vector3(0.38f, 0.8f, 0.24f);
            }
            Part(0, PrimitiveType.Cube, Vector3.zero, new Vector3(0.28f, 0.36f, 0.18f), 0);
            Part(0, PrimitiveType.Sphere, new Vector3(0, 0, 0.11f), Vector3.one * 0.13f, 2);
            Part(1, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.35f, 0.17f, 0.35f), 1);
            var spout = Part(1, PrimitiveType.Cylinder, new Vector3(0, 0.04f, 0.27f), new Vector3(0.08f, 0.2f, 0.08f), 1);
            spout.localRotation = Quaternion.Euler(65, 0, 0);
            Part(1, PrimitiveType.Cube, new Vector3(0, 0.25f, 0), new Vector3(0.27f, 0.045f, 0.06f), 2);
            Part(1, PrimitiveType.Cube, new Vector3(-0.12f, 0.17f, 0), new Vector3(0.045f, 0.15f, 0.06f), 2);
            Part(1, PrimitiveType.Cube, new Vector3(0.12f, 0.17f, 0), new Vector3(0.045f, 0.15f, 0.06f), 2);
            Part(2, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.07f, 0.27f, 0.07f), 0);
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 25f * Mathf.Deg2Rad;
                var blade = Part(2, PrimitiveType.Cube, new Vector3(Mathf.Sin(angle) * 0.28f, 0.25f + Mathf.Cos(angle) * 0.28f, 0), new Vector3(0.16f, 0.07f, 0.05f), 2);
                blade.localRotation = Quaternion.Euler(0, 0, -i * 25f);
            }
        }
        private Transform Part(int item, PrimitiveType shape, Vector3 position, Vector3 scale, int material)
        {
            var obj = GameObject.CreatePrimitive(shape);
            Destroy(obj.GetComponent<Collider>());
            obj.transform.SetParent(heldItems[item].transform, false);
            obj.transform.localPosition = position; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = toolMaterials[material];
            return obj.transform;
        }
        private void Refresh()
        {
            for (int i = 0; i < heldItems.Length; i++)
                heldItems[i].SetActive(game.Model.EquippedItem == (FarmItem)i && game.Model.ItemCount((FarmItem)i, game.ActiveCrop.id) > 0);
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
