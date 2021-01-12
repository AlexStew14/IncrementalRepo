using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the state of all skills and updates the player to reflect skill changes.
/// It also acts as a controller for the UIManager and updates the UI.
/// </summary>
public class Shop : MonoBehaviour
{
    #region Private Fields

    /// <summary>
    /// This dictionary holds all skills with the skillName being the key.
    /// </summary>
    private Dictionary<string, Skill> skillDictionary;

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
    private int playerMoney;

    private float playerMoneyMult;

    #endregion Private Fields

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        blockSpawner = GameObject.FindGameObjectWithTag("BlockSpawner").GetComponent<BlockSpawner>();

        UpdatePlayerMoneyAndUI((int)dataSavingManager.GetOtherValue("Money"));

        playerMoneyMult = (float)dataSavingManager.GetOtherValue("MoneyMultiplier");

        // Load skill dictionary into shop and ui
        skillDictionary = dataSavingManager.GetSkillDictionary();
        uiManager.LoadSkillDescriptions(skillDictionary);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    #endregion Unity Methods

    #region Money Handling

    /// <summary>
    /// This method is called by the player when a block is killed and handles the reward money for killing
    /// blocks.
    /// </summary>
    /// <param name="killReward"></param>
    public void KilledBlock(int killReward)
    {
        UpdatePlayerMoneyAndUI((int)(killReward * playerMoneyMult) + playerMoney);
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
    /// Getter for the player's money.
    /// </summary>
    /// <returns></returns>
    public int GetMoney()
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
        else if (upgradedSkill.type == SkillType.KILLREWARD)
        {
            playerMoneyMult += upgradedSkill.currentStatIncrease;
            dataSavingManager.SetOtherValue("MoneyMultiplier", playerMoneyMult);
            dataSavingManager.Save();
        }
        else if (upgradedSkill.type == SkillType.SPAWNSPEED)
        {
            blockSpawner.FlatSpawnSpeedIncrease(upgradedSkill.currentStatIncrease);
        }
        else if (upgradedSkill.type == SkillType.MOVEMENTSPEED)
        {
            player.FlatMovementSpeedIncrease(upgradedSkill.currentStatIncrease);
        }
        else if (upgradedSkill.type == SkillType.HELPER)
        {
            if (upgradedSkill.level == 2)
            {
                var helper = Instantiate(helperPrefab);
                helper.gameObject.GetComponent<Helper>().Init(upgradedSkill.name);
            }
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
        {
            return false;
        }

        if (upgradedSkill.Upgrade(playerMoney, out int remainingMoney))
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

    #endregion Player Upgrading
}