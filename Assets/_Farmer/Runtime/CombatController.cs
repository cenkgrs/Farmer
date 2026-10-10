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
        private float attackCooldown, grace=1.2f;
        private Text status;
        private CombatAudio sound;
        public void Warn()=>sound?.Warning();
        private GameObject hud;
        public Vector3 Sanctuary=>sanctuary;
        public EnemyView View(int id)=>views[id];
        public bool CanAct=>game!=null&&game.Ready&&!game.WorldInputBlocked&&!game.BuildMode;
        private void Start()
        {
            sound=gameObject.AddComponent<CombatAudio>();
            game=GetComponent<FarmGame>();motor=game.Player.GetComponent<PlayerMotor>();sanctuary=game.Player.position;
            var canvas=new GameObject("Combat HUD",typeof(Canvas),typeof(CanvasScaler));canvas.transform.SetParent(transform,false);
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<Canvas>().overrideSorting=true;canvas.GetComponent<Canvas>().sortingOrder=2;
            var scale=canvas.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1280,720);scale.matchWidthOrHeight=.5f;
            var panel=FarmHud.Panel("Health and expedition",canvas.transform,new Vector2(.5f,1),new Vector2(0,-20),new Vector2(460,100),new Color(.94f,.87f,.71f,.97f));
            HudArtwork.ApplyFrame(panel);
            panel.GetComponent<Image>().raycastTarget=false;hud=panel.gameObject;
            status=FarmHud.Label(panel,"",15,24,20,412,70,new Color(.25f,.20f,.12f));
            Synchronize();
        }
        private void Synchronize()
        {
            if(game==null||!game.Ready)return;
            if(observed==game.Model)return;
            observed=game.Model;attackCooldown=0;grace=1.2f;
            for(int i=0;i<views.Length;i++)
            {
                if(views[i]!=null){views[i].gameObject.SetActive(false);Destroy(views[i].gameObject);views[i]=null;}
                var e=game.Model.Combat.Enemy(i);
                if(!e.located)
                {
                    // Deterministic scan checks authored scenery and runtime construction/resources.
                    bool found=false;
                    for(int ring=0;ring<12&&!found;ring++)for(int a=0;a<8&&!found;a++)
                    {
                        int x=(i==0?28:48)+(a%4)*3, z=(i==0?-22:-42)-ring*3-(a/4);
                        var point=new Vector3(x+.5f,0,z+.5f);
                        if(Vector3.Distance(point,sanctuary)<26||!WorldGround.SupportsCell(x,z,false)||!Clear(point,2.3f))continue;
                        game.Model.Combat.Locate(i,x,z);found=true;
                    }
                    e=game.Model.Combat.Enemy(i);
                }
                if(!e.located)continue;
                var root=new GameObject(EnemyRules.Catalog[i].Name);root.transform.SetParent(transform,false);
                views[i]=root.AddComponent<EnemyView>();views[i].Configure(this,i,new Vector3(e.x+.5f,0,e.z+.5f));
            }
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
        public void RefreshHud()
        {
            var closest=views.Where(v=>v!=null).OrderBy(v=>PlanarDistance(v.Home,game.Player.position)).FirstOrDefault();
            bool near=closest!=null&&PlanarDistance(closest.Home,game.Player.position)<11;
            hud.SetActive(CanAct&&(near||game.Model.Combat.Health<100||game.Model.EquippedItem==FarmItem.Axe));
            string target=closest==null?"":$"{EnemyRules.Catalog[closest.Id].Area} · {Mathf.RoundToInt(PlanarDistance(closest.Home,game.Player.position))} m";
            if(near)target=game.Model.Combat.Enemy(closest.Id).health>0?$"{EnemyRules.Catalog[closest.Id].Name} · {game.Model.Combat.Enemy(closest.Id).health} can · {(closest.WindingUp?"SALDIRI HAZIRLIĞI!":"6: Balta / sol tık")}":"Zulaya yaklaş · Orman: 6 / Maden: 7 + tık";
            status.text=$"CAN  {game.Model.Combat.Health} / 100\n{target}\nSpace: kaçın   ·   H: sebze ye (+25)";
        }
        public void Simulate(float dt,bool focused=true)
        {
            Synchronize();if(game==null||!game.Ready||dt<=0)return;
            if(!CanAct||!focused){sound.Silence();grace=1.2f;foreach(var v in views)if(v!=null)v.CancelAttack();return;}
            attackCooldown=Mathf.Max(0,attackCooldown-dt);grace=Mathf.Max(0,grace-dt);
            if(game.Model.Combat.RefreshDay(game.Model.Day))game.CombatChanged("Yeni gün: dışarıdaki kaynak zulaları yenilendi.");
            foreach(var v in views)if(v!=null)v.Simulate(dt,game.Player.position,grace<=0);
        }
        public bool UsePointer(bool pressed)
        {
            if(!CanAct||Camera.main==null||Mouse.current==null)return false;
            if(!Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()),out var hit,300,~0,QueryTriggerInteraction.Ignore))return false;
            var enemy=hit.collider.GetComponentInParent<EnemyView>();if(enemy==null)return false;
            if(pressed)TryAttack(enemy.Id);return true;
        }
        public bool TryAttack(int id)
        {
            Synchronize();if(!CanAct||id<0||id>=views.Length||views[id]==null)return false;
            var v=views[id];
            if(PlanarDistance(v.transform.position,game.Player.position)>2.6f||!Visible(v.transform.position,v))
            {game.ShowBuildFeedback("Hedefe yaklaş; arada engel olmamalı.");return false;}
            if(game.Model.Combat.Enemy(id).health==0)
            {
                if(id==1&&game.Model.EquippedItem!=FarmItem.Pickaxe){game.ShowBuildFeedback("Maden kaynağı için kazmayı kuşan (7).");return false;}
                if(game.Model.ClaimExpedition(id,out var message)){game.CombatChanged(message);return true;}
                game.ShowBuildFeedback(message);return false;
            }
            if(game.Model.EquippedItem!=FarmItem.Axe){game.ShowBuildFeedback("Saldırmak için baltayı kuşan (6).");return false;}
            if(attackCooldown>0||motor.IsDodging)return false;
            attackCooldown=.6f;sound.Strike();game.Model.Combat.Strike(id,game.Model.Day);v.Flash();
            game.CombatChanged(game.Model.Combat.Enemy(id).health==0?"Açıklık güvenli. Kaynak zulasını almak için tekrar tıkla.":"Balta · 15 hasar",v.transform.position+Vector3.up*.6f);
            return true;
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
            sound.Hurt();
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
