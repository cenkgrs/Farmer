using UnityEngine;
namespace Farmer
{
    public sealed class HuntingArrow:MonoBehaviour
    {
        private Vector3 start;private Transform target;private float elapsed,duration;
        public static float Fly(Vector3 origin,Transform prey)
        {
            var obj=new GameObject("Flying hunting arrow");var arrow=obj.AddComponent<HuntingArrow>();arrow.start=origin;arrow.target=prey;arrow.duration=Mathf.Max(.08f,Vector3.Distance(origin,prey.position)/25);obj.transform.position=origin;
            var mat=VillageArtwork.Material(new Color(.72f,.53f,.22f));VillageArtwork.Part(obj.transform,"Arrow shaft",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.025f,.38f,.025f),mat).localRotation=Quaternion.Euler(90,0,0);
            VillageArtwork.Part(obj.transform,"Arrow head",PrimitiveType.Sphere,new Vector3(0,0,.38f),new Vector3(.07f,.035f,.15f),mat);
            Destroy(obj,arrow.duration+.15f);return arrow.duration;
        }
        private void Update(){if(target==null)return;elapsed+=Time.deltaTime;var end=target.position+Vector3.up*.5f;transform.position=Vector3.Lerp(start,end,Mathf.Clamp01(elapsed/duration));if((end-start).sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(end-start);}
    }
}
