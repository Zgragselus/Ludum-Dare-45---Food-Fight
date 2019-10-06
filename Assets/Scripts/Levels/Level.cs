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

public class BSPNode
{
    public int split;
    public int axis;

    public bool isLeaf;

    public bool isConnected;

    public Vector2Int min;
    public Vector2Int max;

    public Vector2Int realMin;
    public Vector2Int realMax;

    public BSPNode[] children;

    public BSPNode()
    {
        children = new BSPNode[2];
        children[0] = null;
        children[1] = null;

        isConnected = false;
    }
}

public class Level : ILevel
{
    public CellType[,] Map;
    public Unit[,] Units;
    public ILevelObject[,] Objects;

    public int Size;
    private int _minRoomSize;
    private int _maxDepth;

    #region Generator

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

    private CellType SafeLook(int i, int j)
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
    private void GenerateEntrance()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (SafeLook(i, j) == CellType.Wall &&
                    (SafeLook(i + 1, j) == CellType.Floor ||
                    SafeLook(i, j + 1) == CellType.Floor ||
                    SafeLook(i - 1, j) == CellType.Floor ||
                    SafeLook(i, j - 1) == CellType.Floor))
                {
                    candidates.Add(new Vector2Int(i, j));
                }
            }
        }

        int entrance = UnityEngine.Random.Range(0, candidates.Count);

        Map[candidates[entrance].x, candidates[entrance].y] = CellType.Entrance;
    }

    private void GenerateExit()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (SafeLook(i, j) == CellType.Wall &&
                    (SafeLook(i + 1, j) == CellType.Floor ||
                    SafeLook(i, j + 1) == CellType.Floor ||
                    SafeLook(i - 1, j) == CellType.Floor ||
                    SafeLook(i, j - 1) == CellType.Floor))
                {
                    candidates.Add(new Vector2Int(i, j));
                }
            }
        }

        int exit = UnityEngine.Random.Range(0, candidates.Count);

        Map[candidates[exit].x, candidates[exit].y] = CellType.Exit;
    }

    private void BuildCorridorWalls()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (Map[i, j] == CellType.Empty)
                {
                    bool corridorWall = false;

                    for (int k = -1; k <= 1; k++)
                    {
                        for (int l = -1; l <= 1; l++)
                        {
                            if (i + k >= 0 && i + k < Size && j + l >= 0 && j + l < Size)
                            {
                                if (Map[i + k, j + l] == CellType.Corridor)
                                {
                                    corridorWall = true;
                                }
                            }
                        }
                    }

                    if (corridorWall)
                    {
                        Map[i, j] = CellType.Wall;
                    }
                }
            }
        }
    }

    private void BuildCorridorFloors()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (Map[i, j] == CellType.Corridor)
                {
                    Map[i, j] = CellType.Floor;
                }
            }
        }
    }

    private List<BSPNode> GetSubtreeLeafs(BSPNode node)
    {
        List<BSPNode> result = new List<BSPNode>();

        Stack<BSPNode> stack = new Stack<BSPNode>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            BSPNode n = stack.Pop();

            if (n.isLeaf)
            {
                result.Add(n);
            }
            else
            {
                if (n.children[0] != null)
                {
                    stack.Push(n.children[0]);
                }

                if (n.children[1] != null)
                {
                    stack.Push(n.children[1]);
                }
            }
        }

        return result;
    }

    private void FindClosestNodes(List<BSPNode> left, List<BSPNode> right, ref int first, ref int second)
    {
        float minDistance = 999999.9f;
        first = -1;
        second = -1;

        for (int i = 0; i < left.Count; i++)
        {
            for (int j = 0; j < right.Count; j++)
            {
                Vector2Int l = new Vector2Int((left[i].realMin.x + left[i].realMax.x) / 2, (left[i].realMin.y + left[i].realMax.y) / 2);
                Vector2Int r = new Vector2Int((right[j].realMin.x + right[j].realMax.x) / 2, (right[j].realMin.y + right[j].realMax.y) / 2);

                Vector2Int diff = r - l;

                if (diff.magnitude < minDistance)
                {
                    first = i;
                    second = j;
                    minDistance = diff.magnitude;
                }
            }
        }
    }

    private void BuildCorridor(Vector2Int entrance, Vector2Int exit, int axis, int maxWidth)
    {
        Vector2Int distance = new Vector2Int(exit.x - entrance.x, exit.y - entrance.y);

        Vector2Int[] waypoint = new Vector2Int[4];
        waypoint[0] = new Vector2Int(entrance.x, entrance.y);
        if (axis == 0)
        {
            waypoint[1] = new Vector2Int(entrance.x + distance.x / 2, entrance.y);
            waypoint[2] = new Vector2Int(entrance.x + distance.x / 2, exit.y);
        }
        else
        {
            waypoint[1] = new Vector2Int(entrance.x, entrance.y + distance.y / 2);
            waypoint[2] = new Vector2Int(exit.x, entrance.y + distance.y / 2);
        }
        waypoint[3] = new Vector2Int(exit.x, exit.y);

        int x = entrance.x;
        int y = entrance.y;
        Map[x, y] = CellType.Corridor;

        int current = 1;
        while (current != 4)
        {
            while (!(x == waypoint[current].x && y == waypoint[current].y))
            {
                if (x < waypoint[current].x)
                {
                    x++;
                }
                else if (x > waypoint[current].x)
                {
                    x--;
                }
                else if (y < waypoint[current].y)
                {
                    y++;
                }
                else if (y > waypoint[current].y)
                {
                    y--;
                }

                if (maxWidth < 0)
                {
                    for (int i = maxWidth; i < -maxWidth; i++)
                    {
                        for (int j = maxWidth; j < -maxWidth; j++)
                        {
                            if (x + i >= 0 && x + i < Size && y + j >= 0 && y + j < Size)
                            {
                                if (Map[x + i, y + j] == CellType.Empty)
                                {
                                    Map[x + i, y + j] = CellType.Corridor;
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = -maxWidth; i <= maxWidth; i++)
                    {
                        for (int j = -maxWidth; j <= maxWidth; j++)
                        {
                            if (x + i >= 0 && x + i < Size && y + j >= 0 && y + j < Size)
                            {
                                if (Map[x + i, y + j] == CellType.Empty)
                                {
                                    Map[x + i, y + j] = CellType.Corridor;
                                }
                            }
                        }
                    }
                }
            }
            current++;
        }

        Map[x, y] = CellType.Corridor;
    }

    private void RecursiveConnect(BSPNode node)
    {
        if (node.isLeaf)
        {
            node.isConnected = true;
        }
        else
        {
            if (!node.children[0].isConnected)
            {
                RecursiveConnect(node.children[0]);
            }

            if (!node.children[1].isConnected)
            {
                RecursiveConnect(node.children[1]);
            }

            List<BSPNode> nodesLeft = GetSubtreeLeafs(node.children[0]);
            List<BSPNode> nodesRight = GetSubtreeLeafs(node.children[1]);

            int entranceID = -1;
            int exitID = -1;

            int corridorWidth = UnityEngine.Random.Range(-1, 2);

            FindClosestNodes(nodesLeft, nodesRight, ref entranceID, ref exitID);

            if (node.axis == 0)
            {
                int begin = nodesLeft[entranceID].realMin.y + 1;
                int end = nodesLeft[entranceID].realMax.y - 1;
                int position = UnityEngine.Random.Range(begin, end);

                Vector2Int entrance = new Vector2Int(nodesLeft[entranceID].realMax.x - 1, position);

                Map[entrance.x, entrance.y] = CellType.Corridor;

                begin = nodesRight[exitID].realMin.y + 1;
                end = nodesRight[exitID].realMax.y - 1;
                position = UnityEngine.Random.Range(begin, end);

                Vector2Int exit = new Vector2Int(nodesRight[exitID].realMin.x, position);

                Map[exit.x, exit.y] = CellType.Corridor;

                BuildCorridor(entrance, exit, node.axis, corridorWidth);
            }
            else
            {
                int begin = nodesLeft[entranceID].realMin.x + 1;
                int end = nodesLeft[entranceID].realMax.x - 1;
                int position = UnityEngine.Random.Range(begin, end);

                Vector2Int entrance = new Vector2Int(position, nodesLeft[entranceID].realMax.y - 1);

                Map[entrance.x, entrance.y] = CellType.Corridor;

                begin = nodesRight[exitID].realMin.x + 1;
                end = nodesRight[exitID].realMax.x - 1;
                position = UnityEngine.Random.Range(begin, end);

                Vector2Int exit = new Vector2Int(position, nodesRight[exitID].realMin.y);

                Map[exit.x, exit.y] = CellType.Corridor;

                BuildCorridor(entrance, exit, node.axis, corridorWidth);
            }

            node.isConnected = true;
        }

    }

    private void RecursiveFill(BSPNode node)
    {
        if (node.isLeaf)
        {
            int width = node.max.x - node.min.x;
            int height = node.max.y - node.min.y;

            int shrinkWidthRange = width - _minRoomSize;
            if (shrinkWidthRange < 0)
            {
                shrinkWidthRange = 0;
            }

            int shrinkHeightRange = height - _minRoomSize;
            if (shrinkHeightRange < 0)
            {
                shrinkHeightRange = 0;
            }

            int shrinkHorizontal = UnityEngine.Random.Range(0, shrinkWidthRange);
            int shrinkLeft = shrinkHorizontal / 2;
            int shrinkRight = shrinkHorizontal / 2 + shrinkHorizontal % 2;

            int shrinkVertical = UnityEngine.Random.Range(0, shrinkHeightRange);
            int shrinkDown = shrinkVertical / 2;
            int shrinkUp = shrinkVertical / 2 + shrinkVertical % 2;

            node.realMin = new Vector2Int(node.min.x + shrinkLeft, node.min.y + shrinkDown);
            node.realMax = new Vector2Int(node.max.x - shrinkRight, node.max.y - shrinkUp);

            for (int i = node.realMin.x; i < node.realMax.x; i++)
            {
                for (int j = node.realMin.y; j < node.realMax.y; j++)
                {
                    if (i == node.realMin.x || j == node.realMin.y || i == (node.realMax.x - 1) || j == (node.realMax.y - 1))
                    {
                        Map[i, j] = CellType.Wall;
                    }
                    else
                    {
                        Map[i, j] = CellType.Floor;
                    }
                }
            }
        }
        else
        {
            RecursiveFill(node.children[0]);
            RecursiveFill(node.children[1]);
        }
    }

    private BSPNode RecursiveBuild(Vector2Int min, Vector2Int max, int depth)
    {
        int width = max.x - min.x;
        int height = max.y - min.y;

        if (width <= _minRoomSize * 2 || height <= _minRoomSize * 2 || depth >= _maxDepth)
        {
            BSPNode n = new BSPNode
            {
                isLeaf = true,
                min = min,
                max = max
            };
            return n;
        }
        else
        {
            BSPNode n = new BSPNode()
            {
                isLeaf = false,
                min = min,
                max = max
            };

            if (width > height)
            {
                int range = width - 2 * _minRoomSize;
                int rand = UnityEngine.Random.Range(0, range);
                int split = min.x + _minRoomSize + rand;

                Vector2Int leftMin = new Vector2Int(min.x, min.y);
                Vector2Int leftMax = new Vector2Int(split, max.y);

                n.children[0] = RecursiveBuild(leftMin, leftMax, depth + 1);

                Vector2Int rightMin = new Vector2Int(split + 1, min.y);
                Vector2Int rightMax = new Vector2Int(max.x, max.y);

                n.children[1] = RecursiveBuild(rightMin, rightMax, depth + 1);

                n.axis = 0;
            }
            else
            {
                int range = width - 2 * _minRoomSize;
                int rand = UnityEngine.Random.Range(0, range);
                int split = min.y + _minRoomSize + rand;

                Vector2Int leftMin = new Vector2Int(min.x, min.y);
                Vector2Int leftMax = new Vector2Int(max.x, split);

                n.children[0] = RecursiveBuild(leftMin, leftMax, depth + 1);

                Vector2Int rightMin = new Vector2Int(min.x, split + 1);
                Vector2Int rightMax = new Vector2Int(max.x, max.y);

                n.children[1] = RecursiveBuild(rightMin, rightMax, depth + 1);

                n.axis = 1;
            }

            return n;
        }
    }

    private int Log2Int(int value)
    {
        int log = 0;

        while (value > 0)
        {
            value /= 2;
            log++;
        }

        return log;
    }

    internal void Generate(int resolution, int minimumRoomSize, int maximumDepthOffset)
    {
        // Alloc
        Size = resolution;

        _minRoomSize = minimumRoomSize;

        Map = new CellType[Size, Size];
        Units = new Unit[Size, Size];
        Objects = new ILevelObject[Size, Size];

        _maxDepth = Log2Int(Size) - maximumDepthOffset;

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Map[i, j] = CellType.Empty;
            }
        }

        BSPNode root = RecursiveBuild(new Vector2Int(0, 0), new Vector2Int(Size, Size), 0);

        RecursiveFill(root);

        RecursiveConnect(root);

        BuildCorridorWalls();

        BuildCorridorFloors();

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (i == 0 || j == 0 || i == (Size - 1) || j == (Size - 1))
                {
                    if (Map[i, j] != CellType.Empty)
                    {
                        Map[i, j] = CellType.Wall;
                    }
                }
            }
        }

        GenerateEntrance();
        GenerateExit();
    }

    #endregion

    private Dictionary<Unit, (Vector2Int from, Vector2Int to)> _actionsToDo = new Dictionary<Unit, (Vector2Int from, Vector2Int to)>();

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
