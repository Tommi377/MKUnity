using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexVisual : MonoBehaviour {
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private HexTypeMaterialMapSO hexTypeMaterialMapSO;

    private HexTypes hexType;

    public void Initialize(HexTypes hexType) {
        this.hexType = hexType;
        SetHexMaterial();
        SetTrees();
    }

    private void SetHexMaterial() {
        foreach (Renderer renderer in renderers) {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++) {
                mats[i] = hexTypeMaterialMapSO.Map()[hexType];
            }
            renderer.materials = mats;
        }
    }

    private void SetTrees() {
        GameObject treeContainer = transform.Find("Visual").Find("OtherVisual")?.Find("TreeVisual")?.gameObject; // Temp
        if (treeContainer != null) {
            treeContainer.SetActive(hexType == HexTypes.Forest || hexType == HexTypes.Swamp);
        }
    }
}
