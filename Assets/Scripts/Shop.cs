using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    private Dictionary<string, Skill> skillDictionary;

    [SerializeField]
    private Player player;

    // Start is called before the first frame update
    private void Start()
    {
        skillDictionary = new Dictionary<string, Skill>();
        skillDictionary.Add("Damage", new Skill(1, 30, 5, 1, "Dmg", "Damage", (x) => x * 2, (x) => ++x));
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void ApplyPlayerUpgrades(Skill upgradedSkill)
    {
        if (upgradedSkill.type == "Dmg")
        {
            player.FlatDmgIncrease(upgradedSkill.currentValue);
        }
    }

    public bool UpgradeSkill(string skillName, out Skill upgradedSkill)
    {
        upgradedSkill = skillDictionary[skillName];
        if (upgradedSkill == null)
            return false;

        int remainingMoney;
        if (upgradedSkill.Upgrade(player.Money, out remainingMoney))
        {
            player.Money = remainingMoney;
            ApplyPlayerUpgrades(upgradedSkill);
            return true;
        }
        return false;
    }
}