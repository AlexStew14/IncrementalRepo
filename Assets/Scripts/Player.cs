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
    private float damageTimeRemaining = -1.0f;

    private bool damageTimerRunning = false;

    private bool autoEnabled = false;

    // Sprite Renderer for flipping sprite
    private SpriteRenderer sprite;

    private List<Ability> purchasedPassives;
    private List<Ability> purchasedActives;

    private UnityAction<object> purchasedAbility;

    private UnityAction<object> toggleAuto;

    [SerializeField]
    private GameObject[] abilityPrefabs;

    private Dictionary<int, ParticleSystem> abilityEffectsDict;

    private Block targetBlock;

    private bool fightingBlock;

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

        //anim = character.GetComponent<Animator>();
        anim = gameObject.transform.GetChild(0).GetComponent<Animator>();
        //sprite = character.GetComponent<SpriteRenderer>();
        sprite = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();

        purchasedAbility = new UnityAction<object>(PurchasedAbility);
        EventManager.StartListening("PurchasedAbility", purchasedAbility);

        toggleAuto = new UnityAction<object>(ToggleAuto);
        EventManager.StartListening("ToggleAuto", toggleAuto);

        abilityEffectsDict = new Dictionary<int, ParticleSystem>();
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
    private float GetDamage()
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
        List<Transform> targets = ProcAbilities();

        if (targets.Count() == 0)
            targetBlock.TakingDamageisDead(GetDamage());
        else
        {
            foreach (var t in targets)
            {
                Block tBlock = t.gameObject.GetComponent<Block>();
                tBlock.TakingDamageisDead(GetDamage());
            }
        }

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

    private List<Transform> ProcAbilities()
    {
        List<Transform> targets = new List<Transform>();

        foreach (Ability a in purchasedPassives)
        {
            if (a.Cast())
            {
                if (a.abilitySubType == AbilitySubType.MOVEMENTSPEED)
                    playerData.finalMoveSpeed = playerData.baseMoveSpeed * a.totalStatIncrease;
                else if (a.abilitySubType == AbilitySubType.DAMAGE)
                    playerData.finalDamage = playerData.baseDamage * a.totalStatIncrease;
                else if (a.abilitySubType == AbilitySubType.AREADAMAGE)
                {
                    var t = blockSpawner.blockDictionary.Where(s => Vector2.Distance(transform.position, s.Value.position) <= a.radius)
                          .Select(s => s.Value).ToList();

                    targets.AddRange(t);

                    playerData.finalDamage = playerData.finalDamage * a.totalStatIncrease;
                }

                ParticleSystem particles;
                if (abilityEffectsDict.ContainsKey(a.prefabIndex))
                {
                    particles = abilityEffectsDict[a.prefabIndex];
                    if (a.duration > 0)
                    {
                        if (particles.isStopped)
                        {
                            var main = particles.main;
                            main.duration = a.duration;
                        }
                    }
                }
                else
                {
                    GameObject abilityEffect = Instantiate(abilityPrefabs[a.prefabIndex], transform.position, Quaternion.identity);
                    if (a.abilitySubType != AbilitySubType.DAMAGE)
                        abilityEffect.transform.parent = transform;

                    particles = abilityEffect.GetComponent<ParticleSystem>();
                    abilityEffectsDict.Add(a.prefabIndex, particles);
                    if (a.duration > 0)
                    {
                        var main = particles.main;
                        main.duration = a.duration;
                    }
                }

                if (a.abilitySubType == AbilitySubType.DAMAGE)
                    particles.transform.position = targetBlock.transform.position;

                particles.Play();
            }
        }

        return targets;
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
                playerData.finalMoveSpeed = playerData.baseMoveSpeed;
            }
            else if (a.abilitySubType == AbilitySubType.DAMAGE)
            {
                playerData.finalDamage = playerData.baseDamage;
            }
            else if (a.abilitySubType == AbilitySubType.AREADAMAGE)
            {
                playerData.finalDamage = playerData.baseDamage;
            }
        }
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
        playerData.baseMoveSpeed += movementSpeedIncrease;
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
                    transform.position = Vector3.MoveTowards(transform.position, clickPos, playerData.finalMoveSpeed * Time.deltaTime);
                else
                    moving = false;
            }
        }
        else
        {
            if (!fightingBlock)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetBlock.transform.position, playerData.finalMoveSpeed * Time.deltaTime);
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

    private void ToggleAuto(object unused)
    {
        autoEnabled = !autoEnabled;
        if (autoEnabled)
        {
            playerData.baseMoveSpeed /= 3;
            playerData.finalMoveSpeed /= 3;
        }
        else
        {
            playerData.baseMoveSpeed *= 3;
            playerData.finalMoveSpeed *= 3;
        }
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

    public float finalMoveSpeed;

    public float baseMoveSpeed;
}