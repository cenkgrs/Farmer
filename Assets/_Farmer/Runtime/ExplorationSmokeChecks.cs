#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Farmer
{
    public static class ExplorationSmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game=Object.FindFirstObjectByType<FarmGame>();var cc=game.Player.GetComponent<CharacterController>();var camera=Camera.main;
            if(!game.SavePath.Contains("FarmerQA"))throw new System.InvalidOperationException("Expected isolated save.");
            bool campActive=game.Camp.gameObject.activeSelf;var original=game.Model.Snapshot();var cameraPosition=camera.transform.position;var rotation=camera.transform.rotation;var follow=camera.GetComponent<ExplorationCamera>();
            var devices=InputSystem.devices.Where(d=>d.enabled&&(d is Mouse||d is Keyboard)).ToArray();var keyboard=InputSystem.AddDevice<Keyboard>();var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                foreach(var d in devices)InputSystem.DisableDevice(d);game.Camp.gameObject.SetActive(false);
                Load(game,new FarmModel(new[]{game.ActiveCrop.Rules},buildCatalog:game.BuildPieces.Select(d=>d.Rules),worldSeed:732).Snapshot());yield return null;follow.enabled=true;
                Check(game.Model.Money==50&&game.Model.ItemCount(FarmItem.Axe,game.ActiveCrop.id)==1,"Fresh world starts with 50 coins and a permanent axe.");
                Check(Object.FindObjectsByType<ResourceView>(FindObjectsSortMode.None).Length==41,"New world presents 24 trees, 12 wild plants and five chests.");
                var tree=game.Model.Exploration.Nodes.First(n=>n.kind==ResourceKind.Tree);Teleport(cc,tree);yield return new WaitForSecondsRealtime(2f);
                Check(Vector3.Distance(cameraPosition,camera.transform.position)>5&&Quaternion.Angle(rotation,camera.transform.rotation)<.01f,"Camera follows exploration without changing its isometric angle.");
                Check(WorldGround.SupportsCell(tree.x,tree.z,true),"Forest cells have buildable, cultivable ground.");
                var treeView=Object.FindObjectsByType<ResourceView>(FindObjectsSortMode.None).Single(v=>v.Id==tree.id);
                var canopy=treeView.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Canopy");
                var trunk=treeView.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Trunk");
                Check(!canopy.gameObject.activeSelf&&trunk.gameObject.activeSelf,"Nearby tree hides only its separated canopy.");
                treeView.UpdateCanopy(game.Player.position+Vector3.one*20);
                Check(canopy.gameObject.activeSelf&&trunk.GetComponent<MeshFilter>().sharedMesh.vertexCount>1000,"Distant tree restores delivered canopy and trunk.");
                // Freeze resource visibility for a complete-tree art capture.
                var explorer=game.GetComponent<ExplorationController>();explorer.enabled=false;
                try{yield return Closeup(screenshot,"tree-full",treeView.transform.position+Vector3.up*1.5f);}finally{explorer.enabled=true;}

                int wood=game.Model.Building.Wood;yield return Click(mouse,Point(tree));Check(game.Model.Exploration.Node(tree.id).hits==0,"Seeds cannot cut trees.");
                yield return Press(keyboard,Key.Digit6);Check(game.Model.EquippedItem==FarmItem.Axe&&GameObject.Find("Held Axe")!=null,"Shortcut six equips and displays the axe.");
                var axe=GameObject.Find("Held Axe");
                var hand=game.Player.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
                Check(axe.transform.IsChildOf(hand)&&Vector3.Distance(axe.transform.position,hand.position)<.12f,"Delivered axe follows the hand socket.");
                Check(axe.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount>1000&&axe.GetComponentInChildren<Renderer>().sharedMaterial.GetTexture("_BaseMap")!=null,"Axe uses delivered textured geometry.");
                yield return new WaitForSecondsRealtime(.2f);
                var driver=game.Player.GetComponentInChildren<FarmerAnimator>();
                Check(driver.ToolSocket.forward.y<-.85f&&axe.GetComponentInChildren<Renderer>().bounds.center.y<hand.position.y+.2f,"Resting axe cutting edge faces down, with its head away from the chest.");
                yield return Closeup(screenshot,"axe-grip",game.Player.position+Vector3.up*.9f);
                var skeleton=game.Player.GetComponentInChildren<Animator>();
                float wristAngle=Vector3.Angle(hand.position-skeleton.GetBoneTransform(HumanBodyBones.RightLowerArm).position,skeleton.GetBoneTransform(HumanBodyBones.RightMiddleProximal).position-hand.position);
                Check(wristAngle<55,$"Axe carrying wrist follows the forearm instead of folding back ({wristAngle:F1} degrees).");
                yield return Closeup(screenshot,"axe-hand-detail",driver.ToolSocket.position,.48f);
                var hudPoint=new Vector2(60,Screen.height-50);yield return Click(mouse,hudPoint);Check(game.Model.Exploration.Node(tree.id).hits==0,"HUD clicks cannot chop a resource.");
                var barrier=GameObject.CreatePrimitive(PrimitiveType.Cube);barrier.transform.position=new Vector3(tree.x+.5f,1,tree.z-.5f);barrier.transform.localScale=new Vector3(1,2,.2f);Physics.SyncTransforms();
                Check(!game.Gather(tree.id)&&game.Model.Exploration.Node(tree.id).hits==0,"A wall between player and tree blocks gathering.");barrier.SetActive(false);Object.Destroy(barrier);
                yield return AxeSwing(mouse,Point(tree),screenshot,game);
                Check(game.Model.Exploration.Node(tree.id).hits==1,"A reachable click produces one axe hit.");
                game.SaveGame();yield return Press(keyboard,Key.F9);Check(game.Model.Exploration.Node(tree.id).hits==1,"Partial tree chopping survives save and reload.");
                yield return Click(mouse,Point(tree));yield return Click(mouse,Point(tree));Check(game.Model.Building.Wood==wood+8&&game.Model.Exploration.Node(tree.id).collected,"Three hits fell the tree and give eight wood.");
                Check(!Object.FindObjectsByType<ResourceView>(FindObjectsSortMode.None).Single(v=>v.Id==tree.id).GetComponent<Collider>().enabled,"Felled tree no longer blocks movement or construction.");
                var stump=Object.FindObjectsByType<ResourceView>(FindObjectsSortMode.None).Single(v=>v.Id==tree.id).transform.Find("Felled Oak Stump");
                Check(stump.gameObject.activeInHierarchy&&stump.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount>1000,"Felling reveals the supplied stump model.");
                Check(stump.GetComponentInChildren<Renderer>().bounds.size.y>.29f&&stump.GetComponentInChildren<Renderer>().bounds.size.y<.31f&&stump.GetComponentsInChildren<Collider>().Length==0,"Stump is grounded at 0.3m and has no blocking collider.");
                yield return Closeup(screenshot,"axe-stump",new Vector3(tree.x+.5f,.65f,tree.z));
                yield return Click(mouse,Point(tree));Check(game.Model.Building.Wood==wood+8,"Repeated clicking cannot duplicate the tree reward.");
                var plant=game.Model.Exploration.Nodes.First(n=>n.kind==ResourceKind.WildPlant);Teleport(cc,plant);yield return new WaitForSecondsRealtime(2f);
                var plantView=Object.FindObjectsByType<ResourceView>(FindObjectsSortMode.None).Single(v=>v.Id==plant.id);
                Check(plantView.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount>1000&&plantView.GetComponentInChildren<Renderer>().sharedMaterial.GetTexture("_BaseMap")!=null,"Wild plant uses delivered textured geometry.");
                yield return Closeup(screenshot,"wild-plant",plantView.transform.position+Vector3.up*.45f);
                yield return Click(mouse,Point(plant));Check(!game.Model.Exploration.Node(plant.id).collected,"Axe cannot harvest a wild seed plant.");
                yield return Press(keyboard,Key.Digit3);yield return Click(mouse,Point(plant));Check(game.Model.Seeds(game.ActiveCrop.id)==2&&game.Model.Exploration.Node(plant.id).collected,"Sickle collects two usable crop seeds from a wild plant.");
                var chest=game.Model.Exploration.Nodes.First(n=>n.kind==ResourceKind.Chest);Teleport(cc,chest);yield return new WaitForSecondsRealtime(2f);
                var chestView=Object.FindObjectsByType<ResourceView>(FindObjectsSortMode.None).Single(v=>v.Id==chest.id);
                var hinge=chestView.GetComponentsInChildren<Transform>().Single(t=>t.name=="Lid hinge");
                Check(chestView.GetComponentsInChildren<MeshFilter>().Length==2&&Quaternion.Angle(hinge.localRotation,Quaternion.identity)<.1f,"Delivered chest has separate closed body and lid.");
                yield return Closeup(screenshot,"chest-closed",chestView.transform.position+Vector3.up*.4f);
                int money=game.Model.Money;yield return Click(mouse,Point(chest));Check(game.Model.Money==money+chest.coins&&game.Model.Exploration.Node(chest.id).collected,"A chest opens with left click and grants its saved coin reward.");
                Check(Quaternion.Angle(hinge.localRotation,Quaternion.Euler(-105,0,0))<.1f,"Collected chest opens its delivered lid around the rear hinge.");
                Check(hinge.GetComponentInChildren<Renderer>().bounds.max.y-chestView.transform.position.y>.8f,"Opened lid rises above the body instead of rotating into the ground.");
                yield return Closeup(screenshot,"chest-open",chestView.transform.position+Vector3.up*.6f);
                yield return Capture(screenshot,"forest-loot");string saved=JsonUtility.ToJson(game.Model.Snapshot());game.SaveGame();yield return Press(keyboard,Key.F9);
                Check(saved==JsonUtility.ToJson(game.Model.Snapshot()),"Resource positions, loot and all collected states survive loading unchanged.");
                Check(Object.FindObjectsByType<ResourceView>(FindObjectsSortMode.None).Single(v=>v.Id==tree.id).transform.Find("Felled Oak Stump").gameObject.activeInHierarchy,"Collected stump remains visible after save reload.");
                yield return Click(mouse,Point(chest));Check(game.Model.Money==money+chest.coins,"Reopening the same chest after reload gives no extra money.");
                var distant=game.Model.Exploration.Nodes.First(n=>!n.collected&&n.kind==ResourceKind.Chest);Check(!game.Gather(distant.id),"Distant resources cannot be collected remotely.");
                yield return Press(keyboard,Key.Digit4);Check(game.BuildMode,"Free construction remains available outside the farm.");yield return Press(keyboard,Key.Escape);
                yield return Capture(screenshot,"exploration-inventory");
                Debug.Log("FARMER_EXPLORATION_CHECKS_FINISHED");
            }
            finally
            {
                game.Camp.gameObject.SetActive(campActive);follow.enabled=false;camera.transform.SetPositionAndRotation(cameraPosition,rotation);InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(mouse);foreach(var d in devices)InputSystem.EnableDevice(d);Load(game,original);
            }
        }
        private static IEnumerator AxeSwing(Mouse mouse,Vector2 point,string screenshot,FarmGame game)
        {
            float originalScale=Time.timeScale;
            try
            {
                Time.timeScale=.1f;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=point,buttons=1});yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=point});
                yield return new WaitForSeconds(.17f);
                var driver=game.Player.GetComponentInChildren<FarmerAnimator>();
                Check(driver.HarvestActive,"Axe transitions from its relaxed carry into a chopping swing.");
                yield return Closeup(screenshot,"axe-swing",game.Player.position+Vector3.up);
            }
            finally{Time.timeScale=originalScale;}
            yield return new WaitForSecondsRealtime(.7f);
            Check(game.Player.GetComponentInChildren<FarmerAnimator>().ToolSocket.forward.y<-.85f,"Axe returns to relaxed carry after chopping.");
        }
        private static IEnumerator Closeup(string screenshot,string label,Vector3 target,float framing=1.7f)
        {
            var camera=Camera.main;var follow=camera.GetComponent<ExplorationCamera>();
            var position=camera.transform.position;var rotation=camera.transform.rotation;float size=camera.orthographicSize;
            follow.enabled=false;
            try
            {
                camera.transform.position=target+new Vector3(5,2.5f,3);camera.transform.LookAt(target);camera.orthographicSize=framing;
                yield return Capture(screenshot,label);
            }
            finally{camera.transform.SetPositionAndRotation(position,rotation);camera.orthographicSize=size;follow.enabled=true;}
        }
        private static void Load(FarmGame game,FarmSnapshot snapshot){File.WriteAllText(game.SavePath,JsonUtility.ToJson(snapshot));game.LoadGame();}
        private static void Teleport(CharacterController cc,ResourceRecord n){cc.enabled=false;cc.transform.position=new Vector3(n.x+.5f,.1f,n.z-1.5f);cc.enabled=true;Physics.SyncTransforms();}
        private static Vector2 Point(ResourceRecord n)=>Camera.main.WorldToScreenPoint(new Vector3(n.x+.5f,n.kind==ResourceKind.Tree?.8f:.35f,n.z+.5f));
        private static IEnumerator Press(Keyboard k,Key key){InputSystem.QueueStateEvent(k,new KeyboardState(key));yield return null;yield return null;InputSystem.QueueStateEvent(k,new KeyboardState());yield return null;yield return null;}
        private static IEnumerator Click(Mouse m,Vector2 p){InputSystem.QueueStateEvent(m,new MouseState{position=p,buttons=1});yield return null;yield return null;InputSystem.QueueStateEvent(m,new MouseState{position=p});yield return new WaitForSecondsRealtime(.65f);}
        private static IEnumerator Capture(string path,string label){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path)+"-"+label+".png"));yield return new WaitForSecondsRealtime(.3f);}
        private static void Check(bool ok,string message){if(ok)Debug.Log("FARMER_EXPLORATION_CHECK_OK: "+message);else Debug.LogError("FARMER_EXPLORATION_CHECK_FAILED: "+message);}
    }
}
#endif
