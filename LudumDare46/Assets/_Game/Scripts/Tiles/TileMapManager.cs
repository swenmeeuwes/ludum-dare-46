using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapManager : MonoBehaviour {
    [SerializeField] private Tilemap _ground;

    public Tilemap GroundTilemap => _ground;

    public static TileMapManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }
}
