using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QueenAnt : MonoBehaviour {
    [SerializeField] private Egg _eggPrefab;

    [SerializeField] private SpriteRenderer _wings;

    [SerializeField] private float _movementSpeed;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public SpriteRenderer SpriteRenderer => _spriteRenderer;
    public int Health { get; set; } = 10;

    public static QueenAnt Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public void RemoveWings() {
        _wings.DOFade(0, .65f);
        _wings.transform.DOMoveY(-.25f, .65f);
    }

    public void MoveToQueenRoomIfAvailable() {
        StopAllCoroutines();

        var queenRooms = RoomManager.Instance.QueensRooms.ToList();
        if (queenRooms.Count == 0) {
            StartCoroutine(PathFindTo((Vector2Int)AntManager.Instance.ForagerStart));
        } else {
            var room = queenRooms[Random.Range(0, queenRooms.Count)];
            StartCoroutine(PathFindTo(room.Position));
        }
    }

    public void Damage(int amount) {
        CameraManager.Instance.NotifyQueenDamage();

        Health -= amount;

        if (Health <= 0) {
            // GAME OVER
            CameraManager.Instance.MoveToQueen(true);
            _spriteRenderer.DOFade(0, .95f).SetDelay(.5f);

            GameManager.Instance.GameOver();
        }
    }

    public void RequestLayEggs() {
        StopAllCoroutines();
        StartCoroutine(LayEggsSequence());
    }

    private IEnumerator LayEggsSequence() {
        var emptyNurseryRooms = RoomManager.Instance.NurseryRooms.Where(r => r.Eggs.Count == 0).ToList();
        while (emptyNurseryRooms.Count > 0) {
            var room = emptyNurseryRooms[Random.Range(0, emptyNurseryRooms.Count)];
            yield return PathFindTo(room.Position);

            yield return LayEggs(Random.Range(1, 4), room); // 1-3 eggs (4 = excl)

            emptyNurseryRooms = RoomManager.Instance.NurseryRooms.Where(r => r.Eggs.Count == 0).ToList();
        }

        MoveToQueenRoomIfAvailable();
        ActionManager.Instance.UpdateLayEggs();
    }

    private IEnumerator LayEggs(int amount, NurseryRoom room) {
        var startPos = transform.position;

        yield return new WaitForSeconds(1f);

        for (var i = 0; i < amount; i++) {
            yield return transform.DOMove(startPos + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0), .35f);
            yield return new WaitForSeconds(1f);
            var egg = CreateEggAtPosition();
            room.AddEgg(egg);
            yield return new WaitForSeconds(1f);
            yield return transform.DOMove(startPos, .35f);
        }
    }

    public Egg CreateEggAtPosition() {
        var eggObj = Instantiate(_eggPrefab);
        eggObj.transform.position = transform.position;

        return eggObj.GetComponent<Egg>();
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
