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

    [SerializeField]
    private GameObject character;

    private Animator anim;

    // Movement Fields
    private Vector3 targetPos;

    private bool moving = false;

    // Attack Speed handling
    private float damageTimeRemaining = -1.0f;

    private bool damageTimerRunning = false;

    // Sprite Renderer for flipping sprite
    private SpriteRenderer sprite;

    private float cdValue = 15.0f;

    private BlockSpawner blockSpawner;

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
        helperData = dataSavingManager.GetSkill(name).helperData;

        anim = character.GetComponent<Animator>();

        anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(helperData.helperAnimatorName);

        StartCoroutine(StartCountdown(helperData.idleTime));
    }

    // Update is called once per frame
    private void Update()
    {
        if (moving)
            transform.position = Vector3.MoveTowards(transform.position, targetPos, helperData.movementSpeed * Time.deltaTime);

        if (!blockSpawner.ContainsBlock(currentTarget))
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

    private IEnumerator StartCountdown(float countdownValue)
    {
        cdValue = countdownValue;
        countDownRunning = true;
        StopMoving();
        while (cdValue > 0)
        {
            yield return new WaitForSeconds(Math.Min(1.0f, cdValue));
            cdValue--;
        }

        countDownRunning = false;
        if (blockSpawner.blockDictionary.Count == 0)
        {
            StartCoroutine(StartCountdown(helperData.idleTime));
        }
        else
        {
            // Gets closest block
            currentTarget = blockSpawner.blockDictionary.OrderBy(s => Vector2.Distance(transform.position, s.Value.position)).First().Value;
            targetPos = currentTarget.position;

            moving = true;
            anim.SetBool("Moving", true);
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

    public void StopMoving()
    {
        moving = false;
        anim.SetBool("Moving", false);
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
    public string helperAnimatorName;
}