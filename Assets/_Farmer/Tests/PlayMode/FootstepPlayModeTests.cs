using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Farmer.Tests
{
    public sealed class FootstepPlayModeTests
    {
        [UnityTest] public IEnumerator CadenceSuppressesIdleAirTeleportAndUsesSurface()
        {
            var player=new GameObject("Footstep test");var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                player.transform.position=new Vector3(1000,0,1000);
                floor.transform.position=player.transform.position+Vector3.down*.1f;floor.transform.localScale=new Vector3(5,.2f,5);
                var marker=floor.AddComponent<PlacedBlockView>();marker.Record=new BlockRecord{pieceId="wood_floor"};
                var footsteps=player.AddComponent<PlayerFootsteps>();yield return null;
                footsteps.enabled=false;Physics.SyncTransforms();
                Assert.That(Resources.LoadAll<AudioClip>("Footsteps").Length,Is.EqualTo(15));
                Assert.That(footsteps.SurfaceAt(player.transform.position),Is.EqualTo(PlayerFootsteps.Surface.Wood));
                marker.Record.pieceId="stone_path";
                Assert.That(footsteps.SurfaceAt(player.transform.position),Is.EqualTo(PlayerFootsteps.Surface.Stone));
                floor.SetActive(false);Physics.SyncTransforms();
                Assert.That(footsteps.SurfaceAt(player.transform.position),Is.EqualTo(PlayerFootsteps.Surface.Grass));
                for(int i=0;i<20;i++){footsteps.StepDistance(0,true,false,false);footsteps.StepDistance(.4f,false,false,false);footsteps.StepDistance(.4f,true,false,true);footsteps.StepDistance(20,true,false,false);}
                Assert.That(footsteps.StepsPlayed,Is.Zero);
                for(int i=0;i<100;i++)footsteps.StepDistance(3.8f*.01f,true,false,false);
                Assert.That(footsteps.StepsPlayed,Is.EqualTo(2));
                footsteps.StepDistance(0,true,false,false);
                for(int i=0;i<100;i++)footsteps.StepDistance(3.8f*PlayerMotor.SprintMultiplier*.01f,true,true,false);
                Assert.That(footsteps.StepsPlayed,Is.EqualTo(5));
            }
            finally { Object.DestroyImmediate(player);Object.DestroyImmediate(floor); }
        }
    }
}
