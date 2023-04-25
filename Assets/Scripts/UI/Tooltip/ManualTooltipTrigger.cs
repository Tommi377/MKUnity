using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualTooltipTrigger : TooltipTrigger {
    [SerializeField] private string header;
    [SerializeField] private string description;

    protected override string GetHeader() => header;
    protected override string GetDescription() => description;
}
