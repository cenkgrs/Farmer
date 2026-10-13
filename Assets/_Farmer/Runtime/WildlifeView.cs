using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace Farmer
{
    public sealed class WildlifeView:MonoBehaviour
    {
        private DeerLocomotion gait;
        private WildlifeController owner;private CharacterController body;private Transform art;private Transform[] legs;private Vector3 home,destination;
        private Transform healthRoot;private Image healthFill;private float time,decision,stun,death;private int oldHealth;
        public int Id {get;private set;}
        public WildlifeMood Mood {get;private set;}
        public void Configure(WildlifeController source,int id)
        {
            owner=source;Id=id;home=transform.position;destination=home;oldHealth=owner.Model.Animal(id).health;
            art=VillageArtwork.Animal(WildlifeRules.Deer(id)).transform;art.SetParent(transform,false);legs=art.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Leg")).ToArray();
            if(WildlifeRules.Deer(id))gait=art.gameObject.AddComponent<DeerLocomotion>();
            body=gameObject.AddComponent<CharacterController>();body.radius=WildlifeRules.Deer(id)?.35f:.25f;body.height=WildlifeRules.Deer(id)?1.45f:.65f;body.center=Vector3.up*(body.height/2+.05f);body.stepOffset=.15f;body.skinWidth=.025f;
            var health=new GameObject("Animal health",typeof(Canvas));health.transform.SetParent(transform,false);healthRoot=health.transform;healthRoot.localPosition=Vector3.up*(WildlifeRules.Deer(id)?2.25f:1.45f);healthRoot.localScale=Vector3.one*.01f;health.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            var back=FarmHud.Panel("Animal health background",healthRoot,Vector2.one*.5f,Vector2.zero,new Vector2(92,12),new Color(.13f,.1f,.06f,.9f));back.GetComponent<Image>().raycastTarget=false;
            healthFill=FarmHud.Panel("Animal health fill",back,new Vector2(0,1),new Vector2(2,-2),new Vector2(88,8),new Color(.78f,.24f,.16f)).GetComponent<Image>();healthFill.raycastTarget=false;
            healthRoot.gameObject.SetActive(false);
            if(oldHealth==0&&owner.Model.Animal(id).claimed){death=2;art.gameObject.SetActive(false);body.enabled=false;}
        }
        public void Revive(){if(owner.Model.Animal(Id).health<=0)return;body.enabled=false;transform.position=home;body.enabled=true;art.gameObject.SetActive(true);art.localRotation=Quaternion.identity;death=0;gait?.ResetPose();healthRoot.gameObject.SetActive(false);oldHealth=owner.Model.Animal(Id).health;}
        public void Hit(){stun=.38f;Mood=WildlifeMood.Flee;art.localRotation=Quaternion.Euler(0,0,14);}
        public void Simulate(float dt,Vector3 player)
        {
            var a=owner.Model.Animal(Id);time+=dt;
            float distance=CombatController.PlanarDistance(player,transform.position);
            healthRoot.gameObject.SetActive(distance<13 && a.health>0 && a.health<WildlifeRules.Health(Id));
            healthFill.rectTransform.sizeDelta=new Vector2(88f*a.health/WildlifeRules.Health(Id),8);
            if(Camera.main!=null)healthRoot.rotation=Camera.main.transform.rotation;
            if(a.health==0)
            {
                death+=dt;body.enabled=!a.claimed;art.localRotation=Quaternion.Slerp(art.localRotation,Quaternion.Euler(0,0,85),dt*8);
                if(a.claimed&&death>1.4f)art.gameObject.SetActive(false);return;
            }
            body.enabled=true;
            Mood=WildlifeRules.Mood(distance,Mood==WildlifeMood.Flee);
            if(stun>0){stun-=dt;return;}
            art.localRotation=Quaternion.Slerp(art.localRotation,Quaternion.identity,dt*10);
            Vector3 direction=Vector3.zero;float speed=0;
            if(Mood==WildlifeMood.Flee){direction=transform.position-player;speed=WildlifeRules.Deer(Id)?4.1f:3.6f;}
            else if(Mood==WildlifeMood.Watch){Face(player-transform.position,dt);}
            else
            {
                decision-=dt;
                if(decision<=0){decision=3.5f+Id*.2f;float a1=time*.73f+Id*2.4f;destination=home+new Vector3(Mathf.Cos(a1),0,Mathf.Sin(a1))*4;}
                if(CombatController.PlanarDistance(transform.position,destination)>.5f){direction=destination-transform.position;speed=.85f;}
            }
            var previousPosition=transform.position;
            direction.y=0;
            if(speed>0&&direction.sqrMagnitude>.01f)
            {
                direction.Normalize();bool moved=false;
                foreach(float angle in new[]{0f,40f,-40f,80f,-80f,120f,-120f,180f})
                {
                    var d=Quaternion.Euler(0,angle,0)*direction;var next=transform.position+d*(speed*dt+.45f);
                    if(CombatController.PlanarDistance(next,home)>14||!WorldGround.SupportsCell(Mathf.FloorToInt(next.x),Mathf.FloorToInt(next.z),false))continue;
                    if(Physics.SphereCastAll(transform.position+Vector3.up*.5f,.28f,d,.65f,~0,QueryTriggerInteraction.Ignore).Any(h=>!h.transform.IsChildOf(transform)&&h.collider.GetComponent<WorldGround>()==null))continue;
                    body.Move(d*speed*dt+Vector3.down*dt*3);Face(d,dt);moved=true;break;
                }
                if(moved){art.localPosition=Vector3.up*(Mathf.Abs(Mathf.Sin(time*speed*5))*(WildlifeRules.Deer(Id)?.045f:.12f));for(int l=0;l<legs.Length;l++)legs[l].localRotation=Quaternion.Euler(Mathf.Sin(time*speed*6+l*Mathf.PI)*23,0,0);}
            }
            else {art.localPosition=Vector3.up*(Mathf.Sin(time*2)*.012f);foreach(var leg in legs)leg.localRotation=Quaternion.identity;}
            gait?.Step(CombatController.PlanarDistance(previousPosition,transform.position)/Mathf.Max(dt,.001f),dt);
            oldHealth=a.health;
        }
        private void Face(Vector3 d,float dt){d.y=0;if(d.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(d),dt*9);}
    }
}
