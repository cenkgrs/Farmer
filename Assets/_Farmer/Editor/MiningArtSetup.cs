using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Farmer.Editor
{
    public static class MiningArtSetup
    {
        [Serializable] private sealed class MeshData
        {
            public float[] positions, normals, uv;
            public int[] triangles;
        }
        // Only creates the two delivered art assets; never edits or regenerates the world scene.
        public static void Apply()
        {
            const string art = "Assets/_Farmer/Art";
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (string name in new[] { "pickaxe", "stone_outcrop" })
            {
                var data = JsonUtility.FromJson<MeshData>(File.ReadAllText($"ArtSource/mining_v01/{name}_mesh.json"));
                var vertices = Enumerable.Range(0, data.positions.Length / 3).Select(i => new Vector3(data.positions[3*i],data.positions[3*i+1],data.positions[3*i+2])).ToArray();
                var normals = Enumerable.Range(0, vertices.Length).Select(i => new Vector3(data.normals[3*i],data.normals[3*i+1],data.normals[3*i+2])).ToArray();
                var uv = Enumerable.Range(0, vertices.Length).Select(i => new Vector2(data.uv[2*i],data.uv[2*i+1])).ToArray();
                string meshPath = $"{art}/Meshes/{name}.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (mesh == null) { mesh = new Mesh { name = name }; AssetDatabase.CreateAsset(mesh, meshPath); }
                mesh.Clear(); mesh.vertices=vertices; mesh.normals=normals; mesh.uv=uv; mesh.triangles=data.triangles;
                mesh.RecalculateBounds(); mesh.RecalculateTangents(); EditorUtility.SetDirty(mesh);
                ArtImportSetup.CreateMaterial(name);
                var material = AssetDatabase.LoadAssetAtPath<Material>($"{art}/Materials/{name}.mat");
                material.SetColor("_BaseColor", new Color(.8f,.8f,.8f,1));
                var root = new GameObject(name);
                try
                {
                    var model = new GameObject("Delivered mesh",typeof(MeshFilter),typeof(MeshRenderer));
                    model.transform.SetParent(root.transform,false);
                    model.GetComponent<MeshFilter>().sharedMesh=mesh; model.GetComponent<MeshRenderer>().sharedMaterial=material;
                    if (name == "pickaxe")
                    {
                        var gripPoints=vertices.Where(p=>p.y>.23f&&p.y<.29f).ToArray();
                        if(gripPoints.Length==0)throw new InvalidOperationException("Missing pickaxe shaft geometry.");
                        var grip=gripPoints.Aggregate(Vector3.zero,(sum,p)=>sum+p)/gripPoints.Length;
                        // Head lies in the forward chopping plane, shaft passes through the calibrated palm socket.
                        var rotation=Quaternion.Euler(0,-90,0);
                        model.transform.localRotation=rotation; model.transform.localPosition=-(rotation*grip);
                        Debug.Log($"FARMER_PICKAXE_GRIP {grip}");
                    }
                    PrefabUtility.SaveAsPrefabAsset(root,$"Assets/_Farmer/Resources/ExplorationArt/{name}.prefab");
                    Debug.Log($"FARMER_MINING_ART {name} vertices={mesh.vertexCount} triangles={mesh.triangles.Length/3} bounds={mesh.bounds}");
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            AssetDatabase.SaveAssets(); Debug.Log("FARMER_MINING_ART_OK");
        }
    }
}
