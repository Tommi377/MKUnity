using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckUI : MonoBehaviour {
    [SerializeField] private GameObject cardVisualPrefab;

    [SerializeField] private Transform deckVisual;
    [SerializeField] private Transform discardVisual;
    [SerializeField] private Button drawButton;
    [SerializeField] private Button shuffleDiscardButton;
    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private TMP_Text discardCountText;
    [SerializeField] private TMP_Text manaText;

    private void Awake() {
        drawButton.onClick.AddListener(() => ButtonInputManager.Instance.DrawCardClick());
        shuffleDiscardButton.onClick.AddListener(() => ButtonInputManager.Instance.ShuffleDiscardClick());
    }

    private void Start() {
        Player.OnUpdateDeck += UpdateUIEvent;
        Player.OnPlayerDrawCard += UpdateUIEvent;
        Player.OnPlayerDiscardCard += UpdateUIEvent;
        Player.OnShuffleDiscardToDeck += UpdateUIEvent;
        Player.OnPlayerManaUpdate += UpdateUIEvent;

        if (GameManager.Instance.DoneInitializing) {
            UpdateUI();
        } else {
            GameManager.Instance.OnGameManagerDoneInitializing += GameManager_OnGameManagerDoneInitializing;
        }
    }

    private void GameManager_OnGameManagerDoneInitializing(object sender, System.EventArgs e) {
        UpdateUI();
        GameManager.Instance.OnGameManagerDoneInitializing -= GameManager_OnGameManagerDoneInitializing;
    }

    private void UpdateUI() {
        int deckCount = GetDeckCount();
        int discardCount = GetDiscardCount();

        deckCountText.SetText(deckCount.ToString());
        discardCountText.SetText(discardCount.ToString());
        manaText.SetText(GetManaCount().ToString());

        bool renderDeckVisual = deckCount > 0;
        bool renderDiscardVisual = discardCount > 0;

        // Deck Visual
        deckVisual.gameObject.SetActive(renderDeckVisual);
        deckCountText.gameObject.SetActive(renderDeckVisual);
        drawButton.gameObject.SetActive(renderDeckVisual);

        // Discard Pile Visual
        discardVisual.gameObject.SetActive(renderDiscardVisual);
        discardCountText.gameObject.SetActive(renderDiscardVisual);
        shuffleDiscardButton.gameObject.SetActive(renderDiscardVisual);

        if (renderDiscardVisual) {
            DrawTopDiscardCard(GetPlayer().DiscardPile.Last());
        }
    }

    private void DrawTopDiscardCard(Card discardedCard) {
        if (GetDiscardCount() > 0) {
            if (discardVisual.childCount > 0) {
                foreach (Transform child in discardVisual.transform) {
                    Destroy(child.gameObject);
                }
            }

            CardVisual cardVisual = Instantiate(cardVisualPrefab, discardVisual.transform).GetComponent<CardVisual>();
            cardVisual.Init(discardedCard);
        }
    }

    private int GetDeckCount() => GetPlayer().GetDeckCount();
    private int GetDiscardCount() => GetPlayer().GetDiscardCount();
    private int GetManaCount() => GetPlayer().Mana;

    private Player GetPlayer() => GameManager.Instance.CurrentPlayer;

    private void UpdateUIEvent(object sender, System.EventArgs e) {
        UpdateUI();
    }
}
