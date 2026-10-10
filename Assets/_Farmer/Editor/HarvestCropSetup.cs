using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Farmer.Editor
{
    // Extend the catalog without regenerating existing crops, materials or the authored scene.
    public static class HarvestCropSetup
    {
        private const string Art="Assets/_Farmer/Resources/HarvestArt";
        [MenuItem("Farmer/Create remaining 0.3 crops")]
        public static void Apply()
        {
            Directory.CreateDirectory(Art);AssetDatabase.Refresh();
            var leaf=Material("lettuce_leaf",new Color(.35f,.58f,.13f));var lightLeaf=Material("lettuce_heart",new Color(.59f,.74f,.25f));
            var stalk=Material("wheat_stalk",new Color(.51f,.49f,.16f));var grain=Material("wheat_grain",new Color(.84f,.65f,.23f));
            var vine=Material("pumpkin_vine",new Color(.19f,.38f,.10f));var pumpkin=Material("pumpkin_skin",new Color(.88f,.32f,.055f));
            var wicker=Material("basket_wicker",new Color(.48f,.28f,.10f));
            for(int species=0;species<3;species++)
            {
                string id=new[]{"lettuce","wheat","pumpkin"}[species];var stages=new GameObject[4];
                for(int stage=0;stage<4;stage++)
                {
                    var root=new GameObject(id+" stage "+stage);float growth=(stage+1)/4f;
                    if(id=="lettuce")
                    {
                        for(int i=0;i<7;i++)
                        {
                            float angle=i*360f/7;var direction=Quaternion.Euler(0,angle,0)*Vector3.forward;
                            var part=Part(root,"Outer leaf",PrimitiveType.Sphere,direction*(.13f*growth)+Vector3.up*(.09f*growth),new Vector3(.23f,.07f,.36f)*growth,leaf);part.transform.localRotation=Quaternion.Euler(-25,angle,0);
                        }
                        for(int i=0;i<5;i++)
                        {
                            float angle=i*72;var direction=Quaternion.Euler(0,angle,0)*Vector3.forward;
                            var part=Part(root,"Heart leaf",PrimitiveType.Sphere,direction*(.05f*growth)+Vector3.up*(.16f*growth),new Vector3(.16f,.17f,.24f)*growth,lightLeaf);part.transform.localRotation=Quaternion.Euler(-30,angle,0);
                        }
                    }
                    else if(id=="wheat")
                    {
                        for(int i=0;i<7;i++)
                        {
                            float angle=i*2.39996f;var basePoint=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(i==0?0:.14f);float height=(.56f+(i%3)*.07f)*growth;
                            Part(root,"Stalk",PrimitiveType.Cylinder,basePoint+Vector3.up*(height*.5f),new Vector3(.016f,height*.5f,.016f),stage==3?grain:stalk);
                            for(int side=-1;side<=1;side+=2)
                            {var l=Part(root,"Blade",PrimitiveType.Sphere,basePoint+new Vector3(side*.05f*growth,height*.45f,0),new Vector3(.035f,.25f,.015f)*growth,stalk);l.transform.localRotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,-side*35);}
                            if(stage>=2)for(int kernel=0;kernel<6;kernel++)
                            {
                                float side=kernel%2==0?-1:1;var k=Part(root,"Grain",PrimitiveType.Sphere,basePoint+new Vector3(side*.023f,height-.05f+kernel*.018f,0),new Vector3(.042f,.085f,.035f),stage==3?grain:stalk);k.transform.localRotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,side*30);
                            }
                        }
                    }
                    else
                    {
                        for(int i=0;i<5;i++)
                        {
                            float angle=i*72;var direction=Quaternion.Euler(0,angle,0)*Vector3.forward;
                            var branch=Part(root,"Vine",PrimitiveType.Cylinder,direction*.17f*growth+Vector3.up*.025f,new Vector3(.025f,.19f*growth,.025f),vine);branch.transform.localRotation=Quaternion.Euler(90,angle,0);
                            var l=Part(root,"Vine leaf",PrimitiveType.Sphere,direction*.28f*growth+Vector3.up*.065f,new Vector3(.19f,.045f,.22f)*growth,leaf);l.transform.localRotation=Quaternion.Euler(0,angle,0);
                        }
                        if(stage>=2)
                        {
                            float size=stage==3?1:.4f;
                            for(int i=0;i<8;i++){float angle=i*Mathf.PI/4;Part(root,"Pumpkin lobe",PrimitiveType.Sphere,new Vector3(Mathf.Cos(angle)*.115f,.17f,Mathf.Sin(angle)*.115f)*size,new Vector3(.24f,.30f,.24f)*size,stage==3?pumpkin:vine);}
                            Part(root,"Stem",PrimitiveType.Cylinder,new Vector3(0,.34f,0)*size,new Vector3(.05f,.055f,.05f)*size,vine);
                        }
                    }
                    stages[stage]=PrefabUtility.SaveAsPrefabAsset(root,$"{Art}/{id}_stage_{stage}.prefab");Object.DestroyImmediate(root);
                }
                var crop=Asset<CropDefinition>($"Assets/_Farmer/Resources/Crops/{id}.asset");crop.id=id;crop.catalogOrder=200+species;crop.displayName=new[]{"Marul","Buğday","Balkabağı"}[species];
                crop.seedPrice=new[]{6,12,40}[species];crop.salePrice=new[]{10,7,85}[species];crop.wateredDays=new[]{2,3,8}[species];crop.harvestYield=species==1?3:1;crop.regrowDays=0;crop.visualScale=1;crop.growthStages=stages;EditorUtility.SetDirty(crop);
            }
            var recipe=Asset<RecipeDefinition>("Assets/_Farmer/Resources/Recipes/harvest_basket.asset");recipe.id="harvest_basket";recipe.displayName="Hasat sepeti";recipe.catalogOrder=200;recipe.outputCount=1;recipe.salePrice=145;
            recipe.ingredients=new[]{new ItemStack{id="crop:lettuce",count=1},new ItemStack{id="crop:wheat",count=3},new ItemStack{id="crop:pumpkin",count=1},new ItemStack{id="wood",count=2}};EditorUtility.SetDirty(recipe);
            var basket=new GameObject("harvest_basket");
            Part(basket,"Basket",PrimitiveType.Cylinder,new Vector3(0,.12f,0),new Vector3(.56f,.12f,.43f),wicker);
            for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Part(basket,"Weave",PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*.27f,.13f,Mathf.Sin(a)*.20f),new Vector3(.028f,.24f,.028f),grain);}
            Part(basket,"Pumpkin",PrimitiveType.Sphere,new Vector3(.10f,.28f,0),new Vector3(.29f,.25f,.29f),pumpkin);
            for(int i=0;i<4;i++)Part(basket,"Greens",PrimitiveType.Sphere,new Vector3(-.13f,.25f,i*.05f-.08f),new Vector3(.20f,.10f,.16f),lightLeaf);
            for(int i=0;i<3;i++){var w=Part(basket,"Wheat",PrimitiveType.Sphere,new Vector3(-.09f+i*.05f,.35f,-.08f),new Vector3(.035f,.25f,.04f),grain);w.transform.localRotation=Quaternion.Euler(0,0,-20);}
            PrefabUtility.SaveAsPrefabAsset(basket,Art+"/harvest_basket.prefab");Object.DestroyImmediate(basket);AssetDatabase.SaveAssets();Debug.Log("FARMER_HARVEST_CROPS_ASSETS_OK");
        }
        private static T Asset<T>(string path) where T:ScriptableObject {var a=AssetDatabase.LoadAssetAtPath<T>(path);if(a==null){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);}return a;}
        private static Material Material(string id,Color color)
        {var p=Art+"/"+id+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.08f);EditorUtility.SetDirty(m);return m;}
        private static Mesh roundMesh;
        private static Mesh RoundedMesh()
        {
            if(roundMesh!=null)return roundMesh;
            string path=Art+"/rounded_crop.asset";roundMesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(roundMesh!=null)return roundMesh;
            const int sides=8,rings=3;var vertices=new List<Vector3>{Vector3.up*.5f};var triangles=new List<int>();
            for(int ring=1;ring<=rings;ring++)for(int i=0;i<sides;i++)
            {float latitude=Mathf.PI*ring/(rings+1),angle=Mathf.PI*2*i/sides;vertices.Add(new Vector3(Mathf.Sin(latitude)*Mathf.Cos(angle),Mathf.Cos(latitude),Mathf.Sin(latitude)*Mathf.Sin(angle))*.5f);}
            vertices.Add(Vector3.down*.5f);int bottom=vertices.Count-1;
            for(int i=0;i<sides;i++)
            {
                int next=(i+1)%sides;triangles.AddRange(new[]{0,1+i,1+next});
                for(int ring=0;ring<rings-1;ring++){int a=1+ring*sides+i,n=1+ring*sides+next;triangles.AddRange(new[]{a,a+sides,n,n,a+sides,n+sides});}
                triangles.AddRange(new[]{bottom,1+(rings-1)*sides+i,1+(rings-1)*sides+next});
            }
            for(int i=0;i<triangles.Count;i+=3)
            {var a=vertices[triangles[i]];var b=vertices[triangles[i+1]];var c=vertices[triangles[i+2]];if(Vector3.Dot(Vector3.Cross(b-a,c-a),a+b+c)<0){int swap=triangles[i+1];triangles[i+1]=triangles[i+2];triangles[i+2]=swap;}}
            roundMesh=new Mesh{name="Rounded crop 48 triangles"};roundMesh.SetVertices(vertices);roundMesh.SetTriangles(triangles,0);roundMesh.SetNormals(vertices.ConvertAll(v=>v.normalized));roundMesh.RecalculateBounds();AssetDatabase.CreateAsset(roundMesh,path);return roundMesh;
        }
        private static GameObject Part(GameObject root,string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material material)
        {var o=GameObject.CreatePrimitive(type);o.name=name;Object.DestroyImmediate(o.GetComponent<Collider>());o.transform.SetParent(root.transform,false);o.transform.localPosition=pos;o.transform.localScale=scale;if(type==PrimitiveType.Sphere)o.GetComponent<MeshFilter>().sharedMesh=RoundedMesh();o.GetComponent<Renderer>().sharedMaterial=material;return o;}
    }
}
