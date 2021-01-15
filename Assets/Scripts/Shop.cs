using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    private StageManager stageManager;

    /// <summary>
    /// The shop manages the player's money.
    /// This class is the only place the player's money should be altered.
    /// </summary>
    private long playerMoney;

    private float playerMoneyMult;

    #endregion Private Fields

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        blockSpawner = GameObject.FindGameObjectWithTag("BlockSpawner").GetComponent<BlockSpawner>();
        stageManager = GameObject.FindGameObjectWithTag("StageManager").GetComponent<StageManager>();

        skillDictionary = dataSavingManager.GetSkillDictionary();

        UpdatePlayerMoneyAndUI((long)dataSavingManager.GetOtherValue("Money"), skillDictionary);

        playerMoneyMult = (float)dataSavingManager.GetOtherValue("MoneyMultiplier");

        // Load skill dictionary into shop and ui
        uiManager.LoadSkillDescriptions(skillDictionary);
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
        var helpers = skillDictionary.Where(s => s.Value.type == SkillType.HELPER && s.Value.level >= 2).ToList();

        foreach (var s in helpers)
        {
            Vector2 randPos = new Vector2(Random.Range(-2f, 2f), Random.Range(-4f, 4f));
            var helper = Instantiate(helperPrefab, randPos, Quaternion.identity);

            helper.gameObject.GetComponent<Helper>().Init(s.Value.name);
        }
    }

    #endregion Init Methods

    #region Money Handling

    /// <summary>
    /// This method is called by the player when a block is killed and handles the reward money for killing
    /// blocks.
    /// </summary>
    /// <param name="killReward"></param>
    public void KilledBlock(long killReward)
    {
        UpdatePlayerMoneyAndUI((long)(killReward * playerMoneyMult) + playerMoney, skillDictionary);
    }

    /// <summary>
    /// NOTE THIS CURRENTLY SAVES EVERYTIME MONEY IS CHANGED FOR TESTING.
    /// This is the only place where the player's money is changed.
    /// </summary>
    /// <param name="money"></param>
    private void UpdatePlayerMoneyAndUI(long money, Dictionary<string, Skill> skillDict)
    {
        playerMoney = money;
        uiManager.SetMoneyText(money);
        uiManager.SetSkillButtonStatuses(skillDict, money);
        dataSavingManager.SetOtherValue("Money", money);
        dataSavingManager.Save();
    }

    /// <summary>
    /// Getter for the player's money.
    /// </summary>
    /// <returns></returns>
    public long GetMoney()
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
                Vector2 randPos = new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-4f, 4f));
                var helper = Instantiate(helperPrefab, randPos, Quaternion.identity);
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

        if (upgradedSkill.Upgrade(playerMoney, out long remainingMoney))
        {
            UpdatePlayerMoneyAndUI(remainingMoney, skillDictionary);

            dataSavingManager.SetOtherValue("Money", playerMoney);
            dataSavingManager.SetSkill(upgradedSkill.name, upgradedSkill);
            dataSavingManager.Save();

            ApplyUpgrade(upgradedSkill);
            return true;
        }
        return false;
    }

    #endregion Player Upgrading
}