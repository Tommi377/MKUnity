using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class HexMap : MonoBehaviour {
    public static HexMap Instance { get; private set; }

    public Player player;

    Stack<MapTile[]> MapStack = new Stack<MapTile[]>();
    Dictionary<Vector3Int, MapTile> macroCoordinates = new Dictionary<Vector3Int, MapTile>();
    Dictionary<Vector3Int, Hex> microCoordinates = new Dictionary<Vector3Int, Hex>();

    Vector3Int[] hexDirections = {
        new Vector3Int(+1, 0, -1), new Vector3Int(+1, -1, 0), new Vector3Int(0, -1, +1),
        new Vector3Int(-1, 0, +1), new Vector3Int(-1, +1, 0), new Vector3Int(0, +1, -1)
    };

    public GameObject HexPrefab;
    public MapTile MapStart = MapTile.CreateMapTile(new (HexTypes, HexStructureTypes, EntityTypes?)[]{
            (HexTypes.Plains, HexStructureTypes.None, null), (HexTypes.Forest, HexStructureTypes.None, null),
            (HexTypes.Inaccessable, HexStructureTypes.None, null), (HexTypes.Plains, HexStructureTypes.Portal, null), (HexTypes.Plains, HexStructureTypes.None, null),
            (HexTypes.Inaccessable, HexStructureTypes.None, null), (HexTypes.Inaccessable, HexStructureTypes.None, null)
    });

    public MapTile[] MapCountryside = {
        MapTile.CreateMapTile(new (HexTypes, HexStructureTypes, EntityTypes?)[]{
            (HexTypes.Forest, HexStructureTypes.None, EntityTypes.Orc), (HexTypes.Lake, HexStructureTypes.None, null),
            (HexTypes.Forest, HexStructureTypes.None, null), (HexTypes.Forest, HexStructureTypes.MagicalGlade, null), (HexTypes.Plains, HexStructureTypes.Village, null),
            (HexTypes.Plains, HexStructureTypes.None, null), (HexTypes.Plains, HexStructureTypes.None, null)
        }),
        MapTile.CreateMapTile(new (HexTypes, HexStructureTypes, EntityTypes?)[]{
            (HexTypes.Hills, HexStructureTypes.None, EntityTypes.Orc), (HexTypes.Forest, HexStructureTypes.MagicalGlade, null),
            (HexTypes.Plains, HexStructureTypes.None, null), (HexTypes.Hills, HexStructureTypes.None, null), (HexTypes.Plains, HexStructureTypes.Village, null),
            (HexTypes.Hills, HexStructureTypes.CrystalMine, null), (HexTypes.Plains, HexStructureTypes.None, null)
        })
    };


    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }
    }

    private void Start() {
        GenerateMap();
        PlaceMapTile(new Vector3Int(1, -1, 0), MapCountryside[0]);
        PlaceMapTile(new Vector3Int(1, 0, -1), MapCountryside[1]);

        microCoordinates[Vector3Int.zero].PlaceEntity(player);
    }

    public bool TryGetHex(Vector3Int microCoordinate, out Hex hex) {
        if (microCoordinates.TryGetValue(microCoordinate, out hex)) {
            return true;
        }
        return false;
    }

    public static Vector3 MicroToWorldCoordinates(Vector3Int micro) {
        float y = micro.y * Hex.HEX_HEIGHT * (3f / 4f);
        float x = Hex.HEX_WIDTH * (micro.x + micro.y / 2f);

        return new Vector3(x, 0, -y);
    }

    public static Vector3Int MacroToMicroCoordinates(Vector3Int macro) {
        Assert.AreEqual(macro.x + macro.y + macro.z, 0);

        Vector3Int diff = Vector3Int.zero;
        int q = macro.x;
        int r = macro.y;
        int s = macro.z;

        while (q != 0 || r != 0 || s != 0) {
            int[] coord = { q, r, s };

            int maxVal = Mathf.Max(q, r, s);
            int maxArg = maxVal == q ? 0 : maxVal == r ? 1 : 2;
            int minVal = Mathf.Min(q, r, s);
            int minArg = minVal == q ? 0 : minVal == r ? 1 : 2;

            if (maxArg == 0 && minArg == 1) {
                q -= 1;
                r += 1;
                diff += new Vector3Int(1, -3, 2);
            } else if (maxArg == 0 && minArg == 2) {
                q -= 1;
                s += 1;
                diff += new Vector3Int(3, -2, -1);
            } else if (maxArg == 1 && minArg == 2) {
                r -= 1;
                s += 1;
                diff += new Vector3Int(2, 1, -3);
            } else if (maxArg == 1 && minArg == 0) {
                r -= 1;
                q += 1;
                diff += new Vector3Int(-1, 3, -2);
            } else if (maxArg == 2 && minArg == 0) {
                s -= 1;
                q += 1;
                diff += new Vector3Int(-3, 2, 1);
            } else if (maxArg == 2 && minArg == 1) {
                s -= 1;
                r += 1;
                diff += new Vector3Int(-2, -1, 3);
            }
        }
        return diff;
    }

    public static bool HexIsNeigbor(Vector3Int a, Vector3Int b) {
        return HexDistance(a, b) == 1;
    }
    public static int HexDistance(Vector3Int a, Vector3Int b) {
        Vector3Int diff = a - b;
        return (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + Mathf.Abs(diff.z)) / 2;
    }

    public List<Hex> GetNeighbors(Vector3Int coord) {
        List<Hex> neigbors = new List<Hex>();
        foreach (Vector3Int direction in hexDirections) {
            microCoordinates.TryGetValue(coord + direction, out Hex hex);
            if (hex) {
                neigbors.Add(hex);
            }
        }

        return neigbors;
    }

    public bool MoveEntity(Vector3Int start, Vector3Int end, Entity entity) {
        microCoordinates.TryGetValue(start, out Hex startHex);
        microCoordinates.TryGetValue(end, out Hex endHex);
        if (startHex && endHex) {
            if (startHex.Occupied && !endHex.Occupied) {
                if (startHex.Entities.Any(e => e == entity)) {
                    endHex.PlaceEntity(entity);
                    startHex.ClearEntity(entity);
                    return true;
                } else {
                    Debug.Log("Entity type mismatch");
                }
            } else {
                Debug.Log("Start tile doesn't contain an entity / End tile contains an entity");
            }
        } else {
            Debug.Log("Start of end hex doesn't exist");
        }
        return false;
    }

    public void ClearEntityFromCoordinate(Entity entity, Vector3Int microCoord) {
        if (TryGetHex(microCoord, out Hex hex)) {
            if (hex.Entities.Contains(entity)) {
                hex.ClearEntity(entity);
                return;
            }
        }
        Debug.LogError("Couldn't delete entity");
    }

    private void GenerateMap() {
        PlaceMapTile(Vector3Int.zero, MapStart);
    }

    private void PlaceMapTile(Vector3Int origin, MapTile mapTile) {
        macroCoordinates.Add(new Vector3Int(origin.x, origin.y, origin.z), mapTile);

        for (int i = 0; i < 7; i++) {
            Vector3Int microCoordinate = MapTile.CoordinateOffsets[i] + MacroToMicroCoordinates(origin);
            Vector3 worldCoordinate = MicroToWorldCoordinates(microCoordinate);

            Hex hex = Instantiate(HexPrefab, worldCoordinate, Quaternion.identity, this.transform).GetComponent<Hex>();
            hex.name = microCoordinate.ToString();
            hex.Initialize(microCoordinate, mapTile.GetHex(i));
            microCoordinates.Add(microCoordinate, hex);
        }
    }
}