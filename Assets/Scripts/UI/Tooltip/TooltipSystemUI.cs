using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSystemUI : MonoBehaviour {
    public static TooltipSystemUI Instance;

    private bool tooltipEnabled = false;
    private bool blockedByUI;
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

    private void Update() {
        if (blockedByUI && IsTooltipEnabled()) {
            if (tooltip.gameObject.activeSelf && IsPointerOverUI()) {
                tooltip.Hide();
            } else if (!tooltip.gameObject.activeSelf && !IsPointerOverUI()) {
                tooltip.Show();
            }
        }
    }

    public bool IsPointerOverUI() => EventSystem.current.IsPointerOverGameObject();
    public bool IsTooltipEnabled() => tooltipEnabled;

    public void Show(string description, string header, bool blockedByUI) {
        this.blockedByUI = blockedByUI;
        tooltip.SetText(description, header);

        if (hideDelay != null) {
            LeanTween.cancel(hideDelay.uniqueId);
            hideDelay = null;
        }

        showDelay = LeanTween.delayedCall(0.5f, () => {
            tooltipEnabled = true;
            tooltip.Show();
        });
    }

    public void Hide() {
        if (showDelay != null) {
            LeanTween.cancel(showDelay.uniqueId);
            showDelay = null;
        }
        hideDelay = LeanTween.delayedCall(0.05f, () => {
            tooltipEnabled = false;
            tooltip.Hide();
        });
    }
}
