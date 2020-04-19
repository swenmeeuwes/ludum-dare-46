using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;

public class CameraManager : MonoBehaviour {
    public static CameraManager Instance { get; private set; }

    [SerializeField] private Rect _moveableArea;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private TMP_Text _queenAttackedText;

    private Vector3 _previousMousePosition;

    public bool MovedToQueenThisWave { get; set; } = false;
    public bool CanMoveCameraAround { get; set; } = true;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        _queenAttackedText.alpha = 0;
    }

    private void Update() {
        if (Input.GetKey(KeyCode.DownArrow)) {
            Move(Vector3.down);
        } else if (Input.GetKey(KeyCode.UpArrow)) {
            Move(Vector3.up);
        }

        if (Input.GetKey(KeyCode.LeftArrow)) {
            Move(Vector3.left);
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            Move(Vector3.right);
        }

        if(Input.GetMouseButton(1) && _previousMousePosition != null) {
            var deltaMousePos = _previousMousePosition - Input.mousePosition;
            Move(deltaMousePos.normalized * 2f);
        }

        _previousMousePosition = Input.mousePosition;
    }

    public void NotifyQueenDamage() {
        if (!MovedToQueenThisWave) {
            MovedToQueenThisWave = true;
            MoveToQueen();
        }
    }

    public void MoveToQueen(bool endOfGame = false) {
        if (endOfGame) {
            _queenAttackedText.DOFade(0, .25f);
            CanMoveCameraAround = false;
            var pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
            DOTween.To(() => pixelPerfectCamera.assetsPPU, (x) => pixelPerfectCamera.assetsPPU = (int)x, 28f, 1f).SetEase(Ease.Linear);
            transform.DOMove(new Vector3(0, -17, -10), 1f).SetEase(Ease.Linear);

            return;
        }

        var queenPos = QueenAnt.Instance.transform.position;
        queenPos.z = -10;

        _queenAttackedText.DOFade(1, .35f);
        _queenAttackedText.DOFade(0, .65f).SetDelay(3f);
        transform.DOMove(queenPos, .35f);
    }

    private void Move(Vector3 direction) {
        if (!CanMoveCameraAround) return;

        if (!_moveableArea.Contains(transform.position + direction * _moveSpeed * Time.deltaTime)) {
            return;
        }

        transform.position += direction * _moveSpeed * Time.deltaTime;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_moveableArea.position + _moveableArea.size / 2, _moveableArea.size);
    }
}
