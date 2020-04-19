using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour {
    public static RoomManager Instance { get; set; }

    public List<Room> Rooms { get; private set; } = new List<Room>();

    public IEnumerable<FoodStorageRoom> FoodStorageRooms { get { return Rooms.OfType<FoodStorageRoom>(); } }
    public IEnumerable<QueensRoom> QueensRooms { get { return Rooms.OfType<QueensRoom>(); } }
    public IEnumerable<NurseryRoom> NurseryRooms { get { return Rooms.OfType<NurseryRoom>(); } }

    public IEnumerable<ConstructionSite> ConstructionSites { get { return Rooms.OfType<ConstructionSite>(); } }

    private void Awake() {
        Instance = this;
    }

    public void Register(Room room) {
        Rooms.Add(room);
    }

    public void Deregister(Room room) {
        Rooms.Remove(room);
    }
}
