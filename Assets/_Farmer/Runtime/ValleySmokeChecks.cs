#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Farmer
{
    public static class ValleySmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game=Object.FindFirstObjectByType<FarmGame>();
            if(!game.SavePath.Contains("FarmerQA"))throw new System.InvalidOperationException("Expected isolated save.");
            var original=game.Model.Snapshot();var camera=Camera.main;var position=camera.transform.position;float size=camera.orthographicSize;
            var cc=game.Player.GetComponent<CharacterController>();var playerPosition=game.Player.position;
            try
            {
                string[] names={"village_shop","ruin_arch","village_well","cliff_module","granite_boulder","pine_tree","wood_bridge","meadow_bush"};
                foreach(var name in names)
                {
                    var prefab=Resources.Load<GameObject>("ValleyArt/"+name);
                    Check(prefab!=null&&prefab.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount>50&&prefab.GetComponentInChildren<Renderer>().sharedMaterial.GetTexture("_BaseMap")!=null,"Delivered textured model loads: "+name);
                }
                Check(Vector3.Distance(game.Market.position,Vector3.zero)>95,"Market is a separate village destination, away from the starting farm.");
                Check(WorldGround.SupportsCell(-55,85,true)&&WorldGround.SupportsCell(80,60,false),"Expanded flat ground supports farming and building outside the old map.");
                Teleport(cc,new Vector3(0,.1f,0));yield return null;
                Check(!game.NearMarket&&!game.Buy(1),"The starting farm no longer provides remote market shopping.");
                Teleport(cc,game.Market.position+new Vector3(0,.1f,-1.8f));yield return null;
                game.GetComponent<MarketInteraction>().TryOpen();
                int seeds=game.Model.Seeds(game.ActiveCrop.id);Check(game.NearMarket&&game.Buy(1)&&game.Model.Seeds(game.ActiveCrop.id)==seeds+1,"Seeds can be purchased at the relocated village market.");
                game.Model.AdvanceMinutes(360);
                Teleport(cc,new Vector3(-55,.1f,93));yield return null;
                camera.transform.position=new Vector3(-55,1,96)-camera.transform.forward*35;camera.orthographicSize=13;
                yield return Capture(screenshot,"village");
                var scenery=Object.FindObjectsByType<ValleyScenery>(FindObjectsSortMode.None).First(v=>v.name.StartsWith("granite_boulder"));
                var bounds=scenery.GetComponentInChildren<Renderer>().bounds;
                int x=Mathf.FloorToInt(bounds.center.x),z=Mathf.FloorToInt(bounds.center.z);
                game.Model.Building.Place("wood_block",x,0,z,0,out _);game.NotifyTimeAdvanced();yield return null;
                Check(scenery.GetComponentsInChildren<Renderer>().All(r=>!r.enabled),"New scenery yields to saved player construction instead of deleting it.");
                string saved=JsonUtility.ToJson(game.Model.Snapshot());game.SaveGame();game.LoadGame();yield return null;
                Check(saved==JsonUtility.ToJson(game.Model.Snapshot())&&scenery.GetComponentsInChildren<Renderer>().All(r=>!r.enabled),"Faraway construction and scenery conflict handling survive save/load.");
                var motor=game.Player.GetComponent<PlayerMotor>();Teleport(cc,new Vector3(-35,.1f,65));var before=game.Player.position;
                for(int i=0;i<10;i++)motor.Step(Vector2.up,.03f);
                Check(Vector3.Distance(before,game.Player.position)>.5f&&game.Player.position.y>-.2f,"The character walks on the expanded map beyond the old boundary.");
                camera.transform.position=new Vector3(-25,1,59)-camera.transform.forward*80;camera.orthographicSize=36;
                yield return Capture(screenshot,"route");
                Debug.Log("FARMER_VALLEY_CHECKS_FINISHED");
            }
            finally
            {
                File.WriteAllText(game.SavePath,JsonUtility.ToJson(original));game.LoadGame();Teleport(cc,playerPosition);
                camera.transform.position=position;camera.orthographicSize=size;
            }
        }
        private static void Teleport(CharacterController cc,Vector3 p){cc.enabled=false;cc.transform.position=p;cc.enabled=true;Physics.SyncTransforms();}
        private static IEnumerator Capture(string path,string label){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path)+"-"+label+".png"));yield return new WaitForSecondsRealtime(.3f);}
        private static void Check(bool ok,string message){if(ok)Debug.Log("FARMER_VALLEY_CHECK_OK: "+message);else Debug.LogError("FARMER_VALLEY_CHECK_FAILED: "+message);}
    }
}
#endif
