using UnityEngine;

namespace Farmer
{
    // Runtime layout keeps the authored scene editable and leaves player construction untouched.
    public static class CompactWorld
    {
        public static Vector3 Position(Vector3 p)
        {
            // Keep the home clearing and complete village buildings at their original scale.
            float blend=Mathf.InverseLerp(10,78,p.z);
            p+=new Vector3(31,0,-60)*blend;
            return p;
        }
        public static void FitGround(FarmGame game,Vector3 center)
        {
            var ground=GameObject.Find("Ground");if(ground==null||ground.GetComponent<WorldGround>()==null)return;
            var bounds=new Bounds(new Vector3(center.x,0,center.z),new Vector3(116,0,116));
            foreach(var b in game.Model.Building.Blocks)bounds.Encapsulate(new Bounds(new Vector3(b.x,0,b.z),new Vector3(12,0,12)));
            foreach(var n in game.Model.Exploration.Nodes)bounds.Encapsulate(new Bounds(new Vector3(n.x,0,n.z),new Vector3(10,0,10)));
            for(int i=0;i<game.Model.PlotCount;i++){var p=game.Model.Plot(i);bounds.Encapsulate(new Bounds(new Vector3(p.x,0,p.z),new Vector3(10,0,10)));}
            var position=ground.transform.position;position.x=bounds.center.x;position.z=bounds.center.z;ground.transform.position=position;
            var scale=ground.transform.localScale;scale.x=bounds.size.x;scale.z=bounds.size.z;ground.transform.localScale=scale;
            game.Player.GetComponent<PlayerMotor>().WorldBounds=bounds;
            Physics.SyncTransforms();
        }
        public static void Apply(Transform market,Vector3 center)
        {
            var root=GameObject.Find("Valley First Slice");if(root==null)return;
            foreach(Transform child in root.transform)
            {
                if(child.name=="Main road surface")
                {
                    var filter=child.GetComponent<MeshFilter>();if(filter==null)continue;
                    var mesh=Object.Instantiate(filter.sharedMesh);mesh.name="Compact main road";
                    var vertices=mesh.vertices;
                    for(int i=0;i<vertices.Length;i++)vertices[i]=child.InverseTransformPoint(Position(child.TransformPoint(vertices[i])));
                    mesh.vertices=vertices;mesh.RecalculateBounds();mesh.RecalculateNormals();filter.sharedMesh=mesh;
                    continue;
                }
                child.position=Position(child.position);
                if(child.GetComponent<ValleyScenery>()!=null)
                {
                    var delta=child.position-center;delta.y=0;
                    if(delta.magnitude>46){var p=center+delta.normalized*(46+(delta.magnitude-46)*.08f);p.y=child.position.y;child.position=p;}
                }
            }
            if(market!=null&&!market.IsChildOf(root.transform))market.position=Position(market.position);
            Physics.SyncTransforms();
        }
    }
}
