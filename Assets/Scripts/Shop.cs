using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This class handles the state of all skills and updates the player to reflect skill changes.
/// It also acts as a controller for the UIManager and updates the UI.
/// </summary>
public class Shop : MonoBehaviour
{
    #region Private Fields

    private DataSavingManager dataSavingManager;

    private UIManager uiManager;

    private BlockSpawner blockSpawner;

    [SerializeField]
    private Player player;

    [SerializeField]
    private Transform helperPrefab;

    /// <summary>
    /// The shop manages the player's money.
    /// This class is the only place the player's money should be altered.
    /// </summary>
    private double playerMoney;

    private double playerPrestigeMoney;

    // Prestige money is rewarded for block kills above level 100.
    // Until the player prestiges, this prestige money is pending.
    // Upon prestige, the pending prestige money is added to their balance.
    private double pendingPrestigeMoney;

    private double playerMoneyMult;

    private UnityAction<object> blockKilled;

    private UnityAction<object> prestige;

    private UnityAction<object> tryUpgrade;

    private UnityAction<object> offlineProgress;

    [SerializeField]
    private GameObject goldText;

    #endregion Private Fields

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        blockSpawner = GameObject.FindGameObjectWithTag("BlockSpawner").GetComponent<BlockSpawner>();

        offlineProgress = new UnityAction<object>(HandleOfflineProgress);
        EventManager.StartListening("OfflineProgress", offlineProgress);
        blockKilled = new UnityAction<object>(BlockKilled);
        EventManager.StartListening("BlockKilled", blockKilled);

        prestige = new UnityAction<object>(Prestige);
        EventManager.StartListening("Prestige", prestige);

        tryUpgrade = new UnityAction<object>(UpgradeSkill);
        EventManager.StartListening("TryUpgrade", tryUpgrade);

        UpdatePlayerMoneyAndUI((double)dataSavingManager.GetOtherValue("Money"));
        UpdatePlayerPrestigeMoneyAndUI((double)dataSavingManager.GetOtherValue("PrestigeMoney"));
        UpdatePendingPrestigeMoneyAndUI((double)dataSavingManager.GetOtherValue("PendingPrestigeMoney"));

        playerMoneyMult = (double)dataSavingManager.GetOtherValue("MoneyMultiplier");

        // Load skill dictionary into shop and ui
        uiManager.LoadSkillDescriptions(dataSavingManager.GetSkillDictionary());
        uiManager.SetSkillPanelsVisibility(dataSavingManager.GetSkillDictionary());
        uiManager.SetDamagePanelsVisibilty(dataSavingManager.GetSkillDictionary());
        LoadHelpers();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    #endregion Unity Methods

    #region Init Methods

    private void LoadHelpers()
    {
        // Gets only skills that are helpers and unlocked.
        var helpers = dataSavingManager.GetSkillDictionary().Where(s => s.Value.type == SkillType.HELPER && s.Value.level >= 2).ToList();

        foreach (var s in helpers)
        {
            Vector2 randPos = new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-4f, 4f));
            var helper = Instantiate(helperPrefab, randPos, Quaternion.identity);

