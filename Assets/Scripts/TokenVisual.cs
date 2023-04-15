using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TokenVisual : MonoBehaviour {
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image background;

    public void SetBackgroundColor(Color color) {
        background.color = color;
    }

    public void SetText(string text) {
        this.text.SetText(text);
    }
}
