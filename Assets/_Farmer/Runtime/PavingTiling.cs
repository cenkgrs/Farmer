using UnityEngine;
namespace Farmer
{
    // Keep the paving stones at the same size while a designer scales a road segment.
    [ExecuteAlways, RequireComponent(typeof(Renderer))]
    public sealed class PavingTiling : MonoBehaviour
    {
        private MaterialPropertyBlock properties;
        private void OnEnable()=>Apply();
        private void Update(){if(transform.hasChanged){Apply();transform.hasChanged=false;}}
        private void Apply()
        {
            var renderer=GetComponent<Renderer>();if(renderer==null)return;
            properties??=new MaterialPropertyBlock();renderer.GetPropertyBlock(properties);
            var s=transform.lossyScale;
            properties.SetVector("_BaseMap_ST",new Vector4(Mathf.Abs(s.x)/3f,Mathf.Abs(s.z)/3f,0,0));
            renderer.SetPropertyBlock(properties);
        }
    }
}
