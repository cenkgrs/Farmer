using UnityEngine;
using UnityEngine.InputSystem;

namespace Farmer
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform model;
        [SerializeField] private float speed = 3.8f;
        [SerializeField] private float walkableHalfExtent = 9.3f;
        private CharacterController controller;
        private FarmGame game;
        private float verticalSpeed;
        private float dodgeTime, dodgeCooldown, invulnerableTime;
        private Vector3 dodgeDirection;
        public bool Invulnerable => invulnerableTime > 0;
        public bool IsDodging => dodgeTime > 0;
        public bool TryDodge(Vector2 input)
        {
            if(viewCamera==null||dodgeCooldown>0||game!=null&&(!game.Ready||game.WorldInputBlocked||game.BuildMode))return false;
            dodgeDirection=PlanarMovement.Direction(input,viewCamera.transform.forward,viewCamera.transform.right);
            if(dodgeDirection.sqrMagnitude<.01f)dodgeDirection=model!=null?model.forward:transform.forward;
            dodgeDirection.y=0;dodgeDirection.Normalize();dodgeTime=.24f;invulnerableTime=.34f;dodgeCooldown=1.2f;return true;
        }
        public void CancelDodge(){dodgeTime=invulnerableTime=0;}
        public void Teleport(Vector3 position)
        {CancelDodge();verticalSpeed=0;controller.enabled=false;transform.position=position;controller.enabled=true;Physics.SyncTransforms();}


        public float Speed => speed;
        public float PlanarSpeed { get; private set; }
        public Transform Visual => model;
        // Tool actions may aim the visual while movement/collision still follow WASD.
        public Vector3? FacingTarget { get; set; }
        public void Configure(Camera camera, Transform visual) { viewCamera = camera; model = visual; }

        private void Awake() { controller = GetComponent<CharacterController>();game=FindFirstObjectByType<FarmGame>(); }

        private void Update()
        {
            Vector2 input = Vector2.zero;
            var keyboard = Keyboard.current;
            if (Application.isFocused && keyboard != null && (game==null||(!game.WorldInputBlocked)))
            {
                input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0)
                    - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
                input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0)
                    - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
            }
            if(Application.isFocused&&keyboard?.spaceKey.wasPressedThisFrame==true)TryDodge(input);
            if(!Application.isFocused||game!=null&&game.WorldInputBlocked)CancelDodge();
            Step(input, Mathf.Min(Time.deltaTime, 0.1f));
        }

        public void Step(Vector2 input, float deltaTime)
        {
            if (viewCamera == null || deltaTime <= 0f) return;
            Vector3 direction = PlanarMovement.Direction(input, viewCamera.transform.forward, viewCamera.transform.right);
            if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            verticalSpeed = Mathf.Max(verticalSpeed - 20f * deltaTime, -30f);
            float burstTime=Mathf.Min(dodgeTime,deltaTime);
            Vector3 travel=dodgeDirection*8f*burstTime+direction*speed*(deltaTime-burstTime);
            if(dodgeTime>0)direction=dodgeDirection;
            dodgeTime=Mathf.Max(0,dodgeTime-deltaTime);dodgeCooldown=Mathf.Max(0,dodgeCooldown-deltaTime);invulnerableTime=Mathf.Max(0,invulnerableTime-deltaTime);
            Vector3 target = transform.position + travel;
            Vector3 displacement = PlanarMovement.ClampToGround(target, walkableHalfExtent) - transform.position;
            displacement.y = verticalSpeed * deltaTime;
            Vector3 before = transform.position;
            controller.Move(displacement);
            Vector3 moved = transform.position - before; moved.y = 0;
            PlanarSpeed = moved.magnitude / deltaTime;
            Vector3 facing = FacingTarget.HasValue ? FacingTarget.Value - transform.position : direction;
            facing.y = 0;
            if (model != null && facing.sqrMagnitude > 0.001f)
                model.rotation = Quaternion.RotateTowards(model.rotation, Quaternion.LookRotation(facing), 720f * deltaTime);
        }
    }
}
