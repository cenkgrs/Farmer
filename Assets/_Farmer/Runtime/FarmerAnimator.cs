using System.Linq;
using UnityEngine;

namespace Farmer
{
    // Presentation only: actions never delay transactions or move the collision root.
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Animator))]
    public sealed class FarmerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private FarmGame game;
        [SerializeField] private Transform toolSocket;
        private Animator animator;
        private Transform[] fingers;
        private Quaternion[] closedFingers;
        private float gripWeight, pourWeight, harvestTime = HarvestDuration;
        private Vector3 actionTarget, handOffset, harvestStart;
        private Quaternion toolRotation = Quaternion.identity, harvestStartRotation, handGripBasis;
        private bool handRotationCaptured;
        private FarmModel observedModel;
        private FarmItem observedItem;
        private FarmPresentation presentation;
        private WateringStream stream;
        private const float HarvestDuration = .52f;
        private static readonly Vector3 RestHand = new Vector3(.4f, .98f, .32f);
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        public float PourWeight => pourWeight;
        public bool HarvestActive => harvestTime < HarvestDuration;
        public bool WaterVisible => stream != null && stream.Visible;
        public Transform ToolSocket => toolSocket;

        public void Configure(PlayerMotor player, FarmGame farm, Transform socket)
        { motor = player; game = farm; toolSocket = socket; }
        private void Awake()
        {
            animator = GetComponent<Animator>(); animator.applyRootMotion = false;
            fingers = Enumerable.Range((int)HumanBodyBones.RightThumbProximal,
                    (int)HumanBodyBones.RightLittleDistal - (int)HumanBodyBones.RightThumbProximal + 1)
                .Select(i => animator.GetBoneTransform((HumanBodyBones)i)).Where(b => b != null).ToArray();
            // Sample the avatar's finger curl once. Reapplying a full HumanPose after IK would
            // retarget the solved wrist/arm again, so only finger-local rotations are used at runtime.
            using (var handler = new HumanPoseHandler(animator.avatar, animator.transform))
            {
                var original = new HumanPose(); handler.GetHumanPose(ref original);
                var closed = original; closed.muscles = (float[])original.muscles.Clone();
                for (int i = 0; i < HumanTrait.MuscleCount; i++)
                    if (HumanTrait.MuscleName[i].StartsWith("Right ") && HumanTrait.MuscleName[i].Contains("Stretched"))
                        closed.muscles[i] = HumanTrait.MuscleName[i].Contains("Thumb") ? -.35f : -.8f;
                handler.SetHumanPose(ref closed);
                closedFingers = fingers.Select(b => b.localRotation).ToArray();
                handler.SetHumanPose(ref original);
            }
            handOffset = RestHand;
            if (game != null) presentation = game.GetComponent<FarmPresentation>();
        }
        private void OnEnable()
        {
            if (game != null) { game.Harvested += OnHarvest; game.Changed += OnChanged; OnChanged(); }
        }
        private void OnDisable()
        {
            if (game != null) { game.Harvested -= OnHarvest; game.Changed -= OnChanged; }
            ResetAction();
        }
        private void OnDestroy() => stream?.Dispose();
        private void OnApplicationFocus(bool focused) { if (!focused) ResetAction(); }
        private void OnApplicationPause(bool paused) { if (paused) ResetAction(); }
        private void OnChanged()
        {
            // Loading a save or changing equipment cannot carry a swing into the next tool.
            if (game.BuildMode || game.Model != observedModel || game.Model.EquippedItem != observedItem) ResetAction();
            observedModel = game.Model; observedItem = game.Model.EquippedItem;
        }
        private void ResetAction()
        {
            pourWeight = 0; harvestTime = HarvestDuration;
            handOffset = RestHand; toolRotation = Quaternion.identity;
            stream?.Hide();
            if (motor != null) motor.FacingTarget = null;
        }
        private void OnHarvest(int index, string label)
        {
            if (!isActiveAndEnabled || !Application.isFocused) return;
            harvestStart = handOffset; harvestStartRotation = toolRotation;
            harvestTime = 0;
            actionTarget = game.Selection.Layout.Center(new Vector2Int(index % game.Model.Width, index / game.Model.Width), .08f);
        }
        private void Update()
        {
            if (motor == null) return;
            animator.SetFloat(MoveSpeed, Mathf.Clamp01(motor.PlanarSpeed / motor.Speed), .12f, Time.deltaTime);
            bool ready = game != null && game.Ready && game.isActiveAndEnabled && Application.isFocused;
            bool holding = ready && !game.BuildMode && game.Model.ItemCount(game.Model.EquippedItem, game.ActiveCrop.id) > 0;
            gripWeight = Mathf.MoveTowards(gripWeight, holding ? 1 : 0, Time.deltaTime * 8);
            if (!ready) { ResetAction(); return; }
            bool pouring = game.WateringActive && game.Model.EquippedItem == FarmItem.WateringCan;
            if (pouring && game.Selection.HoveredCell is Vector2Int cell)
                actionTarget = game.Selection.Layout.Center(cell, .08f);
            pourWeight = Mathf.MoveTowards(pourWeight, pouring ? 1 : 0, Time.deltaTime / (pouring ? .18f : .7f));
            harvestTime = Mathf.Min(HarvestDuration, harvestTime + Time.deltaTime);
            float pour = Mathf.SmoothStep(0, 1, pourWeight);
            handOffset = Vector3.Lerp(RestHand, new Vector3(.32f, 1.14f, .57f), pour);
            toolRotation = Quaternion.Euler(48f * pour, 0, -6f * pour);
            if (HarvestActive) SampleHarvest();
            motor.FacingTarget = pouring || HarvestActive ? actionTarget : (Vector3?)null;
        }
        private void SampleHarvest()
        {
            // Short anticipation, cutting arc, then recovery. Rapid successful clicks blend from the current pose.
            Vector3 raised = new Vector3(.63f, 1.09f, .28f), cut = new Vector3(-.12f, .83f, .57f);
            Quaternion raisedRotation = Quaternion.Euler(15, -35, -30), cutRotation = Quaternion.Euler(72, 30, 58);
            if (harvestTime < .1f)
            {
                float t = Mathf.SmoothStep(0, 1, harvestTime / .1f);
                handOffset = Vector3.Lerp(harvestStart, raised, t);
                toolRotation = Quaternion.Slerp(harvestStartRotation, raisedRotation, t);
            }
            else if (harvestTime < .28f)
            {
                float t = Mathf.SmoothStep(0, 1, (harvestTime - .1f) / .18f);
                handOffset = Vector3.Lerp(raised, cut, t);
                toolRotation = Quaternion.Slerp(raisedRotation, cutRotation, t);
            }
            else
            {
                float t = Mathf.SmoothStep(0, 1, (harvestTime - .28f) / .24f);
                handOffset = Vector3.Lerp(cut, RestHand, t);
                toolRotation = Quaternion.Slerp(cutRotation, Quaternion.identity, t);
            }
        }
        private void OnAnimatorIK(int layerIndex)
        {
            if (motor == null) return;
            Transform visual = motor.Visual;
            if (!handRotationCaptured)
            {
                var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                var middle = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
                var index = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
                var little = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
                Vector3 fingers = hand.InverseTransformDirection(middle.position - hand.position).normalized;
                Vector3 across = hand.InverseTransformDirection(index.position - little.position).normalized;
                Vector3 palmNormal = Vector3.Cross(fingers, across).normalized;
                handGripBasis = Quaternion.Inverse(Quaternion.LookRotation(fingers, palmNormal));
                handRotationCaptured = true;
            }
            float actionWeight = Mathf.Max(pourWeight, HarvestActive ? Mathf.Sin(Mathf.PI * harvestTime / HarvestDuration) : 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, gripWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, gripWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, visual.TransformPoint(handOffset));
            Vector3 palmDirection = game.Model.EquippedItem == FarmItem.WateringCan ? Vector3.down : Vector3.left;
            Quaternion gripRotation = Quaternion.LookRotation(Vector3.forward, palmDirection) * handGripBasis;
            animator.SetIKRotation(AvatarIKGoal.RightHand, visual.rotation * toolRotation * gripRotation);
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, gripWeight * .65f);
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, visual.TransformPoint(new Vector3(.65f, 1.05f, -.1f)));
            animator.SetLookAtWeight(actionWeight * .35f, .12f, .65f, 0, .65f);
            if (actionWeight > 0) animator.SetLookAtPosition(actionTarget + Vector3.up * .3f);
        }
        private void LateUpdate()
        {
            if (toolSocket == null || motor == null) return;
            for (int i = 0; i < fingers.Length; i++)
                fingers[i].localRotation = Quaternion.Slerp(fingers[i].localRotation, closedFingers[i], gripWeight);
            var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            var middle = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            Vector3 palm = middle == null ? hand.position : Vector3.Lerp(hand.position, middle.position, .6f);
            toolSocket.SetPositionAndRotation(palm, motor.Visual.rotation * toolRotation);
            if (stream == null) stream = new WateringStream(motor.transform);
            float flow = presentation != null && game.Model.EquippedItem == FarmItem.WateringCan && pourWeight > .02f
                ? presentation.WateringIntensity : 0;
            // The prop's spout is forward after its held-instance yaw correction.
            stream.Draw(toolSocket.TransformPoint(new Vector3(.008f, -.13f, .38f)), actionTarget, flow);
        }
    }
}
