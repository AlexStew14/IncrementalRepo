using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the player and holds stats and handles gameplay related to the player.
/// </summary>
public class Player : MonoBehaviour
{
    private float damageTimeRemaining = -1.0f;

    private bool damageTimerRunning = false;

    [SerializeField]
    private PlayerData playerData;

    private SoundManager soundManager;

    private Shop shop;

    private DataSavingManager dataSavingManager;

    private Vector3 clickPos = new Vector3(0, 0, 0);

    private Rigidbody2D rb;

    [SerializeField]
    private GameObject pink_guy;
    private Animator anim;
    private bool moving = false;
    private SpriteRenderer sprite;

    //   private 2Dbox
    // Start is called before the first frame update
    private void Start()
    {
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();

        playerData = dataSavingManager.GetPlayerData();

        transform.position = clickPos;

        anim = pink_guy.GetComponent<Animator>();
        sprite = pink_guy.GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void UpdateDamage()
    {
        playerData.finalDamage = playerData.baseDamage * playerData.prestigeDmgMultiplier * playerData.runDmgMultiplier;
    }

    /// <summary>
    /// Provides the final damage value.
    /// </summary>
    /// <returns></returns>
    public float GetDamage()
    {
        return playerData.finalDamage;
    }

    private void UpdateAttackSpeed()
    {
        playerData.finalAttackSpeed = playerData.baseAttackSpeed * playerData.prestigeAtkSpeedMult * playerData.runAtkSpeedMult;
    }

    /// <summary>
    /// Provides the final attack speed value.
    /// </summary>
    /// <returns></returns>
    public float GetAttackSpeed()
    {
        return playerData.finalAttackSpeed;
    }

    public bool CanAttack()
    {
        return !damageTimerRunning;
    }

    /// <summary>
    /// Called when the player has attacked, used to reset timer.
    /// </summary>
    public void Attacked()
    {
        soundManager.PlayAttack();
        //anim.SetTrigger("Punch");
        damageTimeRemaining = GetAttackSpeed();
        damageTimerRunning = true;
    }

    /// <summary>
    /// Called by the Shop to increase the player by a flat amount when damage is upgraded.
    /// </summary>
    /// <param name="damageIncrease"></param>
    public void FlatDmgIncrease(float damageIncrease)
    {
        playerData.baseDamage += damageIncrease;
        UpdateDamage();
        dataSavingManager.SetPlayerData(playerData);
        dataSavingManager.Save();
    }

    public void FlatAttackSpeedIncrease(float attkSpeedIncrease)
    {
        playerData.baseAttackSpeed -= attkSpeedIncrease;
        UpdateAttackSpeed();
        dataSavingManager.SetPlayerData(playerData);
        dataSavingManager.Save();
    }

    /// <summary>
    /// This will handle the rewards for the player killing enemy.
    /// </summary>
    /// <param name="block"></param>
    public void KilledBlock(Block block)
    {
        shop.KilledBlock(block.GetKillReward());
        soundManager.PlayBlockDestroyed();
    }

    // Update is called once per frame
    private void Update()
    {
        MovePlayer();

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

        anim.SetBool("Moving", moving);
    }

    private void MovePlayer()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 temp = Input.mousePosition;
            temp.z = 10f; // Set this to be the distance you want the object to be placed in front of the camera.
            clickPos = Camera.main.ScreenToWorldPoint(temp);
            moving = true;

            if (clickPos.x < 0)
            {
                sprite.flipX = true;
            }
            else if (clickPos.x >= 0)
            {
                sprite.flipX = false;
            }
        }

        if (transform.position != clickPos && moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, clickPos, playerData.moveSpeed * Time.deltaTime);
        }
        else
        {
            moving = false;
        }
    }

    public void StopMoving()
    {
        clickPos = transform.position;
        moving = false;
    }
}

[Serializable]
public class PlayerData
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