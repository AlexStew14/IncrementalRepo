using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This enum represents what aspect of the player the skill affects.
/// </summary>
public enum SkillType
{
    DMG,
    DMGPCT,
    ATTKSPEED,
    KILLREWARD,
    SPAWNSPEED,
    MOVEMENTSPEED,
    HELPER,
    ABILITY,
    BLOCKSPERLEVEL
}

/// <summary>
/// This is a data class that represents a skill.
/// Skills are created via seeding in DataSavingManager.
/// </summary>
[Serializable]
public class Skill
{
    // The current level of the skill
    public int level { get; set; }

    // The max level of the skill
    public int maxLevel { get; set; }

    // The money cost for upgrading the skill
    public double upgradeCost { get; set; }

    // The current amount the skill is affecting the player, can be flat or percentage.
    public double currentStatIncrease { get; set; }

    public double totalStatIncrease { get; set; }

    // The next value in the improvement function, used for displaying on ui.
    public double nextStatIncrease { get; set; }

    // Every n levels, the skill is multiplied by milestoneMultiplier.
    // This is that n.
    public int milestoneLevel;

    public double milestoneMultipler;

    // The name of the skill
    public string name { get; set; }

    public bool isPrestige = false;

    // The type of skill
    public SkillType type { get; set; }

    // An anonymous function used to determine the price of the next upgrade level given the current level.
    public Func<int, double> costFunction { get; set; }

    // An anonymous function used to determine the value of the next upgrade level given the current value.
    public Func<double, double> improvementFunction { get; set; }

    // HelperData that the skill contains if the skill is of type HELPER
    public HelperData helperData { get; set; } = null;

    /// <summary>
    /// Checks to make sure the upgrade can occur
    /// </summary>
    /// <param name="currentCurrency"></param>
    /// <returns></returns>
    public bool CheckUpgrade(double currentCurrency)
    {
        return (currentCurrency >= upgradeCost) && (level < maxLevel);
    }

    /// <summary>
    /// Calls CheckUpgrade, handles the internal state of the skill and provides the remaining amount of money the player has.
    /// </summary>
    /// <param name="currentCurrency"></param>
    /// <param name="remainingCurrency"></param>
    /// <returns></returns>
    public virtual bool Upgrade(double currentCurrency, out double remainingCurrency)
    {
        if (!CheckUpgrade(currentCurrency))
        {
            remainingCurrency = 0;
            return false;
        }

        remainingCurrency = currentCurrency - upgradeCost;

        ++level;

        totalStatIncrease += nextStatIncrease;
        currentStatIncrease = nextStatIncrease;
        nextStatIncrease = improvementFunction(currentStatIncrease);

        if (level % milestoneLevel == 0)
        {
            nextStatIncrease *= milestoneMultipler;
        }

        upgradeCost = costFunction(level);

        if (type == SkillType.HELPER)
            HelperUpgrade();

        return true;
    }

    private void HelperUpgrade()
    {
        helperData.attackDamage *= (1.0 + currentStatIncrease);
        helperData.attackSpeed /= (1.0 + currentStatIncrease);
        helperData.idleTime /= (1.0 + currentStatIncrease);
        helperData.movementSpeed *= (1.0 + currentStatIncrease);
    }
}