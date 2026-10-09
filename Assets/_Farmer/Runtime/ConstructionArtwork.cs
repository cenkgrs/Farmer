using System.Collections.Generic;
using UnityEngine;
namespace Farmer
{
    public static class ConstructionArtwork
    {
        private static readonly Dictionary<string, Sprite> Cache=new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Rect> Bounds=new Dictionary<string, Rect>
        {
            {"wood_roof",new Rect(0.048644338f,0.105263158f,0.910685805f,0.808612440f)},
            {"wood_floor",new Rect(0.049441786f,0.161881978f,0.902711324f,0.637161085f)},
            {"construction_frame",new Rect(0.023701462f,0.326607818f,0.953605648f,0.373266078f)},
            {"wood_block",new Rect(0.107655502f,0.074162679f,0.787878788f,0.818181818f)},
            {"bed",new Rect(0.053429027f,0.072567783f,0.901116427f,0.841307815f)},
            {"wood_door",new Rect(0.215311005f,0.049441786f,0.573365231f,0.900318979f)},
            {"wood_wall",new Rect(0.276714514f,0.087719298f,0.494417863f,0.825358852f)},
        };
        public static Sprite Get(string id)
        {
            if(Cache.TryGetValue(id,out var sprite))return sprite;
            if(!Bounds.TryGetValue(id,out var b))b=new Rect(0,0,1,1);
            var texture=Resources.Load<Texture2D>("ConstructionArt/"+id);
            if(texture==null){Debug.LogError("Missing construction icon: "+id);return null;}
            sprite=Sprite.Create(texture,new Rect(b.x*texture.width,b.y*texture.height,b.width*texture.width,b.height*texture.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
            Cache.Add(id,sprite);return sprite;
        }
    }
}
