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