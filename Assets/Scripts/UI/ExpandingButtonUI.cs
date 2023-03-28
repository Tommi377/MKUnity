using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExpandingButtonUI : MonoBehaviour {
    [SerializeField] private Transform buttonPrefab;

    private Options defaultOptions = new Options();
    public class Options {
        public bool Interactable = true;
        public Color? BackgroundColor = null;
        public Color? TextColor = null;
    }

    public Button AddButton(string text, UnityAction onClick, Options options = null) {
        options ??= defaultOptions;

        Button button = CreateButton(text, options);
        SetOnClick(button, onClick);
        return button;
    }

    public Button AddButton(string text, UnityAction<Button> onClick, Options options = null) {
        options ??= defaultOptions;

        Button button = CreateButton(text, options);
        SetOnClick(button, onClick);
        return button;
    }

    public void ClearButtons() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        gameObject.SetActive(false);
    }

    public void SetText(Button button, string text) {
        TMP_Text buttonText = button.transform.Find("Text").GetComponent<TMP_Text>();
        buttonText.SetText(text);
    }

    private Button CreateButton(string text, Options options) {
        if (!gameObject.activeSelf) {
            gameObject.SetActive(true);
        }

        Button button = Instantiate(buttonPrefab, transform).GetComponent<Button>();
        TMP_Text buttonText = button.transform.Find("Text").GetComponent<TMP_Text>();
        button.interactable = options.Interactable;
        buttonText.SetText(text);

        if (options.BackgroundColor != null) {
            button.GetComponent<Image>().color = (Color)options.BackgroundColor;
        }

        if (options.TextColor != null) {
            buttonText.color = (Color)options.TextColor;
        }

        return button;
    }

    private void SetOnClick(Button button, UnityAction<Button> onClick) {
        button.onClick.AddListener(() => onClick(button));
    }

    private void SetOnClick(Button button, UnityAction onClick) {
        button.onClick.AddListener(onClick);
    }
}
