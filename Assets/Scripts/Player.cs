using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the player and holds stats and handles gameplay related to the player.
/// </summary>
public class Player : MonoBehaviour
{
    // This represents the base damage before multipliers are applied.
    [SerializeField]
    [Header("Damage")]
    private float baseDamage = 1.0f;

    // This represents damage after multiplers applied.
    [SerializeField]
    private float finalDamage = 1.0f;

    // Multipliers from prestige upgrades.
    [SerializeField]
    private float prestigeDmgMultiplier = 1.0f;

    // Multipliers from the current run.
    [SerializeField]
    private float runDmgMultiplier = 1.0f;

    // This is set to true when the multipliers are changed and damage needs to be recalculated.
    private bool damageUpdated = true;

    // This represents the base attack speed before multipliers are applied.
    // This represents the time in seconds between attacks.
    // Multipliers will decrease the attack speed value
    [SerializeField]
    [Header("Attack Speed")]
    private float baseAttackSpeed = 1.0f;

    [SerializeField]
    private float finalAttackSpeed = 1.0f;

    [SerializeField]
    private float runAtkSpeedMult = 1.0f;

    [SerializeField]
    private float prestigeAtkSpeedMult = 1.0f;

    private bool attackSpeedUpdated = true;

    [SerializeField]
    [Header("Money")]
    public int Money = 0;

    private float damageTimeRemaining = -1.0f;

    private bool damageTimerRunning = false;


    private SoundManager soundManager;

    //   private 2Dbox
    // Start is called before the first frame update
    private void Start()
    {
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();

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
            finalDamage = baseDamage * prestigeDmgMultiplier * runDmgMultiplier;
            damageUpdated = false;
        }
        return finalDamage;
    }

    /// <summary>
    /// Provides the final attack speed value.
    /// </summary>
    /// <returns></returns>
    public float GetAttackSpeed()
    {
        if (attackSpeedUpdated)
        {
            finalAttackSpeed = baseAttackSpeed * prestigeAtkSpeedMult * runAtkSpeedMult;
            attackSpeedUpdated = false;
        }
        return finalAttackSpeed;
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
        baseDamage += damageIncrease;
        damageUpdated = true;
    }

    /// <summary>
    /// This will handle the rewards for the player killing enemy.
    /// </summary>
    /// <param name="block"></param>
    public void KilledBlock(Block block)
    {
        Money++;
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