using UnityEngine;

namespace Farmer
{
    // Two streamed, non-spatial loops crossfade without restarting on UI or clock changes.
    public sealed class FarmMusic : MonoBehaviour
    {
        public const float Volume = .22f;
        public const float FadeSeconds = 6f;
        private FarmGame game;
        private AudioSource day, night;
        private float dayLevel, nightLevel;
        private bool suspended;
        public AudioSource DaySource => day;
        public AudioSource NightSource => night;

        private void Awake()
        {
            game = GetComponent<FarmGame>();
            day = CreateSource("Day music", "Music/day_farm");
            night = CreateSource("Night music", "Music/night_farm");
        }
        private AudioSource CreateSource(string name, string path)
        {
            var child = new GameObject(name);child.transform.SetParent(transform, false);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;source.loop = true;source.spatialBlend = 0;
            source.volume = 0;source.priority = 192;
            source.clip = Resources.Load<AudioClip>(path);
            return source;
        }
        private void OnEnable()
        {
            suspended = !Application.isFocused;
            if (!suspended) { day.Play();night.Play(); }
        }
        private void OnDisable() { day.Stop();night.Stop();dayLevel = nightLevel = 0;day.volume = night.volume = 0; }
        private void OnApplicationFocus(bool focused)
        {
            suspended = !focused;
            if (suspended) { day.Pause();night.Pause(); }
            else if (isActiveAndEnabled) { Resume(day);Resume(night); }
        }
        private static void Resume(AudioSource source)
        {
            source.UnPause();
            if (!source.isPlaying && source.clip != null) source.Play();
        }
        private void Update()
        {
            if (game == null || !game.Ready || suspended) return;
            Step(game.Model.MinuteOfDay, Time.unscaledDeltaTime);
        }
        public void Step(double minuteOfDay, float delta)
        {
            bool daytime = minuteOfDay >= 360 && minuteOfDay < 1140;
            float step = Mathf.Max(0, delta) / FadeSeconds;
            dayLevel = Mathf.MoveTowards(dayLevel, daytime ? 1 : 0, step);
            nightLevel = Mathf.MoveTowards(nightLevel, daytime ? 0 : 1, step);
            day.volume = dayLevel * Volume;night.volume = nightLevel * Volume;
        }
    }
}
