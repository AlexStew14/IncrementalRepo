﻿using System.Collections;
using UnityEngine;
using System;

public enum AbilityType
{
    ACTIVE,
    PASSIVE
}

public enum AbilitySubType
{
    MOVEMENTSPEED,
    DAMAGE,
    AREADAMAGE,
    DAMAGEOVERTIME
}

[Serializable]
public class Ability : Skill
{
    public AbilityType abilityType;
    public AbilitySubType abilitySubType;

    public float duration;
    private float durationLeft;

    public float activationChance;
    public float maxActivationChance;
    public float nextChanceIncrease;

    public float radius;

    public Func<float, float> activationFunction { get; set; }

    public float cooldown;
    private float cooldownLeft;

    public bool activated;

    private bool readyToCast = true;

    public int prefabIndex;

    public override bool Upgrade(double currentCurrency, out double remainingCurrency)
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
        nextStatIncrease = improvementFunction(level);
        upgradeCost = costFunction(level);

        if (activationChance + nextChanceIncrease >= maxActivationChance)
        {
            activationChance = maxActivationChance;
            activationFunction = (x) => 0;
        }
        else
            activationChance = activationChance + nextChanceIncrease;

        nextChanceIncrease = activationFunction(activationChance);

        return true;
    }

    public bool UpdateAndHasEnded(float timePassed)
    {
        if (readyToCast)
            return false;

        if (activated)
        {
            durationLeft -= timePassed;
            if (durationLeft <= 0)
            {
                activated = false;
                cooldownLeft = cooldown;
                return true;
            }
            return false;
        }
        else
        {
            cooldownLeft -= timePassed;
            if (cooldownLeft <= 0)
                readyToCast = true;

            return false;
        }
    }

    public bool Cast()
    {
        if (!readyToCast)
            return false;

        float rand = UnityEngine.Random.Range(.0f, 1.0f);
        Debug.Log("rand: " + rand + " chance: " + activationChance);
        if (rand > activationChance)
            return false;

        activated = true;
        durationLeft = duration;
        readyToCast = false;

        Debug.Log("Ability has procced");
        return true;
    }

    public static string FormatSubType(AbilitySubType abs)
    {
        switch (abs)
        {
            case AbilitySubType.DAMAGE:
                return "Damage";

            case AbilitySubType.MOVEMENTSPEED:
                return "Speed";

            case AbilitySubType.AREADAMAGE:
                return "Area Damage";

            case AbilitySubType.DAMAGEOVERTIME:
                return "Damage Over Time";

            default:
                return "INVALID";
        }
    }
}