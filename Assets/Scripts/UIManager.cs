using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// This class handles all UI, at least for the shop.
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Private Fields

    [SerializeField]
    private GameObject shopPanel;

    private TextMeshProUGUI currentMoney;
    private TextMeshProUGUI currentPrestigeMoney;

    [SerializeField]
    private TextMeshProUGUI pendingPrestigeMoney;

    private Player player;

    /// <summary>
    /// Buttons used for skills in the shop panel.
    /// </summary>
    [SerializeField]
    private Button[] buttons;

    [SerializeField]
    private Button prestigeButton;

    /// <summary>
    /// Panels used for displaying different shop sections.
    /// </summary>
    [SerializeField]
    private GameObject[] panels;

    [SerializeField]
    private GameObject[] damagePanels;

    [SerializeField]
    private GameObject mapLevelPanel;

    [SerializeField]
    private GameObject bossTimerPanel;

    [SerializeField]
    private Slider bossTimerSlider;

    private UnityAction<object> updateLevelUI;

    private UnityAction<object> upgrade;

    private UnityAction<object> togglePrestige;

    private UnityAction<object> startBoss;

    private UnityAction<object> setBossTimer;

    private UnityAction<object> endBoss;

    #endregion Private Fields

    #region Unity Methods

    private void Awake()
    {
        updateLevelUI = new UnityAction<object>(UpdateLevelUI);
        EventManager.StartListening("UpdateLevelUI", updateLevelUI);

        upgrade = new UnityAction<object>(SetSkillDescriptionText);
        EventManager.StartListening("Upgrade", upgrade);

        togglePrestige = new UnityAction<object>(SetPrestigeButtonStatus);
        EventManager.StartListening("TogglePrestige", togglePrestige);
    }

    // Start is called before the first frame update
    private void Start()
    {
        shopPanel.SetActive(false);
        currentMoney = GameObject.FindGameObjectWithTag("CurrentMoney").GetComponent<TextMeshProUGUI>();
        currentPrestigeMoney = GameObject.FindGameObjectWithTag("CurrentPrestigeMoney").GetComponent<TextMeshProUGUI>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        startBoss = new UnityAction<object>(StartBossFight);
        EventManager.StartListening("StartBoss", startBoss);

        setBossTimer = new UnityAction<object>(SetBossTimer);
        EventManager.StartListening("SetBossTimer", setBossTimer);

        endBoss = new UnityAction<object>(EndBossFight);
        EventManager.StartListening("EndBoss", endBoss);
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

            SetSkillDescriptionText((b, s));
        }
    }

    public void SetSkillPanelsVisibility(Dictionary<string, Skill> skillDictionary)
    {
        for (int i = 1; i < damagePanels.Length; i++)
        {
            damagePanels[i].SetActive(skillDictionary["Damage" + i].level > 9);
        }
    }

    public void SetSkillButtonStatuses(Dictionary<string, Skill> skillDictionary, long money, long prestigeMoney)
    {
        foreach (Button b in buttons)
        {
            string skillName = b.name;
            Skill s = skillDictionary[skillName];
            if (s == null)
                continue;

            if (s.isPrestige)
                b.interactable = s.CheckUpgrade(prestigeMoney);
            else
                b.interactable = s.CheckUpgrade(money);
        }
    }

    private void SetSkillDescriptionText(object buttonAndSkill)
    {
        (Button, Skill) input = ((Button, Skill))buttonAndSkill;
        Button skillButton = input.Item1;
        Skill skill = input.Item2;

        //var description = skillButton.gameObject.transform.Find("Description").gameObject.GetComponent<Text>();
        var parent = skillButton.gameObject.transform.parent;

        parent.Find("Level").gameObject.GetComponent<TextMeshProUGUI>().text = "Lv. " + skill.level;

        var bonus = parent.Find("Bonus").gameObject.GetComponent<TextMeshProUGUI>();

        string bonusText = "";

        switch (skill.type)
        {
            case SkillType.DMG:
                bonusText += "+" + string.Format("{0:0.##}", skill.nextStatIncrease) + " dmg";
                break;

            case SkillType.HELPER:
                bonusText += "+" + (skill.nextStatIncrease * 100.0f) + "%";
                break;

            default:
                break;
        }

        bonus.text = bonusText;
        skillButton.gameObject.transform.Find("Price").gameObject.GetComponent<TextMeshProUGUI>().text = skill.upgradeCost.ToString();
    }

    /// <summary>
    /// Called by shop when the player's money changes.
    /// </summary>
    /// <param name="money"></param>
    public void SetMoneyText(long money)
    {
        currentMoney.text = money.ToString();
    }

    public void SetPrestigeMoneyText(long money)
    {
        currentPrestigeMoney.text = money.ToString();
    }

    public void SetPendingPrestigeMoneyText(long money)
    {
        pendingPrestigeMoney.text = "+" + money;
    }

    public void SetPrestigeButtonStatus(object status)
    {
        prestigeButton.interactable = (bool)status;
    }

    public void UpdateLevelUI(System.Object level)
    {
        MapLevel mapLevel = (MapLevel)level;

        var mapLevelInfo = mapLevelPanel.transform.Find("LevelInfo");

        string text = "Lvl:" + mapLevel.mapLevelKey + "\n" + mapLevel.currentCount + "/" + mapLevel.maxCount;

        mapLevelInfo.gameObject.GetComponent<TextMeshProUGUI>().text = text;

        var nextButton = mapLevelPanel.transform.Find("NextLevel");
        var prevButton = mapLevelPanel.transform.Find("PrevLevel");

        nextButton.gameObject.GetComponent<Button>().interactable = mapLevel.currentCount >= mapLevel.maxCount;
        prevButton.gameObject.GetComponent<Button>().interactable = mapLevel.mapLevelKey != 0;
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
        EventManager.TriggerEvent("TryUpgrade", skillButton);
    }

    public void SwitchPanel(GameObject panel)
    {
        foreach (GameObject p in panels)
        {
            p.SetActive(false);
        }

        panel.SetActive(true);
    }

    public void TryPrestige()
    {
        EventManager.TriggerEvent("TryPrestige", null);
    }

    public void NextLevel()
    {
        EventManager.TriggerEvent("TryNextLevel", null);
    }

    public void PrevLevel()
    {
        EventManager.TriggerEvent("TryPrevLevel", null);
    }

    private void StartBossFight(object fightTime)
    {
        float time = (float)fightTime;
        bossTimerSlider.maxValue = time;
        bossTimerSlider.minValue = 0.0f;
        bossTimerSlider.value = time;
        bossTimerPanel.SetActive(true);
    }

    private void SetBossTimer(object fightTime)
    {
        float time = (float)fightTime;
        bossTimerSlider.value = time;
    }

    private void EndBossFight(object unused)
    {
        bossTimerPanel.SetActive(false);
    }

    #endregion Skill UI Methods
}