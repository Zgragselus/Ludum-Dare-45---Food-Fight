using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : Singleton<WorldGenerator>
{
    public Player PlayerPrefab;

    public GameObject FloorPrefab;
    public GameObject WallPrefab;
    public GameObject EntrancePrefab;
    public GameObject ExitPrefab;

    public Unit Enemy01;

    private Transform _levelParent;

    private void Start()
    {
        var go = new GameObject("LevelObjects");
        _levelParent = go.transform;
    }

    public void SpawnStaticWorldBlock(Level level, Vector2Int pos, CellType type)
    {
        GameObject prefab = GetPrefabForCellType(type);
        if (prefab == null)
        {
            return;
        }
        GameObject.Instantiate(prefab, new Vector3(pos.x, 0, pos.y), Quaternion.identity, _levelParent);
    }

    public void SpawnUnit(Level level, Vector2Int pos)
    {
        var unit = Instantiate(Enemy01, new Vector3(pos.x, 0, pos.y), Quaternion.identity, _levelParent);
        unit.CurrentPosition = pos;
        unit.CurrentDirection = Vector2Int.right;

        level.Units.Add(unit);
    }

    public Player CreatePlayer()
    {
        var player = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, _levelParent);
        player.CurrentPosition = Vector2Int.zero;
        player.CurrentDirection = Vector2Int.right;

        return player;
    }

    public GameObject GetPrefabForCellType(CellType type)
    {
        switch (type)
        {
            case CellType.Floor:
            case CellType.Corridor:
                return FloorPrefab;
            case CellType.Wall:
                return WallPrefab;
            case CellType.Entrance:
                return EntrancePrefab;
            case CellType.Exit:
                return ExitPrefab;
        }
        return null;
    }
}
