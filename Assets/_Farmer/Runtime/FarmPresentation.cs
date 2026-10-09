using System.Collections;
using UnityEngine;

namespace Farmer
{
    public sealed class FarmPresentation : MonoBehaviour
    {
        [SerializeField] private FarmGame game;
        [SerializeField] private Renderer[] soil;
        private GameObject[] plants;
        private Renderer soilTemplate;
        private FarmModel observedModel;
        private int[] shownStages;
        private string[] shownCrops;
        private Color[] dryColors;
        private MaterialPropertyBlock block;
        private AudioSource audioSource;
        private AudioClip harvestSound;
        private AudioSource wateringAudio;
        [SerializeField] private AudioClip wateringSound;
        [SerializeField] private Transform heldItemSocket;
        [SerializeField] private GameObject wateringCanPrefab;
        [SerializeField] private GameObject sicklePrefab;
        private const float WateringVolume = 0.0832f; // 30% above the previous 0.064 source gain.
        private const float WateringReleaseSeconds = 0.7f;
        private bool pouringAudio;
        private float releaseElapsed, releaseVolume;
        private readonly GameObject[] heldItems = new GameObject[5];
        private readonly Material[] toolMaterials = new Material[3];

        public float WateringIntensity => wateringAudio != null && wateringAudio.isPlaying ? Mathf.Clamp01(wateringAudio.volume / WateringVolume) : 0;

        public void Configure(FarmGame source, Renderer[] cells) { game = source; soil = cells; }
        private void Start()
        {
            block = new MaterialPropertyBlock();
            soilTemplate = soil[0];
            foreach (var renderer in soil) renderer.gameObject.SetActive(false);
            soil = new Renderer[0]; plants = new GameObject[0];
            shownStages = new int[0]; shownCrops = new string[0]; dryColors = new Color[0];
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
            if (wateringSound == null) throw new System.InvalidOperationException("Watering recording is not assigned.");
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
        private void OnHarvest(int index, string label)
        {
            audioSource.PlayOneShot(harvestSound, 0.5f);
            StartCoroutine(FloatHarvest(index, label));
        }
        private IEnumerator FloatHarvest(int index, string label)
        {
            var obj = new GameObject("Harvest Feedback"); obj.transform.SetParent(transform);
            Vector3 start = game.PlotCenter(index, 1.7f);
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
            if (heldItemSocket == null || wateringCanPrefab == null || sicklePrefab == null)
                throw new System.InvalidOperationException("Held item art is not assigned.");
            Color[] colors = { new Color(0.86f, 0.67f, 0.35f), new Color(0.22f, 0.64f, 0.73f), new Color(0.78f, 0.82f, 0.80f) };
            for (int i = 0; i < 3; i++)
            {
                toolMaterials[i] = new Material(soilTemplate.sharedMaterial);
                toolMaterials[i].SetColor("_BaseColor", colors[i]);
            }
            heldItems[0] = new GameObject("Held Seeds");
            heldItems[0].transform.SetParent(heldItemSocket, false);
            Part(0, PrimitiveType.Cube, new Vector3(0, -.16f, 0), new Vector3(.22f, .3f, .15f), 0);
            Part(0, PrimitiveType.Sphere, new Vector3(0, -.16f, .09f), Vector3.one * .1f, 2);
            heldItems[1] = Instantiate(wateringCanPrefab, heldItemSocket, false);
            heldItems[2] = Instantiate(sicklePrefab, heldItemSocket, false);
            // The supplied model's spout points backwards in its rest prefab. Face it toward the working target.
            heldItems[1].transform.localRotation = Quaternion.Euler(0, 180, 0);
            heldItems[1].name = "Held WateringCan";
            heldItems[2].name = "Held Sickle";
            heldItems[3] = Instantiate(Resources.Load<GameObject>("ExplorationArt/hoe"), heldItemSocket, false);
            heldItems[3].name = "Held Hoe";
            heldItems[4] = Instantiate(Resources.Load<GameObject>("ExplorationArt/axe"), heldItemSocket, false);
            heldItems[4].name = "Held Axe";
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
                heldItems[i].SetActive(!game.BuildMode && game.Model.EquippedItem == (FarmItem)i && game.Model.ItemCount((FarmItem)i, game.ActiveCrop.id) > 0);
            if (observedModel != game.Model)
            {
                foreach (var renderer in soil) Destroy(renderer.gameObject);
                foreach (var plant in plants) if (plant != null) Destroy(plant);
                soil = new Renderer[0]; plants = new GameObject[0]; shownStages = new int[0]; shownCrops = new string[0]; dryColors = new Color[0];
                observedModel = game.Model;
            }
            if (soil.Length != game.Model.PlotCount)
            {
                int previous = soil.Length, count = game.Model.PlotCount;
                System.Array.Resize(ref soil,count);System.Array.Resize(ref plants,count);System.Array.Resize(ref shownStages,count);
                System.Array.Resize(ref shownCrops,count);System.Array.Resize(ref dryColors,count);
                for (int i = previous; i < count; i++)
                {
                    var obj = Instantiate(soilTemplate.gameObject,game.PlotCenter(i,.025f),Quaternion.identity,transform);
                    obj.name = "Tilled Soil " + i;
                    foreach (var collider in obj.GetComponentsInChildren<Collider>()) { collider.enabled=false; Destroy(collider); }
                    obj.SetActive(true); soil[i] = obj.GetComponent<Renderer>();
                    dryColors[i] = soilTemplate.sharedMaterial.GetColor("_BaseColor"); shownStages[i] = -1;
                }
            }
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
                plants[i].transform.position = game.PlotCenter(i);
            }
        }
    }
}
