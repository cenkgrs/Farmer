using System.Collections.Generic;
using UnityEngine;
namespace Farmer
{
    public static class CropArtwork
    {
        private static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        public static Sprite Get(string id)
        {
            if(sprites.TryGetValue(id,out var sprite))return sprite;
            var texture=Resources.Load<Texture2D>("CropIcons/"+id);
            if(texture==null)return null;
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,100);
            sprites[id]=sprite;return sprite;
        }
    }
}
