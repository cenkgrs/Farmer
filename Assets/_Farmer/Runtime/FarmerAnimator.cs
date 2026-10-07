using System.Linq;
using UnityEngine;

namespace Farmer
{
    // Visual animation follows collision-resolved movement; it never moves the gameplay root.
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Animator))]
    public sealed class FarmerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private FarmGame game;
        [SerializeField] private Transform toolSocket;
        private Animator animator;
        private HumanPoseHandler poseHandler;
        private HumanPose pose;
        private int[] gripMuscles;
        private float gripWeight;
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        public void Configure(PlayerMotor player, FarmGame farm, Transform socket)
        { motor = player; game = farm; toolSocket = socket; }
        private void Awake()
        {
            animator = GetComponent<Animator>(); animator.applyRootMotion = false;
            poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            gripMuscles = Enumerable.Range(0, HumanTrait.MuscleCount)
                .Where(i => HumanTrait.MuscleName[i].StartsWith("Right ") && HumanTrait.MuscleName[i].Contains("Stretched")).ToArray();
        }
        private void OnDestroy() => poseHandler?.Dispose();
        private void Update()
        {
            if (motor == null) return;
            animator.SetFloat(MoveSpeed, Mathf.Clamp01(motor.PlanarSpeed / motor.Speed), .12f, Time.deltaTime);
            bool holding = game != null && game.Ready && game.Model.ItemCount(game.Model.EquippedItem, game.ActiveCrop.id) > 0;
            gripWeight = Mathf.MoveTowards(gripWeight, holding ? 1 : 0, Time.deltaTime * 8);
        }
        private void OnAnimatorIK(int layerIndex)
        {
            if (motor == null) return;
            if (gripWeight > 0)
            {
                poseHandler.GetHumanPose(ref pose);
                foreach (int muscle in gripMuscles)
                {
                    float closed = HumanTrait.MuscleName[muscle].Contains("Thumb") ? -.1f : -.45f;
                    pose.muscles[muscle] = Mathf.Lerp(pose.muscles[muscle], closed, gripWeight);
                }
                poseHandler.SetHumanPose(ref pose);
            }
            Transform visual = motor.Visual;
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, gripWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKPosition(AvatarIKGoal.RightHand, visual.TransformPoint(new Vector3(.4f, .98f, .32f)));
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, gripWeight * .65f);
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, visual.TransformPoint(new Vector3(.65f, 1.05f, -.1f)));
        }
        private void LateUpdate()
        {
            if (toolSocket == null || motor == null) return;
            var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            var middle = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            Vector3 palm = middle == null ? hand.position : Vector3.Lerp(hand.position, middle.position, .6f);
            // Keep the container upright while its grip follows the animated palm.
            toolSocket.SetPositionAndRotation(palm, motor.Visual.rotation);
        }
    }
}
