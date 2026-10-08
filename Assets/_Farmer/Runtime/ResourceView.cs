using UnityEngine;
namespace Farmer
{
    public sealed class ResourceView : MonoBehaviour
    {
        public int Id { get; private set; }
        public ResourceKind Kind { get; private set; }
        private Transform crown,lid,trunk,plant;
        private Collider obstacle;
        private bool collected;
        private float shake;
        public void Configure(ResourceRecord record,Material[] materials)
        {
            Id=record.id;Kind=record.kind;
            if(Kind==ResourceKind.Tree)
            {
                trunk=Part("Trunk",PrimitiveType.Cylinder,new Vector3(0,.85f,0),new Vector3(.42f,.85f,.42f),materials[0]);
                crown=new GameObject("Canopy").transform;crown.SetParent(transform,false);
                for(int i=0;i<3;i++)
                {
                    var leaf=Part("Leaves",PrimitiveType.Sphere,new Vector3((i-1)*.55f,2.05f+i*.16f,(i%2)*.28f),new Vector3(1.75f,1.65f,1.65f),materials[1+i%2]);leaf.SetParent(crown,true);
                }
                var box=gameObject.AddComponent<BoxCollider>();box.center=new Vector3(0,.9f,0);box.size=new Vector3(.6f,1.8f,.6f);obstacle=box;
            }
            else if(Kind==ResourceKind.Chest)
            {
                Part("Chest",PrimitiveType.Cube,new Vector3(0,.3f,0),new Vector3(.9f,.6f,.65f),materials[0]);
                lid=new GameObject("Lid hinge").transform;lid.SetParent(transform,false);lid.localPosition=new Vector3(0,.6f,.325f);
                var top=Part("Lid",PrimitiveType.Cube,new Vector3(0,.67f,0),new Vector3(.94f,.14f,.69f),materials[3]);top.SetParent(lid,true);
                for(int i=-1;i<=1;i+=2)Part("Iron band",PrimitiveType.Cube,new Vector3(i*.3f,.31f,-.335f),new Vector3(.06f,.58f,.035f),materials[4]);
                Part("Clasp",PrimitiveType.Cube,new Vector3(0,.53f,-.355f),new Vector3(.12f,.18f,.045f),materials[5]);
                var box=gameObject.AddComponent<BoxCollider>();box.center=new Vector3(0,.38f,0);box.size=new Vector3(.95f,.76f,.7f);obstacle=box;
            }
            else
            {
                plant=new GameObject("Wild seed plant").transform;plant.SetParent(transform,false);
                for(int i=0;i<5;i++)
                {
                    float a=i*Mathf.PI*2/5;
                    var leaf=Part("Leaf",PrimitiveType.Sphere,new Vector3(Mathf.Cos(a)*.2f,.25f,Mathf.Sin(a)*.2f),new Vector3(.17f,.52f,.17f),materials[2]);leaf.localRotation=Quaternion.Euler(Mathf.Sin(a)*35,0,-Mathf.Cos(a)*35);leaf.SetParent(plant,true);
                    var seed=Part("Seed head",PrimitiveType.Sphere,new Vector3(Mathf.Cos(a)*.14f,.54f,Mathf.Sin(a)*.14f),Vector3.one*.14f,materials[5]);seed.SetParent(plant,true);
                }
                var box=gameObject.AddComponent<BoxCollider>();box.center=new Vector3(0,.3f,0);box.size=new Vector3(.65f,.65f,.65f);obstacle=box;
            }
            SetState(record);
        }
        private Transform Part(string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            go.transform.SetParent(transform,false);go.transform.localPosition=pos;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;return go.transform;
        }
        public void SetState(ResourceRecord record)
        {
            collected=record.collected;
            if(Kind==ResourceKind.Tree)
            {
                crown.gameObject.SetActive(!collected);trunk.localScale=new Vector3(.42f,collected?.13f:.85f,.42f);trunk.localPosition=new Vector3(0,collected?.13f:.85f,0);
                obstacle.enabled=!collected;
            }
            else if(Kind==ResourceKind.WildPlant){plant.gameObject.SetActive(!collected);obstacle.enabled=!collected;}
            else lid.localRotation=Quaternion.Euler(collected?-105:0,0,0);
        }
        public void Pulse()=>shake=.25f;
        public void UpdateCanopy(Vector3 player)
        {if(crown!=null&&!collected){var d=transform.position-player;d.y=0;crown.gameObject.SetActive(d.sqrMagnitude>7f);}}
        private void Update()
        {
            if(shake<=0)return;shake=Mathf.Max(0,shake-Time.deltaTime);transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(shake*65)*shake*12);
        }
    }
}
