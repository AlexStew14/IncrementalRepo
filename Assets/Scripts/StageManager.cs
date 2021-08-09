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

    public double blockKillReward { get; private set; }

    public double blockKillPrestigeReward { get; private set; }

    public double blockHealth { get; private set; }

    public bool bossLevel { get; private set; }

    private Func<int, double> killRewardFunc = x => Math.Pow(1.05, x);

    private Func<int, double> blockHealthFunc = x => (Math.Pow(1.08, x) / (.15 * ((x / 10) + 1))) - 3;

    private Func<int, double> prestigeKillRewardFunc = x => Math.Round(Math.Log10(x - 99.9)) + 1;

    public Stage currentStage { get; private set; }

    private MapLevel currentMapLevel;

    private UnityAction<object> blockKilled;
    private UnityAction<object> tryPrestige;
    private UnityAction<object> tryNextLevel;
    private UnityAction<object> tryPrevLevel;
    private UnityAction<object> failedBoss;
    private UnityAction<object> killedBoss;
    private UnityAction<object> toggleAutoStage;

    private bool autoStage;

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

        failedBoss = new UnityAction<object>(FailedBoss);
        EventManager.StartListening("FailedBoss", failedBoss);

        killedBoss = new UnityAction<object>(KilledBoss);
        EventManager.StartListening("KilledBoss", killedBoss);

        toggleAutoStage = new UnityAction<object>(ToggleAutoStage);
        EventManager.StartListening("ToggleAutoStage", toggleAutoStage);

        EventManager.TriggerEvent("TogglePrestige", CanPrestige());

        InitalizeStageAndMapLevel();
        Invoke("HandleOfflineProgress", 1.0f);
    }

    private void BlockKilled(object killed)
    {
        currentMapLevel.KilledBlock();

        dataSavingManager.SetOtherValue("TotalBlocksKilled", (int)dataSavingManager.GetOtherValue("TotalBlocksKilled") + 1);

        EventManager.TriggerEvent("UpdateLevelUI", currentMapLevel);
        EventManager.TriggerEvent("BlockStatsUpdate", Tuple.Create(blockHealth, blockKillReward));

        if (autoStage)
            NextLevel(null);
    }

    private void InitalizeStageAndMapLevel()
    {
        currentMapLevel = dataSavingManager.GetMapLevel((int)dataSavingManager.GetOtherValue("CurrentMapLevel"));

        if (currentMapLevel.mapLevelKey % 10 == 9)
            currentMapLevel = dataSavingManager.GetMapLevel(currentMapLevel.mapLevelKey - 1);

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

    private void HandleOfflineProgress()
    {
        if ((bool)dataSavingManager.GetOtherValue("UnlockedAuto"))
        {
            PlayerData playerData = dataSavingManager.GetPlayerData();
            double secondsOffline = (DateTime.Now - (DateTime)dataSavingManager.GetOtherValue("TimeStamp")).TotalSeconds;

            double blocksKilled;

            if (playerData.baseDamage >= blockHealth)
                blocksKilled = secondsOffline / playerData.baseAttackSpeed;
            else
            {
                double dps = (1 / playerData.baseAttackSpeed) * playerData.baseDamage;

                double totalDamage = secondsOffline * dps;
                blocksKilled = totalDamage / blockHealth;
            }

            double offlineReward = blocksKilled * blockKillReward * (double)dataSavingManager.GetOtherValue("OfflineMultiplier");

            Debug.LogWarning("seconds offline: " + secondsOffline);

            dataSavingManager.SetOtherValue("TimeStamp", DateTime.Now);
            dataSavingManager.Save();

            EventManager.TriggerEvent("OfflineProgress", offlineReward);
        }
        EventManager.TriggerEvent("BlockStatsUpdate", Tuple.Create(blockHealth, blockKillReward));
    }

    private void SwitchMapLevel(int mapLevelKey)
    {
        var mapLevel = dataSavingManager.GetMapLevel(mapLevelKey);
        if (mapLevel == null)
        {
            int maxCount;
            if (mapLevelKey % 10 == 9)
                maxCount = 1;
            else
                maxCount = (int)dataSavingManager.GetOtherValue("BlocksPerLevel");

            mapLevel = new MapLevel
            {
                completed = false,
                mapLevelKey = mapLevelKey,
                currentCount = 0,
                maxCount = maxCount
            };

            dataSavingManager.AddMapLevel(mapLevelKey, mapLevel);

            if (mapLevelKey == 101)
                EventManager.TriggerEvent("TogglePrestige", true);
            else if (mapLevelKey == 10)
            {
                EventManager.TriggerEvent("UnlockedAuto");
                dataSavingManager.SetOtherValue("UnlockedAuto", true);
                dataSavingManager.Save();
            }

            dataSavingManager.SetOtherValue("HighestLevelReached", mapLevelKey);
            dataSavingManager.Save();
            EventManager.TriggerEvent("MiscStatsUpdate");
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

        if (mapLevelKey % 10 == 9)
        {
            blockHealth *= 15;
            blockKillReward *= 10;
            bossLevel = true;
        }
        else
            bossLevel = false;

        if (currentMapLevel.mapLevelKey > 100)
            blockKillPrestigeReward = prestigeKillRewardFunc(currentMapLevel.mapLevelKey);
        else
            blockKillPrestigeReward = 0;

        EventManager.TriggerEvent("LoadMapLevel", currentMapLevel);

        EventManager.TriggerEvent("UpdateLevelUI", currentMapLevel);

        EventManager.TriggerEvent("BlockStatsUpdate", Tuple.Create(blockHealth, blockKillReward));
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

    private void FailedBoss(object unused)
    {
        SwitchMapLevel(currentMapLevel.mapLevelKey);
    }

    private void KilledBoss(object unused)
    {
        SwitchMapLevel(currentMapLevel.mapLevelKey + 1);
    }

    private void ToggleAutoStage(object unused)
    {
        autoStage = !autoStage;
        if (autoStage)
            NextLevel(null);
    }
}