            helper.gameObject.GetComponent<Helper>().Init(s.Value.name);
        }
    }

    #endregion Init Methods

    #region Money Handling

    private void BlockKilled(object b)
    {
        Block deadBlock = (Block)b;
        double moneyEarned = deadBlock.killReward * playerMoneyMult;

        GameObject gText = Instantiate(goldText, deadBlock.transform.position, Quaternion.identity);
        gText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(NumberUtils.FormatLargeNumbers(moneyEarned));

        dataSavingManager.SetOtherValue("TotalMoneyEarned", (double)dataSavingManager.GetOtherValue("TotalMoneyEarned") + moneyEarned);
        dataSavingManager.Save();

        UpdatePlayerMoneyAndUI(moneyEarned + playerMoney);
        UpdatePendingPrestigeMoneyAndUI(deadBlock.killPrestigeReward);
    }

    private void Prestige(object unused)
    {
        dataSavingManager.ResetNonPrestigeSkills(null, true);
        dataSavingManager.Save();

        playerMoney = 0;
        uiManager.SetMoneyText(0);
        dataSavingManager.SetOtherValue("Money", 0.0);

        dataSavingManager.SetOtherValue("TotalPrestigeMoneyEarned", (double)dataSavingManager.GetOtherValue("PrestigeMoney") + pendingPrestigeMoney);
        UpdatePlayerPrestigeMoneyAndUI(playerPrestigeMoney + pendingPrestigeMoney);

        pendingPrestigeMoney = 0;
        uiManager.SetPendingPrestigeMoneyText(0);
        dataSavingManager.SetOtherValue("PendingPrestigeMoney", 0.0);

        dataSavingManager.SetOtherValue("PrestigeCount", (int)dataSavingManager.GetOtherValue("PrestigeCount") + 1);
        dataSavingManager.Save();

        uiManager.LoadSkillDescriptions(dataSavingManager.GetSkillDictionary());

        player.ResetNonPrestige();

        EventManager.TriggerEvent("CurrencyStatsUpdate");
        EventManager.TriggerEvent("MiscStatsUpdate");
        EventManager.TriggerEvent("TogglePrestige", false);
    }

    private void UpdatePendingPrestigeMoneyAndUI(double pendingPrestigeMoney)
    {
        this.pendingPrestigeMoney += pendingPrestigeMoney;
        uiManager.SetPendingPrestigeMoneyText(this.pendingPrestigeMoney);
        dataSavingManager.SetOtherValue("PendingPrestigeMoney", this.pendingPrestigeMoney);
        dataSavingManager.Save();
        EventManager.TriggerEvent("CurrencyStatsUpdate");
    }

    private void UpdatePlayerPrestigeMoneyAndUI(double prestigeMoney)
    {
        playerPrestigeMoney = prestigeMoney;
        uiManager.SetPrestigeMoneyText(playerPrestigeMoney);
        uiManager.SetSkillButtonStatuses(dataSavingManager.GetSkillDictionary(), playerMoney, prestigeMoney);
        dataSavingManager.SetOtherValue("PrestigeMoney", prestigeMoney);
        dataSavingManager.Save();
    }

    /// <summary>
    /// This takes in offline money
    /// </summary>
    /// <param name="offlineMoney"></param>
    private void HandleOfflineProgress(object offlineMoney)
    {
        Debug.LogWarning("Offline triggered");
        UpdatePlayerMoneyAndUI((double)offlineMoney + playerMoney);
    }

    /// <summary>
    /// NOTE THIS CURRENTLY SAVES EVERYTIME MONEY IS CHANGED FOR TESTING.
    /// This is the only place where the player's money is changed.
    /// </summary>
    /// <param name="money"></param>
    private void UpdatePlayerMoneyAndUI(double money)
    {
        playerMoney = money;
        uiManager.SetMoneyText(money);
        uiManager.SetSkillButtonStatuses(dataSavingManager.GetSkillDictionary(), money, playerPrestigeMoney);
        uiManager.SetSkillPanelsVisibility(dataSavingManager.GetSkillDictionary());
        uiManager.SetDamagePanelsVisibilty(dataSavingManager.GetSkillDictionary());
        dataSavingManager.SetOtherValue("Money", money);
        dataSavingManager.Save();
    }

    /// <summary>
    /// Getter for the player's money.
    /// </summary>
    /// <returns></returns>
    public double GetMoney()
    {
        return playerMoney;
    }

    #endregion Money Handling

    #region Player Upgrading

    /// <summary>
    /// This method is called after skills are upgraded and changes the player to reflect the skill upgrades.
    /// It first switches on skill type and then calls player methods to handle changes.
    /// </summary>
    /// <param name="upgradedSkill"></param>
    private void ApplyUpgrade(Skill upgradedSkill)
    {
        if (upgradedSkill.type == SkillType.DMG)
        {
            player.FlatDmgIncrease(upgradedSkill.currentStatIncrease);
        }
        else if (upgradedSkill.type == SkillType.ATTKSPEED)
        {
            player.PctAttackSpeedUpgrade(upgradedSkill.currentStatIncrease);
        }
        else if (upgradedSkill.type == SkillType.KILLREWARD)
        {
            playerMoneyMult = upgradedSkill.currentStatIncrease;
            dataSavingManager.SetOtherValue("MoneyMultiplier", playerMoneyMult);
            dataSavingManager.Save();
        }
        else if (upgradedSkill.type == SkillType.SPAWNSPEED)
        {
            blockSpawner.PctSpawnSpeedUpgrade(upgradedSkill.currentStatIncrease);
        }
        else if (upgradedSkill.type == SkillType.MOVEMENTSPEED)
        {
            player.FlatMovementSpeedIncrease(upgradedSkill.currentStatIncrease);
        }
        else if (upgradedSkill.type == SkillType.HELPER)
        {
            if (upgradedSkill.level == 2)
            {
                Vector2 randPos = new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-4f, 4f));
                var helper = Instantiate(helperPrefab, randPos, Quaternion.identity);
                helper.gameObject.GetComponent<Helper>().Init(upgradedSkill.name);
            }
        }
        else if (upgradedSkill.type == SkillType.ABILITY)
        {
            if (upgradedSkill.level == 1)
            {
                EventManager.TriggerEvent("PurchasedAbility", upgradedSkill as Ability);
            }
        }
        else if (upgradedSkill.type == SkillType.BLOCKSPERLEVEL)
        {
            dataSavingManager.SetOtherValue("BlocksPerLevel", (int)dataSavingManager.GetOtherValue("BlocksPerLevel") - (int)upgradedSkill.currentStatIncrease);
            dataSavingManager.Save();
        }
        else if (upgradedSkill.type == SkillType.DAMAGEMULTIPLIER)
        {
            player.PrestigeDamageUpgrade(upgradedSkill.currentStatIncrease);
        }
        else if (upgradedSkill.type == SkillType.AUTOMOVESPEED)
        {
            dataSavingManager.SetOtherValue("AutoMoveSpeedMultiplier", (double)dataSavingManager.GetOtherValue("AutoMoveSpeedMultiplier") * upgradedSkill.currentStatIncrease);
            dataSavingManager.Save();
        }
    }

    /// <summary>
    /// This method is called by the UIManager when an upgrade button is clicked.
    /// The skillName is provided and upgradedSkill is given alongside a bool as the return value.
    /// If the upgrade succeeds (enough money, not max level, etc), then the method returns true and upgradedSkill
    /// will be the skill that was changed.
    /// </summary>
    /// <param name="skillName"></param>
    /// <param name="upgradedSkill"></param>
    /// <returns></returns>
    private void UpgradeSkill(object skillButton)
    {
        Button sButton = (Button)skillButton;
        string skillName = sButton.name;

        Skill upgradedSkill = dataSavingManager.GetSkillDictionary()[skillName];
        if (upgradedSkill == null)
            return;

        if (upgradedSkill.isPrestige)
        {
            if (upgradedSkill.Upgrade(playerPrestigeMoney, out double remainingPrestigeMoney))
            {
                UpdatePlayerPrestigeMoneyAndUI(remainingPrestigeMoney);
                dataSavingManager.SetOtherValue("PrestigeMoney", remainingPrestigeMoney);
                dataSavingManager.SetSkill(upgradedSkill.name, upgradedSkill);
                dataSavingManager.Save();

                ApplyUpgrade(upgradedSkill);
                EventManager.TriggerEvent("Upgrade", (sButton, upgradedSkill));
                return;
            }
            return;
        }

        if (upgradedSkill.Upgrade(playerMoney, out double remainingMoney))
        {
            UpdatePlayerMoneyAndUI(remainingMoney);
            dataSavingManager.SetOtherValue("Money", remainingMoney);
            dataSavingManager.SetSkill(upgradedSkill.name, upgradedSkill);
            dataSavingManager.Save();

            ApplyUpgrade(upgradedSkill);
            EventManager.TriggerEvent("Upgrade", (sButton, upgradedSkill));
            return;
        }
        return;
    }

    #endregion Player Upgrading
}