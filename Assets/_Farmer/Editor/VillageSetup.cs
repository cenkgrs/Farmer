using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace Farmer.Editor
{
    public static class VillageSetup
    {
        public static void CreateAssets()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            Directory.CreateDirectory("Assets/_Farmer/Resources/GardenPieces");Directory.CreateDirectory("Assets/_Farmer/Resources/VillageArt");
            var stove=VillageArtwork.Stove();int n=0;
            foreach(var mat in stove.GetComponentsInChildren<Renderer>().Select(r=>r.sharedMaterial).Distinct())
            {string p=$"Assets/_Farmer/Art/Materials/stove_{n++}.mat";var old=AssetDatabase.LoadAssetAtPath<Material>(p);if(old==null)AssetDatabase.CreateAsset(mat,p);else {EditorUtility.CopySerialized(mat,old);foreach(var r in stove.GetComponentsInChildren<Renderer>())if(r.sharedMaterial==mat)r.sharedMaterial=old;}}
            var prefab=PrefabUtility.SaveAsPrefabAsset(stove,"Assets/_Farmer/Prefabs/home_stove.prefab");
            string path="Assets/_Farmer/Resources/GardenPieces/home_stove.asset";var def=AssetDatabase.LoadAssetAtPath<BuildDefinition>(path);
            if(def==null){def=ScriptableObject.CreateInstance<BuildDefinition>();AssetDatabase.CreateAsset(def,path);}
            def.id="home_stove";def.displayName="Ocak";def.price=100;def.woodCost=1;def.prefab=prefab;def.footprintLength=1;EditorUtility.SetDirty(def);
            Capture(stove,"Assets/_Farmer/Resources/ConstructionArt/home_stove.png");Object.DestroyImmediate(stove);
            foreach(var id in new[]{"raw_meat","cooked_meat","hide"})
            {var obj=id=="bow"||id=="dagger"?VillageArtwork.Weapon(id=="bow"):VillageArtwork.Food(id);Capture(obj,"Assets/_Farmer/Resources/VillageArt/"+id+".png");Object.DestroyImmediate(obj);}
            AssetDatabase.Refresh();
            foreach(var png in Directory.GetFiles("Assets/_Farmer/Resources/VillageArt","*.png").Concat(new[]{"Assets/_Farmer/Resources/ConstructionArt/home_stove.png"}))
            {var importer=(TextureImporter)AssetImporter.GetAtPath(png);importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
            AssetDatabase.SaveAssets();Debug.Log("FARMER_VILLAGE_ASSETS_OK");
        }
        private static void Capture(GameObject obj,string path)
        {
            var bounds=obj.GetComponentsInChildren<Renderer>().Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});
            var cameraObj=new GameObject("Icon camera");var c=cameraObj.AddComponent<Camera>();c.orthographic=true;c.orthographicSize=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z))*.72f;
            c.transform.position=bounds.center+new Vector3(2,1.7f,-3);c.transform.LookAt(bounds.center);c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=Color.clear;c.nearClipPlane=.01f;
            var sun=new GameObject("Icon key");var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.8f;sun.transform.rotation=Quaternion.Euler(45,-30,0);RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=Color.gray;
            var rt=new RenderTexture(256,256,24);rt.Create();c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;
            var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,image.EncodeToPNG());
            RenderTexture.active=old;c.targetTexture=null;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);Object.DestroyImmediate(cameraObj);Object.DestroyImmediate(sun);
        }
    }
}
