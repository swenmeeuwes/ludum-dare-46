using DG.Tweening;
using UnityEngine;

public class Food : MonoBehaviour {
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public State CurrentState { get; private set; }

    private void Start() {
        CurrentState = State.Surface;

        _spriteRenderer.sprite = _sprites[Random.Range(0, _sprites.Length)];

        FoodManager.Instance.Register(this);
        _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 0);
        _spriteRenderer.DOFade(1, .95f);
    }

    public void PickUp(Ant ant) {
        CurrentState = State.PickedUp;
        transform.SetParent(ant.transform, true);
        transform.DOMoveY(ant.transform.position.y + .45f, .95f);
        //transform.DOMoveX(ant.transform.position.x - .25f, .95f);
    }

    public void Store(FoodStorageRoom room) {
        CurrentState = State.InStorage;

        transform.DOMove(transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0), .35f);

        room.AddFood(this);

        ScoreManager.Instance.NotifyFoodCollected();
    }

    public enum State {
        Surface = 0,
        PickedUp = 1,
        InStorage = 2
    }
}
