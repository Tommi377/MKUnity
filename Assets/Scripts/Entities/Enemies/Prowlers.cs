using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prowlers : Enemy {
    public override EntityTypes EntityType { get { return EntityTypes.Orc; } }
    private void Awake() {
        Armor = 3;
        Attack = 4;
        Fame = 2;
        Roaming = true;
    }
}