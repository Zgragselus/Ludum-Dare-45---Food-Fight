using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CellType : byte
{
    EMPTY = (byte)' ',
    WALL = (byte)'#',
    FLOOR = (byte)'.',
    CORRIDOR = (byte)'|'
}

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
    public int Size;
    int minRoomSize;
    int maxDepth;

    #region Generator

    void BuildCorridor(Vector2Int entrance, Vector2Int exit, int axis)
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

                Map[x, y] = CellType.CORRIDOR;
            }
            current++;
        }
    }

    void RecursiveConnect(BSPNode node)
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

            if (node.children[0].isLeaf && node.children[1].isLeaf)
            {
                if (node.axis == 0)
                {
                    int begin = node.children[0].realMin.y + 1;
                    int end = node.children[0].realMax.y - 1;
                    int position = UnityEngine.Random.Range(begin, end + 1);

                    Vector2Int entrance = new Vector2Int(node.children[0].realMax.x - 1, position);

                    Map[entrance.x, entrance.y] = CellType.CORRIDOR;

                    begin = node.children[1].realMin.y + 1;
                    end = node.children[1].realMax.y - 1;
                    position = UnityEngine.Random.Range(begin, end + 1);

                    Vector2Int exit = new Vector2Int(node.children[1].realMin.x, position);

                    Map[exit.x, exit.y] = CellType.CORRIDOR;

                    BuildCorridor(entrance, exit, node.axis);
                }
                else
                {
                    int begin = node.children[0].realMin.x + 1;
                    int end = node.children[0].realMax.x - 1;
                    int position = UnityEngine.Random.Range(begin, end + 1);

                    Vector2Int entrance = new Vector2Int(position, node.children[0].realMax.y - 1);

                    Map[entrance.x, entrance.y] = CellType.CORRIDOR;

                    begin = node.children[1].realMin.x + 1;
                    end = node.children[1].realMax.x - 1;
                    position = UnityEngine.Random.Range(begin, end + 1);

                    Vector2Int exit = new Vector2Int(position, node.children[1].realMin.y);

                    Map[exit.x, exit.y] = CellType.CORRIDOR;

                    BuildCorridor(entrance, exit, node.axis);
                }
            }
            else
            {

            }

            node.isConnected = true;
        }

    }

    void RecursiveFill(BSPNode node)
    {
        if (node.isLeaf)
        {
            int width = node.max.x - node.min.x;
            int height = node.max.y - node.min.y;

            int shrinkWidthRange = width - minRoomSize;
            if (shrinkWidthRange < 0)
            {
                shrinkWidthRange = 0;
            }

            int shrinkHeightRange = height - minRoomSize;
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
                        Map[i, j] = CellType.WALL;
                    }
                    else
                    {
                        Map[i, j] = CellType.FLOOR;
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

    BSPNode RecursiveBuild(Vector2Int min, Vector2Int max, int depth)
    {
        int width = max.x - min.x;
        int height = max.y - min.y;

        if (width <= minRoomSize * 2 || height <= minRoomSize * 2 || depth >= maxDepth)
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
                int range = width - 2 * minRoomSize;
                int rand = UnityEngine.Random.Range(0, range);
                int split = min.x + minRoomSize + rand;

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
                int range = width - 2 * minRoomSize;
                int rand = UnityEngine.Random.Range(0, range);
                int split = min.y + minRoomSize + rand;

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

    int Log2Int(int value)
    {
        int log = 0;

        while (value > 0)
        {
            value /= 2;
            log++;
        }

        return log;
    }

    internal void Generate()
    {
        // Alloc
        Size = 64;

        minRoomSize = 5;

        Map = new CellType[Size, Size];

        maxDepth = Log2Int(Size) - 4;

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Map[i, j] = CellType.EMPTY;
            }
        }

        BSPNode root = RecursiveBuild(new Vector2Int(0, 0), new Vector2Int(Size, Size), 0);

        RecursiveFill(root);

        RecursiveConnect(root);
    }

    #endregion

    public bool Move(Vector2Int from, Vector2Int to)
    {
        return IsWalkable(to);
    }

    private static Vector2Int[] s_neighbourOffsets = new Vector2Int[]
    {
        Vector2Int.left,
        Vector2Int.right,
        Vector2Int.up,
        Vector2Int.down,
    };

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
        return Map[pos.x, pos.y] == CellType.FLOOR || Map[pos.x, pos.y] == CellType.CORRIDOR;
    }

    private bool IsInRange(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < Map.GetLength(0) && pos.y < Map.GetLength(1);
    }

    public bool IsOccupiedByUnit(Vector2Int pos)
    {
        return false;
    }

    public bool HasPickableItemAt(Vector2Int pos)
    {
        return false;
    }

    public Unit GetUnitAtPosition(Vector2Int pos)
    {
        return null;
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
}
