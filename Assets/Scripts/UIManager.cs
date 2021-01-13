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
    #region Private Fields

    [SerializeField]
    private GameObject shopPanel;

    private Shop shop;

    private TextMeshProUGUI currentMoney;

    private Player player;

    /// <summary>
    /// Buttons used for skills in the shop panel.
    /// </summary>
    [SerializeField]
    private Button[] buttons;

    /// <summary>
    /// Panels used for displaying different shop sections.
    /// </summary>
    [SerializeField]
    private GameObject[] panels;

    #endregion Private Fields

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        shopPanel.SetActive(false);

        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();
        currentMoney = GameObject.FindGameObjectWithTag("CurrentMoney").GetComponent<TextMeshProUGUI>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    #endregion Unity Methods

    #region Skill UI Methods

    /// <summary>
    /// Called by the shop when the game starts.
    /// Sets up the ui of each skill.
    /// </summary>
    /// <param name="skillDictionary"></param>
    public void LoadSkillDescriptions(Dictionary<string, Skill> skillDictionary)
    {
        foreach (Button b in buttons)
        {
            string skillName = b.name;
            Skill s = skillDictionary[skillName];
            if (s == null)
                continue;

            SetDescriptionText(b, s);
        }
    }

    public void SetButtonStates(Dictionary<string, Skill> skillDictionary, int money)
    {
        foreach (Button b in buttons)
        {
            string skillName = b.name;
            Skill s = skillDictionary[skillName];
            if (s == null)
                continue;

            b.interactable = s.CheckUpgrade(money);
        }
    }

    private void SetDescriptionText(Button skillButton, Skill skill)
    {
        //var description = skillButton.gameObject.transform.Find("Description").gameObject.GetComponent<Text>();
        var parent = skillButton.gameObject.transform.parent;

        parent.Find("Level").gameObject.GetComponent<TextMeshProUGUI>().text = "Lv. " + skill.level;
        parent.Find("Bonus").gameObject.GetComponent<TextMeshProUGUI>().text = "+" + skill.nextStatIncrease;
        skillButton.gameObject.transform.Find("Price").gameObject.GetComponent<TextMeshProUGUI>().text = skill.upgradeCost.ToString();

        //description.text = "Price: " + skill.upgradeCost + "\nTotal Stat Increase: " + skill.totalStatIncrease + "\nNext Level Increase: " + skill.nextStatIncrease;
        /*if (shop.GetMoney() < skill.upgradeCost)
        {
            skillButton.interactable = false;
        }
        else
        {
            skillButton.interactable = true; ;
        }*/
    }

    /// <summary>
    /// Called by shop when the player's money changes.
    /// </summary>
    /// <param name="money"></param>
    public void SetMoneyText(int money)
    {
        currentMoney.text = "Money: " + money;
    }

    /// <summary>
    /// This method is called by the shop button and toggles the shopPanel
    /// </summary>
    public void ToggleShopPanel()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
        player.StopMoving();
        player.canMove = !player.canMove;
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
        if (shop.UpgradeSkill(skillName, out Skill upgradedSkill))
        {
            SetDescriptionText(skillButton, upgradedSkill);
        }
    }

    public void SwitchPanel(GameObject panel)
    {
        foreach (GameObject p in panels)
        {
            p.SetActive(false);
        }

        panel.SetActive(true);
    }

    #endregion Skill UI Methods
}