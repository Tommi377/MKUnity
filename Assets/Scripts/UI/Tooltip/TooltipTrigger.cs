using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    protected abstract string GetDescription();
    protected abstract string GetHeader();

    public void OnPointerEnter(PointerEventData eventData) {
        Show(false);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Hide();
    }

    private void OnMouseEnter() {
        if (!TooltipSystemUI.Instance.IsPointerOverUI())
            Show(true);
    }

    private void OnMouseExit() {
        Hide();
    }

    private void Show(bool blockedByUI) {
        TooltipSystemUI.Instance.Show(GetDescription(), GetHeader(), blockedByUI);
    }

    private void Hide() {
        TooltipSystemUI.Instance.Hide();
    }
}
