using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapTileList", menuName = "Map/MapTileList")]
public class MapTileListSO : ScriptableObject {
    public MapTileType TileType;
    public List<MapTileSO> TileList;
}
