using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSystemUI : MonoBehaviour {
    public static TooltipSystemUI Instance;

    private LTDescr showDelay;
    private LTDescr hideDelay;

    [SerializeField] private TooltipUI tooltip;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }
    }

    public bool IsPointerOverUI() => EventSystem.current.IsPointerOverGameObject();
    public bool IsTooltipEnabled() => tooltip.gameObject.activeSelf;

    public void Show(string description, string header = "") {
        tooltip.SetText(description, header);

        if (hideDelay != null) {
            LeanTween.cancel(hideDelay.uniqueId);
            hideDelay = null;
        }

        showDelay = LeanTween.delayedCall(0.5f, () => tooltip.Show());
    }

    public void Hide() {
        if (showDelay != null) {
            LeanTween.cancel(showDelay.uniqueId);
            showDelay = null;
        }
        hideDelay = LeanTween.delayedCall(0.05f, () => tooltip.Hide());
    }
}
