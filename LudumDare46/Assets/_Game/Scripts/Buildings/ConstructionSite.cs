using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionSite : Room {
    public RoomType Type { get; set; }
    public Ant Worker { get; set; }

    public void Complete() {
        ConstructionManager.Instance.BuildAt(Position, Type);

        RoomManager.Instance.Deregister(this);
        GameObject.Destroy(gameObject);
    }
}
