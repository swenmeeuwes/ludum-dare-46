using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyPresenceIndicator : MonoBehaviour {
    private CanvasGroup _canvasGroup;

    public bool Active { get; set; } = false;

    private Vector3 _startPos;

    private void Start() {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;

        _startPos = transform.position;

        StartCoroutine(EnemyPresenceCheckSequence());
    }

    private IEnumerator EnemyPresenceCheckSequence() {
        while (true) {
            var enemiesArePresent = EnemyManager.Instance.Enemies.Count > 0;
            if (enemiesArePresent && !Active) {
                Active = true;

                _canvasGroup.transform.DOMove(new Vector3(Screen.width / 2, Screen.height / 2 + Screen.height / 8, 0), .65f);
                _canvasGroup.transform.DOScale(Vector3.one * 2f, .65f);
                _canvasGroup.DOFade(1, .65f).WaitForCompletion();

                yield return new WaitForSeconds(2f);

                _canvasGroup.transform.DOMove(_startPos, .65f);
                yield return _canvasGroup.transform.DOScale(Vector3.one, .65f);
            } else if (!enemiesArePresent && Active) {
                Active = false;
                yield return _canvasGroup.DOFade(0, .65f).WaitForCompletion();
            }

            yield return new WaitForSeconds(2f);
        }
    }
}
