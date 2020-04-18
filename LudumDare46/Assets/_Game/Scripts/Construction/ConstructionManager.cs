using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionManager : MonoBehaviour {
    [SerializeField] private SpriteRenderer _ghostObject;

    [SerializeField] private Color _canPlaceColor;
    [SerializeField] private Color _cantPlaceColor;

    [SerializeField] private FoodStorageRoom _foodStorageRoomPrefab;

    public static ConstructionManager Instance { get; private set; }

    public bool IsBuilding { get; private set; }
    public RoomType CurrentConstructionType { get; private set; }

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.A)) { // TODO: Hook up to button?
            ToggleBuildMode();
        }

        if (!IsBuilding) {
            return;
        }

        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        var mouseWorldPositionRounded = Vector3Int.RoundToInt(mouseWorldPosition);

        var tileAtPosition = TileMapManager.Instance.GroundTilemap.GetTile(mouseWorldPositionRounded);
        var canBuild = tileAtPosition == null && mouseWorldPositionRounded.y < -7;
        ShowIndicator(canBuild, mouseWorldPositionRounded);


        if (Input.GetMouseButtonDown(0)) {
            DisableBuildMode();

            if (canBuild) {
                AntManager.Instance.RequestBuildAt(Vector2Int.RoundToInt(mouseWorldPosition), CurrentConstructionType);
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            DisableBuildMode();
        }
    }

    public void EnterBuildMode(int roomType) {
        EnterBuildMode(((RoomType[])Enum.GetValues(typeof(RoomType)))[roomType]);
    }

    public void EnterBuildMode(RoomType type) {
        CurrentConstructionType = type;
        EnableBuildMode();
    }

    public void ToggleBuildMode() {
        if (IsBuilding) {
            DisableBuildMode();
        } else {
            EnableBuildMode();
        }
    }

    public void EnableBuildMode() {
        IsBuilding = true;
        _ghostObject.enabled = IsBuilding;
    }

    public void DisableBuildMode() {
        IsBuilding = false;
        _ghostObject.enabled = IsBuilding;
    }

    public void ShowIndicator(bool canBuild, Vector3Int position) {
        _ghostObject.transform.position = position + (Vector3)(Vector2.one * .5f);

        if (canBuild) {
            _ghostObject.DOColor(_canPlaceColor, .35f);
        } else {
            _ghostObject.DOColor(_cantPlaceColor, .35f);
        }
    }

    public void BuildAt(Vector2Int location, RoomType constructionType) {
        switch (constructionType) {
            case RoomType.FoodStorage:
                BuildFoodStorageRoomAt(location);
                break;
        }
    }

    public void BuildFoodStorageRoomAt(Vector2Int position) {
        var roomObj = Instantiate(_foodStorageRoomPrefab, transform);
        roomObj.transform.position = new Vector3(position.x, position.y);

        var room = roomObj.GetComponent<Room>();
        RoomManager.Instance.Register(room);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;
        Gizmos.DrawWireCube(Vector3Int.RoundToInt(mouseWorldPosition) + Vector3.one * .5f, Vector3.one);
    }
}

public enum RoomType {
    FoodStorage = 0,
    QueensRoom = 1
}