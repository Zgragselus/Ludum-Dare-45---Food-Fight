using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class ProceduralLevelGenerator
{
    private int _minRoomSize;
    private int _maxDepth;

    internal void Generate(Level level, int resolution, int minimumRoomSize, int maximumDepthOffset)
    {
        level.IsProcedural = true;

        // Alloc
        level.Size = resolution;

        _minRoomSize = minimumRoomSize;

        level.Map = new CellType[level.Size, level.Size];
        level.Units = new Unit[level.Size, level.Size];
        level.Objects = new PickupObject[level.Size, level.Size];

        _maxDepth = Log2Int(level.Size) - maximumDepthOffset;

        for (int i = 0; i < level.Size; i++)
        {
            for (int j = 0; j < level.Size; j++)
            {
                level.Map[i, j] = CellType.Empty;
            }
        }

        BSPNode root = RecursiveBuild(new Vector2Int(0, 0), new Vector2Int(level.Size, level.Size), 0);

        RecursiveFill(level, root);

        RecursiveConnect(level, root);

        BuildCorridorWalls(level);

        BuildCorridorFloors(level);

        for (int i = 0; i < level.Size; i++)
        {
            for (int j = 0; j < level.Size; j++)
            {
                if (i == 0 || j == 0 || i == (level.Size - 1) || j == (level.Size - 1))
                {
                    if (level.Map[i, j] != CellType.Empty)
                    {
                        level.Map[i, j] = CellType.Wall;
                    }
                }
            }
        }

        GenerateEntrance(level);
        GenerateExit(level);
    }

    private void GenerateEntrance(Level level)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int i = 1; i < level.Size - 1; i++)
        {
            for (int j = 1; j < level.Size - 1; j++)
            {
                if (level.SafeLook(i, j) == CellType.Wall &&
                    (level.SafeLook(i + 1, j) == CellType.Floor ||
                    level.SafeLook(i, j + 1) == CellType.Floor ||
                    level.SafeLook(i - 1, j) == CellType.Floor ||
                    level.SafeLook(i, j - 1) == CellType.Floor))
                {
                    candidates.Add(new Vector2Int(i, j));
                }
            }
        }

        int entrance = UnityEngine.Random.Range(0, candidates.Count);

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (level.Map[candidates[entrance].x + i, candidates[entrance].y + j] == CellType.Empty)
                {
                    level.Map[candidates[entrance].x + i, candidates[entrance].y + j] = CellType.Wall;
                }
            }
        }

        level.Map[candidates[entrance].x, candidates[entrance].y] = CellType.Entrance;
    }

    private void GenerateExit(Level level)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int i = 1; i < level.Size - 1; i++)
        {
            for (int j = 1; j < level.Size - 1; j++)
            {
                if (level.SafeLook(i, j) == CellType.Wall &&
                    (level.SafeLook(i + 1, j) == CellType.Floor ||
                    level.SafeLook(i, j + 1) == CellType.Floor ||
                   level.SafeLook(i - 1, j) == CellType.Floor ||
                    level.SafeLook(i, j - 1) == CellType.Floor))
                {
                    candidates.Add(new Vector2Int(i, j));
                }
            }
        }

        int exit = UnityEngine.Random.Range(0, candidates.Count);

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (level.Map[candidates[exit].x + i, candidates[exit].y + j] == CellType.Empty)
                {
                    level.Map[candidates[exit].x + i, candidates[exit].y + j] = CellType.Wall;
                }
            }
        }

        level.Map[candidates[exit].x, candidates[exit].y] = CellType.Exit;
    }

    private void BuildCorridorWalls(Level level)
    {
        for (int i = 0; i < level.Size; i++)
        {
            for (int j = 0; j < level.Size; j++)
            {
                if (level.Map[i, j] == CellType.Empty)
                {
                    bool corridorWall = false;

                    for (int k = -1; k <= 1; k++)
                    {
                        for (int l = -1; l <= 1; l++)
                        {
                            if (i + k >= 0 && i + k < level.Size && j + l >= 0 && j + l < level.Size)
                            {
                                if (level.Map[i + k, j + l] == CellType.Corridor)
                                {
                                    corridorWall = true;
                                }
                            }
                        }
                    }

                    if (corridorWall)
                    {
                        level.Map[i, j] = CellType.Wall;
                    }
                }
            }
        }
    }

    private void BuildCorridorFloors(Level level)
    {
        for (int i = 0; i < level.Size; i++)
        {
            for (int j = 0; j < level.Size; j++)
            {
                if (level.Map[i, j] == CellType.Corridor)
                {
                    level.Map[i, j] = CellType.Floor;
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

    private void BuildCorridor(Level level, Vector2Int entrance, Vector2Int exit, int axis, int maxWidth)
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
        level.Map[x, y] = CellType.Corridor;

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
                            if (x + i >= 0 && x + i < level.Size && y + j >= 0 && y + j < level.Size)
                            {
                                if (level.Map[x + i, y + j] == CellType.Empty)
                                {
                                    level.Map[x + i, y + j] = CellType.Corridor;
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
                            if (x + i >= 0 && x + i < level.Size && y + j >= 0 && y + j < level.Size)
                            {
                                if (level.Map[x + i, y + j] == CellType.Empty)
                                {
                                    level.Map[x + i, y + j] = CellType.Corridor;
                                }
                            }
                        }
                    }
                }
            }
            current++;
        }

        level.Map[x, y] = CellType.Corridor;
    }

    private void RecursiveConnect(Level level, BSPNode node)
    {
        if (node.isLeaf)
        {
            node.isConnected = true;
        }
        else
        {
            if (!node.children[0].isConnected)
            {
                RecursiveConnect(level, node.children[0]);
            }

            if (!node.children[1].isConnected)
            {
                RecursiveConnect(level, node.children[1]);
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

                level.Map[entrance.x, entrance.y] = CellType.Corridor;

                begin = nodesRight[exitID].realMin.y + 1;
                end = nodesRight[exitID].realMax.y - 1;
                position = UnityEngine.Random.Range(begin, end);

                Vector2Int exit = new Vector2Int(nodesRight[exitID].realMin.x, position);

                level.Map[exit.x, exit.y] = CellType.Corridor;

                BuildCorridor(level, entrance, exit, node.axis, corridorWidth);
            }
            else
            {
                int begin = nodesLeft[entranceID].realMin.x + 1;
                int end = nodesLeft[entranceID].realMax.x - 1;
                int position = UnityEngine.Random.Range(begin, end);

                Vector2Int entrance = new Vector2Int(position, nodesLeft[entranceID].realMax.y - 1);

                level.Map[entrance.x, entrance.y] = CellType.Corridor;

                begin = nodesRight[exitID].realMin.x + 1;
                end = nodesRight[exitID].realMax.x - 1;
                position = UnityEngine.Random.Range(begin, end);

                Vector2Int exit = new Vector2Int(position, nodesRight[exitID].realMin.y);

                level.Map[exit.x, exit.y] = CellType.Corridor;

                BuildCorridor(level, entrance, exit, node.axis, corridorWidth);
            }

            node.isConnected = true;
        }

    }

    private void RecursiveFill(Level level, BSPNode node)
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
                        level.Map[i, j] = CellType.Wall;
                    }
                    else
                    {
                        level.Map[i, j] = CellType.Floor;
                    }
                }
            }
        }
        else
        {
            RecursiveFill(level, node.children[0]);
            RecursiveFill(level, node.children[1]);
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
}
