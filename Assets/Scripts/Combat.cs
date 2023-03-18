using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static GameManager;
using System;

public enum CombatPhases {
    Range,
    Block,
    AssignDamage,
    Attack,
    End
}

public enum CombatTypes {
    Normal,
    Range,
    Siege,
    Block
}
public enum CombatElements {
    Normal,
    Ice,
    Fire,
    ColdFire
}

public class CombatData {
    public int Damage;
    public CombatTypes CombatType;
    public CombatElements CombatElement;

    public CombatData(int damage, CombatTypes combatType, CombatElements combatElement) {
        Damage = damage;
        CombatType = combatType;
        CombatElement = combatElement;
    }

    public override string ToString() {
        return "Combat data: " + Damage + "dmg " + CombatElement + " " + CombatType + " attack";
    }
}

public class CombatResult {
    public Player Player;
    public List<Enemy> Alive;
    public List<Enemy> Defeated;
    public int TotalFame;
}

public class Combat {
    private List<Enemy> enemies = new List<Enemy>();
    private List<Enemy> defeated = new List<Enemy>();

    private List<CombatData> combatCards = new List<CombatData>();
    private List<Enemy> unblocked = new List<Enemy>();

    public CombatPhases CombatPhase { get; private set; } = CombatPhases.Range;
    public Player Player { get; private set; }

    public ReadOnlyCollection<Enemy> Enemies { get => enemies.AsReadOnly(); }
    public ReadOnlyCollection<Enemy> UnblockedEnemies { get => unblocked.AsReadOnly(); }


    /* EVENT DEFINITIONS - START */
    public static event EventHandler OnCombatAttack;
    public static event EventHandler OnCombatBlock;
    public static event EventHandler OnCombatAssign;
    public static event EventHandler OnCombatStart;
    public static event EventHandler<OnCombatPhaseChangeArgs> OnCombatPhaseChange;
    public class OnCombatPhaseChangeArgs : EventArgs {
        public Combat combat;
        public CombatPhases phase;
    }
    public static event EventHandler<OnCombatEndArgs> OnCombatEnd;
    public class OnCombatEndArgs : EventArgs {
        public Combat combat;
        public CombatResult result;
    }
    public static void ResetStaticData() {
        OnCombatAttack = null;
        OnCombatBlock = null;
        OnCombatAssign = null;
        OnCombatStart = null;
        OnCombatEnd = null;
    }
    /* EVENT DEFINITIONS - END */

    public Combat(Player player, IEnumerable<Enemy> enemies) {
        Player = player;
        this.enemies.AddRange(enemies);
        unblocked.AddRange(enemies);
    }

    public void Init() {
        OnCombatStart?.Invoke(this, EventArgs.Empty);
        OnCombatPhaseChange?.Invoke(this, new OnCombatPhaseChangeArgs { combat = this, phase = CombatPhases.Range });

        ButtonInput.Instance.OnCombatEnemyChooseClick += ButtonInput_OnCombatEnemyChooseClick;
        ButtonInput.Instance.OnCombatNextPhaseClick += ButtonInput_OnCombatNextPhaseClick;
        ButtonInput.Instance.OnCombatBlockChooseClick += ButtonInput_OnCombatBlockChooseClick;
        ButtonInput.Instance.OnAssignEnemyDamageClick += ButtonInput_OnAssignEnemyDamageClick;
    }

    public void Dispose() {
        ButtonInput.Instance.OnCombatEnemyChooseClick -= ButtonInput_OnCombatEnemyChooseClick;
        ButtonInput.Instance.OnCombatNextPhaseClick -= ButtonInput_OnCombatNextPhaseClick;
        ButtonInput.Instance.OnCombatBlockChooseClick -= ButtonInput_OnCombatBlockChooseClick;
        ButtonInput.Instance.OnAssignEnemyDamageClick -= ButtonInput_OnAssignEnemyDamageClick;
    }

    public ReadOnlyCollection<CombatData> CombatCards => combatCards.AsReadOnly();

    public bool CombatEnded { get => CombatPhase == CombatPhases.End; }

    public void PlayCombatCard(CombatData combatData) {
        combatCards.Add(combatData);
        Debug.Log(combatData);
    }

    private void NextPhase() {
        if (enemies.Count == 0) {
            CombatPhase = CombatPhases.End;
        }

        switch (CombatPhase) {
            case CombatPhases.Range:
                CombatPhase = CombatPhases.Block;
                break;
            case CombatPhases.Block:
                CombatPhase = CombatPhases.AssignDamage;
                break;
            case CombatPhases.AssignDamage:
                CombatPhase = CombatPhases.Attack;
                break;
            case CombatPhases.Attack:
                CombatPhase = CombatPhases.End;
                break;
        }
        Debug.Log("Next phase: " + CombatPhase);
    }

