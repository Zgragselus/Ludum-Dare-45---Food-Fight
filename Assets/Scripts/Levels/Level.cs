using System;
using System.Collections.Generic;
using UnityEngine;

public enum CellType : byte
{
    Empty = (byte)' ',
    Wall = (byte)'#',
    Floor = (byte)'.',
    Corridor = (byte)'|',
    Entrance = (byte)'E',
    Exit = (byte)'X'
}

public enum ActionType
{
    Move,
    Attack,
    PickUpItem,
    Wait,
    Die,
}

public interface ILevelObject { }

public class Level : ILevel
{
    public CellType[,] Map;
    public Unit[,] Units;
    public ILevelObject[,] Objects;

    public int Size;

    private Dictionary<Unit, (Vector2Int from, Vector2Int to)> _actionsToDo = new Dictionary<Unit, (Vector2Int from, Vector2Int to)>();

    public CellType SafeLook(int i, int j)
    {
        if (i < 0)
        {
            i = 0;
        }

        if (j < 0)
        {
            j = 0;
        }

        if (i >= Size)
        {
            i = Size - 1;
        }

        if (j >= Size)
        {
            j = Size - 1;
        }

        return Map[i, j];
    }

    private List<Vector2Int> GenerateItemLocationList()
    {
        List<Vector2Int> result = new List<Vector2Int>();

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (SafeLook(i, j) == CellType.Floor)
                {
                    result.Add(new Vector2Int(i, j));
                }
            }
        }

        // Shuffle list
        int n = result.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n);
            Vector2Int value = result[k];
            result[k] = result[n];
            result[n] = value;
        }

        return result;
    }

    public void RegisterAction(Unit unit, Vector2Int from, Vector2Int to)
    {
        if (_actionsToDo.ContainsKey(unit))
        {
            throw new InvalidOperationException($"Unit {unit.name} already has a registered action for this frame.");
        }
        _actionsToDo.Add(unit, (from, to));
    }

    public void AddPlayer(Player player, Vector2Int pos)
    {
        if (!IsWalkable(pos) || IsOccupiedByUnit(pos, out _))
        {
            throw new InvalidOperationException("Cannot move player to the given position.");
        }
        Units[pos.x, pos.y] = player;
        player.CurrentPosition = pos;
        player.CurrentDirection = Vector2Int.right;
    }

    private static Vector2Int[] s_neighbourOffsets = new Vector2Int[]
    {
        Vector2Int.left,
        Vector2Int.right,
        Vector2Int.up,
        Vector2Int.down,
    };

    public List<Unit> ActiveUnits = new List<Unit>();

    public Transform WorldParent;

    public int GetNeighbours(Vector2Int pos, in Span<Vector2Int> neighbours)
    {
        int validNeighbours = 0;
        for (var i = 0; i < s_neighbourOffsets.Length; i++)
        {
            var offset = s_neighbourOffsets[i];
            if (!IsInRange(pos + offset) || !IsWalkable(pos + offset))
            {
                continue;
            }

            neighbours[validNeighbours] = pos + offset;
            validNeighbours++;
        }
        return validNeighbours;
    }

    public bool IsWalkable(Vector2Int pos)
    {
        return Map[pos.x, pos.y] == CellType.Floor || Map[pos.x, pos.y] == CellType.Corridor;
    }

    private bool IsInRange(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < Map.GetLength(0) && pos.y < Map.GetLength(1);
    }

    public bool IsOccupiedByUnit(Vector2Int pos, out Unit unit)
    {
        unit = null;
        if (Units[pos.x, pos.y] is Unit u)
        {
            unit = u;
        }
        return unit;
    }

    public bool TryGetAnyWalkablePosition(out Vector2Int pos)
    {
        pos = default;
        for (int i = 0; i < Map.GetLength(0); i++)
        {
            for (int j = 0; j < Map.GetLength(1); j++)
            {
                pos = new Vector2Int(i, j);
                if (IsWalkable(pos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Kill(Unit unit)
    {
        Units[unit.CurrentPosition.x, unit.CurrentPosition.y] = null;
        if (ActiveUnits.Contains(unit))
        {
            ActiveUnits.Remove(unit);
        }
        Debug.Log($"Killed {unit.gameObject.name}");
        GameObject.Destroy(unit.gameObject, 5);
    }

    public void DoActions(List<(Unit, Vector2Int, ActionType type, object payload)> results)
    {
        var tempUnits = new Unit[Size, Size];

        // first do the player
        if (_actionsToDo.TryGetValue(GameManager.Instance.CurrentPlayer, out var playerTransition))
        {
            var otherUnit = Units[playerTransition.to.x, playerTransition.to.y];
            // we want to move somewhere we can't walk; just idle
            if (!IsWalkable(playerTransition.to))
            {
                results.Add((GameManager.Instance.CurrentPlayer, playerTransition.from, ActionType.Wait, null));

                tempUnits[playerTransition.from.x, playerTransition.from.y] = GameManager.Instance.CurrentPlayer;
            }
            // we want to move to an empty space
            else if (otherUnit == null)
            {
                results.Add((GameManager.Instance.CurrentPlayer, playerTransition.to, ActionType.Move, null));

                tempUnits[playerTransition.to.x, playerTransition.to.y] = GameManager.Instance.CurrentPlayer;
            }
            // the space is occupied by some other unit; get unit and it's transition
            else if (_actionsToDo.TryGetValue(otherUnit, out var otherTransition))
            {
                // we want to "switch" places => damage
                if (otherTransition.to == playerTransition.from)
                {
                    results.Add((GameManager.Instance.CurrentPlayer, playerTransition.from, ActionType.Attack, otherUnit));
                    results.Add((otherUnit, otherTransition.from, ActionType.Attack, GameManager.Instance.CurrentPlayer));

                    tempUnits[playerTransition.from.x, playerTransition.from.y] = GameManager.Instance.CurrentPlayer;
                    tempUnits[otherTransition.from.x, otherTransition.from.y] = otherUnit;

                    if (GameManager.Instance.CurrentPlayer.TakeDamage(otherUnit.Damage))
                    {
                        results.Add((GameManager.Instance.CurrentPlayer, playerTransition.from, ActionType.Die, null));
                    }

                    if (otherUnit.TakeDamage(GameManager.Instance.CurrentPlayer.Damage))
                    {
                        results.Add((GameManager.Instance.CurrentPlayer, otherTransition.from, ActionType.Die, null));
                    }
                }
                // move other unit, then move player to the given space
                else
                {
                    results.Add((otherUnit, otherTransition.to, ActionType.Move, null));
                    results.Add((GameManager.Instance.CurrentPlayer, playerTransition.to, ActionType.Move, null));

                    tempUnits[playerTransition.to.x, playerTransition.to.y] = GameManager.Instance.CurrentPlayer;
                    tempUnits[otherTransition.to.x, otherTransition.to.y] = otherUnit;
                }

                _actionsToDo.Remove(otherUnit);
            }

            // pick up item if there is any after moving
            if (Objects[playerTransition.to.x, playerTransition.to.y] is ILevelObject obj)
            {
                results.Add((GameManager.Instance.CurrentPlayer, playerTransition.to, ActionType.PickUpItem, obj));

                Objects[playerTransition.to.x, playerTransition.to.y] = null;
            }

            _actionsToDo.Remove(GameManager.Instance.CurrentPlayer);
        }

        // then the rest
        foreach (var kv in _actionsToDo)
        {
            // unit wants to go to a place where player is => damage it
            if (tempUnits[kv.Value.to.x, kv.Value.to.y] == GameManager.Instance.CurrentPlayer)
            {
                results.Add((kv.Key, kv.Value.from, ActionType.Attack, GameManager.Instance.CurrentPlayer));

                if (GameManager.Instance.CurrentPlayer.TakeDamage(kv.Key.Damage))
                {
                    results.Add((GameManager.Instance.CurrentPlayer, playerTransition.from, ActionType.Die, null));
                }

                tempUnits[kv.Value.from.x, kv.Value.from.y] = kv.Key;
            }
            // unit wants to go to a place where some unit already is => wait where you are
            else if (tempUnits[kv.Value.to.x, kv.Value.to.y] != null)
            {
                results.Add((kv.Key, kv.Value.from, ActionType.Wait, null));

                tempUnits[kv.Value.from.x, kv.Value.from.y] = kv.Key;
            }
            // unit wants to go to and empty place => just move there
            else
            {
                results.Add((kv.Key, kv.Value.to, ActionType.Move, null));

                tempUnits[kv.Value.to.x, kv.Value.to.y] = kv.Key;
            }
        }

        _actionsToDo.Clear();

        Units = tempUnits;
    }
}
