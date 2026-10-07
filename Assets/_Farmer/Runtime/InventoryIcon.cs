using UnityEngine;
using UnityEngine.UI;

namespace Farmer
{
    // Source PNGs remain unchanged; normalized sprite rectangles omit their transparent margins.
    public sealed class InventoryIcon : Image
    {
        public int Kind
        {
            set { sprite=Artwork(value); preserveAspect=true; raycastTarget=false; }
        }
        private static readonly string[] Names={"seed_pouch","watering_can","sickle","hoe","build_hammer","turnip","wood_log","wooden_inventory_frame"};
        private static readonly Rect[] Bounds={
            new Rect(0.11244019f, 0.06698565f, 0.78947368f, 0.86044657f),
            new Rect(0.03827751f, 0.11722488f, 0.92583732f, 0.75917065f),
            new Rect(0.05661882f, 0.06539075f, 0.89952153f, 0.86682616f),
            new Rect(0.12280702f, 0.09250399f, 0.82057416f, 0.79665072f),
            new Rect(0.13237640f, 0.07974482f, 0.78149920f, 0.82854864f),
            new Rect(0.18341308f, 0.04625199f, 0.72089314f, 0.89393939f),
            new Rect(0.05661882f, 0.07256778f, 0.90829346f, 0.81020734f),
            new Rect(0.02510269f, 0.24651811f, 0.95070744f, 0.47910864f),
        };
        private static readonly Sprite[] Cache=new Sprite[8];
        public static Sprite Artwork(int kind)
        {
            if(Cache[kind]!=null)return Cache[kind];
            var texture=Resources.Load<Texture2D>("InventoryArt/"+Names[kind]);
            if(texture==null){Debug.LogError("Missing inventory artwork: "+Names[kind]);return null;}
            var b=Bounds[kind];
            Cache[kind]=Sprite.Create(texture,new Rect(b.x*texture.width,b.y*texture.height,b.width*texture.width,b.height*texture.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
            return Cache[kind];
        }
    }
}
