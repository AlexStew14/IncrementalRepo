using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class BlockSpawner : MonoBehaviour
{
    #region Fields

    // Set in inspector
    public Transform blockPrefab;

    private float timer = -1.0f;

    private float bossTimer = 30.0f;

    private int currentBlockCount = 0;

    private DataSavingManager dataSavingManager;

    [SerializeField]
    private BlockSpawnData blockSpawnData;

    public Dictionary<int, Transform> blockDictionary { get; private set; }

    public int TotalBlocksSpawned { get; private set; }

    [HideInInspector]
    public Sprite[] currentBlockSpriteArray;

    private UnityAction<object> blockKilled;

    private UnityAction<object> loadStage;

    private UnityAction<object> loadMapLevel;

    private bool fightingBoss;

    public Transform bossPrefab;

    #endregion Fields

    #region Unity Methods

    private void Awake()
    {
        loadStage = new UnityAction<object>(LoadStage);
        EventManager.StartListening("LoadStage", loadStage);
    }

    // Start is called before the first frame update
    private void Start()
    {
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        TotalBlocksSpawned = (int)dataSavingManager.GetOtherValue("TotalBlocksSpawned");
        blockSpawnData = dataSavingManager.GetBlockSpawnData();
        blockDictionary = new Dictionary<int, Transform>();

        loadMapLevel = new UnityAction<object>(HandleLevelChange);
        EventManager.StartListening("LoadMapLevel", loadMapLevel);

        blockKilled = new UnityAction<object>(BlockKilled);
        EventManager.StartListening("BlockKilled", blockKilled);
    }

    // Update is called once per frame
    private void Update()
    {
        if (fightingBoss)
        {
            bossTimer -= Time.deltaTime;
            EventManager.TriggerEvent("SetBossTimer", bossTimer);
            if (bossTimer <= 0f)
            {
                EventManager.TriggerEvent("FailedBoss");
            }
            return;
        }

        if (currentBlockCount < blockSpawnData.maxCurrentBlocks)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                CreateBlock(false);
                timer = blockSpawnData.spawnTime;
            }
        }
    }

    #endregion Unity Methods

    #region Block Spawning Methods

    private void CreateBlock(bool createBoss)
    {
        Vector2 randPos = new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-4f, 4f));
        Transform block;
        if (createBoss)
            block = Instantiate(bossPrefab, new Vector2(0, -1), transform.rotation);
        else
            block = Instantiate(blockPrefab, randPos, transform.rotation);

        block.GetComponent<Block>().blockKey = TotalBlocksSpawned;

        SpriteRenderer blockSprite = block.gameObject.GetComponent<SpriteRenderer>();

        blockSprite.sprite = currentBlockSpriteArray[(int)UnityEngine.Random.Range(0f, currentBlockSpriteArray.Length)];

        dataSavingManager.SetOtherValue("TotalBlocksSpawned", TotalBlocksSpawned);

        blockDictionary.Add(block.GetComponent<Block>().blockKey, block);

        ++TotalBlocksSpawned;
        ++currentBlockCount;
    }

    private void BlockKilled(object b)
    {
        Block deadBlock = (Block)b;
        --currentBlockCount;
        blockDictionary.Remove(deadBlock.blockKey);
    }

    public void FlatSpawnSpeedIncrease(float spawnSpeedIncrease)
    {
        blockSpawnData.spawnTime -= spawnSpeedIncrease;
        dataSavingManager.SetBlockSpawnData(blockSpawnData);
        dataSavingManager.Save();
    }

    public bool ContainsBlock(Transform block)
    {
        if (block == null)
            return false;
        return blockDictionary.ContainsKey(block.gameObject.GetComponent<Block>().blockKey);
    }

    private void HandleLevelChange(object level)
    {
        MapLevel mapLevel = (MapLevel)level;

        if (blockDictionary != null)
        {
            // Clear all blocks from the map
            foreach (var k in blockDictionary.Keys)
            {
                --currentBlockCount;
                blockDictionary[k].gameObject.GetComponent<Block>().DestroyBlock();
            }
            blockDictionary.Clear();
        }

        // Handle boss levels
        if (mapLevel.mapLevelKey % 10 == 9)
        {
            StartBossFight();
        }
        else
        {
            fightingBoss = false;
            EventManager.TriggerEvent("EndBoss");
        }
    }

    private void StartBossFight()
    {
        fightingBoss = true;
        if (blockDictionary != null)
        {
            EventManager.TriggerEvent("StartBoss", 30.0f);
            bossTimer = 30.0f;
            CreateBlock(true);
        }
    }

    private void LoadStage(object stage)
    {
        Stage currentStage = (Stage)stage;
        currentBlockSpriteArray = Resources.LoadAll<Sprite>(currentStage.blockSpritesPath);
    }

    #endregion Block Spawning Methods
}

[Serializable]
public class BlockSpawnData
{
    public int maxCurrentBlocks;

    public float spawnTime;
}