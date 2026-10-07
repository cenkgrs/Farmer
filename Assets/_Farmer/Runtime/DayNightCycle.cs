using UnityEngine;
using UnityEngine.Rendering;

namespace Farmer
{
    public sealed class DayNightCycle : MonoBehaviour
    {
        [SerializeField] private FarmGame game;
        [SerializeField] private Light sun;
        [SerializeField, Min(1)] private float realMinutesPerDay = 10;
        private float sinceSave;
        public bool ClockPaused { get; set; }
        public float Daylight { get; private set; }
        public float RealMinutesPerDay => realMinutesPerDay;
        public void Configure(FarmGame source, Light light) { game = source; sun = light; }
        private void Update()
        {
            if (game == null || !game.Ready) return;
            if (Application.isFocused && game.isActiveAndEnabled && !ClockPaused)
            {
                int nights = game.Model.AdvanceMinutes(Time.deltaTime * 1440.0 / (Mathf.Max(1,realMinutesPerDay)*60));
                sinceSave += Time.deltaTime;
                if (nights > 0) { game.NotifyTimeAdvanced(); sinceSave = 0; }
                else if (sinceSave >= 30) { game.SaveGame(); sinceSave = 0; }
            }
            ApplyLighting();
        }
        public void ApplyLighting()
        {
            if (game == null || sun == null) return;
            float hour = (float)game.Model.MinuteOfDay / 60f;
            float rise = Mathf.SmoothStep(0,1,Mathf.InverseLerp(4.5f,7.5f,hour));
            float set = 1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(17,20,hour));
            Daylight = rise*set;
            float warm = Daylight * (1-Daylight) * 4;
            sun.color = Color.Lerp(Color.Lerp(new Color(.65f,.76f,1f),new Color(1,.94f,.82f),Daylight),new Color(1,.62f,.35f),warm*.65f);
            sun.intensity = Mathf.Lerp(.48f,1.5f,Daylight);
            sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(20,55,Daylight),Mathf.Lerp(-70,65,hour/24),0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(new Color(.26f,.32f,.44f),new Color(.55f,.61f,.67f),Daylight);
            Camera.main.backgroundColor = Color.Lerp(new Color(.035f,.06f,.13f),new Color(.63f,.75f,.80f),Daylight);
        }
    }
}
