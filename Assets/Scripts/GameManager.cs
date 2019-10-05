using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILevel
{
    bool Move(Vector2Int from, Vector2Int to);

    int GetNeighbours(Vector2Int pos, ref Span<Vector2Int> neighbours);

    bool IsOccupiedByUnit(Vector2Int pos);
}

public class GameManager : Singleton<GameManager>
{
    public ILevel CurrentLevel;
}
