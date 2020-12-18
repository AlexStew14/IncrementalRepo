using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Skill
{
    public int level { get; private set; }
    public int maxLevel { get; private set; }
    public int upgradeCost { get; private set; }
    public int oldValue { get; private set; }
    public int currentValue { get; private set; }
    private string name;
    public string type { get; private set; }
    private Func<int, int> costFunction;

    private Func<int, int> improvementFunction;

    public Skill(int level, int maxLevel, int upgradeCost, int currentValue, string type, string name, Func<int, int> costFunction, Func<int, int> improvementFunction)
    {
        this.level = level;
        this.maxLevel = maxLevel;
        this.upgradeCost = upgradeCost;
        this.currentValue = currentValue;
        this.type = type;
        this.name = name;
        this.costFunction = costFunction;
        this.improvementFunction = improvementFunction;
        oldValue = 0;
    }

    public bool CheckUpgrade(int currentMoney)
    {
        return (currentMoney >= upgradeCost) && (level < maxLevel);
    }

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