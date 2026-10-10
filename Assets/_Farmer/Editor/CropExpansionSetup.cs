using System.IO;
using UnityEditor;
using UnityEngine;

namespace Farmer.Editor
{
    public static class CropExpansionSetup
    {
        private const string Root="Assets/_Farmer/Resources";
        public static void Apply()
        {
            Directory.CreateDirectory(Root+"/Crops");Directory.CreateDirectory(Root+"/Recipes");
            Directory.CreateDirectory(Root+"/CropArt");Directory.CreateDirectory(Root+"/CropIcons");
            AssetDatabase.Refresh();
            var leaf=Material("crop_leaf",new Color(.21f,.48f,.12f));
            var stem=Material("crop_stem",new Color(.29f,.40f,.08f));
            var orange=Material("carrot_root",new Color(.95f,.34f,.06f));
            var red=Material("tomato_fruit",new Color(.85f,.09f,.045f));
            var greenFruit=Material("tomato_unripe",new Color(.43f,.64f,.16f));
            var wood=Material("produce_crate",new Color(.55f,.30f,.12f));
            foreach(var id in new[]{"carrot","tomato"})
            {
                var stages=new GameObject[4];
                for(int stage=0;stage<4;stage++)
                {
                    var root=new GameObject(id+" stage "+stage);
                    float height=id=="carrot"?.12f+stage*.075f:.14f+stage*.19f;
                    Part(root,"Stem",PrimitiveType.Cylinder,new Vector3(0,height/2,0),new Vector3(.025f,height/2,.025f),stem);
                    int leaves=stage==0?2:stage==1?4:6;
                    for(int i=0;i<leaves;i++)
                    {
                        float angle=i*Mathf.PI*2/leaves;var offset=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                        var part=Part(root,"Leaf",PrimitiveType.Sphere,offset*(.05f+stage*.018f)+Vector3.up*(height*.75f),new Vector3(.065f+stage*.02f,.022f,.15f+stage*.035f),leaf);
                        part.transform.localRotation=Quaternion.Euler(-30,-angle*Mathf.Rad2Deg+90,0);
                    }
                    if(id=="carrot"&&stage>=2)
                        Part(root,"Carrot shoulder",PrimitiveType.Sphere,new Vector3(0,.025f,0),new Vector3(stage==3?.20f:.13f,.16f,stage==3?.20f:.13f),orange);
                    if(id=="tomato"&&stage>=2)
                        for(int i=0;i<3;i++)
                        {
                            float a=i*2.094f;
                            Part(root,"Fruit",PrimitiveType.Sphere,new Vector3(Mathf.Cos(a)*.11f,height*.60f-i*.035f,Mathf.Sin(a)*.11f),Vector3.one*(stage==3?.145f:.075f),stage==3?red:greenFruit);
                        }
                    stages[stage]=PrefabUtility.SaveAsPrefabAsset(root,$"{Root}/CropArt/{id}_stage_{stage}.prefab");
                    Object.DestroyImmediate(root);
                }
                var crop=Asset<CropDefinition>($"{Root}/Crops/{id}.asset");
                crop.id=id;crop.displayName=id=="carrot"?"Havuç":"Domates";
                crop.seedPrice=id=="carrot"?14:24;crop.salePrice=id=="carrot"?27:12;
                crop.wateredDays=id=="carrot"?4:5;crop.harvestYield=id=="carrot"?1:2;
                crop.regrowDays=id=="tomato"?2:0;crop.visualScale=1;crop.growthStages=stages;EditorUtility.SetDirty(crop);
            }
            var recipe=Asset<RecipeDefinition>($"{Root}/Recipes/vegetable_crate.asset");
            recipe.id="vegetable_crate";recipe.displayName="Sebze kasası";recipe.salePrice=90;recipe.outputCount=1;
            recipe.ingredients=new[]{new ItemStack{id="crop:turnip",count=1},new ItemStack{id="crop:carrot",count=1},new ItemStack{id="crop:tomato",count=2},new ItemStack{id="wood",count=2}};
            EditorUtility.SetDirty(recipe);
            var crate=new GameObject("vegetable_crate");
            Part(crate,"Base",PrimitiveType.Cube,new Vector3(0,.03f,0),new Vector3(.6f,.06f,.44f),wood);
            for(int i=0;i<2;i++)
            {
                Part(crate,"Side",PrimitiveType.Cube,new Vector3(0,.12f+i*.11f,-.22f),new Vector3(.6f,.08f,.035f),wood);
                Part(crate,"Side",PrimitiveType.Cube,new Vector3(0,.12f+i*.11f,.22f),new Vector3(.6f,.08f,.035f),wood);
                Part(crate,"End",PrimitiveType.Cube,new Vector3(-.3f,.12f+i*.11f,0),new Vector3(.035f,.08f,.44f),wood);
                Part(crate,"End",PrimitiveType.Cube,new Vector3(.3f,.12f+i*.11f,0),new Vector3(.035f,.08f,.44f),wood);
            }
            for(int i=0;i<6;i++)Part(crate,"Produce",PrimitiveType.Sphere,new Vector3((i%3-1)*.16f,.23f,(i/3-.5f)*.17f),Vector3.one*.17f,i%2==0?red:orange);
            PrefabUtility.SaveAsPrefabAsset(crate,$"{Root}/CropArt/vegetable_crate.prefab");Object.DestroyImmediate(crate);
            AssetDatabase.SaveAssets();Debug.Log("FARMER_CROP_EXPANSION_ASSETS_OK");
        }
        private static T Asset<T>(string path) where T:ScriptableObject
        {
            var asset=AssetDatabase.LoadAssetAtPath<T>(path);
            if(asset==null){asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}return asset;
        }
        private static Material Material(string name,Color color)
        {
            string path=$"Assets/_Farmer/Art/Materials/{name}.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            material.SetColor("_BaseColor",color);material.SetFloat("_Smoothness",.12f);EditorUtility.SetDirty(material);return material;
        }
        private static GameObject Part(GameObject root,string name,PrimitiveType kind,Vector3 position,Vector3 scale,Material material)
        {
            var part=GameObject.CreatePrimitive(kind);part.name=name;Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(root.transform,false);part.transform.localPosition=position;part.transform.localScale=scale;
            part.GetComponent<Renderer>().sharedMaterial=material;return part;
        }
    }
}
