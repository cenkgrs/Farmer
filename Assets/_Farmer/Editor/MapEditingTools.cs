using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Farmer.Editor
{
    public static class MapEditingTools
    {
        [MenuItem("Farmer/Harita/Model paletini seç")]
        public static void Palette(){Selection.activeObject=AssetDatabase.LoadAssetAtPath<Object>("Assets/_Farmer/Resources/ValleyArt");EditorGUIUtility.PingObject(Selection.activeObject);}
        [MenuItem("Farmer/Harita/Köy meydanına odaklan")]
        public static void FocusVillage(){if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.Frame(new Bounds(new Vector3(-55,1,96),new Vector3(32,10,32)),false);}
        [MenuItem("Farmer/Harita/Yüzeyleri düzelt")]
        public static void PolishSurfaces()
        {
            AssetDatabase.Refresh();
            var grassImporter=(TextureImporter)AssetImporter.GetAtPath("Assets/_Farmer/Art/Textures/ground/ground_grass03_albedo.jpg");
            grassImporter.wrapMode=TextureWrapMode.Repeat;grassImporter.maxTextureSize=2048;grassImporter.mipmapEnabled=true;grassImporter.anisoLevel=8;grassImporter.textureCompression=TextureImporterCompression.Uncompressed;grassImporter.SaveAndReimport();
            var pavingImporter=(TextureImporter)AssetImporter.GetAtPath("Assets/_Farmer/Art/Textures/old_paving_stone_albedo.jpg");
            pavingImporter.wrapMode=TextureWrapMode.Repeat;pavingImporter.maxTextureSize=2048;pavingImporter.mipmapEnabled=true;pavingImporter.anisoLevel=8;pavingImporter.textureCompression=TextureImporterCompression.Uncompressed;pavingImporter.SaveAndReimport();
            var groundMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/Ground.mat");
            groundMaterial.SetTextureScale("_BaseMap",new Vector2(64,64));groundMaterial.SetTextureScale("_MainTex",new Vector2(64,64));groundMaterial.SetFloat("_Smoothness",.04f);EditorUtility.SetDirty(groundMaterial);
            var roadMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Art/Materials/valley_path.mat");
            roadMaterial.SetColor("_BaseColor",new Color(.92f,.94f,.92f));roadMaterial.SetColor("_Color",new Color(.92f,.94f,.92f));roadMaterial.SetFloat("_Smoothness",.035f);EditorUtility.SetDirty(roadMaterial);
            var scene=EditorSceneManager.OpenScene(ProjectSetup.ScenePath);
            var root=GameObject.Find("Valley First Slice");
            var points=new[]{new Vector3(-5,0,8),new Vector3(-12,0,32),new Vector3(-30,0,48),new Vector3(-34,0,68),new Vector3(-55,0,86),new Vector3(-55,0,96)};
            var road=CreateOrUpdateMainRoad(root.transform,points,3,roadMaterial);
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if(renderer.name=="Valley path"&&renderer.transform.localScale.x<10){renderer.enabled=false;EditorUtility.SetDirty(renderer);}
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Selection.activeGameObject=road;if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.FrameSelected();
            Debug.Log("FARMER_SURFACE_POLISH_OK");
        }
        internal static GameObject CreateOrUpdateMainRoad(Transform root,Vector3[] points,float width,Material material)
        {
            material.shader=Shader.Find("Farmer/Soft Road");
            material.renderQueue=2990;material.SetOverrideTag("RenderType","Transparent");
            material.SetFloat("_RoadWidth",width);material.SetFloat("_RoadLength",0);material.SetFloat("_EdgeWidth",.55f);
            EditorUtility.SetDirty(material);
            const string meshFolder="Assets/_Farmer/Art/Meshes";
            if(!AssetDatabase.IsValidFolder(meshFolder))AssetDatabase.CreateFolder("Assets/_Farmer/Art","Meshes");
            const string meshPath=meshFolder+"/main_road.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(mesh==null){mesh=new Mesh{name="main_road"};AssetDatabase.CreateAsset(mesh,meshPath);}else mesh.Clear();
            var centers=new System.Collections.Generic.List<Vector3>();const int subdivisions=8;
            for(int segment=0;segment<points.Length-1;segment++)
            {
                var p0=points[Mathf.Max(segment-1,0)];var p1=points[segment];var p2=points[segment+1];var p3=points[Mathf.Min(segment+2,points.Length-1)];
                for(int step=0;step<subdivisions;step++)
                {
                    float t=step/(float)subdivisions,t2=t*t,t3=t2*t;
                    centers.Add(.5f*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t2+(-p0+3*p1-3*p2+p3)*t3));
                }
            }
            centers.Add(points[points.Length-1]);int count=centers.Count;
            var vertices=new Vector3[count*2];var uvs=new Vector2[count*2];var triangles=new int[(count-1)*6];float distance=0;
            for(int i=0;i<count;i++)
            {
                if(i>0)distance+=Vector3.Distance(centers[i-1],centers[i]);
                Vector3 offset;
                if(i==0||i==count-1)
                {
                    var direction=(i==0?centers[1]-centers[0]:centers[count-1]-centers[count-2]).normalized;
                    offset=Vector3.Cross(Vector3.up,direction)*(width*.5f);
                }
                else
                {
                    var previous=(centers[i]-centers[i-1]).normalized;var next=(centers[i+1]-centers[i]).normalized;
                    var previousSide=Vector3.Cross(Vector3.up,previous);var nextSide=Vector3.Cross(Vector3.up,next);
                    var miter=(previousSide+nextSide).normalized;float denominator=Mathf.Max(.35f,Mathf.Abs(Vector3.Dot(miter,nextSide)));
                    offset=miter*Mathf.Min(width,width*.5f/denominator);
                }
                var center=root.InverseTransformPoint(centers[i]+Vector3.up*.012f);var localOffset=root.InverseTransformVector(offset);
                vertices[i*2]=center-localOffset;vertices[i*2+1]=center+localOffset;uvs[i*2]=new Vector2(0,distance/3f);uvs[i*2+1]=new Vector2(1,distance/3f);
                if(i<count-1){int triangle=i*6,a=i*2,b=a+1,c=a+2,d=a+3;triangles[triangle]=a;triangles[triangle+1]=c;triangles[triangle+2]=b;triangles[triangle+3]=b;triangles[triangle+4]=c;triangles[triangle+5]=d;}
            }
            mesh.vertices=vertices;mesh.uv=uvs;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            var road=root.Find("Main road surface");GameObject roadObject;
            if(road==null){roadObject=new GameObject("Main road surface");roadObject.transform.SetParent(root,false);roadObject.AddComponent<MeshFilter>();roadObject.AddComponent<MeshRenderer>();}
            else roadObject=road.gameObject;
            roadObject.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=roadObject.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=true;
            EditorUtility.SetDirty(roadObject);return roadObject;
        }
        // One-time migration: changes only the narrow main-road renderers, never regenerates scenery or the village square.
        public static void ApplyPavingAndBuild()
        {
            AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/_Farmer/Art/Textures/old_paving_stone_albedo.jpg");
            importer.wrapMode=TextureWrapMode.Repeat;importer.maxTextureSize=2048;importer.mipmapEnabled=true;importer.SaveAndReimport();
            var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Art/Materials/valley_path.mat");
            material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Farmer/Art/Textures/old_paving_stone_albedo.jpg"));material.SetColor("_BaseColor",Color.white);material.SetFloat("_Smoothness",.08f);EditorUtility.SetDirty(material);
            var scene=EditorSceneManager.OpenScene(ProjectSetup.ScenePath);
            var root=GameObject.Find("Valley First Slice");
            foreach(var renderer in root.GetComponentsInChildren<Renderer>())
            {
                if(renderer.name!="Valley path"||renderer.transform.localScale.x>10)continue;
                renderer.transform.position=new Vector3(renderer.transform.position.x,.009f,renderer.transform.position.z);
                renderer.sharedMaterial=material;if(renderer.GetComponent<PavingTiling>()==null)renderer.gameObject.AddComponent<PavingTiling>();
                EditorUtility.SetDirty(renderer);
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            ProjectSetup.BuildLinux();
        }
    }
}
