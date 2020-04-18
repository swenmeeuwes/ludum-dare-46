using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathFinder {
    public Tilemap Tilemap { get; private set; }

    public PathFinder(Tilemap tilemap) {
        Tilemap = tilemap;
    }

    public List<Vector2Int> FindShortestPath(Vector2Int startPosition, Vector2Int endPosition, int maxDepth = 10000) {
        var map = new Dictionary<Vector2Int, Node>();
        var open = new List<Node>();
        var closed = new List<Node>();

        open.Add(new Node(startPosition, startPosition, endPosition));

        var depth = 0;
        while (depth < maxDepth) {
            var current = open.Aggregate((currentNode, n) => (currentNode == null || n.FCost < currentNode.FCost ? n : currentNode));

            open.Remove(current);
            closed.Add(current);

            if (current.Position == endPosition) {
                return ConstructPathByTracingBack(current);
            }

            var neighbours = GetWalkableNeighboursFor(current, startPosition, endPosition, ref map);
            foreach (var neighbour in neighbours) {
                if (closed.Contains(neighbour)) {
                    continue;
                }

                var neighbourInOpenList = open.FirstOrDefault(n => n == neighbour);
                if (neighbourInOpenList == null) {
                    // not in open list
                    neighbour.Predecessor = current;
                    open.Add(neighbour);
                } else {
                    // neighbour is in open list, update it's cost if it is lower
                    if (current.GCost + 1 < neighbourInOpenList.GCost) {
                        neighbourInOpenList.GCost = current.GCost + 1;
                        neighbourInOpenList.Predecessor = current;
                    }
                }
            }

            depth++;
        }

        return new List<Vector2Int>();
    }

    private List<Vector2Int> ConstructPathByTracingBack(Node node) {
        var path = new List<Vector2Int>();

        var currentNode = node;
        while (currentNode.Predecessor != null) {
            path.Add(currentNode.Position);
            currentNode = currentNode.Predecessor;
        }

        path.Reverse();

        return path;
    }

    private bool CanMoveTo(Vector3Int position) {
        var tile = Tilemap.GetTile(position);
        return tile == null;
    }

    private List<Node> GetWalkableNeighboursFor(Node node, Vector2Int startPosition, Vector2Int endPosition, ref Dictionary<Vector2Int, Node> map) {
        var directions = new List<Vector2> {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right,
        };

        var neighbours = new List<Node>();
        foreach (var direction in directions) {
            var neighbourPosition = Vector2Int.RoundToInt(node.Position + direction);

            Node neighbour;
            if (!map.TryGetValue(neighbourPosition, out neighbour)) {

                // Create new node
                if (!CanMoveTo((Vector3Int)neighbourPosition)) {
                    continue;
                }

                var deltaEnd = neighbourPosition - (Vector2Int)endPosition;
                neighbour = new Node {
                    Position = neighbourPosition,
                    GCost = node.GCost + 1,
                    HCost = Mathf.Abs(deltaEnd.x) + Mathf.Abs(deltaEnd.y),
                    Predecessor = node
                };

                map.Add(neighbourPosition, neighbour);
            }

            neighbours.Add(neighbour);
        }

        return neighbours;
    }

    public class Node {
        public int GCost { get; set; } // Distance from starting node
        public int HCost { get; set; } // Distance from end node (heuristic)
        public int FCost => GCost + HCost;

        public Vector2Int Position { get; set; }
        public Node Predecessor { get; set; }

        public Node() {

        }

        public Node(Vector2Int position, Vector2Int startPosition, Vector2Int endPosition, Node predecessor = null) {
            var deltaStart = position - startPosition;
            var deltaEnd = position - endPosition;

            Predecessor = predecessor;
            Position = position;

            GCost = Mathf.Abs(deltaStart.x) + Mathf.Abs(deltaStart.y);
            HCost = Mathf.Abs(deltaEnd.x) + Mathf.Abs(deltaEnd.y);
        }
    }
}
