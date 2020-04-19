using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EggManager : MonoBehaviour {
    public static EggManager Instance { get; private set; }

    public List<Egg> Eggs { get; private set; } = new List<Egg>();
    public IEnumerable<Egg> EggsThatCanBeHatched => Eggs.Where(e => e.CanBeHatched);

    private void Awake() {
        Instance = this;
    }

    public void Register(Egg egg) {
        Eggs.Add(egg);
    }

    public void Deregister(Egg egg) {
        Eggs.Remove(egg);
    }
}
