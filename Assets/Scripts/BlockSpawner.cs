using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BlockSpawner : MonoBehaviour
{
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

    private void CreateBlock()
    {
        Vector2 randPos = new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-4f, 4));
        Transform block = Instantiate(blockPrefab, randPos, transform.rotation);

        SpriteRenderer blockSprite = block.gameObject.GetComponent<SpriteRenderer>();

        blockSprite.sprite = currentBlockSpriteArray[(int)UnityEngine.Random.Range(0f, 5f)];

        ++TotalBlocksSpawned;
        ++currentBlockCount;

        dataSavingManager.SetOtherValue("TotalBlocksSpawned", TotalBlocksSpawned);

        BlockDictionary.Add(TotalBlocksSpawned, block);
    }

    public void BlockDestroyed()
    {
        --currentBlockCount;
    }

    public void FlatSpawnSpeedIncrease(float spawnSpeedIncrease)
    {
        blockSpawnData.spawnTime -= spawnSpeedIncrease;
        dataSavingManager.SetBlockSpawnData(blockSpawnData);
        dataSavingManager.Save();
    }
}

[Serializable]
public class BlockSpawnData
{
    public int maxCurrentBlocks;

    public float spawnTime;
}