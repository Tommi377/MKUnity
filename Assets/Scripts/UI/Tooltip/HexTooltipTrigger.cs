using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Hex))]
public class HexTooltipTrigger : TooltipTrigger {
    private Hex hex;

    private void Awake() {
        hex = GetComponent<Hex>();
    }

    protected override string GetHeader() => hex.HexType.ToString();
    protected override string GetDescription() {
        string text = "Move cost: " + hex.GetMoveCost();
        if (hex.ContainsStructure()) {
            text += "\nStructure: " + hex.Structure.StructureType.ToString();
        }
        if (hex.Occupied) {
            text += "\nEntities: ";
            foreach (Entity entity in hex.Entities) {
                text += entity.Name;
            }
        }
        return text;
    }
}
