using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodManager : MonoBehaviour {
    [SerializeField] private Food _foodPrefab;
    [SerializeField] private int _maxFoodItems = 5;
    [SerializeField] private int _foodSpawnInterval = 30;

    public static FoodManager Instance { get; set; }

    public List<Food> Foods { get; } = new List<Food>();
    public IEnumerable<Food> SurfaceFoods => Foods.Where(f => f.CurrentState == Food.State.Surface);

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        StartCoroutine(SpawnFruitSequence());
    }

    private IEnumerator SpawnFruitSequence() {
        while (true) {
            yield return new WaitForSeconds(_foodSpawnInterval);

            if (Foods.Count(f => f.CurrentState == Food.State.Surface) < _maxFoodItems) {
                SpawnFruitAtRandomLocation();
            }
        }
    }

    private void SpawnFruitAtRandomLocation() {
        var fruit = Instantiate(_foodPrefab);

        var ranX = Random.value > .5f ? Random.Range(3, 23) : Random.Range(-3, -23);
        fruit.transform.position = new Vector3(ranX, -2, 0);
    }

    public void Register(Food food) {
        Foods.Add(food);
    }

    public void Deregister(Food food) {
        Foods.Remove(food);
    }
}
