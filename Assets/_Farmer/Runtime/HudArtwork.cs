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
        // Nine texture patches keep the inventory's wood border at a fixed thickness.
        public static void ApplyFrame(RectTransform panel,float border=18)
        {
            panel.GetComponent<Image>().color=Color.clear;Object.Destroy(panel.GetComponent<Outline>());
            var sprite=ConstructionArtwork.Get("construction_frame");var source=sprite.rect;var texture=sprite.texture;
            float[] sx={0,.041f,.958f},sw={.041f,.098f,.042f},sy={0,.35f,.84f},sh={.16f,.30f,.16f};
            for(int row=0;row<3;row++)for(int col=0;col<3;col++)
            {
                var patch=new GameObject("Wood frame patch",typeof(RectTransform),typeof(RawImage));var r=(RectTransform)patch.transform;r.SetParent(panel,false);
                r.anchorMin=new Vector2(col==2?1:0,row==2?1:0);r.anchorMax=new Vector2(col==0?0:1,row==0?0:1);
                r.offsetMin=new Vector2(col==0?0:col==1?border:-border,row==0?0:row==1?border:-border);
                r.offsetMax=new Vector2(col==0?border:col==1?-border:0,row==0?border:row==1?-border:0);
                var image=patch.GetComponent<RawImage>();image.texture=texture;image.uvRect=new Rect((source.x+source.width*sx[col])/texture.width,(source.y+source.height*sy[row])/texture.height,source.width*sw[col]/texture.width,source.height*sh[row]/texture.height);image.raycastTarget=false;
            }
        }
        private static void Strip(RectTransform parent,string name,Texture2D texture,Rect source,float x,float width)
        {
            var o=new GameObject(name,typeof(RectTransform),typeof(RawImage));var r=(RectTransform)o.transform;r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,0);r.sizeDelta=new Vector2(width,parent.sizeDelta.y);
            var image=o.GetComponent<RawImage>();image.texture=texture;image.uvRect=new Rect(source.x/texture.width,source.y/texture.height,source.width/texture.width,source.height/texture.height);image.raycastTarget=false;
        }
    }
}
