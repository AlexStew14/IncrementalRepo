using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the state of all skills and updates the player to reflect skill changes.
/// It also acts as a controller for the UIManager and updates the UI.
/// </summary>
public class Shop : MonoBehaviour
{
    /// <summary>
    /// This dictionary holds all skills with the skillName being the key.
    /// </summary>
    private Dictionary<string, Skill> skillDictionary;

    private DataSavingManager dataSavingManager;

    private UIManager uiManager;

    [SerializeField]
    private Player player;

    /// <summary>
    /// The shop manages the player's money.
    /// This class is the only place the player's money should be altered.
    /// </summary>
    private int playerMoney;

    // Start is called before the first frame update
    private void Start()
    {
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();

        UpdatePlayerMoneyAndUI((int)dataSavingManager.GetOtherValue("Money"));

        // Load skill dictionary into shop and ui
        skillDictionary = dataSavingManager.GetSkillDictionary();
        uiManager.LoadSkillDescriptions(skillDictionary);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void AddPlayerMoney(int moneyToAdd)
    {
        UpdatePlayerMoneyAndUI(moneyToAdd + playerMoney);
    }

    /// <summary>
    /// NOTE THIS CURRENTLY SAVES EVERYTIME MONEY IS CHANGED FOR TESTING.
    /// This is the only place where the player's money is changed.
    /// </summary>
    /// <param name="money"></param>
    private void UpdatePlayerMoneyAndUI(int money)
    {
        playerMoney = money;
        uiManager.SetMoneyText(money);
        dataSavingManager.SetOtherValue("Money", money);
        dataSavingManager.Save();
    }

    /// <summary>
    /// This method is called after skills are upgraded and changes the player to reflect the skill upgrades.
    /// It first switches on skill type and then calls player methods to handle changes.
    /// </summary>
    /// <param name="upgradedSkill"></param>
    private void ApplyPlayerUpgrade(Skill upgradedSkill)
    {
        if (upgradedSkill.type == SkillType.DMG)
        {
            player.FlatDmgIncrease(upgradedSkill.currentStatIncrease);
        }
        else if (upgradedSkill.type == SkillType.ATTKSPEED)
        {
            player.FlatAttackSpeedIncrease(upgradedSkill.currentStatIncrease);
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
    public bool UpgradeSkill(string skillName, out Skill upgradedSkill)
    {
        upgradedSkill = skillDictionary[skillName];
        if (upgradedSkill == null)
            return false;

        int remainingMoney;
        if (upgradedSkill.Upgrade(playerMoney, out remainingMoney))
        {
            UpdatePlayerMoneyAndUI(remainingMoney);

            dataSavingManager.SetOtherValue("Money", playerMoney);
            dataSavingManager.SetSkill(upgradedSkill.name, upgradedSkill);
            dataSavingManager.Save();

            ApplyPlayerUpgrade(upgradedSkill);
            return true;
        }
        return false;
    }
}