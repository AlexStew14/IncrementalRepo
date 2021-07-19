using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    #region Private Fields

    private float maxHealth;

    private float currentHealth;

    public bool isDead { get; private set; } = false;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private GameObject killEffect;

    private Canvas healthCanvas;

    private Rigidbody2D physicsBody;

    private StageManager stageManager;

    public long killReward { get; private set; }

    public long killPrestigeReward { get; private set; }

    private bool isBoss;

    #endregion Private Fields

    public int blockKey;

    [SerializeField]
    private GameObject damageText;

    [SerializeField]
    private GameObject goldText;

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        healthCanvas = GameObject.FindGameObjectWithTag("HealthCanvas").GetComponent<Canvas>();

        stageManager = GameObject.FindGameObjectWithTag("StageManager").GetComponent<StageManager>();
        maxHealth = stageManager.blockHealth;
        currentHealth = maxHealth;
        killReward = stageManager.blockKillReward;
        killPrestigeReward = stageManager.blockKillPrestigeReward;
        isBoss = stageManager.bossLevel;

        physicsBody = GetComponent<Rigidbody2D>();
        InitializeHealthBar();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    #endregion Unity Methods

    #region UI Methods

    /// <summary>
    /// Setups health bar for this block on spawn.
    /// </summary>
    private void InitializeHealthBar()
    {
        slider.maxValue = maxHealth;
        slider.minValue = 0f;
        slider.value = maxHealth;

        var pos = Camera.main.WorldToScreenPoint(transform.position);
        if (!isBoss)
            pos.y += 100.0f;
        else
            pos.y += 500f;

        slider.transform.position = pos;
        slider.transform.SetParent(healthCanvas.transform, true);
    }

    #endregion UI Methods

    #region Death Methods

    /// <summary>
    /// This method is called when the block is killed.
    /// It displays death effects and notifies the player that the block was killed.
    /// </summary>
    private void Killed()
    {
        isDead = true;
        gameObject.layer = 6;

        EventManager.TriggerEvent("BlockKilled", this);

        if (isBoss)
            EventManager.TriggerEvent("KilledBoss");

        GameObject gText = Instantiate(goldText, transform.position, Quaternion.identity);
        gText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(killReward.ToString());

        // stageManager.KilledBlock();
        Destroy(slider.gameObject);
        Instantiate(killEffect.transform, transform.position, transform.rotation);
        physicsBody.bodyType = RigidbodyType2D.Dynamic;
        physicsBody.WakeUp();
        // TODO THE BLOCK SHOULD FLY AWAY FROM THE DIRECTION THE PLAYER HIT IT.
        physicsBody.AddForce(new Vector2(Random.Range(-2000, 2000), Random.Range(-2000, 2000)));

        Destroy(transform.gameObject, 1.0f);
    }

    public void DestroyBlock()
    {
        Destroy(slider.gameObject);
        Destroy(transform.gameObject);
    }

    public bool TakingDamageisDead(float damageTaken)
    {
        currentHealth -= damageTaken;

        GameObject dmgText = Instantiate(damageText, transform.position, Quaternion.identity);
        string damage = string.Format("{0:#}", damageTaken);
        dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(damage);
        slider.value = currentHealth;
        //Debug.Log("Block attacked, health: " + currentHealth);

        if (currentHealth <= 0)
            Killed();

        return isDead;
    }

    #endregion Death Methods
}