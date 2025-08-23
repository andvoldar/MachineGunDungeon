// Pathfinding.cs
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance;

    private PathfindingGrid grid;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        grid = GetComponent<PathfindingGrid>();
        if (grid == null)
            Debug.LogError("❌ PathfindingGrid no encontrado en el mismo GameObject que Pathfinding.");
    }

    /// <summary>
    /// Calcula el camino A* desde startWorldPos hasta targetWorldPos y devuelve
    /// una lista de posiciones (Vector2) que forman el camino (sequence de waypoints).
    /// </summary>
    public List<Vector2> FindPath(Vector2 startWorldPos, Vector2 targetWorldPos)
    {
        List<Vector2> waypoints = new List<Vector2>();

        if (grid == null)
            return waypoints; // Sin grid, devolvemos lista vacía

        var startNode = grid.NodeFromWorldPoint(startWorldPos);
        var targetNode = grid.NodeFromWorldPoint(targetWorldPos);

        // Si el nodo destino no es caminable, devolvemos lista vacía
        if (!targetNode.walkable)
            return waypoints;

        List<PathfindingGrid.Node> openSet = new List<PathfindingGrid.Node>();
        HashSet<PathfindingGrid.Node> closedSet = new HashSet<PathfindingGrid.Node>();
        openSet.Add(startNode);

        // Inicializar costes en nodo inicial
        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        startNode.parent = null;

        while (openSet.Count > 0)
        {
            PathfindingGrid.Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                waypoints = RetracePath(startNode, targetNode);
                break;
            }

            foreach (var neighbor in grid.GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return waypoints;
    }

    /// <summary>
    /// Reconstruye el camino de nodos desde endNode hasta startNode, invierte la lista
    /// y devuelve posiciones de mundo.
    /// </summary>
    private List<Vector2> RetracePath(PathfindingGrid.Node startNode, PathfindingGrid.Node endNode)
    {
        List<PathfindingGrid.Node> nodePath = new List<PathfindingGrid.Node>();
        PathfindingGrid.Node currentNode = endNode;

        while (currentNode != startNode)
        {
            nodePath.Add(currentNode);
            currentNode = currentNode.parent;
        }
        nodePath.Reverse();

        List<Vector2> waypoints = new List<Vector2>();
        foreach (var node in nodePath)
        {
            waypoints.Add(node.worldPosition);
        }
        return waypoints;
    }

    /// <summary>
    /// Calcula la distancia “manhattan diagonal” entre dos nodos.
    /// </summary>
    private int GetDistance(PathfindingGrid.Node a, PathfindingGrid.Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
