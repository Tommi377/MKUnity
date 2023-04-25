using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour {
    public static ItemManager Instance;

    [SerializeField] private ItemListSO itemListSO;

    [SerializeField] private int itemOfferLimit = 3;

    /* EVENT DEFINITIONS - START */
    public event EventHandler<OnItemBuyArgs> OnItemBuy;
    public class OnItemBuyArgs : EventArgs {
        public ItemCard Item;
    }
    /* EVENT DEFINITIONS - END */

    private Stack<ItemCard> ItemCardStack;

    public List<ItemCard> ItemOffer { get; private set; } = new List<ItemCard>();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }

        List<ItemCard> items = new List<ItemCard>();

        foreach (ItemCardCount cardCount in itemListSO.List) {
            for (int i = 0; i < cardCount.Count; i++) {
                items.Add(cardCount.Item.CreateInstance() as ItemCard);
            }
        }
        ItemCardStack = new Stack<ItemCard>(items.OrderBy(x => UnityEngine.Random.value).ToList());

        Debug.Log("Item offer: ");
        for (int i = 0; i < itemOfferLimit; i++) {
            ItemOffer.Add(ItemCardStack.Pop());
            Debug.Log(ItemOffer.Last());
        }
    }

    public List<ItemCard> GetItemOfferForStructure(StructureTypes structureType) => ItemOffer.Where(item => item.Locations.Contains(structureType)).ToList();

    public void BuyItem(ItemCard item) {
        if (!ItemOffer.Contains(item)) {
            Debug.LogError("Item does not exist in the offer");
            return;
        }

        ItemOffer.Remove(item);
        OnItemBuy?.Invoke(this, new OnItemBuyArgs { Item = item });
    }

    // TODO new round init stuff
}
