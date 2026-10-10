using System.Collections.Generic;
using UnityEngine;

namespace Farmer
{
    public sealed class EnemyView : MonoBehaviour
    {
        public int Id {get;private set;}
        public Vector3 Home {get;private set;}
        public bool WindingUp=>windup>0;
        private CombatController combat;
        private CharacterController body;
        private Transform visual, cache;
        private Renderer shell;
        private float windup,recovery,flash,phase;
        private bool wasAlive=true;
        private Vector3 aim;
        private readonly List<Material> materials=new List<Material>();
        private Color normal;
        public void Configure(CombatController owner,int id,Vector3 home)
        {
            combat=owner;Id=id;Home=home;transform.position=home;
            body=gameObject.AddComponent<CharacterController>();body.radius=.48f;body.height=1;body.center=new Vector3(0,.5f,0);body.stepOffset=.15f;body.slopeLimit=30;
            visual=new GameObject("Creature").transform;visual.SetParent(transform,false);
            normal=id==0?new Color(.36f,.68f,.30f):new Color(.37f,.40f,.46f);
            shell=Part("Body",visual,new Vector3(0,.5f,0),new Vector3(1.2f,.85f,1.05f),normal,PrimitiveType.Sphere).GetComponent<Renderer>();
            for(int side=-1;side<=1;side+=2)
            {
                Part("Eye",visual,new Vector3(side*.22f,.65f,.46f),Vector3.one*.18f,new Color(.96f,.94f,.80f),PrimitiveType.Sphere);
                Part("Pupil",visual,new Vector3(side*.22f,.65f,.54f),Vector3.one*.085f,new Color(.12f,.17f,.12f),PrimitiveType.Sphere);
                if(id==1)for(int leg=0;leg<3;leg++)Part("Leg",visual,new Vector3(side*.56f,.22f,(leg-1)*.3f),new Vector3(.52f,.12f,.14f),normal*.65f,PrimitiveType.Cube);
            }
            if(id==1)Part("Shell seam",visual,new Vector3(0,.91f,0),new Vector3(.035f,.03f,.6f),normal*.55f,PrimitiveType.Cube);
            cache=new GameObject("Resource cache").transform;cache.SetParent(transform,false);
            for(int i=0;i<3;i++)Part(id==0?"Timber":"Ore",cache,new Vector3((i-1)*.3f,.22f,0),id==0?new Vector3(.25f,.25f,.85f):Vector3.one*.5f,id==0?new Color(.49f,.31f,.14f):new Color(.44f,.54f,.58f),id==0?PrimitiveType.Cube:PrimitiveType.Sphere);
            cache.gameObject.SetActive(false);
            // Small visual landmark only; does not rewrite terrain or reserve buildable cells.
            var landmark=new GameObject(id==0?"Forest boundary stones":"Mine entrance frame").transform;landmark.SetParent(transform.parent,false);landmark.position=Home+Vector3.back*2;
            landmark.SetParent(transform,true); // Keep lifetime owned by this encounter; detach from movement below.
            marker=landmark;marker.SetParent(transform.parent,true);
            for(int side=-1;side<=1;side+=2)
                Part("Post",marker,new Vector3(side*1.1f,.65f,0),new Vector3(.3f,1.3f,.35f),new Color(.39f,.29f,.18f),PrimitiveType.Cube);
            Part("Lintel",marker,new Vector3(0,1.35f,0),new Vector3(2.7f,.22f,.35f),new Color(.47f,.35f,.20f),PrimitiveType.Cube);
            if(id==1)for(int side=-1;side<=1;side+=2)
                Part("Entrance rock",marker,new Vector3(side*1.6f,.55f,.3f),new Vector3(.7f,1.1f,.9f),normal,PrimitiveType.Sphere);
            ReturnHome();
        }
        private Transform marker;
        private GameObject Part(string name,Transform parent,Vector3 position,Vector3 scale,Color color,PrimitiveType type)
        {
            var part=GameObject.CreatePrimitive(type);part.name=name;part.transform.SetParent(parent,false);part.transform.localPosition=position;part.transform.localScale=scale;
            var collider=part.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;material.SetFloat("_Smoothness",.15f);materials.Add(material);part.GetComponent<Renderer>().sharedMaterial=material;return part;
        }
        public void CancelAttack(){windup=0;recovery=1;}
        public void ReturnHome()
        {var point=combat.SpawnPoint(Home,this);body.enabled=false;if(point.HasValue)transform.position=point.Value;body.enabled=point.HasValue;CancelAttack();}
        public void Flash(){flash=.18f;}
        public void Simulate(float dt,Vector3 player,bool active)
        {
            bool alive=combat.Health(Id)>0;
            visual.gameObject.SetActive(alive);cache.gameObject.SetActive(!alive&&!combat.Claimed(Id));
            if(!alive){wasAlive=false;body.enabled=!combat.Claimed(Id);windup=0;return;}
            if(!wasAlive){ReturnHome();wasAlive=true;}
            if(!body.enabled){visual.gameObject.SetActive(false);recovery-=dt;if(recovery<=0)ReturnHome();return;}
            phase+=dt;flash=Mathf.Max(0,flash-dt);recovery=Mathf.Max(0,recovery-dt);
            shell.sharedMaterial.color=flash>0?Color.white:windup>0?new Color(.94f,.48f,.16f):normal;
            float squash=windup>0?.72f:1+Mathf.Sin(phase*5)*.035f;
            visual.localScale=new Vector3(1/squash,squash,1/squash);
            var rule=EnemyRules.Catalog[Id];
            float distance=CombatController.PlanarDistance(transform.position,player);
            bool inTerritory=CombatController.PlanarDistance(Home,player)<7&&CombatController.PlanarDistance(Home,transform.position)<7.2f;
            if(!active){CancelAttack();return;}
            if(windup>0)
            {
                windup=Mathf.Max(0,windup-dt);
                if(windup==0)
                {
                    var toward=player-transform.position;toward.y=0;
                    if(inTerritory&&distance<rule.Reach&&Vector3.Dot(aim,toward.normalized)>.4f)combat.HitPlayer(Id);
                    recovery=1.35f;
                }
                return;
            }
            if(!inTerritory){Move(Home,rule.Speed,dt);return;}
            if(recovery>0)return;
            if(distance<=rule.Reach-.15f&&combat.Visible(transform.position,this))
            {aim=player-transform.position;aim.y=0;aim.Normalize();if(aim.sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(aim);windup=rule.Windup;combat.Warn();return;}
            Move(player,rule.Speed,dt);
        }
        private void Move(Vector3 target,float speed,float dt)
        {
            var direction=target-transform.position;direction.y=0;
            if(direction.magnitude<.2f)return;direction.Normalize();
            // CharacterController handles scenery, doors and player collisions; slide around corners.
            var old=transform.position;body.Move(direction*speed*dt+Vector3.down*2*dt);
            if(CombatController.PlanarDistance(old,transform.position)<speed*dt*.2f)
                body.Move(Vector3.Cross(Vector3.up,direction)*speed*dt);
            if(CombatController.PlanarDistance(Home,transform.position)>7.5f)ReturnHome();
            transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(direction),240*dt);
        }
        private void OnDestroy(){if(marker!=null)Destroy(marker.gameObject);foreach(var m in materials)if(m!=null)Destroy(m);}
    }
}
