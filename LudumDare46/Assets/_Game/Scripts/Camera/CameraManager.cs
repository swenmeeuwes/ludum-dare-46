using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    [SerializeField] private Rect _moveableArea;
    [SerializeField] private float _moveSpeed;

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
    }

    private void Move(Vector3 direction) {
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
