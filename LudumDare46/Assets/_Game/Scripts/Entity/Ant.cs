﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ant : MonoBehaviour {
    readonly private int surface = -2;

    [SerializeField] private Sprite _builderSprite;
    [SerializeField] private Sprite _foragerSprite;
    [SerializeField] private Sprite _soldierSprite;

    [SerializeField] private float _movementSpeed;
    [SerializeField] private AntType _type;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private Vector3 _idlePosition;
    private Coroutine _currentCoroutine;

    public AntType Type { get { return _type; } set { _type = value; UpdateSprite(); } }
    public bool Alive => Health > 0;
    public int Health { get; set; } = 4;
    public int DamagePower { get; set; } = 1;

    private Vector2Int _lastTurtlePosition;

    public EnemyAnt FightTarget { get; set; }
    public Food CarryingFood { get; set; }

    private void Start() {
        AntManager.Instance.Register(this);

        if (Type == AntType.Soldier) {
            Health = 10;
            DamagePower = 2;
        }

        if (Type == Ant.AntType.Forager) {
            _movementSpeed = 4f;
        }

        Work();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        var ant = collision.transform.GetComponent<EnemyAnt>();
        if (ant == null) {
            return;
        }

        if (FightTarget != null) {
            return; // first finish this fight
        }

        StopAllCoroutines();
        _currentCoroutine = null;
        StartCoroutine(FightSequence(ant));
    }

    private void UpdateSprite() {
        var sprite = _builderSprite;

        switch (Type) {
            case AntType.Builder:
                sprite = _builderSprite;
                break;
            case AntType.Forager:
                sprite = _foragerSprite;
                break;
            case AntType.Soldier:
                sprite = _soldierSprite;
                break;
        }

        _spriteRenderer.sprite = sprite;
    }

    public void Fight(EnemyAnt enemy) {
        if (FightTarget != null) {
            return; // first finish other fight
        }

        StopAllCoroutines();
        _currentCoroutine = StartCoroutine(FightSequence(enemy));
    }

    public void StopCurrent() {
        if (_currentCoroutine != null) {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }
    }

    public void Damage(int amount) {
        Health -= amount;

        if (!Alive) {
            StopAllCoroutines();
            _spriteRenderer.DOFade(0, .85f).OnComplete(() => {
                AntManager.Instance.Deregister(this);
                Destroy(gameObject);
            });
        }
    }

    public void BuildAt(Vector2Int position, RoomType constructionType) {
        StopCurrent();

        var pathFinder = new PathFinder(TileMapManager.Instance.GroundTilemap);
        var path = pathFinder.FindShortestPath(Vector2Int.RoundToInt(transform.position), position);
        _currentCoroutine = StartCoroutine(BuildAtSequence(position, path, constructionType));
    }

    private void Work() {
        if (_currentCoroutine != null) {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }

        switch (_type) {
            case AntType.Builder:
                _idlePosition = transform.position;

                var constructionSites = RoomManager.Instance.ConstructionSites.ToList();
                if (constructionSites.Count > 0) {
                    var site = constructionSites[0];
                    _currentCoroutine = StartCoroutine(ContinueBuildAtSequence(site));
                    return;
                }

                _currentCoroutine = StartCoroutine(TurtleTunnelCoroutine(25));
                break;
            case AntType.Forager:
                _currentCoroutine = StartCoroutine(ForageCoroutine());
                break;
            case AntType.Soldier:
                _currentCoroutine = StartCoroutine(SoldierCoroutine());
                break;

            default:
                break;
        }
    }

    private IEnumerator FightSequence(EnemyAnt enemy) {
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

        Work();
    }

    private IEnumerator SoldierCoroutine() {
        var start = AntManager.Instance.ForagerStart;
        yield return PathFindTo((Vector2Int)start);

        Vector3Int currentDirection = Vector3Int.down;
        bool canMoveTowardsDirection = false;
        Vector3Int targetPos = Vector3Int.RoundToInt(transform.position);

        while (true) {
            //var enemies = EnemyManager.Instance.Enemies;
            //if (enemies.Count > 0) {
            //    var enemyClosestToQueen = 
            //}

            while (!canMoveTowardsDirection) {
                targetPos = Vector3Int.RoundToInt(transform.position) + currentDirection;
                canMoveTowardsDirection = TileMapManager.Instance.GroundTilemap.GetTile(targetPos) == null;

                if (!canMoveTowardsDirection || transform.position.y >= -4) {
                    if (transform.position.y >= -4) {
                        currentDirection = Vector3Int.down;
                    } else {
                        currentDirection = GetRandomDirection();
                    }

                    targetPos = Vector3Int.RoundToInt(transform.position) + currentDirection;
                    canMoveTowardsDirection = TileMapManager.Instance.GroundTilemap.GetTile(targetPos) == null;

                    yield return null;
                }
            }

            yield return MoveTo(targetPos);
            canMoveTowardsDirection = false;
        }
    }

    private Vector3Int GetRandomDirection() {
        var ran = Random.Range(0, 4);
        switch (ran) {
            case 0:
                return Vector3Int.up;
            case 1:
                return Vector3Int.down;
            case 2:
                return Vector3Int.left;
            case 3:
                return Vector3Int.right;
            default:
                return Vector3Int.down;
        }
    }

    private IEnumerator ForageCoroutine() {
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

        if (CarryingFood != null) {
            // Drop off food
            var storageRooms = RoomManager.Instance.FoodStorageRooms.ToList();
            if (storageRooms.Count != 0) {
                var storageRoom = storageRooms[Random.Range(0, storageRooms.Count)];
                yield return PathFindTo(Vector2Int.RoundToInt(storageRoom.transform.position), speedMult: .5f);

                yield return new WaitForSeconds(1f);

                storageRoom.AddFood(CarryingFood);
                CarryingFood = null;

                yield return new WaitForSeconds(3f);
            }

            Work();
            yield break;
        }

        var foods = FoodManager.Instance.SurfaceFoods.ToList();
        if (foods.Count == 0) {
            Work();
            yield break;
        }

        //var closestFood = foods.Aggregate((closest, curr) =>
        //    (curr == null || curr.CurrentState != Food.State.Surface || (curr.transform.position.x * direction.x < 0 && curr.transform.position.x * direction.x > closest.transform.position.x) ? closest : curr));
        var randomFood = foods[Random.Range(0, foods.Count)];
        if (randomFood == null) {
            Work();
            yield break;
        } else {
            var foodPosition = Vector3Int.RoundToInt(randomFood.transform.position);
            var holeOffset = Random.Range(-3, 3);
            var targetPosition = Vector3Int.RoundToInt(foodPosition + Vector3.right * holeOffset);

            yield return MoveTo(new Vector3Int(targetPosition.x, Mathf.RoundToInt(transform.position.y), 0), true, .8f); // Dig horizontally
            yield return MoveTo(targetPosition, true, .8f); // Dig up

            // Look around
            for (var i = 0; i < 4; i++) {
                _spriteRenderer.flipX = !_spriteRenderer.flipX;
                yield return new WaitForSeconds(.5f);
            }

            _spriteRenderer.flipX = transform.position.x > randomFood.transform.position.x; // Face the food

            yield return new WaitForSeconds(1f);

            yield return MoveTo(foodPosition, true); // Move to Food

            yield return new WaitForSeconds(2f); // TODO: play smikkel animation

            // Carry piece of food to storage
            var storageRooms = RoomManager.Instance.FoodStorageRooms.ToList();
            if (storageRooms.Count == 0) {
                Work();
            } else {
                if (randomFood.CurrentState != Food.State.Surface) {
                    Work();
                } else {
                    randomFood.PickUp(this);
                    CarryingFood = randomFood;

                    yield return new WaitForSeconds(2f);

                    var storageRoom = storageRooms[Random.Range(0, storageRooms.Count)];
                    yield return PathFindTo(Vector2Int.RoundToInt(storageRoom.transform.position), speedMult: .5f);

                    yield return new WaitForSeconds(1f);

                    storageRoom.AddFood(randomFood);
                    CarryingFood = null;

                    yield return new WaitForSeconds(3f);

                    Work();
                }
            }
        }
    }

    private IEnumerator TurtleTunnelCoroutine(int tiles) {
        if (_lastTurtlePosition != default) {
            yield return PathFindTo(_lastTurtlePosition);
        } else {
            yield return PathFindTo((Vector2Int)AntManager.Instance.ForagerStart);
        }

        var tilesLeft = tiles;
        var turtleDirection = Vector3.down;
        var nextTurtleTarget = transform.position + turtleDirection;

        while (tilesLeft > 0) {
            if (transform.position.y <= -28) {
                yield return PathFindTo((Vector2Int)AntManager.Instance.ForagerStart);
                _lastTurtlePosition = (Vector2Int)AntManager.Instance.ForagerStart;
                Work();
                yield break;
            }

            var digSpeed = _movementSpeed;
            if (transform.position.y < -5) {
                digSpeed = _movementSpeed / 4;
            } else if (transform.position.y < -10) {
                digSpeed = _movementSpeed / 6;
            } else if (transform.position.y < -20) {
                digSpeed = _movementSpeed / 8;
            } else if (transform.position.y < -25) {
                digSpeed = _movementSpeed / 10;
            } else if (transform.position.y < -30) {
                digSpeed = _movementSpeed / 20;
            } else if (transform.position.y < -32) {
                digSpeed = _movementSpeed / 48;
            }

            while (Vector3.Distance(transform.position, nextTurtleTarget) > .05f) {
                var directionToTarget = (nextTurtleTarget - transform.position).normalized;
                transform.Translate(directionToTarget * digSpeed * Time.deltaTime);

                _spriteRenderer.flipX = directionToTarget.x < 0;

                TileMapManager.Instance.GroundTilemap.SetTile(Vector3Int.RoundToInt(transform.position), null);

                yield return null;
            }

            _lastTurtlePosition = Vector2Int.RoundToInt(transform.position);
            transform.position = nextTurtleTarget;

            turtleDirection = GetNewTurtleDirection(turtleDirection);
            nextTurtleTarget = transform.position + turtleDirection;

            tilesLeft--;
        }

        // Walk back to surface
        var pathFinder = new PathFinder(TileMapManager.Instance.GroundTilemap);
        var path = pathFinder.FindShortestPath(Vector2Int.RoundToInt(transform.position), (Vector2Int)AntManager.Instance.ForagerStart);

        yield return WalkPath(path);

        Work();
    }

    private IEnumerator BuildAtSequence(Vector2Int targetPosition, List<Vector2Int> path, RoomType constructionType) {
        var constructionSite = ConstructionManager.Instance.BuildConstructionSite(targetPosition, constructionType);
        constructionSite.Worker = this;

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

        if (constructionSite != null && constructionSite.gameObject.activeInHierarchy) {
            constructionSite.Complete();
        }

        yield return new WaitForSeconds(.5f);

        Work();
    }

    private IEnumerator ContinueBuildAtSequence(ConstructionSite site) {
        yield return PathFindTo(site.Position);

        var tileMap = TileMapManager.Instance.GroundTilemap;
        for (var i = -1; i <= 1; i++) {
            for (var j = -1; j <= 1; j++) {
                if (i == 0 && j == 0) {
                    continue;
                }

                if (site == null || !site.gameObject.activeInHierarchy) {
                    Work();
                    yield break;
                }

                var tilePos = new Vector3Int(i + site.Position.x, j + site.Position.y, 0);
                var tile = tileMap.GetTile(tilePos);
                if (tile == null) {
                    continue;
                }

                yield return WalkPath(new List<Vector2Int> { (Vector2Int)tilePos });

                tileMap.SetTile(tilePos, null);

                yield return WalkPath(new List<Vector2Int> { site.Position });
            }
        }

        if (site != null && site.gameObject.activeInHierarchy) {
            site.Complete();
        }

        yield return new WaitForSeconds(.5f);

        Work();
    }

    private IEnumerator PathFindTo(Vector2Int position, float speedMult = 1) {
        var pathFinder = new PathFinder(TileMapManager.Instance.GroundTilemap);
        var path = pathFinder.FindShortestPath(Vector2Int.RoundToInt(transform.position), position);

        yield return StartCoroutine(WalkPath(path, speedMult: speedMult));
    }

    private IEnumerator WalkPath(List<Vector2Int> path, bool dig = false, float speedMult = 1) {
        Vector3 nextTargetPosition;
        var pathIndex = 0;
        while (pathIndex < path.Count) {
            nextTargetPosition = new Vector3(path[pathIndex].x, path[pathIndex].y);
            while (Vector3.Distance(transform.position, nextTargetPosition) > .05f) {
                var directionToTarget = (nextTargetPosition - transform.position).normalized;
                transform.Translate(directionToTarget * _movementSpeed * speedMult * Time.deltaTime);

                _spriteRenderer.flipX = directionToTarget.x < 0;

                if (dig) {
                    TileMapManager.Instance.GroundTilemap.SetTile(Vector3Int.RoundToInt(transform.position), null);
                }

                yield return null;
            }

            pathIndex++;
        }
    }

    private IEnumerator MoveTo(Vector3Int position, bool dig = false, float speedMult = 1) {
        while (Vector3.Distance(transform.position, position) > .05f) {
            var directionToTarget = (position - transform.position).normalized;
            transform.Translate(directionToTarget * _movementSpeed * speedMult * Time.deltaTime);

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
        Forager = 1,
        Soldier = 2
    }
}
