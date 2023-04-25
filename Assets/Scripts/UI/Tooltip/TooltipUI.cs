using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class TooltipUI : MonoBehaviour {
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private LayoutElement layoutElement;

    private RectTransform rectTransform;

    [SerializeField] private int characterWrapLimit;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update() {
        if (Application.isEditor) {
            SetTextWrapping();
        }

        SetPosition();
    }

    public void SetText(string description, string header) {
        headerText.gameObject.SetActive(!string.IsNullOrEmpty(header));
        if (!string.IsNullOrEmpty(header)) headerText.SetText(header);

        descriptionText.SetText(description);

        SetTextWrapping();
    }

    public void Show() {
        gameObject.SetActive(true);
        SetPosition();
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    private void SetTextWrapping() {
        int headerLength = headerText.text.Length;
        int descriptionLength = descriptionText.text.Length;

        layoutElement.enabled = headerLength > characterWrapLimit || descriptionLength > characterWrapLimit;
    }

    private void SetPosition() {
        Vector2 position = Input.mousePosition;

        float xFlipPoint = 0.3f;

        float pivotX = Screen.width * xFlipPoint > position.x ? 0 : 1;
        float pivotY = position.y / Screen.height;

        rectTransform.pivot = new Vector2(pivotX, pivotY);
        transform.position = position;
    }
}
