using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CellType : byte
{
    EMPTY = (byte)' ',
    WALL = (byte)'#',
    FLOOR = (byte)'.'
}

/*public interface ILevel
{
    public bool CanMove(Vector2Int from, Vector2Int to);

    public int GetNeighbours(Vector2Int pos, ref Span<Vector2Int> neighbours);
}*/

public class BSPNode
{
    public int split;

    public bool isLeaf;

    public Vector2Int min;
    public Vector2Int max;

    public BSPNode[] children;

    public BSPNode()
    {
        children = new BSPNode[2];
        children[0] = null;
        children[1] = null;
    }
}

public class Level : MonoBehaviour
{
    CellType[][] map;
    int size;
    int minRoomSize;
    int maxDepth;

    public Text text;

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
            int shrinkLeft = 0;
            int shrinkRight = 0;

            int shrinkVertical = UnityEngine.Random.Range(0, shrinkHeightRange);
            int shrinkDown = 0;
            int shrinkUp = 0;

            Vector2Int min = new Vector2Int(node.min.x + shrinkLeft, node.min.y + shrinkDown);
            Vector2Int max = new Vector2Int(node.max.x - shrinkRight, node.max.y - shrinkUp);

            for (int i = min.x; i < max.x; i++)
            {
                for (int j = min.y; j < max.y; j++)
                {
                    if (i == min.x || j == min.y || i == (max.x - 1) || j == (max.y - 1))
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
            }
            else
            {
                int range = width - 2 * minRoomSize;
                int rand = UnityEngine.Random.Range(0, range);
                int split = min.x + minRoomSize + rand;

                Vector2Int leftMin = new Vector2Int(min.x, min.y);
                Vector2Int leftMax = new Vector2Int(max.x, split);

                n.children[0] = RecursiveBuild(leftMin, leftMax, depth + 1);

                Vector2Int rightMin = new Vector2Int(min.x, split + 1);
                Vector2Int rightMax = new Vector2Int(max.x, max.y);

                n.children[1] = RecursiveBuild(rightMin, rightMax, depth + 1);
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
}
