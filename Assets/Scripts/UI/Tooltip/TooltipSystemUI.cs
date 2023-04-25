using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSystemUI : MonoBehaviour {
    public static TooltipSystemUI Instance;

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

        if (hideDelay != null)
            LeanTween.cancel(hideDelay.uniqueId);

        tooltip.Show();
    }

    public void Hide() {
        hideDelay = LeanTween.delayedCall(0.1f, () => tooltip.Hide());
    }
}
