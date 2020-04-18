using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ant : MonoBehaviour {
    readonly private int surface = -2;

    [SerializeField] private float _movementSpeed;
    [SerializeField] private AntType _type;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private Vector3 _idlePosition;
    private Coroutine _currentCoroutine;

    public AntType Type => _type;

    private void Start() {
        AntManager.Instance.Register(this);

        Work();
    }

    public void BuildAt(Vector2Int position, RoomType constructionType) {
        if (_currentCoroutine != null) {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }

        var pathFinder = new PathFinder(TileMapManager.Instance.GroundTilemap);
        var path = pathFinder.FindShortestPath(Vector2Int.RoundToInt(transform.position), position);
        _currentCoroutine = StartCoroutine(BuildAtSequence(position, path, constructionType));
    }

    private void Work() {
        switch (_type) {
            case AntType.Builder:
                _idlePosition = transform.position;
                _currentCoroutine = StartCoroutine(TurtleTunnelCoroutine(20));
                break;
            case AntType.Forager:
                _currentCoroutine = StartCoroutine(ForageCoroutine());
                break;

            default:
                break;
        }
    }

    private IEnumerator ForageCoroutine() {
        var direction = Random.Range(0, 2) == 0 ? Vector3.left : Vector3.right;

        // First go to forager start
        var start = AntManager.Instance.ForagerStart;

        var pathFinder = new PathFinder(TileMapManager.Instance.GroundTilemap);
        var path = pathFinder.FindShortestPath(Vector2Int.RoundToInt(transform.position), (Vector2Int)start);

        if (path.Count == 0) {
            // Force our way through
            yield return MoveTo(start, true);
        } else {
            yield return WalkPath(path);
        }

        var foods = FoodManager.Instance.SurfaceFoods;
        var closestFood = foods.Aggregate((closest, curr) => 
            (curr == null || (curr.transform.position.x * direction.x < 0 && curr.transform.position.x * direction.x > closest.transform.position.x) ? closest : curr));

        var foodPosition = Vector3Int.RoundToInt(closestFood.transform.position);
        var holeOffset = Random.Range(-3, 3);
        var targetPosition = Vector3Int.RoundToInt(foodPosition + Vector3.right * holeOffset);

        yield return MoveTo(new Vector3Int(targetPosition.x, Mathf.RoundToInt(transform.position.y), 0), true); // Dig horizontally
        yield return MoveTo(targetPosition, true); // Dig up
        yield return MoveTo(foodPosition, true); // Move to Food

        yield return new WaitForSeconds(2f); // TODO: play smikkel animation

        // TODO: Carry piece of food to storage
        var storageRooms = RoomManager.Instance.FoodStorageRooms.ToList();
        if (storageRooms.Count == 0) {
            Work();
        } else {
            var storageRoom = storageRooms[Random.Range(0, storageRooms.Count)];
            yield return PathFindTo(Vector2Int.RoundToInt(storageRoom.transform.position));

            Work();
        }
    }

    private IEnumerator TurtleTunnelCoroutine(int tiles) {
        var tilesLeft = tiles;
        var turtleDirection = Vector3.down;
        var nextTurtleTarget = transform.position + turtleDirection * 5f;

        while (tilesLeft > 0) {
            while (Vector3.Distance(transform.position, nextTurtleTarget) > .05f) {
                var directionToTarget = (nextTurtleTarget - transform.position).normalized;
                transform.Translate(directionToTarget * _movementSpeed * Time.deltaTime);

                _spriteRenderer.flipX = directionToTarget.x < 0;

                TileMapManager.Instance.GroundTilemap.SetTile(Vector3Int.RoundToInt(transform.position), null);

                yield return null;
            }

            transform.position = nextTurtleTarget;

            turtleDirection = GetNewTurtleDirection(turtleDirection);
            nextTurtleTarget = transform.position + turtleDirection;

            tilesLeft--;
        }

        // Walk back to surface
        var pathFinder = new PathFinder(TileMapManager.Instance.GroundTilemap);
        var path = pathFinder.FindShortestPath(Vector2Int.RoundToInt(transform.position), Vector2Int.RoundToInt(_idlePosition));
        
        yield return WalkPath(path);
    }

    private IEnumerator BuildAtSequence(Vector2Int targetPosition, List<Vector2Int> path, RoomType constructionType) {
        yield return WalkPath(path);

        var tileMap = TileMapManager.Instance.GroundTilemap;
        for (var i = -1; i <= 1; i++) {
            for (var j = -1; j <= 1; j++) {
                if (i == 0 && j == 0) {
                    continue;
                }

                var tilePos = new Vector3Int(i + targetPosition.x, j + targetPosition.y, 0);
                var tile = tileMap.GetTile(tilePos);
                if (tile == null) {
                    continue;
                }

                yield return WalkPath(new List<Vector2Int> { (Vector2Int)tilePos });

                tileMap.SetTile(tilePos, null);

                yield return WalkPath(new List<Vector2Int> { targetPosition });
            }
        }

        ConstructionManager.Instance.BuildAt(targetPosition, constructionType);
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

    private Vector3 GetNewTurtleDirection(Vector3 previousDirection) {
        var ran = Random.Range(0, 5); // max = exclusive
        if (ran == 0 && previousDirection != Vector3.right) {
            _spriteRenderer.flipX = true;
            return Vector3.left;
        } else if (ran == 1 && previousDirection != Vector3.left) {
            _spriteRenderer.flipX = false;
            return Vector3.right;
        }

        // else move down
        return Vector3.down;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(Vector3Int.RoundToInt(transform.position), .5f);
    }

    public enum AntType {
        Builder = 0,
        Forager = 1
    }
}
