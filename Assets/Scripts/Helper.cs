using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Helper : MonoBehaviour, IAttacker
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

    private Transform currentTarget;
    private Shop shop;

    private bool countDownRunning = false;

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
        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();

        transform.position = targetPos;
        helperData = dataSavingManager.GetSkill(name).helperData;
        StartCoroutine(StartCountdown(helperData.idleTime));
    }

    // Update is called once per frame
    private void Update()
    {
        MoveHelper();

        if (currentTarget != null && currentTarget.gameObject.GetComponent<Block>().isDead)
        {
            currentTarget = null;
            StartCoroutine(StartCountdown(helperData.idleTime));
        }

        if (damageTimerRunning)
        {
            if (damageTimeRemaining > 0)
            {
                damageTimeRemaining -= Time.deltaTime;
            }
            else
            {
                damageTimerRunning = false;
            }
        }
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
        countDownRunning = true;
        moving = false;
        while (cdValue > 0)
        {
            UnityEngine.Debug.Log("Countdown: " + cdValue);
            yield return new WaitForSeconds(Math.Min(1.0f, cdValue));
            cdValue--;
        }

        countDownRunning = false;
        if (blockSpawner.BlockDictionary.Count == 0)
        {
            StartCoroutine(StartCountdown(helperData.idleTime));
        }
        else
        {
            // Gets closest block
            targetPos = blockSpawner.BlockDictionary.OrderBy(s => Vector2.Distance(transform.position, s.Value.position)).First().Value.position;
            currentTarget = blockSpawner.BlockDictionary.ElementAt(0).Value;

            moving = true;
        }
    }

    #region IAttacker Methods

    public bool CanAttack(Transform target)
    {
        if (!damageTimerRunning && !countDownRunning)
        {
            currentTarget = target;
            targetPos = currentTarget.position;
            return true;
        }
        return false;
    }

    public float GetDamage()
    {
        return helperData.attackDamage;
    }

    public float GetAttackSpeed()
    {
        return helperData.attackSpeed;
    }

    public void Attacked()
    {
        damageTimeRemaining = GetAttackSpeed();
        damageTimerRunning = true;
    }

    public void KilledBlock(Block block)
    {
        shop.KilledBlock(block.GetKillReward());
    }

    public void StopMoving()
    {
        moving = false;
        targetPos = transform.position;
    }

    #endregion IAttacker Methods
}

[Serializable]
public class HelperData
{
    public float attackDamage;
    public float attackSpeed;
    public float idleTime;
    public float movementSpeed;
}