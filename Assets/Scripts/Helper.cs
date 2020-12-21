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
    private Vector3 targetPos = new Vector3(3,3,0);
    private bool moving = false;

    // Attack Speed handling
    private float damageTimeRemaining = -1.0f;
    private bool damageTimerRunning = false;

    // Sprite Renderer for flipping sprite
    private SpriteRenderer sprite;

    private float cdValue = 15.0f;

    private BlockSpawner blockSpawner;

    private float offset = 1.5f;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        blockSpawner = GameObject.FindGameObjectWithTag("BlockSpawner").GetComponent<BlockSpawner>();

        helperData = dataSavingManager.GetHelperData("Helper1");

        transform.position = targetPos;

        StartCoroutine(StartCountdown(15.0f));
    }

    // Update is called once per frame
    void Update()
    {
        MoveHelper();
    }

    private void MoveHelper()
    {
        if (transform.position != targetPos && moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, helperData.moveSpeed * Time.deltaTime);
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
    // This represents the base damage before multipliers are applied.
    public float baseDamage;

    // This represents damage after multiplers applied.
    public float finalDamage;

    // Multipliers from prestige upgrades.
    public float prestigeDmgMultiplier;

    // Multipliers from the current run.
    public float runDmgMultiplier;

    // This represents the base attack speed before multipliers are applied.
    // This represents the time in seconds between attacks.
    // Multipliers will decrease the attack speed value
    public float baseAttackSpeed;

    public float finalAttackSpeed;

    public float runAtkSpeedMult;

    public float prestigeAtkSpeedMult;

    public float moveSpeed;
}
