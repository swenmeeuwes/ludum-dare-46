using DG.Tweening;
using UnityEngine;

public class BackgroundAnimator : MonoBehaviour {
    [SerializeField] private float _cycleX = 8;
    [SerializeField] private float _cycleDuration = 20; // in seconds

    private void Start() {
        DoAnimationCycle();
    }

    private void DoAnimationCycle() {
        transform.DOMoveX(_cycleX, _cycleDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                transform.position = new Vector3(0, transform.position.y, transform.position.z);
                DoAnimationCycle();
            });
    }
}
