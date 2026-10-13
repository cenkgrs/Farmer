using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Farmer.Tests
{
    public sealed class FarmMusicPlayModeTests
    {
        [UnityTest] public IEnumerator ImportedLoopsFadeAcrossClockChanges()
        {
            var root = new GameObject("Music test");
            try
            {
                var music = root.AddComponent<FarmMusic>();
                yield return null;
                Assert.That(music.DaySource.clip, Is.Not.Null);
                Assert.That(music.NightSource.clip, Is.Not.Null);
                Assert.That(music.DaySource.clip.length, Is.EqualTo(182.8f).Within(1));
                Assert.That(music.NightSource.clip.length, Is.EqualTo(198f).Within(1));
                foreach (var source in new[]{music.DaySource,music.NightSource})
                {
                    Assert.That(source.loop, Is.True);
                    Assert.That(source.spatialBlend, Is.Zero);
                    Assert.That(source.clip.loadType, Is.EqualTo(AudioClipLoadType.Streaming));
                }
                music.Step(360, 6);
                Assert.That(music.DaySource.volume, Is.EqualTo(FarmMusic.Volume).Within(.001));
                Assert.That(music.NightSource.volume, Is.Zero);
                music.Step(1139, 6);
                Assert.That(music.DaySource.volume, Is.EqualTo(FarmMusic.Volume).Within(.001));
                music.Step(1140, 3);
                Assert.That(music.DaySource.volume, Is.EqualTo(FarmMusic.Volume/2).Within(.001));
                Assert.That(music.NightSource.volume, Is.EqualTo(FarmMusic.Volume/2).Within(.001));
                music.Step(0, 3);
                Assert.That(music.DaySource.volume, Is.Zero);
                Assert.That(music.NightSource.volume, Is.EqualTo(FarmMusic.Volume).Within(.001));
                // Sleep/load can reverse the fade before the previous transition ends.
                music.Step(360, 2);music.Step(359, 1);
                Assert.That(music.DaySource.volume, Is.EqualTo(FarmMusic.Volume/6).Within(.001));
                music.Step(360, 6);
                Assert.That(music.NightSource.volume, Is.Zero);
                music.enabled = false;
                Assert.That(music.DaySource.isPlaying || music.NightSource.isPlaying, Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
