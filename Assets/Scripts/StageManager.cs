using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    private GameObject stageBackground;

    private Animator anim;

    private SpriteRenderer spriteRenderer;

    private DataSavingManager dataSavingManager;

    private BlockSpawner blockSpawner;

    private Shop shop;

    private int blockKillReward;

    public float blockHealth { get; private set; }

    private Func<int, int> killRewardFunc = x => (int)Mathf.Ceil(Mathf.Pow(1.3f, x)) + x;

    private Func<int, float> blockHealthFunc = x => 5 * Mathf.Pow(1.5f, x);

    public Stage currentStage { get; private set; }

    private MapLevel currentMapLevel;

    private UIManager uiManager;

    // Start is called before the first frame update
    private void Start()
    {
        anim = stageBackground.GetComponent<Animator>();
        spriteRenderer = stageBackground.GetComponent<SpriteRenderer>();
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        blockSpawner = GameObject.FindGameObjectWithTag("BlockSpawner").GetComponent<BlockSpawner>();
        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();

        InitalizeStageAndMapLevel();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void KilledBlock()
    {
        currentMapLevel.KilledBlock();

        shop.KilledBlock(blockKillReward);

        uiManager.SetMapLevelText(currentMapLevel.currentCount, currentMapLevel.maxCount, currentMapLevel.mapLevelKey);
    }

    private void InitalizeStageAndMapLevel()
    {
        currentMapLevel = dataSavingManager.GetMapLevel((int)dataSavingManager.GetOtherValue("CurrentMapLevel"));
        currentStage = dataSavingManager.GetStage((int)dataSavingManager.GetOtherValue("CurrentStage"));

        blockHealth = blockHealthFunc(currentMapLevel.mapLevelKey);
        blockKillReward = killRewardFunc(currentMapLevel.mapLevelKey);

        anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(currentStage.animatorName);
        blockSpawner.currentBlockSpriteArray = Resources.LoadAll<Sprite>(currentStage.blockSpritesPath);

        uiManager.SetMapLevelText(currentMapLevel.currentCount, currentMapLevel.maxCount, currentMapLevel.mapLevelKey);
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
                maxCount = 25
            };

            dataSavingManager.AddMapLevel(mapLevelKey, mapLevel);
        }

        currentMapLevel = mapLevel;
        dataSavingManager.SetOtherValue("CurrentMapLevel", mapLevelKey);
        dataSavingManager.Save();

        if (mapLevelKey % 10 == 0)
        {
            SwitchStage(mapLevelKey % 100);
        }

        blockHealth = blockHealthFunc(mapLevelKey);
        blockKillReward = killRewardFunc(mapLevelKey);

        uiManager.SetMapLevelText(currentMapLevel.currentCount, currentMapLevel.maxCount, currentMapLevel.mapLevelKey);

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
}