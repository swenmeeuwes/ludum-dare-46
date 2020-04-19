using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; set; }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence() {
        var queenAnt = QueenAnt.Instance;
        queenAnt.transform.position = new Vector3(-20, 10, 0);

        yield return queenAnt.transform.DOMove(new Vector3(-2, -2, 0), 5f).WaitForCompletion();

        yield return new WaitForSeconds(1f);
        queenAnt.RemoveWings();
        yield return new WaitForSeconds(1f);

        queenAnt.CreateEggAtPosition().SecondsUntilHatch = 1f;

        yield return new WaitForSeconds(1f);

        yield return queenAnt.transform.DOMove(new Vector3(0, -2, 0), .45f).WaitForCompletion();
        yield return new WaitForSeconds(.85f);

        QueenAnt.Instance.SpriteRenderer.flipX = true; // look at egg
    }
}
