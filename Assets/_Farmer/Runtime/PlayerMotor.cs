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
        private float verticalSpeed;

        public float Speed => speed;
        public void Configure(Camera camera, Transform visual) { viewCamera = camera; model = visual; }

        private void Awake() => controller = GetComponent<CharacterController>();

        private void Update()
        {
            Vector2 input = Vector2.zero;
            var keyboard = Keyboard.current;
            if (Application.isFocused && keyboard != null)
            {
                input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0)
                    - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
                input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0)
                    - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
            }
            Step(input, Mathf.Min(Time.deltaTime, 0.1f));
        }

        public void Step(Vector2 input, float deltaTime)
        {
            if (viewCamera == null || deltaTime <= 0f) return;
            Vector3 direction = PlanarMovement.Direction(input, viewCamera.transform.forward, viewCamera.transform.right);
            if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            verticalSpeed = Mathf.Max(verticalSpeed - 20f * deltaTime, -30f);
            Vector3 target = transform.position + direction * speed * deltaTime;
            Vector3 displacement = PlanarMovement.ClampToGround(target, walkableHalfExtent) - transform.position;
            displacement.y = verticalSpeed * deltaTime;
            controller.Move(displacement);
            if (model != null && direction.sqrMagnitude > 0.001f)
                model.rotation = Quaternion.RotateTowards(model.rotation, Quaternion.LookRotation(direction), 720f * deltaTime);
        }
    }
}
