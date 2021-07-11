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

    private SpriteRenderer spriteRenderer;

    private DataSavingManager dataSavingManager;

    private BlockSpawner blockSpawner;

    private Shop shop;

    public long blockKillReward { get; private set; }

    public float blockHealth { get; private set; }

    private Func<int, long> killRewardFunc = x => (long)Math.Round(Math.Pow(1.3, x)) + x;

    private Func<int, float> blockHealthFunc = x => 5 * Mathf.Pow(1.5f, x);

    private Func<int, long> prestigeKillRewardFunc = x => (long)Math.Round(Math.Pow(1.12, x - 100));

    public Stage currentStage { get; private set; }

    private MapLevel currentMapLevel;

    private UIManager uiManager;

    private UnityAction<System.Object> blockKilled;

    // Start is called before the first frame update
    private void Start()
    {
        anim = stageBackground.GetComponent<Animator>();
        spriteRenderer = stageBackground.GetComponent<SpriteRenderer>();
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        blockSpawner = GameObject.FindGameObjectWithTag("BlockSpawner").GetComponent<BlockSpawner>();
        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();

        blockKilled = new UnityAction<object>(BlockKilled);

        EventManager.StartListening("BlockKilled", blockKilled);

        uiManager.SetPrestigeButtonStatus(CanPrestige());

        InitalizeStageAndMapLevel();
    }

    public void BlockKilled(System.Object killed)
    {
        currentMapLevel.KilledBlock();

        if (currentMapLevel.mapLevelKey > 100)
        {
            shop.AddPendingPrestigeMoney(prestigeKillRewardFunc(currentMapLevel.mapLevelKey));
        }

        EventManager.TriggerEvent("UpdateLevelUI", currentMapLevel);
    }

    private void InitalizeStageAndMapLevel()
    {
        currentMapLevel = dataSavingManager.GetMapLevel((int)dataSavingManager.GetOtherValue("CurrentMapLevel"));
        currentStage = dataSavingManager.GetStage((int)dataSavingManager.GetOtherValue("CurrentStage"));

        blockHealth = blockHealthFunc(currentMapLevel.mapLevelKey);
        blockKillReward = killRewardFunc(currentMapLevel.mapLevelKey);

        anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(currentStage.animatorName);
        blockSpawner.currentBlockSpriteArray = Resources.LoadAll<Sprite>(currentStage.blockSpritesPath);

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
                uiManager.SetPrestigeButtonStatus(true);
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

        blockSpawner.ClearBlocks();
    }

    private void SwitchStage(int stageKey)
    {
        currentStage = dataSavingManager.GetStage(stageKey);
        dataSavingManager.SetOtherValue("CurrentStage", stageKey);
        dataSavingManager.Save();
        anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(currentStage.animatorName);
        blockSpawner.currentBlockSpriteArray = Resources.LoadAll<Sprite>(currentStage.blockSpritesPath);
    }

    public void NextLevel()
    {
        if (currentMapLevel.completed)
            SwitchMapLevel(currentMapLevel.mapLevelKey + 1);
    }

    public void PrevLevel()
    {
        if (currentMapLevel.mapLevelKey > 0)
            SwitchMapLevel(currentMapLevel.mapLevelKey - 1);
    }

    public bool CanPrestige()
    {
        return dataSavingManager.GetMapLevel(101) != null;
    }

    public void Prestige()
    {
        if (!CanPrestige())
            return;

        dataSavingManager.ClearMapLevelDictionary();
        SwitchMapLevel(0);
        shop.Prestige();
    }
}