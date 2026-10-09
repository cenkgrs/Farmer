using UnityEngine;
namespace Farmer
{
    public sealed class ResourceView : MonoBehaviour
    {
        public int Id { get; private set; }
        public ResourceKind Kind { get; private set; }
        private Transform crown,lid,trunk,plant;
        private GameObject stump;
        private Collider obstacle;
        private float shake;
        public void Configure(ResourceRecord record,Material[] materials)
        {
            Id=record.id;Kind=record.kind;
            if(Kind==ResourceKind.Tree)
            {
                var tree=Instantiate(Resources.Load<GameObject>("ExplorationArt/tree_oak"),transform,false);
                trunk=tree.transform.Find("Trunk");crown=tree.transform.Find("Canopy");
                stump=Instantiate(Resources.Load<GameObject>("ExplorationArt/tree_stump"),transform,false);
                stump.name="Felled Oak Stump";
                var box=gameObject.AddComponent<BoxCollider>();box.center=new Vector3(0,.9f,0);box.size=new Vector3(.6f,1.8f,.6f);obstacle=box;
            }
            else if(Kind==ResourceKind.Chest)
            {
                var chest=Instantiate(Resources.Load<GameObject>("ExplorationArt/treasure_chest"),transform,false);
                lid=chest.transform.Find("Lid hinge");
                var box=gameObject.AddComponent<BoxCollider>();box.center=new Vector3(0,.38f,0);box.size=new Vector3(.95f,.76f,.7f);obstacle=box;
            }
            else
            {
                plant=Instantiate(Resources.Load<GameObject>("ExplorationArt/wild_plant"),transform,false).transform;
                var box=gameObject.AddComponent<BoxCollider>();box.center=new Vector3(0,.3f,0);box.size=new Vector3(.65f,.65f,.65f);obstacle=box;
            }
            SetState(record);
        }
        public void SetState(ResourceRecord record)
        {
            bool collected=record.collected;
            if(Kind==ResourceKind.Tree)
            {
                crown.gameObject.SetActive(!collected);trunk.gameObject.SetActive(!collected);stump.SetActive(collected);
                obstacle.enabled=!collected;
            }
            else if(Kind==ResourceKind.WildPlant){plant.gameObject.SetActive(!collected);obstacle.enabled=!collected;}
            else lid.localRotation=Quaternion.Euler(collected?-105:0,0,0);
        }
        public void Pulse()=>shake=.25f;
        private void Update()
        {
            if(shake<=0)return;shake=Mathf.Max(0,shake-Time.deltaTime);transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(shake*65)*shake*12);
        }
    }
}
