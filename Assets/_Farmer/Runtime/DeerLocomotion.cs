using UnityEngine;

namespace Farmer
{
    // A small skin and four two-joint legs fitted to the delivered deer mesh.
    // Preserves its UVs/materials; no per-frame vertex edits or external service.
    public sealed class DeerLocomotion : MonoBehaviour
    {
        private readonly Transform[] hips=new Transform[4],knees=new Transform[4];
        private readonly Vector3[] hipRest=new Vector3[4],kneeRest=new Vector3[4],footRest=new Vector3[4];
        private Mesh skin;
        private float phase,blend;
        public SkinnedMeshRenderer Skin {get;private set;}
        public float MotionBlend=>blend;
        private void Awake()
        {
            var filter=GetComponentInChildren<MeshFilter>();if(filter==null)return;
            var renderer=filter.GetComponent<MeshRenderer>();if(renderer==null)return;
            var space=filter.transform;var bones=new Transform[9];
            bones[0]=Bone("Deer body",space,Vector3.zero);
            for(int i=0;i<4;i++)
            {
                bool front=i<2;float side=i%2==0?-1:1;
                hipRest[i]=new Vector3(side*.14f,.82f,front?-.17f:.64f);
                kneeRest[i]=new Vector3(side*.14f,.39f,front?-.16f:.77f);
                footRest[i]=new Vector3(side*.14f,.055f,front?-.25f:.74f);
                hips[i]=Bone((front?"Front":"Hind")+(side<0?" left":" right")+" hip",bones[0],hipRest[i]);
                knees[i]=Bone("Knee",hips[i],kneeRest[i]-hipRest[i]);bones[1+i*2]=hips[i];bones[2+i*2]=knees[i];
            }
            skin=Instantiate(filter.sharedMesh);skin.name="Deer walking skin";
            var vertices=skin.vertices;var weights=new BoneWeight[vertices.Length];
            for(int i=0;i<vertices.Length;i++)
            {
                var p=vertices[i];int leg=(p.z<.25f?0:2)+(p.x<0?0:1);
                float limb=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.58f,.94f,p.y));
                if(p.y>.58f)limb*=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.16f,.34f,Mathf.Abs(p.z-hipRest[leg].z)));
                float lower=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.30f,.49f,p.y));
                weights[i]=new BoneWeight{boneIndex0=0,weight0=1-limb,boneIndex1=1+leg*2,weight1=limb*(1-lower),boneIndex2=2+leg*2,weight2=limb*lower};
            }
            var bind=new Matrix4x4[bones.Length];for(int i=0;i<bones.Length;i++)bind[i]=bones[i].worldToLocalMatrix*space.localToWorldMatrix;
            skin.boneWeights=weights;skin.bindposes=bind;
            var skinnedObject=new GameObject("Animated deer mesh");skinnedObject.transform.SetParent(space,false);Skin=skinnedObject.AddComponent<SkinnedMeshRenderer>();Skin.sharedMesh=skin;Skin.sharedMaterials=renderer.sharedMaterials;Skin.bones=bones;Skin.rootBone=bones[0];
            var bounds=skin.bounds;bounds.Expand(new Vector3(.2f,.3f,1));Skin.localBounds=bounds;Skin.updateWhenOffscreen=false;
            renderer.enabled=false;
        }
        private static Transform Bone(string name,Transform parent,Vector3 position)
        {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;}
        public void ResetPose()
        {phase=blend=0;foreach(var t in hips)if(t!=null)t.localRotation=Quaternion.identity;foreach(var t in knees)if(t!=null)t.localRotation=Quaternion.identity;}
        public void Step(float speed,float dt)
        {
            if(Skin==null||dt<=0)return;
            speed=Mathf.Clamp(speed,0,4.5f);blend=Mathf.MoveTowards(blend,speed>.05f?1:0,dt*7);
            if(blend==0){ResetPose();return;}
            float run=Mathf.InverseLerp(1.2f,4.1f,speed),frequency=Mathf.Lerp(1.55f,3f,run);
            phase=Mathf.Repeat(phase+dt*frequency,1);
            for(int i=0;i<4;i++)
            {
                // Four-beat walk transitions to diagonal pairs when fleeing.
                float offset=i==0?0:i==1?.5f:i==2?Mathf.Lerp(.75f,.5f,run):Mathf.Lerp(.25f,0,run);
                float cycle=Mathf.Repeat(phase+offset,1),stance=.62f;
                float stride=Mathf.Lerp(.40f,.85f,run),travel,lift;
                if(cycle<stance){travel=Mathf.Lerp(-stride*.5f,stride*.5f,cycle/stance);lift=0;}
                else{float swing=(cycle-stance)/(1-stance);travel=Mathf.Lerp(stride*.5f,-stride*.5f,Mathf.SmoothStep(0,1,swing));lift=Mathf.Sin(swing*Mathf.PI)*Mathf.Lerp(.12f,.23f,run);}
                var target=footRest[i]+new Vector3(0,lift+.035f,travel);
                Solve(i,target,blend);
            }
        }
        private void Solve(int i,Vector3 target,float amount)
        {
            var upper=kneeRest[i]-hipRest[i];var lower=footRest[i]-kneeRest[i];var delta=target-hipRest[i];
            float a=upper.magnitude,b=lower.magnitude,d=Mathf.Clamp(new Vector2(delta.y,delta.z).magnitude,.1f,a+b-.002f);
            float aim=Mathf.Atan2(delta.z,-delta.y),bend=Mathf.Acos(Mathf.Clamp((a*a+d*d-b*b)/(2*a*d),-1,1));
            float angle=aim+(i<2?-bend:bend),restUpper=Mathf.Atan2(upper.z,-upper.y),restLower=Mathf.Atan2(lower.z,-lower.y);
            var remaining=new Vector2(d*Mathf.Sin(aim)-a*Mathf.Sin(angle),d*Mathf.Cos(aim)-a*Mathf.Cos(angle));
            float lowerAngle=Mathf.Atan2(remaining.x,remaining.y);
            // Rotation around X maps downwards Y to negative Z.
            hips[i].localRotation=Quaternion.Slerp(Quaternion.identity,Quaternion.Euler(-(angle-restUpper)*Mathf.Rad2Deg,0,0),amount);
            knees[i].localRotation=Quaternion.Slerp(Quaternion.identity,Quaternion.Euler(-((lowerAngle-restLower)-(angle-restUpper))*Mathf.Rad2Deg,0,0),amount);
        }
        private void OnDestroy(){if(skin!=null)Destroy(skin);}
    }
}
