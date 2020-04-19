using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ActionManager : MonoBehaviour {
    public static ActionManager Instance { get; set; }

    [SerializeField] private Button _layEggsButton;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        _layEggsButton.interactable = false;
    }

    public void UpdateLayEggs() {
        var nurseryRooms = RoomManager.Instance.NurseryRooms.ToList();
        var emptyRoom = nurseryRooms.FirstOrDefault(r => r.Eggs.Count == 0);

        _layEggsButton.interactable = emptyRoom != null;
    }

    public void LayEggs() {
        QueenAnt.Instance.RequestLayEggs();
        _layEggsButton.interactable = false;
    }
}
