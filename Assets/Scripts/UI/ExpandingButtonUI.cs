using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExpandingButtonUI : MonoBehaviour {
    [SerializeField] private Transform buttonPrefab;

    public Button AddButton(string text, UnityAction onClick) {
        if (!gameObject.activeSelf) {
            gameObject.SetActive(true);
        }

        Button button = Instantiate(buttonPrefab, transform).GetComponent<Button>();
        button.onClick.AddListener(onClick);
        button.transform.Find("Text").GetComponent<TMP_Text>().SetText(text);
        return button;
    }

    public void ClearButtons() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        gameObject.SetActive(false);
    }
}
