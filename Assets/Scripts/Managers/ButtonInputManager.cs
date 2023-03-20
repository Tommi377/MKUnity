using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInputManager : MonoBehaviour {
    public static ButtonInputManager Instance;

    /* ------------------------- */
    /* EVENT DEFINITIONS - START */
    /* ------------------------- */
    /* PHASE RELATED */
    public event EventHandler OnDrawStartHandClick;
    public event EventHandler OnEndStartPhaseClick;
    public event EventHandler OnEndMovementClick;
    public event EventHandler OnEndEndPhaseClick;
    public event EventHandler<OnActionChooseClickArgs> OnActionChooseClick;
    public class OnActionChooseClickArgs : EventArgs {
        public ActionTypes actionType;
    }

    /* COMBAT RELATED */
    public event EventHandler<OnCombatEnemyChooseClickArgs> OnCombatEnemyChooseClick;
    public class OnCombatEnemyChooseClickArgs : EventArgs {
        public List<Enemy> enemies;
    }
    public event EventHandler OnCombatNextPhaseClick;
    public event EventHandler<OnCombatBlockChooseClickArgs> OnCombatBlockChooseClick;
    public class OnCombatBlockChooseClickArgs : EventArgs {
        public Enemy blockedEnemy;
    }
    public event EventHandler<OnAssignEnemyDamageClickArgs> OnAssignEnemyDamageClick;
    public class OnAssignEnemyDamageClickArgs : EventArgs {
        public Enemy damagingEnemy;
    }

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

    /* MISC RELATED */
    public event EventHandler OnChannelManaClick;

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

    public void DrawStartHandClick() {
        OnDrawStartHandClick?.Invoke(this, EventArgs.Empty);
    }

    public void EndStartPhaseClick() {
        OnEndStartPhaseClick?.Invoke(this, EventArgs.Empty);
    }

    public void EndMovementClick() {
        OnEndMovementClick?.Invoke(this, EventArgs.Empty);
    }

    public void EndEndPhaseClick() {
        OnEndEndPhaseClick?.Invoke(this, EventArgs.Empty);
    }

    public void ActionChooseClick(ActionTypes actionType) {
        OnActionChooseClick?.Invoke(this, new OnActionChooseClickArgs { actionType = actionType });
    }

    public void CombatEnemyChooseClick(List<Enemy> enemies) {
        OnCombatEnemyChooseClick?.Invoke(this, new OnCombatEnemyChooseClickArgs { enemies = enemies });
    }

    public void CombatNextPhaseClick() {
        OnCombatNextPhaseClick?.Invoke(this, EventArgs.Empty);
    }

    public void CombatBlockChooseClick(Enemy blockedEnemy) {
        OnCombatBlockChooseClick?.Invoke(this, new OnCombatBlockChooseClickArgs { blockedEnemy = blockedEnemy });
    }

    public void AssignEnemyDamageClick(Enemy enemy) {
        OnAssignEnemyDamageClick?.Invoke(this, new OnAssignEnemyDamageClickArgs { damagingEnemy = enemy });
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

    public void ChannelManaClick() {
        OnChannelManaClick?.Invoke(this, EventArgs.Empty);
    }
}
