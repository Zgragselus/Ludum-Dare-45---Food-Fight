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

public class Level : MonoBehaviour, ILevel
{
    CellType[][] map;
    int size;
    int minRoomSize;
    int maxDepth;

    public Text text;

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

                map[x][y] = CellType.CORRIDOR;
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

                    map[entrance.x][entrance.y] = CellType.CORRIDOR;
                    
                    begin = node.children[1].realMin.y + 1;
                    end = node.children[1].realMax.y - 1;
                    position = UnityEngine.Random.Range(begin, end + 1);

                    Vector2Int exit = new Vector2Int(node.children[1].realMin.x, position);

                    map[exit.x][exit.y] = CellType.CORRIDOR;

                    BuildCorridor(entrance, exit, node.axis);
                }
                else
                {
                    int begin = node.children[0].realMin.x + 1;
                    int end = node.children[0].realMax.x - 1;
                    int position = UnityEngine.Random.Range(begin, end + 1);

                    Vector2Int entrance = new Vector2Int(position, node.children[0].realMax.y - 1);

                    map[entrance.x][entrance.y] = CellType.CORRIDOR;

                    begin = node.children[1].realMin.x + 1;
                    end = node.children[1].realMax.x - 1;
                    position = UnityEngine.Random.Range(begin, end + 1);

                    Vector2Int exit = new Vector2Int(position, node.children[1].realMin.y);

                    map[exit.x][exit.y] = CellType.CORRIDOR;

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
                        map[i][j] = CellType.WALL;
                    }
                    else
                    {
                        map[i][j] = CellType.FLOOR;
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

    void Generate()
    {
        // Alloc
        size = 64;

        minRoomSize = 5;

        map = new CellType[size][];
        for (int i = 0; i < size; i++)
        {
            map[i] = new CellType[size];
        }

        maxDepth = Log2Int(size) - 4;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                map[i][j] = CellType.EMPTY;
            }
        }

        BSPNode root = RecursiveBuild(new Vector2Int(0, 0), new Vector2Int(size, size), 0);

        RecursiveFill(root);

        RecursiveConnect(root);
    }

    // Start is called before the first frame update
    void Start()
    {
        Generate();

        string tmp = "";
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                tmp += (char)map[i][j];
            }
            tmp += "\r\n";
        }
        text.text = tmp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Move(Vector2Int from, Vector2Int to)
    {
        throw new NotImplementedException();
    }

    public int GetNeighbours(Vector2Int pos, ref Span<Vector2Int> neighbours)
    {
        throw new NotImplementedException();
    }

    public bool IsOccupiedByUnit(Vector2Int pos)
    {
        throw new NotImplementedException();
    }
}
