using System.Linq;
using UnityEngine;
namespace Farmer
{
    // Direction is fixed at release. Swept collision prevents tunnelling through a target.
    public sealed class HuntingArrow:MonoBehaviour
    {
        public const float Range=12,Speed=22,Radius=.075f;
        private FarmGame game;private FarmModel launchedModel;private Vector3 direction;private Material material;
        public float Travelled {get;private set;}
        public bool Finished {get;private set;}
        public Collider HitCollider {get;private set;}
        public static HuntingArrow Launch(FarmGame source,Vector3 origin,Vector3 heading)
        {
            var obj=new GameObject("Flying hunting arrow");var arrow=obj.AddComponent<HuntingArrow>();arrow.game=source;arrow.launchedModel=source.Model;arrow.direction=heading.normalized;
            obj.transform.SetPositionAndRotation(origin,Quaternion.LookRotation(arrow.direction));
            arrow.material=VillageArtwork.Material(new Color(.72f,.53f,.22f));
            VillageArtwork.Part(obj.transform,"Arrow shaft",PrimitiveType.Cylinder,new Vector3(0,0,-.3f),new Vector3(.022f,.26f,.022f),arrow.material).localRotation=Quaternion.Euler(90,0,0);
            VillageArtwork.Part(obj.transform,"Arrow head",PrimitiveType.Sphere,Vector3.zero,new Vector3(.065f,.03f,.12f),arrow.material);
            return arrow;
        }
        private void Update(){if(Application.isFocused)Simulate(Time.deltaTime);}
        public void Simulate(float dt)
        {
            if(Finished||dt<=0)return;
            if(game==null||!game.Ready||game.Model!=launchedModel){Finish();return;}
            if(game.WorldInputBlocked||game.BuildMode)return;
            var inside=Physics.OverlapSphere(transform.position,Radius,~0,QueryTriggerInteraction.Ignore).FirstOrDefault(Valid);
            if(inside!=null){Hit(inside);return;}
            float distance=Mathf.Min(Speed*dt,Range-Travelled);
            var hit=Physics.SphereCastAll(transform.position,Radius,direction,distance,~0,QueryTriggerInteraction.Ignore).Where(h=>Valid(h.collider)).OrderBy(h=>h.distance).FirstOrDefault();
            if(hit.collider!=null){Travelled+=hit.distance;transform.position+=direction*hit.distance;Hit(hit.collider);return;}
            transform.position+=direction*distance;Travelled+=distance;if(Travelled>=Range-.001f)Finish();
        }
        private bool Valid(Collider c)=>!c.transform.IsChildOf(game.Player)&&!c.transform.IsChildOf(transform);
        private void Hit(Collider c)
        {
            HitCollider=c;var animal=c.GetComponentInParent<WildlifeView>();var enemy=c.GetComponentInParent<EnemyView>();
            if(animal!=null)game.GetComponent<WildlifeController>().ArrowHit(animal.Id);
            else if(enemy!=null)game.GetComponent<CombatController>().ArrowHit(enemy.Id);
            Finish();
        }
        private void Finish(){Finished=true;Destroy(gameObject);}
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
