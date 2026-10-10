using System.Collections.Generic;
using UnityEngine;
namespace Farmer
{
    // Illustrated seed packets are independent of the world plant prefabs and harvest icons.
    public static class SeedArtwork
    {
        private static readonly Dictionary<string,Sprite> Cache=new Dictionary<string,Sprite>();
        public static Sprite Get(string id)
        {
            if(Cache.TryGetValue(id,out var sprite))return sprite;
            var texture=Resources.Load<Texture2D>("SeedArt/"+id);
            if(texture==null)return null;
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);Cache[id]=sprite;return sprite;
        }
    }
}
