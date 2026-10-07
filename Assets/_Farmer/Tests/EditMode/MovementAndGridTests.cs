using System;
using NUnit.Framework;
using UnityEngine;

namespace Farmer.Tests
{
    public class MovementAndGridTests
    {
        private readonly FarmGridLayout grid = new FarmGridLayout(new Vector2(-3, -3), 6, 6, 1f);

        [TestCase(-3f, -3f, true, 0, 0)]
        [TestCase(-3.001f, 0f, false, -1, 3)]
        [TestCase(0f, -3.001f, false, 3, -1)]
        [TestCase(2.999f, 2.999f, true, 5, 5)]
        [TestCase(3f, 0f, false, 6, 3)]
        [TestCase(0f, 3f, false, 3, 6)]
        [TestCase(-2f, -2f, true, 1, 1)]
        public void WorldCoordinatesRespectHalfOpenBounds(float x, float z, bool expected, int cx, int cz)
        {
            Assert.That(grid.TryGetCell(new Vector3(x, 100, z), out var cell), Is.EqualTo(expected));
            Assert.That(cell, Is.EqualTo(new Vector2Int(cx, cz)));
        }

        [Test]
        public void CentersRoundTripWithDifferentCellSizeAndOrigin()
        {
            var custom = new FarmGridLayout(new Vector2(-4.3f, 2.7f), 3, 8, 0.7f);
            for (int x = 0; x < custom.Width; x++)
                for (int z = 0; z < custom.Depth; z++)
                {
                    var cell = new Vector2Int(x, z);
                    Assert.That(custom.TryGetCell(custom.Center(cell), out var actual), Is.True);
                    Assert.That(actual, Is.EqualTo(cell));
                }
        }

        [Test]
        public void ReachUsesGroundDistanceAndRejectsInvalidCells()
        {
            var cell = new Vector2Int(0, 0);
            var actor = grid.Center(cell) + new Vector3(2.5f, 9, 0);
            Assert.That(grid.IsWithinReach(cell, actor, 2.5f), Is.True);
            Assert.That(grid.IsWithinReach(cell, actor + Vector3.right * 0.01f, 2.5f), Is.False);
            Assert.That(grid.IsWithinReach(new Vector2Int(6, 0), actor, 100), Is.False);
        }

        [Test]
        public void InvalidGridConfigurationFailsImmediately()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FarmGridLayout(Vector2.zero, 0, 6, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FarmGridLayout(Vector2.zero, 6, 6, float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Center(new Vector2Int(-1, 0)));
        }

        [Test]
        public void DiagonalSpeedMatchesCardinalSpeedWithIsometricCamera()
        {
            Quaternion rotation = Quaternion.Euler(35.264f, 45, 0);
            Vector3 forward = rotation * Vector3.forward, right = rotation * Vector3.right;
            var up = PlanarMovement.Direction(Vector2.up, forward, right);
            var diagonal = PlanarMovement.Direction(Vector2.one, forward, right);
            Assert.That(up.magnitude, Is.EqualTo(1).Within(0.0001f));
            Assert.That(diagonal.magnitude, Is.EqualTo(up.magnitude).Within(0.0001f));
            Assert.That(up.y, Is.Zero);
            Assert.That(Vector3.Dot(up, new Vector3(1, 0, 1).normalized), Is.GreaterThan(0.999f));
            Assert.That(PlanarMovement.Direction(Vector2.zero, forward, right), Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void GroundBoundsPreventLeavingAnyEdgeWithoutChangingHeight()
        {
            Assert.That(PlanarMovement.ClampToGround(new Vector3(20, 0.8f, -20), 9.3f),
                Is.EqualTo(new Vector3(9.3f, 0.8f, -9.3f)));
        }
    }
}
