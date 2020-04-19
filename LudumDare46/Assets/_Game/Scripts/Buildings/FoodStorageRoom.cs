using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FoodStorageRoom : Room {
    public List<Food> Food { get; private set; } = new List<Food>();

    public void AddFood(Food food) {
        food.transform.SetParent(transform, true);

        food.transform.DOMove(transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0), .35f);

        Food.Add(food);
    }
}
