using System.Collections;
using System.Collections.Generic;
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

    private List<IAttacker> collidingAttackers;

    public long killReward { get; private set; }

    #endregion Private Fields

    public int blockKey;

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        healthCanvas = GameObject.FindGameObjectWithTag("HealthCanvas").GetComponent<Canvas>();

        stageManager = GameObject.FindGameObjectWithTag("StageManager").GetComponent<StageManager>();
        maxHealth = stageManager.blockHealth;
        currentHealth = maxHealth;
        killReward = stageManager.blockKillReward;

        physicsBody = GetComponent<Rigidbody2D>();
        collidingAttackers = new List<IAttacker>();
        InitializeHealthBar();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    /// <summary>
    /// Handles physics and collisions.
    /// Blocks take damage when they collide with player if the player can attack.
    /// </summary>
    private void FixedUpdate()
    {
        if (collidingAttackers.Count != 0 && !isDead)
        {
            foreach (IAttacker a in collidingAttackers)
            {
                if (a.CanAttack(transform))
                {
                    a.Attacked();
                    a.StopMoving();
                    currentHealth -= a.GetDamage();
                    slider.value = currentHealth;
                    Debug.Log("Block attacked, health: " + currentHealth);

                    if (currentHealth <= 0)
                    {
                        Killed();
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Unity method for collision enter.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject c = collision.gameObject;

        if (c.tag == "Player")
        {
            collidingAttackers.Add(c.GetComponent<Player>());
        }
        else if (c.tag == "Helper")
        {
            collidingAttackers.Add(c.GetComponent<Helper>());
        }
    }

    /// <summary>
    /// Unity method for collision exit.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        GameObject c = collision.gameObject;

        if (c.tag == "Player")
        {
            collidingAttackers.Remove(c.GetComponent<Player>());
        }
        else if (c.tag == "Helper")
        {
            collidingAttackers.Remove(c.GetComponent<Helper>());
        }
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
        pos.y += 100.0f;
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

    #endregion Death Methods
}