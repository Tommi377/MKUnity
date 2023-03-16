using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public enum HexTypes {
    Plains,
    Hills,
    Forest,
    Wasteland,
    Desert,
    Swamp,
    Lake,
    Mountain,
    Inaccessable
}
public enum HexStructureTypes {
    None,
    Portal,
    MagicalGlade,
    CrystalMine,
    Village,
    MageTower,
    Monastery,
    Keep,
    AncientRuins
}

public class Hex : MonoBehaviour {
    Transform _entityContainerTransform;

#nullable enable
    [SerializeField]
    GameObject? _structureGameObject;
#nullable disable
    List<Entity> _entities = new List<Entity>();
    public ReadOnlyCollection<Entity> Entities => _entities.AsReadOnly();

    public static float HEX_SIZE = 1f;
    public static float HEX_WIDTH = Mathf.Sqrt(3) * HEX_SIZE;
    public static float HEX_HEIGHT = 2 * HEX_SIZE;

    public int Q { get; private set; }
    public int R { get; private set; }
    public int S { get; private set; }
    public Vector3Int Position { get => new Vector3Int(Q, R, S); }
    public HexTypes HexType { get; private set; }
    public HexStructureTypes StructureType { get; private set; }


#nullable enable
    public GameObject? StructureGameObject { get => _structureGameObject; private set => _structureGameObject = value; }
#nullable disable

    void Awake() {
        _entityContainerTransform = transform.Find("EntityContainer");
    }

    public void Initialize(Vector3Int coordinate, HexInfo hexInfo) {
        Initialize(coordinate.x, coordinate.y, coordinate.z, hexInfo);
    }
    public void Initialize(int q, int r, int s, HexInfo hexInfo) {
        Q = q;
        R = r;
        S = s;
        HexType = hexInfo.HexType;
        StructureType = hexInfo.StructureType;
        EntityTypes? entityType = hexInfo.EntityType;

        // TODO: make this pick random
        if (entityType != null) {
            switch (entityType) {
                case EntityTypes.Orc:
                    GameObject entityPrefab = Resources.Load("Prefabs/Prowler") as GameObject;
                    Entity entity = Instantiate(entityPrefab, _entityContainerTransform).GetComponent<Entity>();
                    PlaceEntity(entity);
                    break;
            }
        }

        RenderHex();
    }
    public bool Occupied { get => _entities.Count > 0; }

    public List<Enemy> GetEnemies() {
        List<Enemy> enemies = new List<Enemy>();
        foreach (Entity entity in _entities) {
            if (entity is Enemy) {
                enemies.Add(entity as Enemy);
            }
        }
        return enemies;
    }

    public void PlaceEntity(Entity entity) {
        entity.Position = Position;
        entity.gameObject.transform.SetParent(_entityContainerTransform, false);
        _entities.Add(entity);
    }

    public void ClearEntity(Entity entity) {
        _entities.Remove(entity);
    }

    public void ClearEntities() {
        _entities.Clear();
    }

    public int GetMoveCost() {
        // TODO: Move cost modifiers
        return defaultMoveCosts[(int)HexType, (int)RoundManager.Instance.Time];
    }

    private static int[,] defaultMoveCosts = new int[,] { { 2, 2 }, { 3, 3 }, { 3, 5 }, { 4, 4 }, { 5, 3 }, { 5, 5 }, { -1, -1 }, { -1, -1 } };

    private void RenderHex() {
        Renderer mr = GetComponentInChildren<Renderer>();
        switch (HexType) {
            case HexTypes.Plains:
                mr.material.color = new Color(0.5f, 1f, 0f, 1f);
                break;
            case HexTypes.Hills:
                mr.material.color = new Color(0.5f, 0.3f, 0f, 1f);
                break;
            case HexTypes.Forest:
                mr.material.color = new Color(0f, 0.5f, 0f, 1f);
                break;
            case HexTypes.Wasteland:
                mr.material.color = new Color(0.25f, 0.25f, 0f, 1f);
                break;
            case HexTypes.Desert:
                mr.material.color = new Color(1f, 0.75f, 0f, 1f);
                break;
            case HexTypes.Swamp:
                mr.material.color = new Color(0f, 1f, 0.75f, 1f);
                break;
            case HexTypes.Lake:
                mr.material.color = new Color(0f, 0.5f, 1f, 1f);
                break;
            case HexTypes.Mountain:
                mr.material.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                break;
            case HexTypes.Inaccessable:
                mr.material.color = new Color(0f, 0f, 0f, 1f);
                break;
        }
    }
}
