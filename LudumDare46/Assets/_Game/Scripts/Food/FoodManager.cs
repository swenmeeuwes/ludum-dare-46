using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour {
    public static FoodManager Instance { get; set; }

    private List<Food> _surfaceFoods = new List<Food>();

    public List<Food> SurfaceFoods => _surfaceFoods;

    private void Awake() {
        Instance = this;
    }

    public void Register(Food food) {
        _surfaceFoods.Add(food);
    }

    public void Deregister(Food food) {
        _surfaceFoods.Remove(food);
    }
}
