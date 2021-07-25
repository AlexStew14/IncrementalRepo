using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
    private Animator currentMoneyAnimator;

    private TextMeshProUGUI currentPrestigeMoney;

    [SerializeField]
    private TextMeshProUGUI pendingPrestigeMoney;

    private Player player;

    private bool autoUnlocked = false;
    private bool autoMoveEnabled = false;
    private bool autoStageEnabled = false;

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
    private GameObject autoMoveButton;

    [SerializeField]
    private GameObject autoStageButton;

    [SerializeField]
    private GameObject[] abilityPanels;

    [SerializeField]
    private GameObject[] damagePanels;

    [SerializeField]
    private GameObject mapLevelPanel;

    [SerializeField]
    private GameObject bossTimerPanel;

    [SerializeField]
    private GameObject offlinePanel;

    [SerializeField]
    private Slider bossTimerSlider;

    private UnityAction<object> updateLevelUI;
    private UnityAction<object> upgrade;
    private UnityAction<object> togglePrestige;
    private UnityAction<object> startBoss;
    private UnityAction<object> setBossTimer;
    private UnityAction<object> endBoss;
    private UnityAction<object> goldArrived;
    private UnityAction<object> unlockedAuto;
    private UnityAction<object> offlineProgress;

    private DataSavingManager dataSavingManager;

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
        autoMoveButton.SetActive(false);
        autoStageButton.SetActive(false);
        offlinePanel.SetActive(false);

        currentMoney = GameObject.FindGameObjectWithTag("CurrentMoney").GetComponent<TextMeshProUGUI>();
        currentMoneyAnimator = currentMoney.GetComponent<Animator>();
        currentPrestigeMoney = GameObject.FindGameObjectWithTag("CurrentPrestigeMoney").GetComponent<TextMeshProUGUI>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        startBoss = new UnityAction<object>(StartBossFight);
        EventManager.StartListening("StartBoss", startBoss);

        setBossTimer = new UnityAction<object>(SetBossTimer);
        EventManager.StartListening("SetBossTimer", setBossTimer);

        endBoss = new UnityAction<object>(EndBossFight);
        EventManager.StartListening("EndBoss", endBoss);

        goldArrived = new UnityAction<object>(GoldArrived);
        EventManager.StartListening("GoldArrived", goldArrived);

        unlockedAuto = new UnityAction<object>(UnlockedAuto);
        EventManager.StartListening("UnlockedAuto", unlockedAuto);

        offlineProgress = new UnityAction<object>(HandleOfflineProgress);
        EventManager.StartListening("OfflineProgress", offlineProgress);

        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        if ((bool)dataSavingManager.GetOtherValue("UnlockedAuto"))
            UnlockedAuto(null);
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
        for (int i = 1; i < abilityPanels.Length; i++)
        {
            abilityPanels[i].SetActive(skillDictionary["Ability" + i].level > 9);
        }
    }

    public void SetDamagePanelsVisibilty(Dictionary<string, Skill> skillDictionary)
    {
        for (int i = 1; i < damagePanels.Length; i++)
        {
            damagePanels[i].SetActive(skillDictionary["Damage" + i].level > 9);
        }
    }

    public void SetSkillButtonStatuses(Dictionary<string, Skill> skillDictionary, double money, double prestigeMoney)
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

        var name = parent.Find("Name").gameObject.GetComponent<TextMeshProUGUI>();

        string nameText = "";
        string bonusText = "";

        switch (skill.type)
        {
            case SkillType.DMG:
                bonusText += "+" + NumberUtils.FormatLargeNumbers(skill.nextStatIncrease);
                nameText = "Base Damage\nTotal: " + NumberUtils.FormatLargeNumbers(skill.totalStatIncrease);
                break;

            case SkillType.ATTKSPEED:
                bonusText += string.Format("-{0:P2}", 1 - skill.nextStatIncrease);
                nameText = "Attack Delay\nTotal: " + skill.totalStatIncrease;
                break;

            case SkillType.MOVEMENTSPEED:
                bonusText += "+" + skill.nextStatIncrease;
                nameText = "Movement Speed\nTotal: " + skill.totalStatIncrease;
                break;

            case SkillType.SPAWNSPEED:
                bonusText += string.Format("-{0:P2}", 1 - skill.nextStatIncrease);
                nameText = "Block Spawn Time\nTotal: " + skill.totalStatIncrease;
                break;

            case SkillType.HELPER:
                bonusText += "+" + (skill.nextStatIncrease * 100.0f) + "%";
                break;

            case SkillType.ABILITY:
                Ability a = skill as Ability;
                bonusText = "+" + NumberUtils.FormatLargeNumbers(a.nextStatIncrease)
                    + "x " + Ability.FormatSubType(a.abilitySubType)
                    + string.Format("\n{0:P2} Chance", a.nextChanceIncrease);

                nameText = NumberUtils.FormatLargeNumbers(a.totalStatIncrease)
                    + "x " + Ability.FormatSubType(a.abilitySubType)
                    + string.Format("\n{0:P2} Chance", a.activationChance);
                break;

            default:
                break;
        }

        bonus.text = bonusText;
        name.text = nameText;

        string upgradeText = "Maxed";
        if (skill.level < skill.maxLevel)
            upgradeText = NumberUtils.FormatLargeNumbers(skill.upgradeCost);

        skillButton.gameObject.transform.Find("Price").gameObject.GetComponent<TextMeshProUGUI>().text = upgradeText;
    }

    /// <summary>
    /// Called by shop when the player's money changes.
    /// </summary>
    /// <param name="money"></param>
    public void SetMoneyText(double money)
    {
        currentMoney.text = NumberUtils.FormatLargeNumbers(money);
    }

    public void SetPrestigeMoneyText(double money)
    {
        currentPrestigeMoney.text = NumberUtils.FormatLargeNumbers(money);
    }

    public void SetPendingPrestigeMoneyText(double money)
    {
        pendingPrestigeMoney.text = "+" + NumberUtils.FormatLargeNumbers(money);
    }

    public void SetPrestigeButtonStatus(object status)
    {
        prestigeButton.interactable = (bool)status;
    }

    public void UpdateLevelUI(object level)
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
        autoMoveButton.SetActive(!autoMoveButton.activeSelf && autoUnlocked);
        autoStageButton.SetActive(!autoStageButton.activeSelf && autoUnlocked);
        player.StopMoving();
    }

    /// <summary>
    /// This method is called by clicking the shop buttons for skill upgrades.
    /// The method takes in the button that clicks it.
    /// This calls UpgradeSkill on the shop and sets the description text for display.
    /// </summary>
    /// <param name="skillButton"></param>
    public void UpgradeSkill(Button skillButton)
    {
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
        double time = (double)fightTime;
        bossTimerSlider.value = (float)time;
    }

    private void EndBossFight(object unused)
    {
        bossTimerPanel.SetActive(false);
    }

    private void GoldArrived(object unused)
    {
        currentMoneyAnimator.Play("moneyBounce", -1, 0f);
    }

    public void ToggleAutoMove()
    {
        player.StopMoving();
        autoMoveEnabled = !autoMoveEnabled;
        if (autoMoveEnabled)
            autoMoveButton.GetComponent<Image>().color = Color.green;
        else
            autoMoveButton.GetComponent<Image>().color = Color.red;

        EventManager.TriggerEvent("ToggleAutoMove");
    }

    public void ToggleAutoStage()
    {
        player.StopMoving();
        autoStageEnabled = !autoStageEnabled;
        if (autoStageEnabled)
            autoStageButton.GetComponent<Image>().color = Color.green;
        else
            autoStageButton.GetComponent<Image>().color = Color.red;

        EventManager.TriggerEvent("ToggleAutoStage");
    }

    private void UnlockedAuto(object unused)
    {
        autoMoveButton.SetActive(true);
        autoStageButton.SetActive(true);
        autoUnlocked = true;
    }

    private void HandleOfflineProgress(object offlineMoney)
    {
        offlinePanel.SetActive(true);
        player.StopMoving();

        TextMeshProUGUI rewardText = offlinePanel.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        rewardText.text = "Welcome Back!\nYou've Earned " + NumberUtils.FormatLargeNumbers((double)offlineMoney) + " Gold while away!";
        GoldArrived(null);
    }

    public void CloseOfflinePanel()
    {
        offlinePanel.SetActive(false);
    }

    #endregion Skill UI Methods
}