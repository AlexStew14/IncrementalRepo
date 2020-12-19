using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the player and holds stats and handles gameplay related to the player.
/// </summary>
public class Player : MonoBehaviour
{
    // This is set to true when the multipliers are changed and damage needs to be recalculated.
    private bool damageUpdated = true;

    private bool attackSpeedUpdated = true;

    private float damageTimeRemaining = -1.0f;

    private bool damageTimerRunning = false;

    [SerializeField]
    private PlayerData playerData;

    private SoundManager soundManager;

    private Shop shop;

    private DataSavingManager dataSavingManager;

    //   private 2Dbox
    // Start is called before the first frame update
    private void Start()
    {
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();

        playerData = dataSavingManager.GetPlayerData();
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    /// <summary>
    /// Provides the final damage value.
    /// </summary>
    /// <returns></returns>
    public float GetDamage()
    {
        if (damageUpdated)
        {
            playerData.finalDamage = playerData.baseDamage * playerData.prestigeDmgMultiplier * playerData.runDmgMultiplier;
            damageUpdated = false;
        }
        return playerData.finalDamage;
    }

    /// <summary>
    /// Provides the final attack speed value.
    /// </summary>
    /// <returns></returns>
    public float GetAttackSpeed()
    {
        if (attackSpeedUpdated)
        {
            playerData.finalAttackSpeed = playerData.baseAttackSpeed * playerData.prestigeAtkSpeedMult * playerData.runAtkSpeedMult;
            attackSpeedUpdated = false;
        }
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
        dataSavingManager.SetPlayerData(playerData);
        dataSavingManager.Save();
        damageUpdated = true;
    }

    /// <summary>
    /// This will handle the rewards for the player killing enemy.
    /// </summary>
    /// <param name="block"></param>
    public void KilledBlock(Block block)
    {
        shop.AddPlayerMoney(block.GetKillReward());
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
    }

    private void MovePlayer()
    {
        Vector2 pos = transform.position;
        Vector3 temp = Input.mousePosition;
        temp.z = 10f; // Set this to be the distance you want the object to be placed in front of the camera.
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(temp);

        this.transform.position = mousePos;
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
}