using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour {
    [SerializeField] private Transform cardGraphics;
    [SerializeField] private Transform highlight;
    [SerializeField] private GameObject actionCardPrefab;
    [SerializeField] private GameObject woundCardPrefab;
    [SerializeField] private GameObject itemCardPrefab;

    public Card Card { get; private set; }

    public void Init(Card card) {
        Card = card;
        UpdateVisual();
    }

    public void Select() {
        highlight.gameObject.SetActive(true);
    }

    public void Deselect() {
        highlight.gameObject.SetActive(false);
    }

    private void UpdateVisual() {
        gameObject.name = Card.Name;

        switch (Card.Type) {
            case Card.Types.Action:
                GameObject actionCardVisual = Instantiate(actionCardPrefab, cardGraphics);

                ActionCard actionCard = Card as ActionCard;
                actionCardVisual.transform.Find("CardInfo/Name").GetComponent<TMP_Text>().SetText(actionCard.Name);
                actionCardVisual.transform.Find("CardInfo/DescUp").GetComponent<TMP_Text>().SetText(actionCard.DescUp);
                actionCardVisual.transform.Find("CardInfo/DescDown").GetComponent<TMP_Text>().SetText(actionCard.DescDown);
                break;
            case Card.Types.Wound:
                GameObject woundVisual = Instantiate(woundCardPrefab, cardGraphics);
                break;
            case Card.Types.Item:
                ItemCardVisual itemCardVisual = Instantiate(itemCardPrefab, cardGraphics).GetComponent<ItemCardVisual>();
                itemCardVisual.Init(Card as ItemCard);
                break;
        }
    }
}
