using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the state of all skills and updates the player to reflect skill changes.
/// </summary>
public class Shop : MonoBehaviour
{
    /// <summary>
    /// This dictionary holds all skills with the skillName being the key.
    /// </summary>
    private Dictionary<string, Skill> skillDictionary;

    private DataSavingManager dataSavingManager;

    [SerializeField]
    private Player player;

    // Start is called before the first frame update
    private void Start()
    {
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        skillDictionary = dataSavingManager.GetSkillDictionary();
        player.Money = (int)dataSavingManager.GetOtherValue("Money");
    }

    // Update is called once per frame
    private void Update()
    {
    }

    /// <summary>
    /// This method is called after skills are upgraded and changes the player to reflect the skill upgrades.
    /// It first switches on skill type and then calls player methods to handle changes.
    /// </summary>
    /// <param name="upgradedSkill"></param>
    private void ApplyPlayerUpgrades(Skill upgradedSkill)
    {
        if (upgradedSkill.type == SkillType.DMG)
        {
            player.FlatDmgIncrease(upgradedSkill.currentValue);
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
        if (upgradedSkill.Upgrade(player.Money, out remainingMoney))
        {
            player.Money = remainingMoney;
            dataSavingManager.SetOtherValue("Money", player.Money);
            dataSavingManager.SetSkill(upgradedSkill.name, upgradedSkill);
            dataSavingManager.Save();

            ApplyPlayerUpgrades(upgradedSkill);
            return true;
        }
        return false;
    }
}