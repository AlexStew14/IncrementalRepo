using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    #region Private Fields

    [SerializeField]
    private float maxHealth = 5.0f;

    private float currentHealth = 5.0f;

    private string blockType;

    public bool isDead { get; private set; } = false;

    private Player player;

    [SerializeField]
    private int killReward = 1;

    [SerializeField]
    private int killExpReward = 10;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private GameObject killEffect;

    private Canvas healthCanvas;

    private Rigidbody2D physicsBody;

    private BlockSpawner blockSpawner;

    private List<IAttacker> collidingAttackers;

    #endregion Private Fields

    public int blockKey;

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        healthCanvas = GameObject.FindGameObjectWithTag("HealthCanvas").GetComponent<Canvas>();
        blockSpawner = GameObject.FindGameObjectWithTag("BlockSpawner").GetComponent<BlockSpawner>();
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
                    currentHealth -= a.GetDamage();
                    slider.value = currentHealth;
                    Debug.Log("Block attacked, health: " + currentHealth);
                    a.Attacked();
                    a.StopMoving();
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
            player.Punching(true);
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
            player.Punching(false);
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
        pos.y += 80.0f;
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
        blockSpawner.BlockDestroyed(transform);
        isDead = true;
        gameObject.layer = 6;
        player.KilledBlock(this);
        Destroy(slider.gameObject);
        var effect = Instantiate(killEffect.transform, transform.position, transform.rotation);
        physicsBody.bodyType = RigidbodyType2D.Dynamic;
        physicsBody.WakeUp();
        // TODO THE BLOCK SHOULD FLY AWAY FROM THE DIRECTION THE PLAYER HIT IT.
        physicsBody.AddForce(new Vector2(Random.Range(-2000, 2000), Random.Range(-2000, 2000)));

        Destroy(transform.gameObject, 1.0f);
    }

    /// <summary>
    /// Simple getter for the kill reward
    /// </summary>
    /// <returns></returns>
    public int GetKillReward()
    {
        return killReward;
    }

    public int GetKillExpReward()
    {
        return killExpReward;
    }

    #endregion Death Methods
}