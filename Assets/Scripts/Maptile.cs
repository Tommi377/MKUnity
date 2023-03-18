using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MapTile {
    HexInfo[] HexTiles = new HexInfo[7];
    public static Vector3Int[] CoordinateOffsets = {
        new Vector3Int(0, -1, 1), new Vector3Int(1, -1, 0),
        new Vector3Int(-1, 0, 1), new Vector3Int(0, 0, 0), new Vector3Int(1, 0, -1),
        new Vector3Int(-1, 1, 0), new Vector3Int(0, 1, -1),
    };

    public MapTile(HexInfo[] hexes) {
        Assert.AreEqual(hexes.Length, 7);
        for (int i = 0; i < 7; i++) {
            HexTiles[i] = hexes[i];
        }
    }

    public static MapTile CreateMapTile((HexTypes, HexStructureTypes, EntityTypes?)[] InfoTuple) {
        Assert.AreEqual(InfoTuple.Length, 7);
        HexInfo[] hexes = new HexInfo[7];

        for (int i = 0; i < 7; i++) {
            hexes[i] = new HexInfo(InfoTuple[i].Item1, InfoTuple[i].Item2, InfoTuple[i].Item3);
        }

        return new MapTile(hexes);
    }

    public HexInfo GetHex(int i) {
        return HexTiles[i];
    }
}

public struct HexInfo {
    public HexTypes HexType;
    public HexStructureTypes StructureType;
    public EntityTypes? EntityType;
    public HexInfo(HexTypes hexType, HexStructureTypes structureType, EntityTypes? entityType) {
        HexType = hexType;
        StructureType = structureType;
        EntityType = entityType;
    }
}