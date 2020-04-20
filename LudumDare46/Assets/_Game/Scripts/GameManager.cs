using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    [SerializeField] private TMP_Text[] _introTexts;

    [SerializeField] private TMP_Text _hailQueenText;
    [SerializeField] private TMP_Text _instructionText;
    [SerializeField] private CanvasGroup _gameOverPanel;

    [SerializeField] private Button _restartButton;

    public static GameManager Instance { get; set; }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        foreach (var item in _introTexts) {
            item.alpha = 0;
        }

        _hailQueenText.alpha = 0;
        _instructionText.alpha = 0;
        _gameOverPanel.alpha = 0;

        _restartButton.interactable = false;

        StartCoroutine(StartGameSequence());
    }

    public void GameOver() {
        _gameOverPanel.DOFade(1, .95f).SetDelay(1.5f);
        ScoreManager.Instance.Counting = false;
        _restartButton.interactable = true;
    }

    public void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // RESTART
    }

    private IEnumerator StartGameSequence() {
        var queenAnt = QueenAnt.Instance;
        queenAnt.transform.position = new Vector3(-20, 10, 0);

        yield return new WaitForSeconds(2f);

        foreach (var introText in _introTexts) {
            yield return introText.DOFade(1, .65f).WaitForCompletion();
            yield return new WaitForSeconds(2f);
            yield return introText.DOFade(0, .65f).WaitForCompletion();
            yield return new WaitForSeconds(.5f);
        }

        _hailQueenText.DOFade(1, .65f).SetDelay(4f);
        yield return queenAnt.transform.DOMove(new Vector3(-2, -2, 0), 5f).WaitForCompletion();
        _hailQueenText.DOFade(0, .65f).SetDelay(1f);

        yield return new WaitForSeconds(2f);
        queenAnt.RemoveWings();
        yield return new WaitForSeconds(1f);

        queenAnt.CreateEggAtPosition().SecondsUntilHatch = 1f;

        yield return new WaitForSeconds(1f);

        _instructionText.DOFade(1, .65f);

        yield return queenAnt.transform.DOMove(new Vector3(0, -2, 0), .45f).WaitForCompletion();
        yield return new WaitForSeconds(.85f);

        QueenAnt.Instance.SpriteRenderer.flipX = true; // look at egg

        yield return new WaitForSeconds(1.2f);
        _instructionText.DOFade(0, .65f);
    }
}
