using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnt : MonoBehaviour {
    [SerializeField] private float _movementSpeed;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public bool Alive { get; set; } = true;

    private void Start() {
        StartCoroutine(AttackQueen());
    }

    public IEnumerator AttackQueen() {
        while (Alive) {
            if (Vector3.Distance(QueenAnt.Instance.transform.position, transform.position) < .1f) {
                yield return new WaitForSeconds(1f);
            } else {
                yield return PathFindTo(Vector2Int.RoundToInt(QueenAnt.Instance.transform.position));
            }
        }
    }

    private IEnumerator PathFindTo(Vector2Int position) {
        var pathFinder = new PathFinder(TileMapManager.Instance.GroundTilemap);
        var path = pathFinder.FindShortestPath(Vector2Int.RoundToInt(transform.position), position);

        yield return StartCoroutine(WalkPath(path));
    }

    private IEnumerator WalkPath(List<Vector2Int> path, bool dig = false) {
        Vector3 nextTargetPosition;
        var pathIndex = 0;
        while (pathIndex < path.Count) {
            nextTargetPosition = new Vector3(path[pathIndex].x, path[pathIndex].y);
            while (Vector3.Distance(transform.position, nextTargetPosition) > .05f) {
                var directionToTarget = (nextTargetPosition - transform.position).normalized;
                transform.Translate(directionToTarget * _movementSpeed * Time.deltaTime);

                _spriteRenderer.flipX = directionToTarget.x < 0;

                if (dig) {
                    TileMapManager.Instance.GroundTilemap.SetTile(Vector3Int.RoundToInt(transform.position), null);
                }

                yield return null;
            }

            pathIndex++;
        }
    }

    private IEnumerator MoveTo(Vector3Int position, bool dig = false) {
        while (Vector3.Distance(transform.position, position) > .05f) {
            var directionToTarget = (position - transform.position).normalized;
            transform.Translate(directionToTarget * _movementSpeed * Time.deltaTime);

            _spriteRenderer.flipX = directionToTarget.x < 0;

            if (dig) {
                TileMapManager.Instance.GroundTilemap.SetTile(Vector3Int.RoundToInt(transform.position), null);
            }

            yield return null;
        }
    }
}
