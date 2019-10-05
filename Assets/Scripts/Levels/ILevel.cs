using System;
using UnityEngine;

/// <summary>
/// Any level, both premade or generated, need to implement these interfaces.
/// </summary>
public interface ILevel
{
    bool Move(Vector2Int from, Vector2Int to);

    int GetNeighbours(Vector2Int pos, in Span<Vector2Int> neighbours);

    bool IsOccupiedByUnit(Vector2Int pos, out Unit unit);

    bool HasPickableItemAt(Vector2Int pos);
}
