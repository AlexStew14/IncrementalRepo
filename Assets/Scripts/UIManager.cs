using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles all UI, at least for the shop.
/// </summary>
public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject shopPanel;

    [SerializeField]
    private Shop shop;

    private TextMeshProUGUI currentMoney;

    private Player player;

    // Start is called before the first frame update
    private void Start()
    {
        shopPanel.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        currentMoney = GameObject.FindGameObjectWithTag("CurrentMoney").GetComponent<TextMeshProUGUI>();
        currentMoney.text = "Money: " + player.Money;
    }

    // Update is called once per frame
    private void Update()
    {
        currentMoney.text = "Money: " + player.Money;
    }

    /// <summary>
    /// This method is called by clicking the shop buttons for skill upgrades.
    /// The method takes in the button that clicks it.
    /// This calls UpgradeSkill on the shop and sets the description text for display.
    /// </summary>
    /// <param name="skillButton"></param>
    public void UpgradeSkill(Button skillButton)
    {
        string skillName = skillButton.name;
        Skill upgradedSkill;
        if (shop.UpgradeSkill(skillName, out upgradedSkill))
        {
            var description = skillButton.gameObject.transform.Find("Description").gameObject.GetComponent<Text>();
            description.text = "Price: " + upgradedSkill.upgradeCost + "\nCurrent Value: " + upgradedSkill.currentValue;
        }
    }

    /// <summary>
    /// This method is called by the shop button and toggles the shopPanel
    /// </summary>
    public void ToggleShopPanel()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
    }
}