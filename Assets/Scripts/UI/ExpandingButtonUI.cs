using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExpandingButtonUI : MonoBehaviour {
    [SerializeField] private Transform buttonPrefab;


    public Button AddButton(string text, UnityAction onClick, bool interactable = true) {
        Button button = CreateButton(text, interactable);
        SetOnClick(button, onClick);
        return button;
    }

    public Button AddButton(string text, UnityAction<Button> onClick, bool interactable = true) {
        Button button = CreateButton(text, interactable);
        SetOnClick(button, onClick);
        return button;
    }

    private Button CreateButton(string text, bool interactable) {
        if (!gameObject.activeSelf) {
            gameObject.SetActive(true);
        }

        Button button = Instantiate(buttonPrefab, transform).GetComponent<Button>();
        button.interactable = interactable;
        button.transform.Find("Text").GetComponent<TMP_Text>().SetText(text);
        return button;
    }

    private void SetOnClick(Button button, UnityAction<Button> onClick) {
        button.onClick.AddListener(() => onClick(button));
    }

    private void SetOnClick(Button button, UnityAction onClick) {
        button.onClick.AddListener(onClick);
    }

    public void ClearButtons() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        gameObject.SetActive(false);
    }
}
