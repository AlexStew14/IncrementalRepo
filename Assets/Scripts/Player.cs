using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// This class represents the player and holds stats and handles gameplay related to the player.
/// </summary>
public class Player : MonoBehaviour
{
    #region Private Fields

    private SoundManager soundManager;

    private DataSavingManager dataSavingManager;

    private BlockSpawner blockSpawner;

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
    private double damageTimeRemaining = -1.0;

    private bool damageTimerRunning = false;

    private bool autoEnabled = false;

    // Sprite Renderer for flipping sprite
    private SpriteRenderer sprite;

    private List<Ability> purchasedPassives;
    private List<Ability> purchasedActives;

    private Dictionary<string, double> appliedAbilities;

    private UnityAction<object> purchasedAbility;

    private UnityAction<object> toggleAutoMove;

    [SerializeField]
    private GameObject[] abilityPrefabs;

    private Dictionary<int, ParticleSystem> abilityEffectsDict;

    private Block targetBlock;

    private bool fightingBlock;

    private double finalMoveSpeed;

    #endregion Private Fields

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        blockSpawner = GameObject.FindGameObjectWithTag("BlockSpawner").GetComponent<BlockSpawner>();

        playerData = dataSavingManager.GetPlayerData();
        var purchasedAbilities = dataSavingManager.GetAbilityDictionary().Values.Where(s => s.level > 0).ToList();

        purchasedPassives = purchasedAbilities.Where(a => a.abilityType == AbilityType.PASSIVE).ToList();
        purchasedActives = purchasedAbilities.Where(a => a.abilityType == AbilityType.ACTIVE).ToList();

        transform.position = clickPos;

        finalMoveSpeed = playerData.baseMoveSpeed;

        //anim = character.GetComponent<Animator>();
        anim = gameObject.transform.GetChild(0).GetComponent<Animator>();
        //sprite = character.GetComponent<SpriteRenderer>();
        sprite = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();

        abilityEffectsDict = new Dictionary<int, ParticleSystem>();
        appliedAbilities = new Dictionary<string, double>();

        purchasedAbility = new UnityAction<object>(PurchasedAbility);
        EventManager.StartListening("PurchasedAbility", purchasedAbility);

