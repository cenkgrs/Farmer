using System;
using UnityEngine;

namespace Farmer
{
    public sealed class FarmGridLayout
    {
        public Vector2 Origin { get; }
        public int Width { get; }
        public int Depth { get; }
        public float CellSize { get; }

        public FarmGridLayout(Vector2 origin, int width, int depth, float cellSize)
        {
            if (width <= 0 || depth <= 0 || cellSize <= 0 || !float.IsFinite(cellSize))
                throw new ArgumentOutOfRangeException(nameof(cellSize), "Grid dimensions must be positive and finite.");
            Origin = origin;
            Width = width;
            Depth = depth;
            CellSize = cellSize;
        }

        public bool Contains(Vector2Int cell) => cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Depth;

        // Lower edges belong to a cell; upper edges belong to the next cell (or outside).
        public bool TryGetCell(Vector3 world, out Vector2Int cell)
        {
            cell = new Vector2Int(Mathf.FloorToInt((world.x - Origin.x) / CellSize),
                Mathf.FloorToInt((world.z - Origin.y) / CellSize));
            return Contains(cell);
        }

        public Vector3 Center(Vector2Int cell, float height = 0f)
        {
            if (!Contains(cell)) throw new ArgumentOutOfRangeException(nameof(cell));
            return new Vector3(Origin.x + (cell.x + 0.5f) * CellSize, height,
                Origin.y + (cell.y + 0.5f) * CellSize);
        }

        public bool IsWithinReach(Vector2Int cell, Vector3 actor, float reach)
        {
            if (!Contains(cell) || reach < 0) return false;
            Vector3 delta = Center(cell) - actor;
            delta.y = 0f;
            return delta.sqrMagnitude <= reach * reach;
        }
    }
}
