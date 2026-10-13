using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Farmer
{
    // All enemies are confined to optional expedition pockets, away from the starting farm.
    public sealed class CombatController : MonoBehaviour
    {
        private FarmGame game;
        private FarmModel observed;
        private PlayerMotor motor;
        private Vector3 sanctuary;
        private readonly EnemyView[] views=new EnemyView[2];
        private float attackCooldown, grace=1.2f, impactDelay, hitStop;
        private int pendingTarget=-1;
        public bool ImpactPaused => hitStop>0;
        private CombatAudio sound;
        public void Warn()=>sound?.Warning();
        public Vector3 Sanctuary=>sanctuary;
        public EnemyView View(int id)=>views[id];
        public bool CanAct=>game!=null&&game.Ready&&!game.WorldInputBlocked&&!game.BuildMode;
        private void Start()
        {
            sound=gameObject.AddComponent<CombatAudio>();
            game=GetComponent<FarmGame>();motor=game.Player.GetComponent<PlayerMotor>();sanctuary=game.Player.position;
            Synchronize();
        }
        private void Synchronize()
        {
            if(game==null||!game.Ready)return;
            if(observed==game.Model)return;
            observed=game.Model;attackCooldown=0;grace=1.2f;pendingTarget=-1;hitStop=0;
            for(int i=0;i<views.Length;i++)
            {
                if(views[i]!=null){views[i].gameObject.SetActive(false);Destroy(views[i].gameObject);views[i]=null;}
                var e=game.Model.Combat.Enemy(i);
                if(!e.located || PlanarDistance(new Vector3(e.x,0,e.z),sanctuary)>34)
                {
                    // Deterministic scan checks authored scenery and runtime construction/resources.
                    bool found=false;
                    for(int ring=0;ring<6&&!found;ring++)for(int a=0;a<8&&!found;a++)
                    {
                        int x=Mathf.RoundToInt(sanctuary.x)+(i==0?18:-18)+(a%4)*2, z=Mathf.RoundToInt(sanctuary.z)-12-ring*2-(a/4);
                        var point=new Vector3(x+.5f,0,z+.5f);
                        if(Mathf.Abs(x)>110||Mathf.Abs(z)>110||Vector3.Distance(point,sanctuary)<17 || PlanarDistance(point,sanctuary)>34 || new Vector2(x,z).magnitude<16||!WorldGround.SupportsCell(x,z,false)||!Clear(point,2.3f))continue;
                        game.Model.Combat.Locate(i,x,z);found=true;
                    }
                    e=game.Model.Combat.Enemy(i);
                }
                if(!e.located)continue;
                var root=new GameObject(EnemyRules.Catalog[i].Name);root.transform.SetParent(transform,false);
                views[i]=root.AddComponent<EnemyView>();views[i].Configure(this,i,new Vector3(e.x+.5f,0,e.z+.5f));
            }
            CombatOccluder.Attach(this,game);
            game.SaveGame();
        }
        public Vector3? SpawnPoint(Vector3 home,EnemyView ignore)
        {
            for(int r=0;r<5;r++)for(int x=-r;x<=r;x++)for(int z=-r;z<=r;z++)
            {
                var p=home+new Vector3(x,0,z);int cx=Mathf.FloorToInt(p.x),cz=Mathf.FloorToInt(p.z);
                if(PlanarDistance(home,p)>5||game.Model.IndexAt(cx,cz)>=0||!WorldGround.SupportsCell(cx,cz,false))continue;
                if(!Physics.OverlapCapsule(p+Vector3.up*.5f,p+Vector3.up*.95f,.49f,~0,QueryTriggerInteraction.Ignore)
                    .Any(c=>c.GetComponent<WorldGround>()==null&&c.GetComponentInParent<EnemyView>()!=ignore))return p;
            }
            return null;
        }
        private bool Clear(Vector3 point,float radius)=>!Physics.OverlapSphere(point+Vector3.up*.9f,radius,~0,QueryTriggerInteraction.Ignore)
            .Any(c=>c.GetComponent<WorldGround>()==null&&!c.transform.IsChildOf(game.Player));
        private void Update()
        {
            Synchronize();if(game==null||!game.Ready)return;
            Simulate(Mathf.Min(Time.deltaTime,.1f),Application.isFocused);
            if(CanAct&&Application.isFocused&&Keyboard.current?.hKey.wasPressedThisFrame==true)
            {if(game.Model.EatHarvest(out string message))game.CombatChanged(message);else game.ShowBuildFeedback(message);}
            RefreshHud();
        }
        public void RefreshHud()=>GetComponent<FarmHud>()?.RefreshHealth();
        public void Simulate(float dt,bool focused=true)
        {
            Synchronize();if(game==null||!game.Ready||dt<=0)return;
            if(!CanAct||!focused){sound.Silence();pendingTarget=-1;hitStop=0;grace=1.2f;foreach(var v in views)if(v!=null)v.CancelAttack();return;}
            if(hitStop>0){hitStop=Mathf.Max(0,hitStop-dt);return;}
            if(pendingTarget>=0)
            {
                impactDelay-=dt;
                if(game.Model.EquippedItem!=FarmItem.Axe||motor.IsDodging)pendingTarget=-1;
                else if(impactDelay<=0){int id=pendingTarget;pendingTarget=-1;ResolveStrike(id);}
            }
            attackCooldown=Mathf.Max(0,attackCooldown-dt);grace=Mathf.Max(0,grace-dt);
            if(game.Model.Combat.RefreshDay(game.Model.Day))game.CombatChanged("Yeni gün: dışarıdaki kaynak zulaları yenilendi.");
            foreach(var v in views)if(v!=null)v.Simulate(dt,game.Player.position,grace<=0);
        }
        public bool UsePointer(bool pressed)
        {
            if(!CanAct||Camera.main==null||Mouse.current==null)return false;
            var ray=Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            EnemyView enemy=null;
            if(Physics.Raycast(ray,out var hit,300,~0,QueryTriggerInteraction.Ignore))enemy=hit.collider.GetComponentInParent<EnemyView>();
            if(enemy==null && game.Model.EquippedItem==FarmItem.Axe)
            {
                // Screen-space assistance scales with zoom; walls still block the actual strike.
                float radius=Mathf.Clamp(Screen.height*.09f,48,110),best=radius;
                foreach(var candidate in views)
                {
                    if(candidate==null||Health(candidate.Id)<=0||PlanarDistance(candidate.transform.position,game.Player.position)>3.1f)continue;
                    var screen=Camera.main.WorldToScreenPoint(candidate.transform.position+Vector3.up*.5f);
                    float d=Vector2.Distance(Mouse.current.position.ReadValue(),screen);
                    if(screen.z>0&&d<best&&Visible(candidate.transform.position,candidate)){enemy=candidate;best=d;}
                }
            }
            if(enemy==null)return false;
            if(pressed)TryAttack(enemy.Id);return true;
        }

        public bool TryAttack(int id)
        {
            Synchronize();if(!CanAct||id<0||id>=views.Length||views[id]==null)return false;
            var v=views[id];
            if(PlanarDistance(v.transform.position,game.Player.position)>3.1f||!Visible(v.transform.position,v))
            {game.ShowBuildFeedback("Hedefe yaklaş; arada engel olmamalı.");return false;}
            if(game.Model.Combat.Enemy(id).health==0)
            {
                if(id==1&&game.Model.EquippedItem!=FarmItem.Pickaxe){game.ShowBuildFeedback("Maden kaynağı için kazmayı kuşan (7).");return false;}
                if(game.Model.ClaimExpedition(id,out var message)){game.CombatChanged(message);return true;}
                game.ShowBuildFeedback(message);return false;
            }
            if(game.Model.EquippedItem!=FarmItem.Axe){game.ShowBuildFeedback("Saldırmak için baltayı kuşan (6).");return false;}
            if(attackCooldown>0||motor.IsDodging)return false;
            attackCooldown=.6f;pendingTarget=id;impactDelay=.2f;
            CombatFeedback.Slash(game.Player,v.transform.position);
            game.Player.GetComponentInChildren<FarmerAnimator>()?.BeginCombatSwing(v.transform.position+Vector3.up*.6f);
            motor.FacingTarget=v.transform.position;
            return true;
        }
        private void ResolveStrike(int id)
        {
            var v=views[id];
            if(v==null||Health(id)<=0||PlanarDistance(v.transform.position,game.Player.position)>3.2f||!Visible(v.transform.position,v))return;
            int damage=Mathf.Min(15,Health(id));
            sound.Strike();game.Model.Combat.Strike(id,game.Model.Day);v.Flash();
            hitStop=.065f;motor.CombatFreeze=.065f;
            CombatFeedback.Burst(v.transform.position+Vector3.up*.6f,damage.ToString(),new Color(1,.82f,.28f));
            string message="Balta · "+damage+" hasar";
            if(Health(id)==0)
            {
                if(game.Model.ClaimExpedition(id,out var reward))
                {message=reward;CombatFeedback.Burst(v.transform.position+Vector3.up*2.2f,id==0?"+12 ODUN":"+12 TAŞ",new Color(.55f,1,.45f));}
                else message="Düşman yenildi · "+reward;
            }
            game.CombatChanged(message);
        }
        public bool Visible(Vector3 target,EnemyView view)
        {
            var origin=game.Player.position+Vector3.up*.65f;var d=target+Vector3.up*.65f-origin;
            return !Physics.RaycastAll(origin,d.normalized,d.magnitude,~0,QueryTriggerInteraction.Ignore)
                .Any(h=>!h.transform.IsChildOf(game.Player)&&h.transform.GetComponentInParent<EnemyView>()!=view);
        }
        public void HitPlayer(int id)
        {
            if(!CanAct||grace>0||motor.Invulnerable||views[id]==null||!Visible(views[id].transform.position,views[id]))return;
            sound.Hurt();motor.CombatFreeze=.065f;hitStop=.065f;
            CombatFeedback.Burst(game.Player.position+Vector3.up*1.3f,"−"+EnemyRules.Catalog[id].Damage,new Color(1,.25f,.18f));
            if(game.Model.Combat.Damage(EnemyRules.Catalog[id].Damage))
            {
                int loss=game.Model.RecoverFromDefeat();motor.Teleport(SafeReturn());grace=2;
                foreach(var v in views)if(v!=null)v.ReturnHome();
                game.CombatChanged($"Çiftliğe döndün · {loss} para kaybı. Çantan korundu.");
            }
            else game.CombatChanged($"-{EnemyRules.Catalog[id].Damage} can · Space ile kaçın, H ile iyileş.");
        }
        private Vector3 SafeReturn()
        {
            for(int r=0;r<10;r++)for(int x=-r;x<=r;x++)for(int z=-r;z<=r;z++)
            {var p=sanctuary+new Vector3(x,0,z);if(WorldGround.SupportsCell(Mathf.FloorToInt(p.x),Mathf.FloorToInt(p.z),false)&&Clear(p,.48f))return p+Vector3.up*.1f;}
            return sanctuary+Vector3.up*2;
        }
        public int Health(int id)=>game.Model.Combat.Enemy(id).health;
        public bool Claimed(int id)=>game.Model.Combat.Enemy(id).rewardDay==game.Model.Day;
        public static float PlanarDistance(Vector3 a,Vector3 b){a.y=b.y=0;return Vector3.Distance(a,b);}
    }
}
