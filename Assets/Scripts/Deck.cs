using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Deck : MonoBehaviour {
    [SerializeField] private CardListSO cardListSO;
    [SerializeField] private List<Card> cards = new List<Card>();

    private void Awake() {
        foreach (ActionCardCount actionCardCount in cardListSO.List) {
            for(int i = 0; i < actionCardCount.Count; i++) {
                cards.Add(actionCardCount.Card.CreateInstance());
            }
        }
        Shuffle();
    }

    public int Count => cards.Count;

    public bool Empty => cards.Count == 0;

    // Adds the card on top
    public void Add(Card card) {
        cards.Add(card);
    }

    // Adds the cards on top
    public void Add(List<Card> cards) {
        this.cards.AddRange(cards);
    }

    public void Shuffle() {
        this.cards = this.cards.OrderBy(x => Random.value).ToList();
    }

    public Card Draw() {
        Card card = this.cards.Last();
        this.cards.RemoveAt(cards.Count - 1);
        return card;
    }
}
