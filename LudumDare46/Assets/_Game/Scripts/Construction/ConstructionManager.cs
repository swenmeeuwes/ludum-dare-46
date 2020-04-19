using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConstructionManager : MonoBehaviour {
    [SerializeField] private SpriteRenderer _ghostObject;
    [SerializeField] private TMP_Text _ghostText;

    [SerializeField] private Color _canPlaceColor;
    [SerializeField] private Color _cantPlaceColor;

    [SerializeField] private FoodStorageRoom _foodStorageRoomPrefab;
    [SerializeField] private QueensRoom _queensRoomPrefab;
    [SerializeField] private NurseryRoom _nurseryRoomPrefab;
    [SerializeField] private ConstructionSite _constructionSitePrefab;

    public static ConstructionManager Instance { get; private set; }

    public bool IsBuilding { get; private set; }
    public RoomType CurrentConstructionType { get; private set; }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        _ghostObject.enabled = false;
        _ghostText.enabled = false;
    }

    private void Update() {
        if (!IsBuilding) {
            return;
        }

        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        var mouseWorldPositionRounded = Vector3Int.RoundToInt(mouseWorldPosition);

        var tileAtPosition = TileMapManager.Instance.GroundTilemap.GetTile(mouseWorldPositionRounded);
        bool canBuild = true;

        if (tileAtPosition != null) {
            canBuild = false;
            _ghostText.text = "Room has to be build on empty space";
        }
        
        if (canBuild && mouseWorldPositionRounded.y > -7) {
            canBuild = false;
            _ghostText.text = "Too close to the surface";
        }

        if (canBuild && mouseWorldPositionRounded.y <= -28) {
            canBuild = false;
            _ghostText.text = "Too deep";
        }

        if (canBuild) {
            // Check if there are other rooms nearby
            foreach (var room in RoomManager.Instance.Rooms) {
                var dist = Vector2Int.Distance((Vector2Int)mouseWorldPositionRounded, room.Position);
                if (dist < 4) {
                    canBuild = false;
                    _ghostText.text = "Too close to another room";
                    break;
                }
            }
        }


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
        _ghostText.enabled = IsBuilding;
    }

    public void DisableBuildMode() {
        IsBuilding = false;
        _ghostObject.enabled = IsBuilding;
        _ghostText.enabled = IsBuilding;
    }

    public void ShowIndicator(bool canBuild, Vector3Int position) {
        _ghostObject.transform.position = position + (Vector3)(Vector2.one * .5f);
        _ghostText.enabled = !canBuild;

        if (canBuild) {
            _ghostObject.DOColor(_canPlaceColor, .35f);
        } else {
            _ghostObject.DOColor(_cantPlaceColor, .35f);
        }
    }

    public ConstructionSite BuildConstructionSite(Vector2Int location, RoomType constructionType) {
        var roomObj = Instantiate(_constructionSitePrefab, transform);
        roomObj.transform.position = new Vector3(location.x, location.y);

        var room = roomObj.GetComponent<ConstructionSite>();
        room.Type = constructionType;
        RoomManager.Instance.Register(room);

        return room;
    }

    public void BuildAt(Vector2Int location, RoomType constructionType) {
        switch (constructionType) {
            case RoomType.FoodStorage:
                BuildFoodStorageRoomAt(location);
                break;
            case RoomType.QueensRoom:
                BuildQueenRoomAt(location);
                break;
            case RoomType.Nursery:
                BuildNurseryRoomAt(location);
                break;
        }
    }

    public void BuildFoodStorageRoomAt(Vector2Int position) {
        var roomObj = Instantiate(_foodStorageRoomPrefab, transform);
        roomObj.transform.position = new Vector3(position.x, position.y);

        var room = roomObj.GetComponent<Room>();
        RoomManager.Instance.Register(room);
    }

    public void BuildQueenRoomAt(Vector2Int position) {
        var roomObj = Instantiate(_queensRoomPrefab, transform);
        roomObj.transform.position = new Vector3(position.x, position.y);

        var room = roomObj.GetComponent<Room>();
        RoomManager.Instance.Register(room);

        QueenAnt.Instance.MoveToQueenRoomIfAvailable();
    }

    public void BuildNurseryRoomAt(Vector2Int position) {
        var roomObj = Instantiate(_nurseryRoomPrefab, transform);
        roomObj.transform.position = new Vector3(position.x, position.y);

        var room = roomObj.GetComponent<Room>();
        RoomManager.Instance.Register(room);

        ActionManager.Instance.UpdateLayEggs();
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
    QueensRoom = 1,
    Nursery = 2
}