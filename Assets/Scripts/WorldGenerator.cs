using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    Enemy01,
    Enemy02,
    Boss,
}

public class WorldGenerator : Singleton<WorldGenerator>
{
    public Player PlayerPrefab;

    public MapTile FloorPrefab;
    public MapTile FloorPrefabAlternative;
    public MapTile WallPrefab;
    public MapTile EntrancePrefab;
    public MapTile ExitPrefab;

    public MapTile WallPrefabEnd;
    public MapTile WallPrefabStraight;
    public MapTile WallPrefabCorner;
    public MapTile WallPrefabT;
    public MapTile WallPrefabCross;

    public Unit Enemy01;
    public Unit Boss;

    public PickupObject Powerup01;

    public MapTile SpawnStaticWorldBlock(Level level, Vector2Int pos, CellType type)
    {
        Quaternion rotation = Quaternion.identity;
        MapTile prefab = GetPrefabForCellType(level, type, pos, ref rotation);
        if (prefab == null)
        {
            return null;
        }
        var tile = Instantiate(prefab, level.WorldParent);
        tile.transform.localPosition = new Vector3(pos.x, 0, pos.y);
        tile.transform.localRotation = rotation;

        return tile;
    }

    public void SpawnUnit(Level level, Vector2Int pos, UnitType type)
    {
        Unit prefab = GetPrefabForUnitType(type);
        if (prefab == null)
        {
            return;
        }

        var unit = Instantiate(prefab, level.WorldParent);
        unit.transform.localPosition = new Vector3(pos.x, 0, pos.y);
        unit.transform.localRotation = Quaternion.identity;
        unit.CurrentPosition = pos;
        unit.CurrentDirection = Vector2Int.right;

        level.ActiveUnits.Add(unit);
        level.Units[pos.x, pos.y] = unit;
    }

    public Player CreatePlayer()
    {
        var player = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
        player.CurrentPosition = Vector2Int.zero;
        player.CurrentDirection = Vector2Int.right;

        return player;
    }

    private MapTile GetWallTile(Level level, Vector2Int pos, ref Quaternion rotation)
    {
        int count = 0;

        if (level.SafeLookEdges(pos.x - 1, pos.y) == CellType.Wall)
        {
            count++;
        }
        if (level.SafeLookEdges(pos.x + 1, pos.y) == CellType.Wall)
        {
            count++;
        }
        if (level.SafeLookEdges(pos.x, pos.y - 1) == CellType.Wall)
        {
            count++;
        }
        if (level.SafeLookEdges(pos.x, pos.y + 1) == CellType.Wall)
        {
            count++;
        }

        if (count == 4)
        {
            return WallPrefabCross;
        }
        else if (count == 3)
        {
            if (level.SafeLookEdges(pos.x - 1, pos.y) != CellType.Wall)
            {
                rotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);
            }
            else if (level.SafeLookEdges(pos.x + 1, pos.y) != CellType.Wall)
            {
                rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
            }
            else if (level.SafeLookEdges(pos.x, pos.y - 1) != CellType.Wall)
            {
                rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            }
            else if (level.SafeLookEdges(pos.x, pos.y + 1) != CellType.Wall)
            {
                rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            }

            return WallPrefabT;
        }
        else if (count == 2)
        {
            if ((level.SafeLookEdges(pos.x - 1, pos.y) != CellType.Wall) && (level.SafeLookEdges(pos.x + 1, pos.y) != CellType.Wall))
            {
                rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                return WallPrefabStraight;
            }
            else if ((level.SafeLookEdges(pos.x, pos.y - 1) != CellType.Wall) && (level.SafeLookEdges(pos.x, pos.y + 1) != CellType.Wall))
            {
                rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                return WallPrefabStraight;
            }
            else
            {
                if ((level.SafeLookEdges(pos.x - 1, pos.y) != CellType.Wall) && (level.SafeLookEdges(pos.x, pos.y - 1) != CellType.Wall))
                {
                    rotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);
                }
                else if ((level.SafeLookEdges(pos.x + 1, pos.y) != CellType.Wall) && (level.SafeLookEdges(pos.x, pos.y - 1) != CellType.Wall))
                {
                    rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
                }
                else if ((level.SafeLookEdges(pos.x + 1, pos.y) != CellType.Wall) && (level.SafeLookEdges(pos.x + 1, pos.y) != CellType.Wall))
                {
                    rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                }

                return WallPrefabCorner;
            }
        }
        else
        {
            if (level.SafeLookEdges(pos.x - 1, pos.y) == CellType.Wall)
            {
                rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            }
            else if (level.SafeLookEdges(pos.x + 1, pos.y) == CellType.Wall)
            {
                rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            }
            else if (level.SafeLookEdges(pos.x, pos.y - 1) == CellType.Wall)
            {
                rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
            }
            else if (level.SafeLookEdges(pos.x, pos.y + 1) == CellType.Wall)
            {
                rotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);
            }

            return WallPrefabEnd;
        }
    }

    public MapTile GetPrefabForCellType(Level level, CellType type, Vector2Int pos, ref Quaternion rotation)
    {
        switch (type)
        {
            case CellType.Floor:
            case CellType.Corridor:
                return (UnityEngine.Random.Range(0, 2) == 0 ? FloorPrefab : FloorPrefabAlternative);
            case CellType.Wall:
                return GetWallTile(level, pos, ref rotation);
            case CellType.Entrance:
                return EntrancePrefab;
            case CellType.Exit:
                return ExitPrefab;
        }
        return null;
    }

    public Unit GetPrefabForUnitType(UnitType type)
    {
        switch (type)
        {
            case UnitType.Enemy01:
                return Enemy01;
            case UnitType.Boss:
                return Boss;
        }
        return null;
    }

    internal void SpawnPickupItem(Level level, Vector2Int pos)
    {
        var goParent = new GameObject("dummyparent");
        goParent.transform.parent = level.WorldParent;
        goParent.transform.localPosition = new Vector3(pos.x, 0, pos.y);
        goParent.transform.localRotation = Quaternion.identity;

        var obj = Instantiate(Powerup01, goParent.transform);

        level.Objects[pos.x, pos.y] = obj;
    }
}
