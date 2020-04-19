using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TMP_Text _scoreText;

    public float TimeScore { get; set; }
    public int FoodScore { get; private set; }
    public bool Counting { get; set; } = true;

    public int Score => Mathf.RoundToInt(TimeScore) + FoodScore;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if (Counting) {
            TimeScore += Time.deltaTime;
        }
        _scoreText.text = $"Score: {Score}";
    }

    public void NotifyFoodCollected() {
        if (!Counting) {
            return;
        }

        FoodScore += 30;
    }
}
