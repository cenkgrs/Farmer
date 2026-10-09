using System.Linq;
using UnityEngine;
namespace Farmer
{
    // New fixed scenery yields to existing saved construction; never delete player-owned data.
    public sealed class ValleyScenery : MonoBehaviour
    {
        private FarmGame game;
        private Renderer[] renderers;
        private Collider[] colliders;
        private Bounds bounds;
        private void Start()
        {
            game=FindFirstObjectByType<FarmGame>();renderers=GetComponentsInChildren<Renderer>();colliders=GetComponentsInChildren<Collider>();
            if(renderers.Length==0)return;
            bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            game.Changed+=Refresh;Refresh();
        }
        private void Refresh()
        {
            bool blocked=game.Model.Building.Blocks.Any(b=>Contains(b.x,b.z));
            if(!blocked)for(int i=0;i<game.Model.PlotCount;i++){var p=game.Model.Plot(i);if(Contains(p.x,p.z)){blocked=true;break;}}
            foreach(var r in renderers)r.enabled=!blocked;foreach(var c in colliders)c.enabled=!blocked;
        }
        private bool Contains(int x,int z)=>x+1>bounds.min.x-.2f&&x<bounds.max.x+.2f&&z+1>bounds.min.z-.2f&&z<bounds.max.z+.2f;
        private void OnDestroy(){if(game!=null)game.Changed-=Refresh;}
    }
}
