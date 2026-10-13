using System.Linq;
using UnityEngine;

namespace Farmer
{
    [RequireComponent(typeof(PlayerMotor), typeof(CharacterController))]
    public sealed class PlayerFootsteps : MonoBehaviour
    {
        private PlayerMotor motor;
        private CharacterController controller;
        private FarmGame game;
        private AudioSource source;
        private AudioClip[][] clips;
        private MeshFilter[] paving;
        private Vector3 previousPosition;
        private float distance;
        private int previousClip = -1;
        public int StepsPlayed { get; private set; }
        public enum Surface { Grass, Stone, Wood }
        public Surface LastSurface { get; private set; }

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();controller = GetComponent<CharacterController>();
            game = FindFirstObjectByType<FarmGame>();
            source = gameObject.AddComponent<AudioSource>();source.playOnAwake = false;
            source.spatialBlend = 0;source.volume = .35f;source.priority = 100;
            clips = new[]{Load("grass"), Load("concrete"), Load("wood")};
        }
        private static AudioClip[] Load(string material) => Enumerable.Range(0,5)
            .Select(i=>Resources.Load<AudioClip>($"Footsteps/footstep_{material}_{i:000}")).Where(c=>c!=null).ToArray();
        private void Start()
        {
            // These decorative road meshes deliberately have no movement colliders.
            paving = FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)
                .Where(m=>m.GetComponent<PavingTiling>()!=null||m.name=="Main road surface").ToArray();
        }
        private void OnEnable() { previousPosition=transform.position;distance=0; }
        private void OnDisable() { source.Stop();distance=0; }
        private void OnApplicationFocus(bool focused) { if(!focused){source.Stop();distance=0;}previousPosition=transform.position; }
        private void LateUpdate()
        {
            var moved=transform.position-previousPosition;previousPosition=transform.position;moved.y=0;
            bool blocked=!Application.isFocused||!motor.enabled||motor.IsDodging||motor.CombatFreeze>0
                ||game!=null&&(!game.Ready||game.WorldInputBlocked);
            StepDistance(moved.magnitude,controller.isGrounded,motor.IsSprinting,blocked);
        }
        public void StepDistance(float travelled,bool grounded,bool sprinting,bool blocked)
        {
            // Teleports and respawns must not generate a burst of footsteps.
            if(blocked||!grounded||travelled<.001f||travelled>1.5f){distance=0;if(blocked)source.Stop();return;}
            distance+=travelled;
            float stride=sprinting?2f:1.65f;
            if(distance<stride)return;
            distance%=stride;
            LastSurface=SurfaceAt(transform.position);
            var bank=clips[(int)LastSurface];if(bank.Length==0)return;
            int index=bank.Length==1?0:(previousClip+Random.Range(1,bank.Length))%bank.Length;
            previousClip=index;source.pitch=Random.Range(.94f,1.06f);
            source.PlayOneShot(bank[index],sprinting?.9f:.7f);StepsPlayed++;
        }
        public Surface SurfaceAt(Vector3 position)
        {
            // The topmost physical surface wins, including raised player-built floors.
            foreach(var hit in Physics.RaycastAll(position+Vector3.up*.25f,Vector3.down,.65f,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))
            {
                if(hit.transform.IsChildOf(transform)||hit.normal.y<.7f)continue;
                var block=hit.collider.GetComponentInParent<PlacedBlockView>();
                if(block?.Record!=null)return block.Record.pieceId=="stone_path"?Surface.Stone:Surface.Wood;
                break;
            }
            if(game!=null&&game.Ready)
                foreach(var b in game.Model.Building.Blocks)
                    if(b.x==Mathf.FloorToInt(position.x)&&b.z==Mathf.FloorToInt(position.z)&&Mathf.Abs(position.y-b.level)<.4f)
                    {if(b.pieceId=="stone_path")return Surface.Stone;if(b.pieceId=="wood_floor")return Surface.Wood;}
            if(paving!=null)foreach(var mesh in paving)
            {
                if(mesh==null||!mesh.gameObject.activeInHierarchy||Mathf.Abs(position.y-mesh.transform.position.y)>.4f)continue;
                var point=mesh.transform.InverseTransformPoint(position);var bounds=mesh.sharedMesh.bounds;
                if(point.x<bounds.min.x||point.x>bounds.max.x||point.z<bounds.min.z||point.z>bounds.max.z)continue;
                if(mesh.GetComponent<PavingTiling>()!=null)return Surface.Stone;
                var vertices=mesh.sharedMesh.vertices;var triangles=mesh.sharedMesh.triangles;
                for(int i=0;i<triangles.Length;i+=3)
                    if(Inside(point,vertices[triangles[i]],vertices[triangles[i+1]],vertices[triangles[i+2]]))return Surface.Stone;
            }
            return Surface.Grass;
        }
        private static bool Inside(Vector3 p,Vector3 a,Vector3 b,Vector3 c)
        {
            float Cross(Vector3 u,Vector3 v)=>u.x*v.z-u.z*v.x;
            if(Mathf.Abs(Cross(b-a,c-a))<.00001f)return false;
            float x=Cross(b-a,p-a),y=Cross(c-b,p-b),z=Cross(a-c,p-c);
            return (x>=0&&y>=0&&z>=0)||(x<=0&&y<=0&&z<=0);
        }
    }
}
