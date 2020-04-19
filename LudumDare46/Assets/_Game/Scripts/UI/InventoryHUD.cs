using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryHUD : MonoBehaviour {
    public static InventoryHUD Instance { get; private set; }

    [SerializeField] private TMP_Text _eggsAmount;
    [SerializeField] private TMP_Text _foodAmount;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        UpdateEggCount();
        UpdateFoodCount();
    }

    private void UpdateEggCount() {
        var eggs = EggManager.Instance.Eggs.Count;
        var hatchableEggs = EggManager.Instance.EggsThatCanBeHatched.Count();

        _eggsAmount.text = $"{hatchableEggs}/{eggs}";
    }

    private void UpdateFoodCount() {
        var storageRooms = RoomManager.Instance.FoodStorageRooms;
        var food = storageRooms.Sum(r => r.Food.Count);
        _foodAmount.text = food.ToString();
    }
}
