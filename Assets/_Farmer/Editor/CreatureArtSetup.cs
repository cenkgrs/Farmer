using System;
using Object=UnityEngine.Object;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Farmer.Editor
{
    public static class CreatureArtSetup
    {
        [Serializable] private sealed class MeshData
        {
            public float[] positions, normals, uv;
            public int[] triangles;
        }
        // Import delivered static creature meshes without touching the authored scene.
        public static void Apply()
        {
            const string art = "Assets/_Farmer/Art";
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Single);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (string name in new[] { "rabbit", "forest_slime", "stone_beetle", "deer", "defne" })
            {
                var data = JsonUtility.FromJson<MeshData>(File.ReadAllText($"ArtSource/creatures_v01/{name}_mesh.json"));
                var vertices = Enumerable.Range(0, data.positions.Length / 3).Select(i => new Vector3(data.positions[3*i],data.positions[3*i+1],data.positions[3*i+2])).ToArray();
                var normals = Enumerable.Range(0, vertices.Length).Select(i => new Vector3(data.normals[3*i],data.normals[3*i+1],data.normals[3*i+2])).ToArray();
                var uv = Enumerable.Range(0, vertices.Length).Select(i => new Vector2(data.uv[2*i],data.uv[2*i+1])).ToArray();
                string meshPath = $"{art}/Meshes/{name}.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (mesh == null) { mesh = new Mesh { name = name }; AssetDatabase.CreateAsset(mesh, meshPath); }
                mesh.Clear(); mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32; mesh.vertices=vertices; mesh.normals=normals; mesh.uv=uv; mesh.triangles=data.triangles;
                mesh.RecalculateBounds(); mesh.RecalculateTangents(); EditorUtility.SetDirty(mesh);
                ArtImportSetup.CreateMaterial(name);
                var material = AssetDatabase.LoadAssetAtPath<Material>($"{art}/Materials/{name}.mat");
                material.SetColor("_BaseColor", new Color(.8f,.8f,.8f,1));
                var root = new GameObject(name);
                try
                {
                    var model = new GameObject("Delivered mesh",typeof(MeshFilter),typeof(MeshRenderer));
                    model.transform.SetParent(root.transform,false);model.transform.localRotation=Quaternion.Euler(0,180,0);
                    model.GetComponent<MeshFilter>().sharedMesh=mesh; model.GetComponent<MeshRenderer>().sharedMaterial=material;
                    Capture(root,$"Logs/{name}-front.png");
                    PrefabUtility.SaveAsPrefabAsset(root,$"Assets/_Farmer/Resources/ExplorationArt/{name}.prefab");
                    Debug.Log($"FARMER_CREATURE_ART {name} vertices={mesh.vertexCount} triangles={mesh.triangles.Length/3} bounds={mesh.bounds}");
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            AssetDatabase.SaveAssets(); Debug.Log("FARMER_CREATURE_ART_OK");
        }
        private static void Capture(GameObject obj,string path)
        {
            var bounds=obj.GetComponentsInChildren<Renderer>().Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});
            var cameraObj=new GameObject("Icon camera");var c=cameraObj.AddComponent<Camera>();c.orthographic=true;c.orthographicSize=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z))*.72f;
            c.transform.position=bounds.center+new Vector3(1.6f,1.1f,3);c.transform.LookAt(bounds.center);c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=Color.clear;c.nearClipPlane=.01f;
            var sun=new GameObject("Icon key");var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.8f;sun.transform.rotation=Quaternion.Euler(45,-30,0);RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=Color.gray;
            var rt=new RenderTexture(512,512,24);rt.Create();c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;
            var image=new Texture2D(512,512,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,image.EncodeToPNG());
            RenderTexture.active=old;c.targetTexture=null;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);Object.DestroyImmediate(cameraObj);Object.DestroyImmediate(sun);
        }
    }
}
