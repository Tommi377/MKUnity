using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    protected abstract string GetDescription();
    protected abstract string GetHeader();

    public void OnPointerEnter(PointerEventData eventData) {
        Show();
    }

    public void OnPointerExit(PointerEventData eventData) {
        Hide();
    }

    private void OnMouseEnter() {
        if (!TooltipSystemUI.Instance.IsPointerOverUI())
            Show();
    }

    private void OnMouseOver() {
        if (TooltipSystemUI.Instance.IsTooltipEnabled() && TooltipSystemUI.Instance.IsPointerOverUI()) {
            Hide();
        } else if (!TooltipSystemUI.Instance.IsTooltipEnabled() && !TooltipSystemUI.Instance.IsPointerOverUI()) {
            Show();
        }
    }

    private void OnMouseExit() {
        Hide();
    }

    private void Show() {
        TooltipSystemUI.Instance.Show(GetDescription(), GetHeader());
    }

    private void Hide() {
        TooltipSystemUI.Instance.Hide();
    }
}
