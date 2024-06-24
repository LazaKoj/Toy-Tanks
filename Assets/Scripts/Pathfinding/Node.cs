using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool walkable;
    public Vector3 worldPosition;
    public Node parent;
    public int gridX;
    public int gridZ;
    public int gCost;
    public int hCost;
    int heapIndex;

    public Node(bool isWalkable, Vector3 position, int gridX, int gridY)
    {
        walkable = isWalkable;
        worldPosition = position;

        this.gridX = gridX;
        this.gridZ = gridY;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);

        if (compare == 0)
            compare = hCost.CompareTo(nodeToCompare.hCost);

        return -compare;
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }

        set
        {
            heapIndex = value;
        }
    }
}
