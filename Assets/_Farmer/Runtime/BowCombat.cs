using UnityEngine;

namespace Farmer
{
    public sealed class BowCombat:MonoBehaviour
    {
        private FarmGame game;private float nextShot;
        private void Awake()=>game=GetComponent<FarmGame>();
        public bool FireRay(Ray ray)
        {
            var plane=new Plane(Vector3.up,game.Player.position+Vector3.up*.5f);
            return plane.Raycast(ray,out float distance)&&FireDirection(ray.GetPoint(distance)-game.Player.position);
        }
        public bool FireDirection(Vector3 direction)
        {
            var motor=game.Player.GetComponent<PlayerMotor>();direction.y=0;
            if(!game.Ready||game.WorldInputBlocked||game.BuildMode||game.Model.EquippedItem!=FarmItem.Bow||!game.Model.OwnsBow||motor.IsDodging||Time.time<nextShot||direction.sqrMagnitude<.01f)return false;
            nextShot=Time.time+.65f;direction.Normalize();
            HuntingArrow.Launch(game,game.Player.position+Vector3.up*.5f,direction);
            var aim=game.Player.position+direction*4;motor.FacingTarget=aim;
            game.Player.GetComponentInChildren<FarmerAnimator>()?.BeginCombatSwing(aim+Vector3.up*.5f);
            return true;
        }
    }
}
