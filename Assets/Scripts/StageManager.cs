using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    private GameObject stageBackground;

    private Animator anim;

    private DataSavingManager dataSavingManager;

    public long blockKillReward { get; private set; }

    public long blockKillPrestigeReward { get; private set; }

    public float blockHealth { get; private set; }

    private Func<int, long> killRewardFunc = x => (long)Math.Round(Math.Pow(1.3, x)) + x;

    private Func<int, float> blockHealthFunc = x => 5 * Mathf.Pow(1.5f, x);

    private Func<int, long> prestigeKillRewardFunc = x => (long)Math.Round(Math.Pow(1.12, x - 100));

    public Stage currentStage { get; private set; }

    private MapLevel currentMapLevel;

    private UnityAction<object> blockKilled;

    private UnityAction<object> tryPrestige;

    private UnityAction<object> tryNextLevel;

    private UnityAction<object> tryPrevLevel;

    // Start is called before the first frame update
    private void Start()
    {
        anim = stageBackground.GetComponent<Animator>();
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();

        blockKilled = new UnityAction<object>(BlockKilled);
        EventManager.StartListening("BlockKilled", blockKilled);

        tryPrestige = new UnityAction<object>(Prestige);
        EventManager.StartListening("TryPrestige", tryPrestige);

        tryNextLevel = new UnityAction<object>(NextLevel);
        EventManager.StartListening("TryNextLevel", tryNextLevel);

        tryPrevLevel = new UnityAction<object>(PrevLevel);
        EventManager.StartListening("TryPrevLevel", tryPrevLevel);

        EventManager.TriggerEvent("TogglePrestige", CanPrestige());

        InitalizeStageAndMapLevel();
    }

    private void BlockKilled(object killed)
    {
        currentMapLevel.KilledBlock();

        EventManager.TriggerEvent("UpdateLevelUI", currentMapLevel);
    }

    private void InitalizeStageAndMapLevel()
    {
        currentMapLevel = dataSavingManager.GetMapLevel((int)dataSavingManager.GetOtherValue("CurrentMapLevel"));
        currentStage = dataSavingManager.GetStage((int)dataSavingManager.GetOtherValue("CurrentStage"));

        blockHealth = blockHealthFunc(currentMapLevel.mapLevelKey);
        blockKillReward = killRewardFunc(currentMapLevel.mapLevelKey);

        if (currentMapLevel.mapLevelKey > 100)
            blockKillPrestigeReward = prestigeKillRewardFunc(currentMapLevel.mapLevelKey);
        else
            blockKillPrestigeReward = 0;

        anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(currentStage.animatorName);

        EventManager.TriggerEvent("LoadStage", currentStage);

        EventManager.TriggerEvent("UpdateLevelUI", currentMapLevel);
    }

    private void SwitchMapLevel(int mapLevelKey)
    {
        var mapLevel = dataSavingManager.GetMapLevel(mapLevelKey);
        if (mapLevel == null)
        {
            mapLevel = new MapLevel
            {
                completed = false,
                mapLevelKey = mapLevelKey,
                currentCount = 0,
                maxCount = (int)dataSavingManager.GetOtherValue("MaxKillCount")
            };

            dataSavingManager.AddMapLevel(mapLevelKey, mapLevel);

            if (mapLevelKey == 101)
                EventManager.TriggerEvent("TogglePrestige", true);
        }

        currentMapLevel = mapLevel;
        dataSavingManager.SetOtherValue("CurrentMapLevel", mapLevelKey);
        dataSavingManager.Save();

        int mapLevelRemainder = mapLevelKey % (int)dataSavingManager.GetOtherValue("LevelPerStage");

        if (mapLevelRemainder == 0)
        {
            SwitchStage(mapLevelKey % 100);
        }
        else if (mapLevelRemainder == (int)dataSavingManager.GetOtherValue("LevelPerStage") - 1)
        {
            SwitchStage((mapLevelKey % 100) - mapLevelRemainder);
        }

        blockHealth = blockHealthFunc(mapLevelKey);
        blockKillReward = killRewardFunc(mapLevelKey);

        if (currentMapLevel.mapLevelKey > 100)
            blockKillPrestigeReward = prestigeKillRewardFunc(currentMapLevel.mapLevelKey);
        else
            blockKillPrestigeReward = 0;

        EventManager.TriggerEvent("LoadMapLevel");

        EventManager.TriggerEvent("UpdateLevelUI", currentMapLevel);
    }

    private void SwitchStage(int stageKey)
    {
        currentStage = dataSavingManager.GetStage(stageKey);
        dataSavingManager.SetOtherValue("CurrentStage", stageKey);
        dataSavingManager.Save();

        anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(currentStage.animatorName);

        EventManager.TriggerEvent("LoadStage", currentStage);
    }

    private void NextLevel(object unused)
    {
        if (currentMapLevel.completed)
            SwitchMapLevel(currentMapLevel.mapLevelKey + 1);
    }

    private void PrevLevel(object unused)
    {
        if (currentMapLevel.mapLevelKey > 0)
            SwitchMapLevel(currentMapLevel.mapLevelKey - 1);
    }

    public bool CanPrestige()
    {
        return dataSavingManager.GetMapLevel(101) != null;
    }

    private void Prestige(object unused)
    {
        if (!CanPrestige())
            return;

        dataSavingManager.ClearMapLevelDictionary();
        SwitchMapLevel(0);

        EventManager.TriggerEvent("Prestige", null);
    }
}