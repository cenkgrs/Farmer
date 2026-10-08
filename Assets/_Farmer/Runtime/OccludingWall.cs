using System.Collections.Generic;
using UnityEngine;

namespace Farmer
{
    // Per-piece material copies: transparency never changes the source asset or physics.
    public sealed class OccludingWall : MonoBehaviour
    {
        private Renderer[] renderers;
        private Material[][] opaque,transparent;
        private readonly Dictionary<Material,Material> copies=new Dictionary<Material,Material>();
        private bool usingTransparent;
        public float Opacity { get; private set; }=1;
        public BlockRecord Record=>GetComponent<PlacedBlockView>().Record;
        private void Awake()
        {
            renderers=GetComponentsInChildren<Renderer>();opaque=new Material[renderers.Length][];transparent=new Material[renderers.Length][];
            var template=Resources.Load<Material>("BuildingArt/TransparentWood");
            for(int i=0;i<renderers.Length;i++)
            {
                opaque[i]=renderers[i].sharedMaterials;transparent[i]=new Material[opaque[i].Length];
                for(int j=0;j<opaque[i].Length;j++)
                {
                    var source=opaque[i][j];
                    if(!copies.TryGetValue(source,out var copy))
                    {
                        copy=new Material(template);copy.SetColor("_BaseColor",source.GetColor("_BaseColor"));
                        copy.SetTexture("_BaseMap",source.GetTexture("_BaseMap"));copies.Add(source,copy);
                    }
                    transparent[i][j]=copy;
                }
            }
        }
        public void SetFaded(bool faded)
        {
            Opacity=Mathf.MoveTowards(Opacity,faded?.16f:1,Time.deltaTime*3.5f);
            bool transparentNow=Opacity<.999f;
            if(transparentNow!=usingTransparent)
            {
                usingTransparent=transparentNow;
                for(int i=0;i<renderers.Length;i++)renderers[i].sharedMaterials=transparentNow?transparent[i]:opaque[i];
            }
            if(transparentNow)foreach(var pair in copies)
            {
                var color=pair.Key.GetColor("_BaseColor");color.a*=Opacity;pair.Value.SetColor("_BaseColor",color);
            }
        }
        private void OnDisable()=>RestoreOpaque();
        public void RestoreOpaque()
        {
            if(renderers==null)return;
            for(int i=0;i<renderers.Length;i++)if(renderers[i]!=null)renderers[i].sharedMaterials=opaque[i];
            usingTransparent=false;Opacity=1;
        }
        private void OnDestroy(){foreach(var material in copies.Values)Destroy(material);}
    }
}
