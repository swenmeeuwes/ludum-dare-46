using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NurseryRoom : Room {
    public List<Egg> Eggs { get; set; } = new List<Egg>();

    public void AddEgg(Egg egg) {
        egg.NurseryRoom = this;
        Eggs.Add(egg);
    }
}
