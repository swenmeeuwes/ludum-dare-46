using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TMP_Text _scoreText;

    public float TimeScore { get; set; }
    public int FoodScore => FoodManager.Instance.Foods.Count(f => f.CurrentState == Food.State.InStorage) * 30;
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
}
