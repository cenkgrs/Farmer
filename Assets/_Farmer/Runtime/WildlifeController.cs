using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Farmer
{
    public sealed class WildlifeController : MonoBehaviour
    {
        private FarmGame game;private FarmModel observed;private PlayerMotor motor;private CombatAudio sound;
        private WildlifeView[] views=new WildlifeView[WildlifeRules.Count];
        private float cooldown,delay;private bool released;private int pending=-1;
        private Vector3 home;private FarmItem pendingWeapon;
        private float Reach=>Model.EquippedItem==FarmItem.Bow?12:3.4f;
        public WildlifeView View(int id)=>views[id];
        public FarmModel Model=>game.Model;
        public bool CanAct=>game!=null&&game.Ready&&!game.WorldInputBlocked&&!game.BuildMode;
        private void Start(){game=GetComponent<FarmGame>();motor=game.Player.GetComponent<PlayerMotor>();sound=GetComponent<CombatAudio>();home=game.Player.position;Sync();}
        private void Sync()
        {
            if(observed==game.Model)return;observed=game.Model;pending=-1;
            for(int i=0;i<views.Length;i++)
            {
                if(views[i]!=null){views[i].gameObject.SetActive(false);Destroy(views[i].gameObject);}
                var a=Model.Animal(i);Vector3? point=null;
                if(a.located)point=FindSpace(new Vector3(a.x+.5f,0,a.z+.5f));
                if(!point.HasValue)
                {float angle=i*Mathf.PI/3;point=FindSpace(home+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*18);}
                if(!point.HasValue)continue;
                Model.LocateAnimal(i,Mathf.FloorToInt(point.Value.x),Mathf.FloorToInt(point.Value.z));
                var root=new GameObject((WildlifeRules.Deer(i)?"Deer ":"Rabbit ")+i);root.transform.SetParent(transform,false);root.transform.position=point.Value;
                views[i]=root.AddComponent<WildlifeView>();views[i].Configure(this,i);
            }
            game.SaveGame();
        }
        public Vector3? FindSpace(Vector3 desired)
        {
            for(int r=0;r<10;r++)for(int x=-r;x<=r;x++)for(int z=-r;z<=r;z++)
            {var p=new Vector3(Mathf.Floor(desired.x)+x+.5f,0,Mathf.Floor(desired.z)+z+.5f);if(Mathf.Abs(p.x)>109||Mathf.Abs(p.z)>109||!WorldGround.SupportsCell(Mathf.FloorToInt(p.x),Mathf.FloorToInt(p.z),false))continue;if(!Physics.OverlapCapsule(p+Vector3.up*.3f,p+Vector3.up*1.1f,.45f,~0,QueryTriggerInteraction.Ignore).Any(c=>c.GetComponent<WorldGround>()==null))return p;}
            return null;
        }
        private void Update(){if(game!=null)Simulate(Mathf.Min(Time.deltaTime,.1f),Application.isFocused);}
        public void Simulate(float dt,bool focused=true)
        {
            Sync();if(!CanAct||!focused){pending=-1;return;}
            if(Model.RefreshWildlife()){foreach(var v in views)if(v!=null)v.Revive();game.CombatChanged("Yeni gün: yaban hayatı yenilendi.");}
            cooldown=Mathf.Max(0,cooldown-dt);
            if(pending>=0)
            {
                delay-=dt;
                if(Model.EquippedItem!=pendingWeapon||motor.IsDodging)pending=-1;
                else if(delay<=0){if(pendingWeapon==FarmItem.Bow&&!released){released=true;delay=HuntingArrow.Fly(game.Player.position+Vector3.up,views[pending].transform);}
                    else{int id=pending;pending=-1;Impact(id);}}
            }
            foreach(var v in views)if(v!=null)v.Simulate(dt,game.Player.position);
        }
        public bool UsePointer(bool pressed)
        {
            if(!CanAct||Camera.main==null||Mouse.current==null)return false;
            var ray=Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());WildlifeView target=null;
            if(Physics.Raycast(ray,out var hit,200,~0,QueryTriggerInteraction.Ignore))target=hit.collider.GetComponentInParent<WildlifeView>();
            if(target==null&&FarmModel.HuntingWeapon(Model.EquippedItem))
            {
                float best=Mathf.Clamp(Screen.height*.09f,48,110);
                foreach(var v in views)
                {if(v==null||Model.Animal(v.Id).claimed||CombatController.PlanarDistance(v.transform.position,game.Player.position)>Reach||!Visible(v))continue;var screen=Camera.main.WorldToScreenPoint(v.transform.position+Vector3.up*.5f);float d=Vector2.Distance(screen,Mouse.current.position.ReadValue());if(screen.z>0&&d<best){best=d;target=v;}}
            }
            if(target==null)return false;if(pressed)TryAttack(target.Id);return true;
        }
        private bool Visible(WildlifeView v)
        {var from=game.Player.position+Vector3.up*.65f;var delta=v.transform.position+Vector3.up*.65f-from;return !Physics.RaycastAll(from,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore).Any(h=>!h.transform.IsChildOf(game.Player)&&!h.transform.IsChildOf(v.transform));}
        public bool TryAttack(int id)
        {
            if(!CanAct||id<0||id>=views.Length||views[id]==null)return false;
            var v=views[id];if(CombatController.PlanarDistance(v.transform.position,game.Player.position)>Reach||!Visible(v)){game.ShowBuildFeedback("Avına yaklaş; arada engel olmamalı.");return false;}
            if(Model.Animal(id).health==0){bool ok=Model.ClaimAnimal(id,out var m);game.CombatChanged(m);return ok;}
            if(!FarmModel.HuntingWeapon(Model.EquippedItem)){game.ShowBuildFeedback("Avlanmak için balta (6), yay (8) veya hançer (9) kuşan.");return false;}
            if(cooldown>0||motor.IsDodging)return false;
            cooldown=Model.EquippedItem==FarmItem.Bow?.8f:.6f;pending=id;pendingWeapon=Model.EquippedItem;released=false;delay=.2f;
            if(pendingWeapon!=FarmItem.Bow)CombatFeedback.Slash(game.Player,v.transform.position);game.Player.GetComponentInChildren<FarmerAnimator>()?.BeginCombatSwing(v.transform.position+Vector3.up*.5f);motor.FacingTarget=v.transform.position;return true;
        }
        private void Impact(int id)
        {
            var v=views[id];if(v==null||!Visible(v)||CombatController.PlanarDistance(v.transform.position,game.Player.position)>Reach+.5f)return;
            int before=Model.Animal(id).health;if(!Model.StrikeAnimal(id))return;
            v.Hit();sound?.Strike();motor.CombatFreeze=.065f;
            CombatFeedback.Burst(v.transform.position+Vector3.up,"−"+Mathf.Min(15,before),new Color(1,.8f,.25f));
            string message="Av · 15 hasar";
            if(Model.Animal(id).health==0&&Model.ClaimAnimal(id,out message))CombatFeedback.Burst(v.transform.position+Vector3.up*2,$"+{WildlifeRules.Meat(id,Model.Animal(id).roughKill)} ET · +{WildlifeRules.Hides(id,Model.Animal(id).roughKill)} POST",new Color(.75f,1,.5f));
            game.CombatChanged(message);
        }
    }
}
