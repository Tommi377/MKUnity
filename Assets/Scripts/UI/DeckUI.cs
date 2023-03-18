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

    private void Awake() {
        drawButton.onClick.AddListener(() => ButtonInput.Instance.DrawCardClick());
        shuffleDiscardButton.onClick.AddListener(() => ButtonInput.Instance.ShuffleDiscardClick());
    }

    private void Start() {
        Player.OnPlayerDrawCard += Player_OnPlayerDrawCard;
        Player.OnPlayerDiscardCard += Player_OnPlayerDiscardCard;
        Player.OnShuffleDiscardToDeck += Player_OnShuffleDiscardToDeck;

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

    private Player GetPlayer() => GameManager.Instance.CurrentPlayer;


    private void Player_OnPlayerDrawCard(object sender, Player.OnPlayerDrawCardArgs e) {
        UpdateUI();
    }

    private void Player_OnPlayerDiscardCard(object sender, Player.OnPlayerDiscardCardArgs e) {
        UpdateUI();
    }

    private void Player_OnShuffleDiscardToDeck(object sender, System.EventArgs e) {
        UpdateUI();
    }
}
