using System;
using UnityEngine;

public class ButtonInputManager : MonoBehaviour {
    public static ButtonInputManager Instance;

    /* ------------------------- */
    /* EVENT DEFINITIONS - START */
    /* ------------------------- */
    /* CARD RELATED */
    public event EventHandler<OnCardActionClickArgs> OnCardActionClick;
    public class OnCardActionClickArgs : EventArgs {
        public Card card;
        public CardChoice choice;
    }
    public event EventHandler OnShuffleDiscardClick;
    public event EventHandler OnDrawCardClick;
    public event EventHandler<OnChoiceEffectDoneClickArgs> OnChoiceEffectDoneClick;
    public class OnChoiceEffectDoneClickArgs : EventArgs {
        public int choiceId;
    }
    public event EventHandler<OnRecruitUnitClickArgs> OnRecruitUnitClick;
    public class OnRecruitUnitClickArgs : EventArgs {
        public UnitCard unitCard;
    }

    /* MISC RELATED */
    public event EventHandler OnChannelManaClick;
    public event EventHandler<OnInfluenceChoiceClickArgs> OnInfluenceChoiceClick;
    public class OnInfluenceChoiceClickArgs : EventArgs {
        public InfluenceAction influenceAction;
    }

    /* ----------------------- */
    /* EVENT DEFINITIONS - END 
    /* ----------------------- */

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }
    }

    public void CardActionClick(Card card, CardChoice choice) {
        OnCardActionClick?.Invoke(this, new OnCardActionClickArgs {
            card = card,
            choice = choice
        });
    }

    public void ShuffleDiscardClick() {
        OnShuffleDiscardClick?.Invoke(this, EventArgs.Empty);
    }

    public void DrawCardClick() {
        OnDrawCardClick?.Invoke(this, EventArgs.Empty);
    }

    public void ChoiceEffectDoneClick(int id) {
        OnChoiceEffectDoneClick?.Invoke(this, new OnChoiceEffectDoneClickArgs { choiceId = id });
    }

    public void RecruitUnitClick(UnitCard unitCard) {
        OnRecruitUnitClick?.Invoke(this, new OnRecruitUnitClickArgs { unitCard = unitCard });
    }

    public void ChannelManaClick() {
        OnChannelManaClick?.Invoke(this, EventArgs.Empty);
    }

    public void InfluenceChoiceClick(InfluenceAction influenceAction) {
        OnInfluenceChoiceClick?.Invoke(this, new OnInfluenceChoiceClickArgs { influenceAction = influenceAction });
    }
}
