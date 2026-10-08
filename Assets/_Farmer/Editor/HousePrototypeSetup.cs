using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Farmer.Editor
{
    public static class HousePrototypeSetup
    {
        public static void ApplyAndBuild()
        {
            var scene=EditorSceneManager.OpenScene(ProjectSetup.ScenePath,OpenSceneMode.Additive);
            try
            {
                var game=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<FarmGame>()).Single();
                var wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/Wood.mat");
                var trim=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/WoodTrim.mat");
                var metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/SelectionLine.mat");
                EnsureTransparentMaterial(wood);
                var floor=Create("wood_floor","Ahşap döşeme",1,BuildPlacement.Floor,false,wood,trim,metal);
                var wall=Create("wood_wall","Ahşap duvar",2,BuildPlacement.Edge,false,wood,trim,metal);
                var door=Create("wood_door","Ahşap kapı",3,BuildPlacement.Edge,true,wood,trim,metal);
                var serialized=new SerializedObject(game);var pieces=serialized.FindProperty("buildPieces");
                pieces.arraySize=5;pieces.GetArrayElementAtIndex(2).objectReferenceValue=floor;pieces.GetArrayElementAtIndex(3).objectReferenceValue=wall;pieces.GetArrayElementAtIndex(4).objectReferenceValue=door;serialized.ApplyModifiedPropertiesWithoutUndo();
                if(game.GetComponent<DoorInteraction>()==null)game.gameObject.AddComponent<DoorInteraction>();
                if(game.GetComponent<HouseVisibility>()==null)game.gameObject.AddComponent<HouseVisibility>();
                EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            }
            finally {EditorSceneManager.CloseScene(scene,true);}
            Debug.Log("FARMER_HOUSE_SETUP_OK");ProjectSetup.BuildLinux();
        }
        private static void EnsureTransparentMaterial(Material wood)
        {
            const string folder="Assets/_Farmer/Resources/BuildingArt";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/_Farmer/Resources","BuildingArt");
            string path=folder+"/TransparentWood.mat";
            if(AssetDatabase.LoadAssetAtPath<Material>(path)!=null)return;
            var m=new Material(wood);m.name="TransparentWood";
            m.SetFloat("_Surface",1);m.SetFloat("_Blend",0);m.SetFloat("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha",(int)UnityEngine.Rendering.BlendMode.One);m.SetFloat("_DstBlendAlpha",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite",0);m.SetOverrideTag("RenderType","Transparent");m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.DisableKeyword("_ALPHAPREMULTIPLY_ON");m.renderQueue=(int)UnityEngine.Rendering.RenderQueue.Transparent;
            m.SetShaderPassEnabled("ShadowCaster",false);AssetDatabase.CreateAsset(m,path);
        }
        private static BuildDefinition Create(string id,string label,int cost,BuildPlacement placement,bool door,Material wood,Material trim,Material metal)
        {
            string dataPath=$"Assets/_Farmer/Data/{id}.asset";
            var existing=AssetDatabase.LoadAssetAtPath<BuildDefinition>(dataPath);
            if(existing!=null)return existing;
            var root=new GameObject(id);
            try
            {
                if(placement==BuildPlacement.Floor)
                {
                    for(int i=0;i<5;i++)Part(root.transform,new Vector3(-.4f+i*.2f,-.485f,0),new Vector3(.192f,.03f,.99f),i%2==0?wood:trim);
                    Box(root.transform,new Vector3(0,-.485f,0),new Vector3(.99f,.03f,.99f));
                }
                else if(!door)
                {
                    for(int i=0;i<9;i++)Part(root.transform,new Vector3(0,-.35f+i*.26f,.5f),new Vector3(.98f,.25f,.075f),i%3==0?trim:wood);
                    for(int side=-1;side<=1;side+=2)Part(root.transform,new Vector3(side*.455f,.7f,.5f),new Vector3(.09f,2.4f,.105f),trim);
                    Box(root.transform,new Vector3(0,.7f,.5f),new Vector3(.98f,2.4f,.08f));
                }
                else
                {
                    for(int side=-1;side<=1;side+=2)
                    {
                        Part(root.transform,new Vector3(side*.465f,.7f,.5f),new Vector3(.07f,2.4f,.14f),trim);
                        Box(root.transform,new Vector3(side*.465f,.7f,.5f),new Vector3(.07f,2.4f,.14f));
                    }
                    Part(root.transform,new Vector3(0,1.83f,.5f),new Vector3(1,.14f,.14f),trim);
                    Box(root.transform,new Vector3(0,1.83f,.5f),new Vector3(1,.14f,.14f));
                    var hinge=new GameObject("Hinge").transform;hinge.SetParent(root.transform,false);hinge.localPosition=new Vector3(-.425f,-.47f,.5f);
                    for(int i=0;i<4;i++)Part(hinge,new Vector3(.105f+i*.211f,1.09f,0),new Vector3(.205f,2.18f,.065f),wood);
                    for(int i=0;i<2;i++)Part(hinge,new Vector3(.422f,.4f+i*1.35f,-.045f),new Vector3(.78f,.12f,.04f),trim);
                    Part(hinge,new Vector3(.73f,1.04f,-.08f),new Vector3(.055f,.1f,.08f),trim);
                    Box(hinge,new Vector3(.422f,1.09f,0),new Vector3(.844f,2.18f,.075f));
                }
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,$"Assets/_Farmer/Prefabs/{id}.prefab");
                var definition=ScriptableObject.CreateInstance<BuildDefinition>();definition.id=id;definition.displayName=label;definition.woodCost=cost;definition.placement=placement;definition.isDoor=door;definition.prefab=prefab;
                AssetDatabase.CreateAsset(definition,dataPath);return definition;
            }
            finally{Object.DestroyImmediate(root);}
        }
        private static void Box(Transform root,Vector3 center,Vector3 size){var b=root.gameObject.AddComponent<BoxCollider>();b.center=center;b.size=size;}
        private static void Part(Transform root,Vector3 position,Vector3 size,Material material)
        {
            var p=GameObject.CreatePrimitive(PrimitiveType.Cube);Object.DestroyImmediate(p.GetComponent<Collider>());p.transform.SetParent(root,false);p.transform.localPosition=position;p.transform.localScale=size;p.GetComponent<Renderer>().sharedMaterial=material;
        }
    }
}
