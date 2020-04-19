using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
    public Vector2Int Position { get { return Vector2Int.RoundToInt(transform.position); } }
}
