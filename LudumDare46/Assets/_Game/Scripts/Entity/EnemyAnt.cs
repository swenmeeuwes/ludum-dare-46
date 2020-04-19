using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnt : MonoBehaviour {
    [SerializeField] private float _movementSpeed;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public int DamagePower { get; set; } = 2;
    public bool Alive => Health > 0;
    public int Health { get; set; } = 10;

    public Ant FightTarget { get; set; }

    private void Start() {
        StartCoroutine(AttackQueen());
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        var ant = collision.transform.GetComponent<Ant>();
        if (ant == null) {
            return;
        }

        if (FightTarget != null) {
            return; // first finish this fight
        }

        StopAllCoroutines();
        StartCoroutine(FightSequence(ant));
    }

    public void Damage(int amount) {
        Health -= amount;

        if (!Alive) {
            StopAllCoroutines();
            _spriteRenderer.DOFade(0, .85f).OnComplete(() => {
                EnemyManager.Instance.Enemies.Remove(this);
                Destroy(gameObject);
            });
        }
    }

    private IEnumerator FightSequence(Ant enemy) {
        FightTarget = enemy;

        var startPos = transform.position;
        var enemyDeltaPos = enemy.transform.position - transform.position;

        _spriteRenderer.flipX = enemyDeltaPos.x < 0;

        while (enemy.Alive && Alive) {
            yield return transform.DOPunchPosition(enemyDeltaPos.normalized, .35f, vibrato: 0, elasticity: 0).WaitForCompletion();
            enemy.Damage(DamagePower);

            yield return new WaitForSeconds(1.2f);
            yield return transform.DOMove(startPos, .35f).WaitForCompletion();
        }

        FightTarget = null;

        yield return AttackQueen();
    }

    public IEnumerator AttackQueen() {
        while (Alive) {
            var queenPos = QueenAnt.Instance.transform.position;
            if (Vector3.Distance(queenPos, transform.position) < 1.5f) {
                var queenDeltaPos = queenPos - transform.position;
                yield return transform.DOPunchPosition(queenDeltaPos.normalized, .35f, vibrato: 0, elasticity: 0).WaitForCompletion();
                QueenAnt.Instance.Damage(DamagePower);
                yield return new WaitForSeconds(1f);
            } else {
                var pathFinder = new PathFinder(TileMapManager.Instance.GroundTilemap);
                var path = pathFinder.FindShortestPath(Vector2Int.RoundToInt(transform.position), Vector2Int.RoundToInt(queenPos));
                path.RemoveAt(path.Count - 1);

                yield return WalkPath(path);
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
