using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EntityTypes {
    Player,
    Orc,
    Guard,
    Mage,
    Dungeon,
    Ruin,
    Dragon,
    Royal
}

public abstract class Entity : MonoBehaviour {
    public abstract EntityTypes EntityType { get; }
    public bool IsAggressive() {
        return EntityType != EntityTypes.Player;
    }
    public Vector3Int Position = Vector3Int.zero;

    public void DestroySelf() {
        HexMap.Instance.ClearEntityFromCoordinate(this, Position);
        Destroy(gameObject);
    }

    public bool Move(Hex hex) {
        return HexMap.Instance.MoveEntity(Position, hex.Position, this);
    }

    public bool TryGetHex(out Hex hex) {
        if (HexMap.Instance.TryGetHex(Position, out hex)) {
            return true;
        }
        return false;
    }
}