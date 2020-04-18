using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour {
    public static RoomManager Instance { get; set; }

    public List<Room> Rooms { get; private set; } = new List<Room>();

    public IEnumerable<FoodStorageRoom> FoodStorageRooms { get { return Rooms.OfType<FoodStorageRoom>(); } }

    private void Awake() {
        Instance = this;
    }

    public void Register(Room room) {
        Rooms.Add(room);
    }
}
