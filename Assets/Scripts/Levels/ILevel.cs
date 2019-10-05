using System;
using UnityEngine;

/// <summary>
/// Any level, both premade or generated, need to implement these interfaces.
/// </summary>
public interface ILevel
{
    int GetNeighbours(Vector2Int pos, in Span<Vector2Int> neighbours);
}
