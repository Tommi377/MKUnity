using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Entity {
    public int Armor { get; protected set; }
    public int Attack { get; protected set; }
    public int Fame { get; protected set; }
    public bool Roaming { get; protected set; } = false;
}