        toggleAutoMove = new UnityAction<object>(ToggleAutoMove);
        EventManager.StartListening("ToggleAutoMove", toggleAutoMove);
    }

    private void Awake()
    {
        //Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    private void Update()
    {
        if ((targetBlock == null || targetBlock.isDead) && autoEnabled)
            GetNewTarget();

        if (CanAttack())
            Attacked();

        if (targetBlock != null || !autoEnabled)
            MovePlayer();
        else
            moving = false;

        if (damageTimerRunning)
        {
            if (damageTimeRemaining > 0)
                damageTimeRemaining -= Time.deltaTime;
            else
                damageTimerRunning = false;
        }

        anim.SetBool("Moving", moving);

        CheckAbilities();
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
    private double GetDamage()
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
    public double GetAttackSpeed()
    {
        return playerData.finalAttackSpeed;
    }

    private bool CanAttack()
    {
        return targetBlock != null && !targetBlock.isDead && fightingBlock && !damageTimerRunning;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject c = collision.gameObject;
        if (c.CompareTag("Block"))
        {
            targetBlock = c.GetComponent<Block>();
            fightingBlock = true;
            moving = false;
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        GameObject c = collision.gameObject;
        if (c.CompareTag("Block"))
        {
            targetBlock = null;
            fightingBlock = false;
        }
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        GameObject c = collision.gameObject;
        if (c.CompareTag("Block"))
        {
            if (targetBlock == null)
                targetBlock = c.GetComponent<Block>();

            fightingBlock = true;
        }
    }

    /// <summary>
    /// Called when the player has attacked, used to reset timer.
    /// </summary>
    public void Attacked()
    {
        sprite.flipX = targetBlock.transform.position.x < transform.position.x;

        anim.Play("Player_Punch");
        soundManager.PlayAttack();

        bool dealtDamage = ProcAbilities();

        if (!dealtDamage)
            targetBlock.TakingDamageisDead(GetDamage());

        damageTimeRemaining = GetAttackSpeed();
        damageTimerRunning = true;
    }

    private void PurchasedAbility(object ability)
    {
        Ability a = (Ability)ability;
        if (a.abilityType == AbilityType.ACTIVE)
            purchasedActives.Add(a);
        else
            purchasedPassives.Add(a);
    }

    /// <summary>
    /// Code from https://answers.unity.com/questions/956636/how-to-randomly-iterate-through-a-list-of-objects.html
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    private void ShuffleArray<T>(T[] array)
    {
        int n = array.Length;
        for (int i = 0; i < n; i++)
        {
            // Pick a new index higher than current for each item in the array
            int r = i + UnityEngine.Random.Range(0, n - i);

            // Swap item into new spot
            T t = array[r];
            array[r] = array[i];
            array[i] = t;
        }
    }

    private bool ProcAbilities()
    {
        var shuffledPassives = purchasedPassives.ToArray();
        ShuffleArray(shuffledPassives);

        bool damageDealt = false;
        foreach (Ability a in shuffledPassives)
        {
            // Only want to cast max 1 damaging ability per cast
            if ((!damageDealt || a.abilitySubType == AbilitySubType.MOVEMENTSPEED) && a.Cast())
            {
                if (a.abilitySubType == AbilitySubType.MOVEMENTSPEED)
                    finalMoveSpeed *= a.totalStatIncrease;
                else if (a.abilitySubType == AbilitySubType.DAMAGE)
                {
                    if (targetBlock != null && !targetBlock.isDead)
                        targetBlock.TakingDamageisDead(playerData.baseDamage * a.totalStatIncrease);

                    damageDealt = true;
                }
                else if (a.abilitySubType == AbilitySubType.AREADAMAGE)
                {
                    var t = blockSpawner.blockDictionary.Where(s => Vector2.Distance(transform.position, s.Value.position) <= a.radius)
                          .Select(s => s.Value).ToList();

                    foreach (var b in t)
                    {
                        Block block = b.gameObject.GetComponent<Block>();
                        if (block != null && !block.isDead)
                            block.TakingDamageisDead(playerData.baseDamage * a.totalStatIncrease);
                    }

                    damageDealt = true;
                }
                else if (a.abilitySubType == AbilitySubType.DAMAGEOVERTIME)
                {
                    if (targetBlock != null && !targetBlock.isDead)
                        targetBlock.DamageOverTime((playerData.finalDamage * a.totalStatIncrease) / (a.duration * 5), a.duration);

                    damageDealt = true;
                }

                appliedAbilities.Add(a.name, a.totalStatIncrease);

                ParticleSystem particles;
                if (abilityEffectsDict.ContainsKey(a.prefabIndex))
                {
                    particles = abilityEffectsDict[a.prefabIndex];
                }
                else
                {
                    GameObject abilityEffect = Instantiate(abilityPrefabs[a.prefabIndex], transform.position, Quaternion.identity);
                    if (a.abilitySubType == AbilitySubType.MOVEMENTSPEED)
                        abilityEffect.transform.parent = transform;

                    particles = abilityEffect.GetComponent<ParticleSystem>();
                    abilityEffectsDict.Add(a.prefabIndex, particles);
                    if (a.duration > 0)
                    {
                        var main = particles.main;
                        main.duration = a.duration;
                    }
                }

                if (targetBlock != null && a.abilitySubType != AbilitySubType.MOVEMENTSPEED)
                    particles.transform.position = targetBlock.transform.position;

                particles.Play();
            }
        }
        return damageDealt;
    }

    private void CheckAbilities()
    {
        List<Ability> endedAbilities = new List<Ability>();

        foreach (Ability a in purchasedActives)
        {
            if (a.UpdateAndHasEnded(Time.deltaTime))
                endedAbilities.Add(a);
        }

        foreach (Ability a in purchasedPassives)
        {
            if (a.UpdateAndHasEnded(Time.deltaTime))
                endedAbilities.Add(a);
        }

        if (endedAbilities.Count() > 0)
            UndoAbilites(endedAbilities);
    }

    private void UndoAbilites(List<Ability> endedAbilities)
    {
        foreach (Ability a in endedAbilities)
        {
            if (a.abilitySubType == AbilitySubType.MOVEMENTSPEED)
            {
                finalMoveSpeed /= appliedAbilities[a.name];
            }

            appliedAbilities.Remove(a.name);
        }
    }

    /// <summary>
    /// Called by the Shop to increase the player by a flat amount when damage is upgraded.
    /// </summary>
    /// <param name="damageIncrease"></param>
    public void FlatDmgIncrease(double damageIncrease)
    {
        playerData.baseDamage += damageIncrease;
        UpdateDamage();
        SavePlayerData();
    }

    public void PctAttackSpeedUpgrade(double atkSpdMult)
    {
        playerData.baseAttackSpeed *= atkSpdMult;
        UpdateAttackSpeed();
        SavePlayerData();
    }

    public void FlatMovementSpeedIncrease(double movementSpeedIncrease)
    {
        playerData.baseMoveSpeed += movementSpeedIncrease;
        SavePlayerData();
    }

    private void SavePlayerData()
    {
        dataSavingManager.SetPlayerData(playerData);
        dataSavingManager.Save();
        EventManager.TriggerEvent("PlayerUpgraded");
    }

    #endregion Skill Methods

    #region Movement

    private void MovePlayer()
    {
        if (!autoEnabled)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 temp = Input.mousePosition;
                temp.z = 10f;
                clickPos = Camera.main.ScreenToWorldPoint(temp);
                moving = true;

                sprite.flipX = clickPos.x < transform.position.x;
            }

            if (moving)
            {
                if (transform.position != clickPos)
                    transform.position = Vector3.MoveTowards(transform.position, clickPos, (float)(finalMoveSpeed * Time.deltaTime));
                else
                    moving = false;
            }
        }
        else
        {
            if (!fightingBlock)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetBlock.transform.position, (float)(finalMoveSpeed * Time.deltaTime));
                moving = true;
                sprite.flipX = clickPos.x < transform.position.x;
            }
        }
    }

    private void GetNewTarget()
    {
        if (blockSpawner.blockDictionary.Count() > 0)
        {
            var target = blockSpawner.blockDictionary.OrderBy(s => Vector2.Distance(transform.position, s.Value.position)).First().Value;
            targetBlock = target.gameObject.GetComponent<Block>();
            fightingBlock = false;
        }
        else
        {
            targetBlock = null;
        }
    }

    public void StopMoving()
    {
        //clickPos = transform.position;
        moving = false;
    }

    private void ToggleAutoMove(object unused)
    {
        double autoMoveSpeedMult = (double)dataSavingManager.GetOtherValue("AutoMoveSpeedMultiplier");

        autoEnabled = !autoEnabled;
        if (autoEnabled)
            finalMoveSpeed /= autoMoveSpeedMult;
        else
            finalMoveSpeed *= autoMoveSpeedMult;
    }

    #endregion Movement
}

/// <summary>
/// Stores the base and final stats for the players skills as well as their prestige multiplies.
/// </summary>
[Serializable]
public class PlayerData
{
    // This represents the base damage before multipliers are applied.
    public double baseDamage;

    // This represents damage after multiplers applied.
    public double finalDamage;

    // Multipliers from prestige upgrades.
    public double prestigeDmgMultiplier;

    // Multipliers from the current run.
    public double runDmgMultiplier;

    // This represents the base attack speed before multipliers are applied.
    // This represents the time in seconds between attacks.
    // Multipliers will decrease the attack speed value
    public double baseAttackSpeed;

    public double finalAttackSpeed;

    public double runAtkSpeedMult;

    public double prestigeAtkSpeedMult;

    public double baseMoveSpeed;
}