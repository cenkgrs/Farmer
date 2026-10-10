using UnityEngine;
namespace Farmer
{
    // Short soft cues are synthesized locally; no external sound license or network dependency.
    public sealed class CombatAudio : MonoBehaviour
    {
        private AudioSource source;
        private AudioClip swing, warning, hurt;
        private void Awake()
        {
            source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.volume=.22f;
            swing=Tone("Soft axe strike",180,.12f);warning=Tone("Enemy preparation",370,.22f);hurt=Tone("Player damage",105,.18f);
        }
        private static AudioClip Tone(string name,float frequency,float duration)
        {
            const int rate=22050;var samples=new float[(int)(rate*duration)];
            for(int i=0;i<samples.Length;i++){float t=(float)i/rate,envelope=Mathf.Sin(Mathf.PI*i/samples.Length);samples[i]=Mathf.Sin(2*Mathf.PI*frequency*(t-.5f*t*t))*envelope*.45f;}
            var clip=AudioClip.Create(name,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        public void Strike()=>source.PlayOneShot(swing);
        public void Warning()=>source.PlayOneShot(warning,.65f);
        public void Hurt()=>source.PlayOneShot(hurt);
        public void Silence()=>source.Stop();
        private void OnDestroy(){if(swing!=null)Destroy(swing);if(warning!=null)Destroy(warning);if(hurt!=null)Destroy(hurt);}
    }
}
