using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper : MonoBehaviour
{
    #region Private Fields

    private SoundManager soundManager;

    private DataSavingManager dataSavingManager;

    [SerializeField]
    private HelperData helperData;

    // Movement Fields
    private Vector3 targetPos = new Vector3(3, 3, 0);

    private bool moving = false;

    // Attack Speed handling
    private float damageTimeRemaining = -1.0f;

    private bool damageTimerRunning = false;

    // Sprite Renderer for flipping sprite
    private SpriteRenderer sprite;

    private float cdValue = 15.0f;

    private BlockSpawner blockSpawner;

    private float offset = 1.5f;

    #endregion Private Fields

    // Start is called before the first frame update
    private void Start()
    {
    }

    public void Init(string name)
    {
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        blockSpawner = GameObject.FindGameObjectWithTag("BlockSpawner").GetComponent<BlockSpawner>();

        transform.position = targetPos;
        helperData = dataSavingManager.GetSkill(name).helperData;
        StartCoroutine(StartCountdown(helperData.idleTime));
    }

    // Update is called once per frame
    private void Update()
    {
        MoveHelper();
    }

    private void MoveHelper()
    {
        if (transform.position != targetPos && moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, helperData.movementSpeed * Time.deltaTime);
        }
        else
        {
            moving = false;
        }
    }

    private bool FindBlockInRadius()
    {
        Vector3 helperPos = this.transform.position;
        foreach (int c in blockSpawner.BlockDictionary.Keys)
        {
            Vector3 blockPos = blockSpawner.BlockDictionary[c].position;

            if (blockPos.x < helperPos.x + offset && blockPos.x > helperPos.x - offset &&
                    blockPos.y < helperPos.y + offset && blockPos.y > helperPos.y - offset)
            {
                targetPos = blockPos;
                return true;
            }
        }
        return false;
    }

    private IEnumerator StartCountdown(float countdownValue)
    {
        cdValue = countdownValue;
        while (cdValue > 0)
        {
            UnityEngine.Debug.Log("Countdown: " + cdValue);
            yield return new WaitForSeconds(1.0f);
            cdValue--;
        }
        moving = FindBlockInRadius();
    }
}

[Serializable]
public class HelperData
{
    public float attackDamage;
    public float attackSpeed;
    public float idleTime;
    public float movementSpeed;
}