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
    ATTKSPEEDPCT
}

/// <summary>
/// This is a data class that represents a skill.
/// Skills are created via initialization statement. (Example in Shop start)
/// </summary>
[Serializable]
public class Skill
{
    // The current level of the skill
    public int level { get; set; }

    // The max level of the skill
    public int maxLevel { get; set; }

    // The money cost for upgrading the skill
    public int upgradeCost { get; set; }

    // The old value of the skill (value is how much the skill affects the player)
    public float oldValue { get; set; }

    // The current value of the skill
    public float currentValue { get; set; }

    // The name of the skill
    public string name { get; set; }

    // The type of skill
    public SkillType type { get; set; }

    // An anonymous function used to determine the price of the next upgrade level given the current price.
    public Func<int, int> costFunction { get; set; }

    // An anonymous function used to determine the value of the next upgrade level given the current value.
    public Func<float, float> improvementFunction { get; set; }

    /// <summary>
    /// Checks to make sure the upgrade can occur
    /// </summary>
    /// <param name="currentMoney"></param>
    /// <returns></returns>
    public bool CheckUpgrade(int currentMoney)
    {
        return (currentMoney >= upgradeCost) && (level < maxLevel);
    }

    /// <summary>
    /// Calls CheckUpgrade, handles the internal state of the skill and provides the remaining amount of money the player has.
    /// </summary>
    /// <param name="currentMoney"></param>
    /// <param name="remainingMoney"></param>
    /// <returns></returns>
    public bool Upgrade(int currentMoney, out int remainingMoney)
    {
        if (!CheckUpgrade(currentMoney))
        {
            remainingMoney = 0;
            return false;
        }

        remainingMoney = currentMoney - upgradeCost;
        ++level;
        oldValue = currentValue;
        currentValue = improvementFunction(currentValue);
        upgradeCost = costFunction(upgradeCost);

        return true;
    }
}