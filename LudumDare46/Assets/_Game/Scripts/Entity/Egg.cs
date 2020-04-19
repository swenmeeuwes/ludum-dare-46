using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : MonoBehaviour {
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public float SecondsUntilHatch { get; set; } = 10f;

    public float LayedAt { get; private set; }
    public bool CanBeHatched => LayedAt + SecondsUntilHatch < Time.time;

    public NurseryRoom NurseryRoom { get; set; }

    private void Start() {
        StartCoroutine(Wiggle());

        LayedAt = Time.time;

        _spriteRenderer.transform.localScale = Vector3.one * .1f;
        _spriteRenderer.transform.DOScale(Vector3.one, Mathf.Max(.1f, SecondsUntilHatch));

        EggManager.Instance.Register(this);
    }

    public void Hatch(Ant.AntType antType) {
        AntManager.Instance.CreateAntAt(transform.position, antType);

        EggManager.Instance.Deregister(this);

        if (NurseryRoom != null) {
            NurseryRoom.Eggs.Remove(this);
        }

        _spriteRenderer.DOFade(0, .35f).OnComplete(() => Destroy(gameObject));
    }

    private IEnumerator Wiggle() {
        yield return new WaitForSeconds(SecondsUntilHatch);

        while (true) {
            yield return new WaitForSeconds(5f);

            yield return _spriteRenderer.transform.DOShakeRotation(.35f, strength: 45, vibrato: 5).WaitForCompletion();
        }
    }
}
