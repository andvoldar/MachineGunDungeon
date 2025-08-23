// PathfindingGrid.cs
using System.Collections.Generic;
using UnityEngine;

public class PathfindingGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Tamaño del área de juego en unidades del mundo (ancho, alto).")]
    public Vector2 gridWorldSize = new Vector2(20, 12);
    [Tooltip("Radio de cada nodo (media celda).")]
    public float nodeRadius = 0.5f;
    [Tooltip("Capa que contiene los obstáculos (Collider2D).")]
    public LayerMask obstacleLayer;

    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    /// <summary>
    /// Representa un nodo en la grilla.
    /// </summary>
    public class Node
    {
        public bool walkable;
        public Vector2 worldPosition;
        public int gridX, gridY;
        public int gCost, hCost;
        public Node parent;

        public int fCost { get { return gCost + hCost; } }

        public Node(bool _walkable, Vector2 _worldPos, int _gridX, int _gridY)
        {
            walkable = _walkable;
            worldPosition = _worldPos;
            gridX = _gridX;
            gridY = _gridY;
            gCost = int.MaxValue;
            hCost = 0;
            parent = null;
        }
    }

    /// <summary>
    /// Construye (o reconstruye) la grilla de nodos.
    /// Ahora es público para que DungeonsGenerator pueda invocarlo después de generar colliders.
    /// </summary>
    public void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector2 worldBottomLeft = (Vector2)transform.position
            - Vector2.right * (gridWorldSize.x / 2)
            - Vector2.up * (gridWorldSize.y / 2);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = worldBottomLeft
                    + Vector2.right * (x * nodeDiameter + nodeRadius)
                    + Vector2.up * (y * nodeDiameter + nodeRadius);

                // Si colisiona con un collider en obstacleLayer, no es caminable
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius, obstacleLayer);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    /// <summary>
    /// Dada una posición en el mundo, devuelve el nodo correspondiente en la grilla.
    /// </summary>
    public Node NodeFromWorldPoint(Vector2 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    /// <summary>
    /// Devuelve todos los vecinos (hasta 8) de un nodo dado.
    /// </summary>
    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                if (xOffset == 0 && yOffset == 0)
                    continue;

                int checkX = node.gridX + xOffset;
                int checkY = node.gridY + yOffset;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    neighbors.Add(grid[checkX, checkY]);
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Dibuja visualmente la grilla en la escena (opcional, para debug).
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));
        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
