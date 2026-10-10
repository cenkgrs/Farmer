using UnityEngine;
using UnityEngine.UI;
namespace Farmer
{
    // Reuse a blank slot and the two end caps of the existing inventory texture.
    public static class HudArtwork
    {
        public static void ApplyHeader(RectTransform header)
        {
            header.GetComponent<Image>().color=Color.clear;
            Object.Destroy(header.GetComponent<Outline>());
            var sprite=ConstructionArtwork.Get("construction_frame");var r=sprite.rect;var texture=sprite.texture;
            Strip(header,"Wood left",texture,new Rect(r.x,r.y,r.width*.041f,r.height),0,18);
            Strip(header,"Parchment",texture,new Rect(r.x+r.width*.041f,r.y,r.width*.098f,r.height),18,244);
            Strip(header,"Wood right",texture,new Rect(r.x+r.width*.958f,r.y,r.width*.042f,r.height),262,18);
            var coin=new GameObject("Wallet coin",typeof(RectTransform),typeof(RawImage));var c=(RectTransform)coin.transform;c.SetParent(header,false);
            c.anchorMin=c.anchorMax=c.pivot=new Vector2(0,1);c.anchoredPosition=new Vector2(32,-61);c.sizeDelta=new Vector2(29,29);
            var image=coin.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>("MarketArt/approved");
            image.uvRect=new Rect(1206f/1672,(941-153f)/941,41f/1672,41f/941);image.raycastTarget=false;
        }
        private static void Strip(RectTransform parent,string name,Texture2D texture,Rect source,float x,float width)
        {
            var o=new GameObject(name,typeof(RectTransform),typeof(RawImage));var r=(RectTransform)o.transform;r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,0);r.sizeDelta=new Vector2(width,116);
            var image=o.GetComponent<RawImage>();image.texture=texture;image.uvRect=new Rect(source.x/texture.width,source.y/texture.height,source.width/texture.width,source.height/texture.height);image.raycastTarget=false;
        }
    }
}
