using System.Collections;
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
    DAMAGE
}

[Serializable]
public class Ability : Skill
{
    public AbilityType abilityType;
    public AbilitySubType abilitySubType;

    public float duration;
    private float durationLeft;

    public int activationChance;

    public float cooldown;
    private float cooldownLeft;

    public bool activated;

    private bool readyToCast;

    public int prefabIndex;

    public override bool Upgrade(long currentCurrency, out long remainingCurrency)
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
        upgradeCost = costFunction(level);

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

        int rand = UnityEngine.Random.Range(1, 101);
        if (rand >= activationChance)
            return false;

        activated = true;
        durationLeft = duration;
        readyToCast = false;

        Debug.Log("Ability has procced");
        return true;
    }
}