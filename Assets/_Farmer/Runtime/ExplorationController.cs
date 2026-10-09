using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Farmer
{
    [DefaultExecutionOrder(-120)]
    public sealed class ExplorationController : MonoBehaviour
    {
        [SerializeField] private Material baseMaterial;
        private FarmGame game;
        private ExplorationModel observed;
        private int revision=-1;
        private readonly Dictionary<int,ResourceView> views=new Dictionary<int,ResourceView>();
        private Material[] materials;
        private ResourceRecord target;
        private float nextHit;
        public string Hint { get; private set; }
        public int? TargetId=>target?.id;
        public void Configure(Material material)=>baseMaterial=material;
        private void Start()
        {
            game=GetComponent<FarmGame>();
            var colors=new[]{new Color(.36f,.20f,.09f),new Color(.24f,.40f,.12f),new Color(.38f,.53f,.18f),new Color(.61f,.35f,.13f),new Color(.22f,.25f,.25f),new Color(.94f,.73f,.29f)};
            materials=colors.Select(c=>{var m=new Material(baseMaterial);m.SetColor("_BaseColor",c);return m;}).ToArray();
            game.Changed+=Refresh;Refresh();
        }
        private void OnDestroy(){if(game!=null)game.Changed-=Refresh;if(materials!=null)foreach(var m in materials)Destroy(m);}
        private void Refresh()
        {
            if(game.Model.Exploration!=observed)
            {
                foreach(var view in views.Values){view.gameObject.SetActive(false);Destroy(view.gameObject);}views.Clear();
                observed=game.Model.Exploration;revision=-1;nextHit=0;target=null;
            }
            if(revision==observed.Revision)return;revision=observed.Revision;
            foreach(var n in observed.Nodes)
            {
                if(!views.TryGetValue(n.id,out var view))
                {
                    var go=new GameObject(n.kind+" "+n.id);go.transform.SetParent(transform,false);go.transform.position=new Vector3(n.x+.5f,0,n.z+.5f);
                    view=go.AddComponent<ResourceView>();view.Configure(n,materials);views.Add(n.id,view);
                }
                else view.SetState(n);
            }
            Physics.SyncTransforms();
        }
        private void Update()
        {
            if(game==null)return;RefreshPointer();
        }
        public bool RefreshPointer()
        {
            target=null;Hint=null;
            if(game==null||!game.Ready||game.BuildMode||!Application.isFocused||Mouse.current==null)return false;
            game.Selection.RefreshPointer();if(game.Selection.PointerBlocked)return false;
            var ray=Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if(Physics.Raycast(ray,out var hit,150,~0,QueryTriggerInteraction.Ignore))
            {
                var view=hit.collider.GetComponent<ResourceView>();
                if(view!=null)target=observed.Node(view.Id);
            }
            if(target==null&&game.Selection.WorldCell is Vector2Int cell)target=observed.Nodes.FirstOrDefault(n=>n.x==cell.x&&n.z==cell.y&&!n.collected);
            if(target==null)return false;
            string label=target.kind==ResourceKind.Tree?"Ağaç":target.kind==ResourceKind.Chest?"Keşif sandığı":"Yabani tohum bitkisi";
            Hint=label+" · "+(target.collected?"Toplandı":!InReach(target)?"Biraz yaklaş":target.kind==ResourceKind.Tree?$"Balta (6) · Sol tık · {target.hits}/3":target.kind==ResourceKind.WildPlant?"Orak (3) · Sol tıkla tohum topla":"Sol tıkla aç");
            return true;
        }
        public bool InReach(ResourceRecord node)
        {
            var d=new Vector3(node.x+.5f,game.Player.position.y,node.z+.5f)-game.Player.position;if(d.sqrMagnitude>6.25f)return false;
            var from=game.Player.position+Vector3.up*.8f;var to=new Vector3(node.x+.5f,.5f,node.z+.5f);var delta=to-from;
            foreach(var hit in Physics.RaycastAll(from,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore))
                if(!hit.collider.transform.IsChildOf(game.Player)&&hit.collider.GetComponent<ResourceView>()?.Id!=node.id)return false;
            return true;
        }
        public bool UseTarget()
        {
            if(!RefreshPointer()||!InReach(target)){game.ShowBuildFeedback("Kaynağa biraz yaklaş.");return false;}
            if(Time.time<nextHit)return false;
            var node=target;bool ok=game.Gather(node.id);
            if(ok){nextHit=Time.time+.55f;views[node.id].Pulse();}
            return ok;
        }
    }
}
