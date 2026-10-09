using UnityEngine;
namespace Farmer
{
    public sealed class FurnitureLighting:MonoBehaviour
    {
        private Light lamp;private FarmGame game;
        private void Awake(){lamp=GetComponentInChildren<Light>();game=FindFirstObjectByType<FarmGame>();}
        private void Update()
        {
            if(lamp==null||game==null||!game.Ready)return;
            float hour=(float)game.Model.MinuteOfDay/60f;
            float night=hour<6?1:hour<7?7-hour:hour<18?0:hour<19?hour-18:1;
            lamp.enabled=night>0;lamp.intensity=2.5f*night;
        }
    }
}