    private void AttackEnemies(List<Enemy> targets) {
        if (targets.Count > 0) {
            Debug.Log(Player + ", " + targets + ", " + combatCards + ", " + CombatPhase);
            CombatAttack combatAttack = new CombatAttack(Player, targets, combatCards, CombatPhase);
            if (combatAttack.IsEnemyDead()) {
                defeated.AddRange(targets);
                foreach (Enemy target in targets) {
                    enemies.Remove(target);
                }
                Debug.Log("Enemy defeated!! remaining: " + enemies.Count);
            }
            Debug.Log("Enemy left with hp: " + combatAttack.TotalArmor);
        } else {
            Debug.Log("Must have targets to attack");
        }

        OnCombatAttack?.Invoke(this, EventArgs.Empty);
        combatCards.Clear();
    }

    private void BlockEnemy(Enemy target) {
        if (target) {
            Enemy blockedEnemy = target;
            if (UnblockedEnemies.Contains(blockedEnemy)) {
                CombatBlock combatBlock = new CombatBlock(Player, blockedEnemy, combatCards);
                int damageReceived = combatBlock.PlayerReceivedDamage();

                if (combatBlock.FullyBlocked) {
                    Debug.Log("Enemy was fully blocked");
                    unblocked.Remove(blockedEnemy);
                } else {
                    Debug.Log("Enemy was not fully blocked, take " + damageReceived + " damage");
                }
            } else {
                Debug.Log("Enemy is already blocked!");
            }
        } else {
            Debug.Log("Can't block if target count is not 1");
        }

        OnCombatBlock?.Invoke(this, EventArgs.Empty);
        combatCards.Clear();
    }

    private void CombatNextPhase() {
        NextPhase();
        OnCombatPhaseChange?.Invoke(this, new OnCombatPhaseChangeArgs { combat = this, phase = CombatPhase });
        if (CombatEnded) {
            CombatResult result = new CombatResult {
                Player = Player,
                Alive = enemies,
                Defeated = defeated,
                TotalFame = defeated.Sum(enemy => enemy.Fame),
            };
            OnCombatEnd?.Invoke(this, new OnCombatEndArgs { combat = this, result = result });
        }
    }

    private void AssignDamageToPlayer(Enemy enemy) {
        int woundCardAmount = Mathf.CeilToInt((float)enemy.Attack / Player.Armor);
        Player.TakeWounds(woundCardAmount);
        unblocked.Remove(enemy);
        OnCombatAssign?.Invoke(this, EventArgs.Empty);
    }

    /* ------------------- EVENTS ---------------------- */

    private void ButtonInput_OnCombatEnemyChooseClick(object sender, ButtonInput.OnCombatEnemyChooseClickArgs e) {
        AttackEnemies(e.enemies);
    }

    private void ButtonInput_OnCombatNextPhaseClick(object sender, EventArgs e) {
        CombatNextPhase();
    }

    private void ButtonInput_OnCombatBlockChooseClick(object sender, ButtonInput.OnCombatBlockChooseClickArgs e) {
        BlockEnemy(e.blockedEnemy);
    }

    private void ButtonInput_OnAssignEnemyDamageClick(object sender, ButtonInput.OnAssignEnemyDamageClickArgs e) {
        AssignDamageToPlayer(e.damagingEnemy);
    }
}

public class CombatAttack {
    Player Player;
    List<Enemy> Enemies;

    List<CombatData> CombatCards;
    CombatPhases CombatPhase;

    public int TotalArmor { get; private set; } = 0;

    public CombatAttack(Player player, List<Enemy> enemies, List<CombatData> combatCards, CombatPhases combatPhase) {
        Player = player;
        Enemies = enemies;
        CombatCards = combatCards;
        CombatPhase = combatPhase;

        Enemies.ForEach(enemy => TotalArmor += enemy.Armor);

        foreach (CombatData combatCard in CombatCards) {
            DealDamage(combatCard);
        }
    }

    void DealDamage(CombatData combatCard) {
        if (combatCard.CombatType == CombatTypes.Block) {
            return;
        }

        // TODO: add resistance checking
        // TODO: add siege checking
        if (CombatPhase == CombatPhases.Range) {
            if (combatCard.CombatType == CombatTypes.Range || combatCard.CombatType == CombatTypes.Siege) {
                TotalArmor -= combatCard.Damage;
            }
        } else if (CombatPhase == CombatPhases.Attack) {
            TotalArmor -= combatCard.Damage;
        } else {
            Debug.Log("Can't deal damage with this card");
        }
    }

    public bool IsEnemyDead() {
        return TotalArmor <= 0;
    }
}
public class CombatBlock {
    private Player player;
    private Enemy enemy;
    private List<CombatData> combatCards;

    private int totalDamage = 0;
    private int totalBlock = 0;

    public CombatBlock(Player player, Enemy enemy, List<CombatData> combatCards) {
        this.player = player;
        this.enemy = enemy;
        this.combatCards = combatCards;

        // Modify this attack depending on modifiers
        totalDamage = enemy.Attack;

        foreach (CombatData combatCard in combatCards) {
            ReceiveDamage(combatCard);
        }
    }

    private void ReceiveDamage(CombatData combatCard) {
        if (combatCard.CombatType != CombatTypes.Block) {
            return;
        }
        // TODO: add resistance and other checking
        totalBlock += combatCard.Damage;
    }
    public int PlayerReceivedDamage() {
        return FullyBlocked ? 0 : totalDamage;
    }

    public bool FullyBlocked { get => totalDamage <= totalBlock; }
}