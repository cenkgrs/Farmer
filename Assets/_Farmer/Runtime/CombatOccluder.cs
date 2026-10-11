using UnityEngine;

namespace Farmer
{
    public sealed class CombatOccluder : MonoBehaviour
    {
        private Renderer[] renderers;
        private Material[][] originals, faded;
        private CombatController combat;
        private FarmGame game;
        private float opacity=1;
        private bool transparent;
        public static void Attach(CombatController owner,FarmGame farm)
        {
            foreach(var resource in Object.FindObjectsByType<ResourceView>(FindObjectsSortMode.None))
                if(resource.name.StartsWith("Tree"))Add(resource.gameObject);
            foreach(var scenery in Object.FindObjectsByType<ValleyScenery>(FindObjectsSortMode.None))
                if(scenery.name.Contains("pine_tree"))Add(scenery.gameObject);
            void Add(GameObject root)
            {
                if(root.GetComponent<CombatOccluder>()!=null)return;
                var o=root.AddComponent<CombatOccluder>();o.combat=owner;o.game=farm;
            }
        }
        private void Start()
        {
            renderers=GetComponentsInChildren<Renderer>();originals=new Material[renderers.Length][];faded=new Material[renderers.Length][];
            var template=Resources.Load<Material>("BuildingArt/TransparentWood");
            for(int i=0;i<renderers.Length;i++)
            {
                originals[i]=renderers[i].sharedMaterials;faded[i]=new Material[originals[i].Length];
                for(int j=0;j<originals[i].Length;j++)
                {
                    var source=originals[i][j];var copy=new Material(template);
                    copy.SetTexture("_BaseMap",source.GetTexture("_BaseMap"));copy.SetColor("_BaseColor",source.GetColor("_BaseColor"));faded[i][j]=copy;
                }
            }
        }
        private void LateUpdate()
        {
            if(Camera.main==null||game==null||renderers.Length==0)return;
            bool blocked=Occludes(game.Player.position+Vector3.up);
            for(int i=0;i<2&&!blocked;i++)
            {
                var enemy=combat.View(i);
                if(enemy!=null&&CombatController.PlanarDistance(enemy.transform.position,game.Player.position)<8)blocked=Occludes(enemy.transform.position+Vector3.up*.7f);
            }
            float before=opacity;opacity=Mathf.MoveTowards(opacity,blocked?.14f:1,Time.deltaTime*5);
            if(Mathf.Approximately(before,opacity))return;
            bool swap=transparent!=(opacity<.999f);transparent=opacity<.999f;
            for(int i=0;i<renderers.Length;i++)
            {
                if(swap)renderers[i].sharedMaterials=transparent?faded[i]:originals[i];
                for(int j=0;j<faded[i].Length;j++){var c=originals[i][j].GetColor("_BaseColor");c.a*=opacity;faded[i][j].SetColor("_BaseColor",c);}
            }
        }
        private bool Occludes(Vector3 target)
        {
            Vector3 delta=target-Camera.main.transform.position;var ray=new Ray(Camera.main.transform.position,delta.normalized);
            foreach(var r in renderers)if(r.enabled&&r.bounds.IntersectRay(ray,out float d)&&d<delta.magnitude-.4f)return true;
            return false;
        }
        private void OnDestroy()
        {
            if(faded==null)return;
            for(int i=0;i<faded.Length;i++){if(renderers[i]!=null)renderers[i].sharedMaterials=originals[i];foreach(var m in faded[i])Destroy(m);}
        }
    }
}
