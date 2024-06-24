using System.Collections.Generic;
using UnityEngine;

public class PathfindingGrid : MonoBehaviour
{

    public Vector3 gridWorldSize; //size of area for pathfinding to apply. only X and Z matter in inspector
    public GameObject player;
    public float nodeRadius; //size of each node for pathfinding. smaller nodes = more precise pathfinding = worse performance
    public LayerMask unwalkableMask; //layer which objects will pathfind around. "Obstacle" in this implementation
    public List<Node> path;
    public bool displayGridGizmos; //only used to show debug in scene
    Node[,] grid; //grid which pathfinding will be traced through

    float nodeDiameter; //nodeRadius * 2
    int gridSizeX; 
    int gridSizeZ;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);

        CreateGrid();
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition) //
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.z / 2) / gridWorldSize.z;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeZ - 1) * percentY);

        return grid[x, y];
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; ++x)
            for (int z = -1; z <= 1; ++z)
            {
                if (x == 0 && z == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkZ = node.gridZ + z;

                if (checkX >= 0 && checkX < gridSizeX && checkZ >= 0 && checkZ < gridSizeZ)
                    neighbors.Add(grid[checkX, checkZ]);
            }

        return neighbors;
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeZ;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 0, gridWorldSize.z));

        if (grid != null && displayGridGizmos)
        {
            Node playerNode = NodeFromWorldPoint(player.transform.position);

            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;

                if (playerNode == n)
                    Gizmos.color = Color.cyan;

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeZ];
        Vector3 worldBottomLeft = transform.position - (Vector3.right * (gridWorldSize.x / 2)) -
            (Vector3.forward * (gridWorldSize.z / 2));

        for (int x = 0; x < gridSizeX; ++x)
            for (int z = 0; z < gridSizeZ; ++z)
            {
                Vector3 point = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(point, nodeRadius, unwalkableMask));
                grid[x, z] = new Node(walkable, point, x, z);
            }
    }
}
