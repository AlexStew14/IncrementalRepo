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

    private int currentBlockCount = 0;

    private DataSavingManager dataSavingManager;

    [SerializeField]
    private BlockSpawnData blockSpawnData;

    public Dictionary<int, Transform> blockDictionary { get; private set; }

    public int TotalBlocksSpawned { get; private set; }

    [HideInInspector]
    public Sprite[] currentBlockSpriteArray;

    private UnityAction<System.Object> blockKilled;

    private UnityAction<object> loadStage;

    private UnityAction<object> loadMapLevel;

    #endregion Fields

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        TotalBlocksSpawned = (int)dataSavingManager.GetOtherValue("TotalBlocksSpawned");
        blockSpawnData = dataSavingManager.GetBlockSpawnData();
        blockDictionary = new Dictionary<int, Transform>();

        blockKilled = new UnityAction<object>(BlockKilled);
        EventManager.StartListening("BlockKilled", blockKilled);

        loadStage = new UnityAction<object>(LoadStage);
        EventManager.StartListening("LoadStage", loadStage);

        loadMapLevel = new UnityAction<object>(ClearBlocks);
        EventManager.StartListening("LoadMapLevel", loadMapLevel);
    }

    // Update is called once per frame
    private void Update()
    {
        if (currentBlockCount < blockSpawnData.maxCurrentBlocks)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                CreateBlock();
                timer = blockSpawnData.spawnTime;
            }
        }
    }

    #endregion Unity Methods

    #region Block Spawning Methods

    private void CreateBlock()
    {
        Vector2 randPos = new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-4f, 4f));
        Transform block = Instantiate(blockPrefab, randPos, transform.rotation);
        block.GetComponent<Block>().blockKey = TotalBlocksSpawned;

        SpriteRenderer blockSprite = block.gameObject.GetComponent<SpriteRenderer>();

        blockSprite.sprite = currentBlockSpriteArray[(int)UnityEngine.Random.Range(0f, currentBlockSpriteArray.Length)];

        dataSavingManager.SetOtherValue("TotalBlocksSpawned", TotalBlocksSpawned);

        blockDictionary.Add(block.GetComponent<Block>().blockKey, block);

        ++TotalBlocksSpawned;
        ++currentBlockCount;
    }

    public void BlockKilled(System.Object b)
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

    public void ClearBlocks(object unused)
    {
        foreach (var k in blockDictionary.Keys)
        {
            --currentBlockCount;
            blockDictionary[k].gameObject.GetComponent<Block>().DestroyBlock();
        }
        blockDictionary.Clear();
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