using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "HexTypeMaterialMap", menuName = "Temp/HexTypeMaterialMap")]
public class HexTypeMaterialMapSO : ScriptableObject {
    [SerializeField] private HexTypes[] hexTypeKey;
    [SerializeField] private Material[] materialValue;

    public Dictionary<HexTypes, Material> Map() {
        Dictionary<HexTypes, Material> map = new Dictionary<HexTypes, Material>();
        for (int i = 0; i < hexTypeKey.Length; i++) {
            map.Add(hexTypeKey[i], materialValue[i]);
        }
        return map;
    }
}
