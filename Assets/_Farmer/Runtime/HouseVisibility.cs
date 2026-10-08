using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Farmer
{
    [DefaultExecutionOrder(200)]
    public sealed class HouseVisibility : MonoBehaviour
    {
        private FarmGame game;
        private BuildingModel observed;
        private int revision=-1;
        private Vector2Int lastCell=new Vector2Int(int.MinValue,int.MinValue);
        private readonly HashSet<Vector2Int> floors=new HashSet<Vector2Int>(),room=new HashSet<Vector2Int>();
        private readonly HashSet<(int,int,int)> edges=new HashSet<(int,int,int)>();
        private readonly HashSet<OccludingWall> blockers=new HashSet<OccludingWall>();
        private OccludingWall[] walls=new OccludingWall[0];
        public int FadedCount=>walls.Count(w=>w!=null&&w.Opacity<.5f);
        private void Awake()=>game=GetComponent<FarmGame>();
        private void LateUpdate()
        {
            if(!game.Ready||Camera.main==null)return;
            var model=game.Model.Building;bool changed=observed!=model||revision!=model.Revision;
            if(changed)
            {
                observed=model;revision=model.Revision;floors.Clear();edges.Clear();
                foreach(var b in model.Blocks)
                {
                    var kind=model.Rules(b.pieceId).Placement;
                    if(kind==BuildPlacement.Floor)floors.Add(new Vector2Int(b.x,b.z));
                    if(kind==BuildPlacement.Edge)edges.Add(BuildingModel.Edge(b.x,b.z,b.rotation));
                }
                walls=FindObjectsByType<OccludingWall>(FindObjectsSortMode.None);
            }
            var cell=new Vector2Int(Mathf.FloorToInt(game.Player.position.x),Mathf.FloorToInt(game.Player.position.z));
            if(changed||cell!=lastCell){lastCell=cell;FindRoom(cell);}
            blockers.Clear();var camera=Camera.main;
            // An unfinished room or bare-ground shelter still reveals walls directly covering the character.
            for(int sample=0;sample<3;sample++)
            {
                var target=game.Player.position+Vector3.up*(.25f+sample*.7f);
                var origin=camera.orthographic?target-camera.transform.forward*40:camera.transform.position;
                var delta=target-origin;
                foreach(var hit in Physics.RaycastAll(origin,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore))
                {
                    var wall=hit.collider.GetComponentInParent<OccludingWall>();if(wall!=null)blockers.Add(wall);
                }
            }
            foreach(var wall in walls)
            {
                if(wall==null)continue;
                var b=wall.Record;var edge=BuildingModel.Edge(b.x,b.z,b.rotation);
                var a=edge.axis==0?new Vector2Int(edge.x,edge.z-1):new Vector2Int(edge.x-1,edge.z);
                var c=new Vector2Int(edge.x,edge.z);
                bool inA=room.Contains(a),inC=room.Contains(c),front=false;
                if(inA!=inC)
                {
                    var outward=edge.axis==0?Vector3.forward:Vector3.right;if(inC)outward=-outward;
                    front=Vector3.Dot(outward,-camera.transform.forward)>.05f;
                }
                wall.SetFaded(front||blockers.Contains(wall));
            }
        }
        private void FindRoom(Vector2Int start)
        {
            room.Clear();if(!floors.Contains(start))return;
            var queue=new Queue<Vector2Int>();queue.Enqueue(start);room.Add(start);
            var directions=new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
            while(queue.Count>0)
            {
                var cell=queue.Dequeue();
                for(int r=0;r<4;r++)
                {
                    var next=cell+directions[r];
                    if(floors.Contains(next)&&!edges.Contains(BuildingModel.Edge(cell.x,cell.y,r))&&room.Add(next))queue.Enqueue(next);
                }
            }
        }
        private void OnDisable(){foreach(var wall in walls)if(wall!=null)wall.RestoreOpaque();}
    }
}
