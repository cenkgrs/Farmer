using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        private Transform healthRoot;
        private Image healthFill;
        private Text healthText;
        private float lunge,deathTime;
        private bool strikePending;
        private LineRenderer warningRing;
        private readonly List<Transform> legs=new List<Transform>();
        private Vector3 recoil;
        private void LateUpdate()
        {
            if(healthRoot==null||Camera.main==null)return;
            healthRoot.rotation=Camera.main.transform.rotation;
            int hp=combat.Health(Id);
            healthRoot.gameObject.SetActive((hp>0||!combat.Claimed(Id))&&Vector3.Distance(Camera.main.transform.position,transform.position)<45);
            healthFill.rectTransform.sizeDelta=new Vector2(156f*hp/EnemyRules.Catalog[Id].Health,12);
            healthFill.color=WindingUp?new Color(1,.55f,.12f):new Color(.85f,.22f,.18f);
            healthText.text=hp>0?$"{EnemyRules.Catalog[Id].Name}  {hp}/{EnemyRules.Catalog[Id].Health}":combat.Claimed(Id)?(Id==0?"+12 ODUN · ÇANTADA":"+12 TAŞ · ÇANTADA"):Id==0?"12 ODUN · TIKLA":"12 TAŞ · KAZMA (7)";
        }
        public void Configure(CombatController owner,int id,Vector3 home)
        {
            combat=owner;Id=id;Home=home;transform.position=home;
            body=gameObject.AddComponent<CharacterController>();body.radius=.48f;body.height=1;body.center=new Vector3(0,.5f,0);body.stepOffset=.15f;body.slopeLimit=30;
            visual=new GameObject("Creature").transform;visual.SetParent(transform,false);
            normal=id==0?new Color(.36f,.68f,.30f):new Color(.37f,.40f,.46f);
            var delivered=Resources.Load<GameObject>("ExplorationArt/"+(id==0?"forest_slime":"stone_beetle"));
            if(delivered!=null)
            {
                var mesh=Instantiate(delivered,visual,false);shell=mesh.GetComponentInChildren<Renderer>();
                var material=new Material(shell.sharedMaterial);shell.sharedMaterial=material;materials.Add(material);normal=material.color;
            }
            else
            {
            shell=Part("Body",visual,new Vector3(0,.5f,0),new Vector3(1.2f,.85f,1.05f),normal,PrimitiveType.Sphere).GetComponent<Renderer>();
            for(int side=-1;side<=1;side+=2)
            {
                Part("Eye",visual,new Vector3(side*.22f,.65f,.46f),Vector3.one*.18f,new Color(.96f,.94f,.80f),PrimitiveType.Sphere);
                Part("Pupil",visual,new Vector3(side*.22f,.65f,.54f),Vector3.one*.085f,new Color(.12f,.17f,.12f),PrimitiveType.Sphere);
                if(id==1)for(int leg=0;leg<3;leg++)legs.Add(Part("Leg",visual,new Vector3(side*.56f,.22f,(leg-1)*.3f),new Vector3(.52f,.12f,.14f),normal*.65f,PrimitiveType.Cube).transform);
            }
            if(id==1)Part("Shell seam",visual,new Vector3(0,.91f,0),new Vector3(.035f,.03f,.6f),normal*.55f,PrimitiveType.Cube);
            }
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
            var health=new GameObject("Enemy health",typeof(Canvas));health.transform.SetParent(transform,false);
            healthRoot=health.transform;healthRoot.localPosition=Vector3.up*1.65f;healthRoot.localScale=Vector3.one*.012f;
            health.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            var back=FarmHud.Panel("Health background",healthRoot,new Vector2(.5f,.5f),Vector2.zero,new Vector2(164,40),new Color(.12f,.09f,.07f,.9f));
            back.GetComponent<Image>().raycastTarget=false;
            healthText=FarmHud.Label(back,"",12,4,2,156,20,new Color(1,.94f,.76f));healthText.alignment=TextAnchor.MiddleCenter;
            healthFill=FarmHud.Panel("Health fill",back,new Vector2(0,1),new Vector2(4,-24),new Vector2(156,12),Color.red).GetComponent<Image>();healthFill.raycastTarget=false;
            var ring=new GameObject("Attack warning");ring.transform.SetParent(transform,false);
            warningRing=ring.AddComponent<LineRenderer>();warningRing.useWorldSpace=false;warningRing.loop=true;warningRing.positionCount=40;
            var warningMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));warningMaterial.color=new Color(1,.48f,.12f);materials.Add(warningMaterial);warningRing.sharedMaterial=warningMaterial;
            warningRing.startWidth=warningRing.endWidth=.045f;
            for(int i=0;i<40;i++){float angle=i*Mathf.PI*2/40;warningRing.SetPosition(i,new Vector3(Mathf.Cos(angle),.035f,Mathf.Sin(angle))*EnemyRules.Catalog[id].Reach);}
            warningRing.enabled=false;
            ReturnHome();
        }
        private Transform marker;
        private GameObject Part(string name,Transform parent,Vector3 position,Vector3 scale,Color color,PrimitiveType type)
        {
            var part=GameObject.CreatePrimitive(type);part.name=name;part.transform.SetParent(parent,false);part.transform.localPosition=position;part.transform.localScale=scale;
            var collider=part.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;material.SetFloat("_Smoothness",.15f);materials.Add(material);part.GetComponent<Renderer>().sharedMaterial=material;return part;
        }
        public void CancelAttack(){windup=0;lunge=0;strikePending=false;recovery=1;}
        public void ReturnHome()
        {var point=combat.SpawnPoint(Home,this);body.enabled=false;if(point.HasValue)transform.position=point.Value;body.enabled=point.HasValue;CancelAttack();}
        public void Flash(){flash=.24f;windup=0;lunge=0;strikePending=false;recovery=.38f;recoil=-transform.forward*.32f;}
        public void Simulate(float dt,Vector3 player,bool active)
        {
            bool alive=combat.Health(Id)>0;
            warningRing.enabled=alive&&windup>0;
            visual.gameObject.SetActive(alive);cache.gameObject.SetActive(!alive&&!combat.Claimed(Id));
            if(!alive)
            {
                if(wasAlive){deathTime=.35f;wasAlive=false;}
                deathTime=Mathf.Max(0,deathTime-dt);visual.gameObject.SetActive(deathTime>0);
                visual.localScale=Vector3.one*Mathf.Clamp01(deathTime/.35f);
                visual.localRotation=Quaternion.Euler(0,0,(1-deathTime/.35f)*85);
                body.enabled=!combat.Claimed(Id);windup=0;strikePending=false;return;
            }
            if(!wasAlive){ReturnHome();wasAlive=true;}
            if(!body.enabled){visual.gameObject.SetActive(false);recovery-=dt;if(recovery<=0)ReturnHome();return;}
            lunge=Mathf.Max(0,lunge-dt);
            if(strikePending&&lunge<=.12f)
            {
                strikePending=false;var toward=player-transform.position;toward.y=0;
                if(active&&CombatController.PlanarDistance(Home,player)<7&&toward.magnitude<EnemyRules.Catalog[Id].Reach&&Vector3.Dot(aim,toward.normalized)>.4f)combat.HitPlayer(Id);
            }
            recoil=Vector3.Lerp(recoil,Vector3.zero,dt*13);
            visual.localPosition=recoil+Vector3.forward*(Mathf.Sin(lunge/.22f*Mathf.PI)*.65f);
            phase+=dt;
            for(int i=0;i<legs.Count;i++)legs[i].localRotation=Quaternion.Euler(0,Mathf.Sin(phase*12+i*2)*18,Mathf.Sin(phase*12+i*2)*12);
            flash=Mathf.Max(0,flash-dt);recovery=Mathf.Max(0,recovery-dt);
            shell.sharedMaterial.color=flash>0?Color.white:windup>0?new Color(.94f,.48f,.16f):normal;
            float squash=windup>0?.72f:1+Mathf.Sin(phase*5)*.035f;
            visual.localScale=new Vector3(1/squash,squash,1/squash);
            if(Id==0&&windup<=0&&recovery<=0)visual.localPosition+=Vector3.up*(Mathf.Abs(Mathf.Sin(phase*5))*.16f);
            visual.localRotation=Quaternion.Euler(windup>0?-18:0,0,flash>0?Mathf.Sin(flash*100)*12:0);
            var rule=EnemyRules.Catalog[Id];
            float distance=CombatController.PlanarDistance(transform.position,player);
            bool inTerritory=CombatController.PlanarDistance(Home,player)<7&&CombatController.PlanarDistance(Home,transform.position)<7.2f;
            if(!active){CancelAttack();return;}
            if(windup>0)
            {
                windup=Mathf.Max(0,windup-dt);
                if(windup==0)
                {
                    lunge=.22f;strikePending=true;
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
