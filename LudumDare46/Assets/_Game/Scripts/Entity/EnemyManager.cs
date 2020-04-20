using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyAnt _enemyAntPrefab;

    public static EnemyManager Instance { get; set; }

    public List<EnemyAnt> Enemies { get; set; } = new List<EnemyAnt>();

    public int Wave { get; set; } = 1;
    public float NextInterval { get; set; } = 40;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        StartCoroutine(SpawnEnemySequence());
    }

    private IEnumerator SpawnEnemySequence() {
        yield return new WaitForSeconds(50); // start

        while (true) {
            yield return new WaitForSeconds(NextInterval + Random.Range(0, 10));

            CameraManager.Instance.MovedToQueenThisWave = false;
            QueenAnt.Instance.RestoreHP();

            var amountOfEnemies = Mathf.CeilToInt(Wave / 2f);
            for (int i = 0; i < amountOfEnemies; i++) {
                SpawnEnemy();
                yield return new WaitForSeconds(.85f);
            }

            Wave++;
        }
    }

    public void SpawnEnemy() {
        var enemy = Instantiate(_enemyAntPrefab);
        enemy.transform.position = new Vector3(Random.value > .5f ? -25 : 25, -2, 0);

        Enemies.Add(enemy);
    }
}
