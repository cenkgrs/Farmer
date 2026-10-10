using System.IO;
using UnityEditor;
using UnityEngine;
namespace Farmer.Editor
{
    // Creates only the garden catalog and prefabs; never rewrites the authored world.
    public static class GardenSetup
    {
        private const string Art="Assets/_Farmer/Resources/GardenArt";
        private const string Data="Assets/_Farmer/Resources/GardenPieces";
        [MenuItem("Farmer/Create garden pieces")]
        public static void Apply()
        {
            Directory.CreateDirectory(Art);Directory.CreateDirectory(Data);AssetDatabase.Refresh();
            var wood=Material("garden_wood",new Color(.48f,.29f,.12f));
            var trim=Material("garden_trim",new Color(.66f,.44f,.22f));
            var iron=Material("garden_iron",new Color(.22f,.23f,.20f));
            var stone=new[]{Material("path_sandstone",new Color(.57f,.55f,.46f)),Material("path_slate",new Color(.45f,.48f,.45f)),Material("path_warm",new Color(.65f,.61f,.51f))};
            foreach(string id in new[]{"garden_fence","garden_gate","stone_path"})
            {
                var root=new GameObject(id);
                if(id=="stone_path")
                {
                    var mesh=StoneMesh();
                    for(int z=0;z<3;z++)for(int x=0;x<3;x++)
                    {
                        var slab=new GameObject("Stone",typeof(MeshFilter),typeof(MeshRenderer));slab.transform.SetParent(root.transform,false);
                        slab.transform.localPosition=new Vector3((x-1)*.32f,-.49f,(z-1)*.32f);slab.transform.localScale=new Vector3(.30f,.05f,.30f);
                        slab.transform.localRotation=Quaternion.Euler(0,(x+z)%2*90,0);slab.GetComponent<MeshFilter>().sharedMesh=mesh;slab.GetComponent<Renderer>().sharedMaterial=stone[(x+z*2)%3];
                    }
                    Box(root, new Vector3(0,-.465f,0),new Vector3(.96f,.07f,.96f));
                }
                else
                {
                    for(int side=-1;side<=1;side+=2)
                    {
                        Part(root,"Post",new Vector3(side*.455f,0,.5f),new Vector3(.09f,1,.12f),wood);
                        Part(root,"Post cap",new Vector3(side*.455f,.50f,.5f),new Vector3(.13f,.06f,.16f),trim);
                    }
                    if(id=="garden_fence")
                    {
                        for(int rail=0;rail<2;rail++)Part(root,"Rail",new Vector3(0,-.23f+rail*.48f,.5f),new Vector3(.84f,.12f,.07f),trim);
                        for(int slat=0;slat<4;slat++)Part(root,"Picket",new Vector3((slat-1.5f)*.20f,-.025f,.515f),new Vector3(.105f,.83f,.065f),wood);
                        Box(root,new Vector3(0,0,.5f),new Vector3(1,1,.16f));
                    }
                    else
                    {
                        Box(root,new Vector3(-.455f,0,.5f),new Vector3(.09f,1,.12f));Box(root,new Vector3(.455f,0,.5f),new Vector3(.09f,1,.12f));
                        var hinge=new GameObject("Hinge");hinge.transform.SetParent(root.transform,false);hinge.transform.localPosition=new Vector3(-.40f,-.43f,.5f);
                        for(int rail=0;rail<2;rail++)Part(hinge,"Gate rail",new Vector3(.4f,.13f+rail*.55f,0),new Vector3(.8f,.11f,.08f),trim);
                        for(int slat=0;slat<4;slat++)Part(hinge,"Gate slat",new Vector3(.10f+slat*.20f,.42f,0),new Vector3(.10f,.82f,.055f),wood);
                        var brace=Part(hinge,"Brace",new Vector3(.4f,.4f,-.045f),new Vector3(.88f,.07f,.035f),trim);brace.transform.localRotation=Quaternion.Euler(0,0,37);
                        Part(hinge,"Latch",new Vector3(.72f,.55f,-.065f),new Vector3(.14f,.065f,.035f),iron);
                        Box(hinge,new Vector3(.4f,.42f,0),new Vector3(.8f,.84f,.10f));
                    }
                }
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,Art+"/"+id+".prefab");Object.DestroyImmediate(root);
                string path=Data+"/"+id+".asset";var definition=AssetDatabase.LoadAssetAtPath<BuildDefinition>(path);
                if(definition==null){definition=ScriptableObject.CreateInstance<BuildDefinition>();AssetDatabase.CreateAsset(definition,path);}
                definition.id=id;definition.displayName=id=="garden_fence"?"Ahşap çit":id=="garden_gate"?"Bahçe kapısı":"Taş yol";
                definition.isOutdoor=true;definition.woodCost=id=="stone_path"?0:id=="garden_gate"?3:2;definition.stoneCost=id=="stone_path"?2:0;
                definition.placement=id=="stone_path"?BuildPlacement.Floor:BuildPlacement.Edge;definition.isDoor=id=="garden_gate";definition.prefab=prefab;EditorUtility.SetDirty(definition);
            }
            AssetDatabase.SaveAssets();Debug.Log("FARMER_GARDEN_ASSETS_OK");
        }
        private static Mesh StoneMesh()
        {
            string path=Art+"/path_stone.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh!=null)return mesh;
            var rim=new[]{new Vector2(-.5f,-.33f),new Vector2(-.30f,-.5f),new Vector2(.32f,-.5f),new Vector2(.5f,-.28f),new Vector2(.5f,.30f),new Vector2(.31f,.5f),new Vector2(-.32f,.5f),new Vector2(-.5f,.29f)};
            var vertices=new Vector3[17];vertices[16]=Vector3.up;var triangles=new int[8*9];
            for(int i=0;i<8;i++) {vertices[i]=new Vector3(rim[i].x,0,rim[i].y);vertices[i+8]=new Vector3(rim[i].x*.87f,1,rim[i].y*.87f);int n=(i+1)%8,j=i*9;triangles[j]=16;triangles[j+1]=n+8;triangles[j+2]=i+8;triangles[j+3]=i;triangles[j+4]=i+8;triangles[j+5]=n+8;triangles[j+6]=i;triangles[j+7]=n+8;triangles[j+8]=n;}
            mesh=new Mesh{name="Beveled path stone"};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        private static Material Material(string id,Color color)
        {
            string path=Art+"/"+id+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}material.SetColor("_BaseColor",color);material.SetFloat("_Smoothness",.08f);EditorUtility.SetDirty(material);return material;
        }
        private static GameObject Part(GameObject root,string name,Vector3 position,Vector3 scale,Material material)
        {
            var part=GameObject.CreatePrimitive(PrimitiveType.Cube);part.name=name;Object.DestroyImmediate(part.GetComponent<Collider>());part.transform.SetParent(root.transform,false);part.transform.localPosition=position;part.transform.localScale=scale;part.GetComponent<Renderer>().sharedMaterial=material;return part;
        }
        private static void Box(GameObject root,Vector3 center,Vector3 size){var box=root.AddComponent<BoxCollider>();box.center=center;box.size=size;}
    }
}
