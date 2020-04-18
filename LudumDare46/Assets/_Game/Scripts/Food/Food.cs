using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour {
    private void Start() {
        FoodManager.Instance.Register(this);
    }
}
