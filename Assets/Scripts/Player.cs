using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the player and holds stats and handles gameplay related to the player.
/// </summary>
public class Player : MonoBehaviour, IAttacker
{
    #region Private Fields

    private SoundManager soundManager;

    private Shop shop;

    private DataSavingManager dataSavingManager;

    [SerializeField]
    private PlayerData playerData;

    // Animation Fields
    [SerializeField]
    private GameObject character;

    private Animator anim;

    // Movement Fields
    private Vector3 clickPos = new Vector3(0, 0, 0);

    private bool moving = false;

    // Attack Speed handling
    private float damageTimeRemaining = -1.0f;

    private bool damageTimerRunning = false;

    // Sprite Renderer for flipping sprite
    private SpriteRenderer sprite;

    public bool canMove = true;

    #endregion Private Fields

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();

        playerData = dataSavingManager.GetPlayerData();

        transform.position = clickPos;

        //anim = character.GetComponent<Animator>();
        anim = GameObject.FindGameObjectWithTag("Character").GetComponent<Animator>();
        //sprite = character.GetComponent<SpriteRenderer>();
        sprite = GameObject.FindGameObjectWithTag("Character").GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        //Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    private void Update()
    {
        if (canMove)
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

    #endregion Unity Methods

    #region Skill Methods

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

    public bool CanAttack(Transform target)
    {
        return !damageTimerRunning;
    }

    /// <summary>
    /// Called when the player has attacked, used to reset timer.
    /// </summary>
    public void Attacked()
    {
        anim.Play("Player_Punch");
        soundManager.PlayAttack();
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
        SavePlayerData();
    }

    public void FlatAttackSpeedIncrease(float attkSpeedIncrease)
    {
        playerData.baseAttackSpeed -= attkSpeedIncrease;
        UpdateAttackSpeed();
        SavePlayerData();
    }

    public void FlatMovementSpeedIncrease(float movementSpeedIncrease)
    {
        playerData.moveSpeed += movementSpeedIncrease;
        SavePlayerData();
    }

    private void SavePlayerData()
    {
        dataSavingManager.SetPlayerData(playerData);
        dataSavingManager.Save();
    }

    #endregion Skill Methods

    #region Movement

    private void MovePlayer()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 temp = Input.mousePosition;
            temp.z = 10f;
            clickPos = Camera.main.ScreenToWorldPoint(temp);
            moving = true;

            if (clickPos.x < transform.position.x)
            {
                sprite.flipX = true;
            }
            else if (clickPos.x >= transform.position.x)
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
        //clickPos = transform.position;
        moving = false;
    }

    #endregion Movement
}

/// <summary>
/// Stores the base and final stats for the players skills as well as their prestige multiplies.
/// </summary>
[Serializable]
public class PlayerData
{
    public float level;

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