using System.Collections.Generic;
using UnityEngine;
namespace Farmer
{
    // Shared art entry points allow authored replacements without changing game rules.
    public static class VillageArtwork
    {
        private static readonly Dictionary<string,Sprite> icons=new Dictionary<string,Sprite>();
        public static Sprite Icon(string id)
        {
            if(icons.TryGetValue(id,out var s))return s;
            var t=Resources.Load<Texture2D>("VillageArt/"+id);
            if(t==null)return null;
            return icons[id]=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f);
        }
        public static Material Material(Color c)
        {var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=c;m.SetFloat("_Smoothness",.12f);return m;}
        public static Transform Part(Transform root,string name,PrimitiveType type,Vector3 p,Vector3 scale,Material material)
        {
            var obj=GameObject.CreatePrimitive(type);obj.name=name;obj.transform.SetParent(root,false);obj.transform.localPosition=p;obj.transform.localScale=scale;
            obj.GetComponent<Renderer>().sharedMaterial=material;var collider=obj.GetComponent<Collider>();Object.DestroyImmediate(collider);return obj.transform;
        }
        public static GameObject Animal(bool deer)
        {
            var root=new GameObject(deer?"Deer artwork":"Rabbit artwork");var t=root.transform;
            var fur=Material(deer?new Color(.54f,.30f,.13f):new Color(.66f,.53f,.39f));var cream=Material(new Color(.94f,.84f,.66f));var dark=Material(new Color(.08f,.05f,.035f));
            Part(t,"Body",PrimitiveType.Sphere,new Vector3(0,deer?.9f:.38f,0),deer?new Vector3(.64f,.75f,1.2f):new Vector3(.55f,.6f,.8f),fur);
            if(deer)Part(t,"Neck",PrimitiveType.Capsule,new Vector3(0,1.23f,.44f),new Vector3(.3f,.43f,.34f),fur).localRotation=Quaternion.Euler(-22,0,0);
            float y=deer?1.65f:.64f,z=deer?.68f:.35f;
            Part(t,"Head",PrimitiveType.Sphere,new Vector3(0,y,z),deer?new Vector3(.39f,.43f,.59f):new Vector3(.43f,.45f,.43f),fur);
            Part(t,"Muzzle",PrimitiveType.Sphere,new Vector3(0,y-.07f,z+.23f),new Vector3(.26f,.21f,.22f),cream);
            Part(t,"Nose",PrimitiveType.Sphere,new Vector3(0,y-.015f,z+.33f),new Vector3(.1f,.075f,.055f),dark);
            foreach(int sign in new[]{-1,1})
            {
                Part(t,"Eye",PrimitiveType.Sphere,new Vector3(sign*.175f,y+.05f,z+.14f),new Vector3(.065f,.077f,.065f),dark);
                var ear=Part(t,"Ear",PrimitiveType.Sphere,new Vector3(sign*.18f,y+(deer?.21f:.4f),z-.09f),new Vector3(.14f,deer?.3f:.63f,.11f),fur);ear.localRotation=Quaternion.Euler(deer?20:0,0,sign*(deer?-45:-13));
                Part(ear,"Inner ear",PrimitiveType.Sphere,new Vector3(0,0,.35f),new Vector3(.6f,.8f,.45f),cream);
                if(deer){var antler=Part(t,"Antler",PrimitiveType.Capsule,new Vector3(sign*.14f,y+.43f,z-.08f),new Vector3(.055f,.23f,.055f),cream);antler.localRotation=Quaternion.Euler(-12,0,sign*-18);Part(t,"Antler tip",PrimitiveType.Capsule,new Vector3(sign*.24f,y+.54f,z-.1f),new Vector3(.045f,.12f,.045f),cream).localRotation=Quaternion.Euler(0,0,sign*-48);}
                for(int leg=0;leg<2;leg++)Part(t,"Leg"+sign+leg,PrimitiveType.Capsule,new Vector3(sign*(deer?.22f:.2f),deer?.36f:.14f,leg==0?-.35f:.3f),new Vector3(deer?.14f:.2f,deer?.3f:.105f,deer?.17f:.3f),fur);
            }
            Part(t,"Tail",PrimitiveType.Sphere,new Vector3(0,deer?1:.45f,deer?-.59f:-.4f),new Vector3(.22f,.22f,.22f),cream);
            return root;
        }
        public static GameObject Stove()
        {
            var root=new GameObject("Stone cooking stove");var stone=Material(new Color(.4f,.37f,.32f));var iron=Material(new Color(.12f,.13f,.12f));var ember=Material(new Color(1,.35f,.04f));var wood=Material(new Color(.3f,.15f,.06f));
            Part(root.transform,"Hearth",PrimitiveType.Cube,new Vector3(0,-.28f,0),new Vector3(.88f,.4f,.8f),stone);
            Part(root.transform,"Fire opening",PrimitiveType.Cube,new Vector3(0,-.24f,-.405f),new Vector3(.55f,.26f,.025f),iron);
            for(int i=-1;i<=1;i++)Part(root.transform,"Embers",PrimitiveType.Sphere,new Vector3(i*.15f,-.31f,-.425f),new Vector3(.15f,.075f,.06f),ember);
            Part(root.transform,"Iron top",PrimitiveType.Cylinder,new Vector3(0,-.04f,0),new Vector3(.93f,.055f,.85f),iron);
            Part(root.transform,"Cooking pot",PrimitiveType.Sphere,new Vector3(0,.1f,0),new Vector3(.48f,.33f,.48f),iron);
            Part(root.transform,"Pot lid",PrimitiveType.Cylinder,new Vector3(0,.24f,0),new Vector3(.48f,.035f,.48f),stone);
            Part(root.transform,"Handle",PrimitiveType.Sphere,new Vector3(0,.3f,0),new Vector3(.12f,.09f,.12f),wood);
            var c=root.AddComponent<BoxCollider>();c.center=new Vector3(0,-.07f,0);c.size=new Vector3(.9f,.85f,.85f);
            return root;
        }
        public static GameObject Weapon(bool bow)
        {
            var root=new GameObject(bow?"Hunting bow":"Hunting dagger");var wood=Material(new Color(.42f,.22f,.075f));var metal=Material(new Color(.65f,.73f,.76f));
            if(bow)
            {
                Vector3 previous=new Vector3(0,-.52f,0);
                for(int i=1;i<=12;i++){float a=-Mathf.PI/2+i*Mathf.PI/12;var p=new Vector3(Mathf.Cos(a)*.22f,Mathf.Sin(a)*.52f,0);var segment=Part(root.transform,"Bow limb",PrimitiveType.Cylinder,(p+previous)/2,new Vector3(.035f,Vector3.Distance(p,previous)/2,.035f),wood);segment.up=p-previous;previous=p;}
                Part(root.transform,"Bowstring",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.008f,.52f,.008f),metal);
            }
            else
            {
                Part(root.transform,"Grip",PrimitiveType.Cylinder,new Vector3(0,-.07f,0),new Vector3(.07f,.11f,.07f),wood);
                Part(root.transform,"Guard",PrimitiveType.Cube,new Vector3(0,.06f,0),new Vector3(.19f,.035f,.055f),metal);
                Part(root.transform,"Blade",PrimitiveType.Cube,new Vector3(0,.23f,0),new Vector3(.075f,.33f,.025f),metal);
                Part(root.transform,"Point",PrimitiveType.Sphere,new Vector3(0,.4f,0),new Vector3(.068f,.12f,.025f),metal);
            }
            return root;
        }
        public static GameObject Food(string id)
        {
            var root=new GameObject(id);bool hide=id=="hide",raw=id=="raw_meat";
            var mat=Material(hide?new Color(.55f,.33f,.16f):raw?new Color(.7f,.19f,.17f):new Color(.44f,.2f,.07f));var cream=Material(new Color(.93f,.79f,.56f));
            Part(root.transform,"Main",PrimitiveType.Sphere,Vector3.zero,new Vector3(.8f,.14f,hide?.85f:.5f),mat);
            if(hide){foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})Part(root.transform,"Pelt leg",PrimitiveType.Sphere,new Vector3(x*.32f,0,z*.32f),new Vector3(.26f,.09f,.42f),mat);}
            else{Part(root.transform,"Bone",PrimitiveType.Sphere,new Vector3(.23f,.072f,0),new Vector3(.11f,.02f,.12f),cream);for(int i=0;i<3;i++)Part(root.transform,"Marbling",PrimitiveType.Cube,new Vector3(-.23f+i*.12f,.06f,0),new Vector3(.018f,.028f,.25f),raw?cream:mat);}
            return root;
        }
        public static GameObject Guide()
        {
            var root=new GameObject("Defne");var cloth=Material(new Color(.23f,.32f,.12f));var skin=Material(new Color(.83f,.59f,.37f));var linen=Material(new Color(.9f,.83f,.65f));var hair=Material(new Color(.25f,.12f,.05f));
            Part(root.transform,"Skirt",PrimitiveType.Capsule,new Vector3(0,.58f,0),new Vector3(.55f,.42f,.4f),cloth);
            Part(root.transform,"Vest",PrimitiveType.Capsule,new Vector3(0,1.1f,0),new Vector3(.47f,.31f,.3f),cloth);
            Part(root.transform,"Head",PrimitiveType.Sphere,new Vector3(0,1.62f,.025f),new Vector3(.35f,.43f,.33f),skin);
            Part(root.transform,"Hair",PrimitiveType.Sphere,new Vector3(0,1.71f,-.04f),new Vector3(.38f,.35f,.35f),hair);
            Part(root.transform,"Braid",PrimitiveType.Capsule,new Vector3(.18f,1.36f,-.04f),new Vector3(.11f,.28f,.12f),hair);
            foreach(int side in new[]{-1,1}){Part(root.transform,"Sleeve",PrimitiveType.Capsule,new Vector3(side*.29f,1.03f,0),new Vector3(.18f,.3f,.18f),linen);Part(root.transform,"Hand",PrimitiveType.Sphere,new Vector3(side*.29f,.73f,.03f),new Vector3(.13f,.17f,.13f),skin);Part(root.transform,"Boot",PrimitiveType.Capsule,new Vector3(side*.14f,.16f,.05f),new Vector3(.17f,.15f,.27f),hair);Part(root.transform,"Eye",PrimitiveType.Sphere,new Vector3(side*.075f,1.64f,.18f),new Vector3(.028f,.032f,.02f),hair);}
            var c=root.AddComponent<CapsuleCollider>();c.center=Vector3.up*.85f;c.height=1.7f;c.radius=.32f;
            return root;
        }
    }
}
