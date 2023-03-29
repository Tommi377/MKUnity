using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "MapTile", menuName = "Map/MapTile")]
public class MapTileSO : ScriptableObject {
    [SerializeField] private MapTileType tileType;
    [SerializeField] private HexInfo[] hexTiles = new HexInfo[7];


    public static Vector3Int[] CoordinateOffsets = {
        new Vector3Int(0, -1, 1), new Vector3Int(1, -1, 0),
        new Vector3Int(-1, 0, 1), new Vector3Int(0, 0, 0), new Vector3Int(1, 0, -1),
        new Vector3Int(-1, 1, 0), new Vector3Int(0, 1, -1),
    };

    //public MapTile(HexInfo[] hexes) {
    //    Assert.AreEqual(hexes.Length, 7);
    //    for (int i = 0; i < 7; i++) {
    //        hexTiles[i] = hexes[i];
    //    }
    //}

    //public static MapTile CreateMapTile((HexTypes, StructureTypes, EntityTypes?)[] InfoTuple) {
    //    Assert.AreEqual(InfoTuple.Length, 7);
    //    HexInfo[] hexes = new HexInfo[7];

    //    for (int i = 0; i < 7; i++) {
    //        hexes[i] = new HexInfo(InfoTuple[i].Item1, InfoTuple[i].Item2, InfoTuple[i].Item3);
    //    }

    //    return new MapTile(hexes);
    //}
    public MapTileType TileType => tileType;

    public HexInfo GetHex(int i) {
        return hexTiles[i];
    }
}

[Serializable]
public struct HexInfo {
    public HexTypes HexType;
    public StructureTypes StructureType;
    public EntityTypes EntityType;
    public HexInfo(HexTypes hexType, StructureTypes structureType, EntityTypes entityType) {
        HexType = hexType;
        StructureType = structureType;
        EntityType = entityType;
    }
}

public enum MapTileType { Countryside, Core }