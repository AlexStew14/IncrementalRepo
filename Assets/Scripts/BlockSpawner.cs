using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public Dictionary<int, Transform> BlockDictionary { get; private set; }

    public int TotalBlocksSpawned { get; private set; }

    [SerializeField]
    private Sprite[] currentBlockSpriteArray;

    #endregion Fields

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        TotalBlocksSpawned = (int)dataSavingManager.GetOtherValue("TotalBlocksSpawned");
        blockSpawnData = dataSavingManager.GetBlockSpawnData();
        BlockDictionary = new Dictionary<int, Transform>();
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
        Vector2 randPos = new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-4f, 4));
        Transform block = Instantiate(blockPrefab, randPos, transform.rotation);
        block.GetComponent<Block>().blockKey = TotalBlocksSpawned;

        SpriteRenderer blockSprite = block.gameObject.GetComponent<SpriteRenderer>();

        blockSprite.sprite = currentBlockSpriteArray[(int)UnityEngine.Random.Range(0f, 5f)];

        dataSavingManager.SetOtherValue("TotalBlocksSpawned", TotalBlocksSpawned);

        BlockDictionary.Add(block.GetComponent<Block>().blockKey, block);

        ++TotalBlocksSpawned;
        ++currentBlockCount;
    }

    public void BlockDestroyed(Transform block)
    {
        --currentBlockCount;
        BlockDictionary.Remove(block.GetComponent<Block>().blockKey);
    }

    public void FlatSpawnSpeedIncrease(float spawnSpeedIncrease)
    {
        blockSpawnData.spawnTime -= spawnSpeedIncrease;
        dataSavingManager.SetBlockSpawnData(blockSpawnData);
        dataSavingManager.Save();
    }

    #endregion Block Spawning Methods
}

[Serializable]
public class BlockSpawnData
{
    public int maxCurrentBlocks;

    public float spawnTime;
}