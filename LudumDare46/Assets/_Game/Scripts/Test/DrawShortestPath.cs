using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DrawShortestPath : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private Transform _start;
    [SerializeField] private Transform _end;

    private List<Vector2Int> _path;
    private float _startTime;

    private void Start() {
        _startTime = Time.time;
        Perform();
    }

    private void Perform() {
        var pathFinder = new PathFinder(_tilemap);

        
        _path = pathFinder.FindShortestPath(Vector2Int.RoundToInt(_start.position), Vector2Int.RoundToInt(_end.position));

        Debug.Log("Time taken: " + (Time.time - _startTime));
    }

    private void OnDrawGizmos() {
        if (_path == null) {
            return;
        }

        Gizmos.color = Color.red;
        foreach (var item in _path) {
            Gizmos.DrawSphere(new Vector3(item.x, item.y), .25f);
        }
    }
}
