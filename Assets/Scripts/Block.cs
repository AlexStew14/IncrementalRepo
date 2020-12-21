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

    private bool colliding = false;

    private bool isDead = false;

    private Player player;

    [SerializeField]
    private int killReward = 1;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private GameObject killEffect;

    private Canvas healthCanvas;

    private Rigidbody2D physicsBody;

    private BlockSpawner blockSpawner;

    #endregion Private Fields

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        healthCanvas = GameObject.FindGameObjectWithTag("HealthCanvas").GetComponent<Canvas>();
        blockSpawner = GameObject.FindGameObjectWithTag("BlockSpawner").GetComponent<BlockSpawner>();
        physicsBody = GetComponent<Rigidbody2D>();
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
        if (colliding && !isDead)
        {
            if (player.CanAttack())
            {
                currentHealth -= player.GetDamage();
                slider.value = currentHealth;
                Debug.Log("Block attacked, health: " + currentHealth);
                player.Attacked();
                if (currentHealth <= 0)
                {
                    Killed();
                }
            }
            player.StopMoving();
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
            colliding = true;
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
            colliding = false;
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
        isDead = true;
        gameObject.layer = 6;
        player.KilledBlock(this);
        Destroy(slider.gameObject);
        var effect = Instantiate(killEffect.transform, transform.position, transform.rotation);
        physicsBody.bodyType = RigidbodyType2D.Dynamic;
        physicsBody.WakeUp();
        // TODO THE BLOCK SHOULD FLY AWAY FROM THE DIRECTION THE PLAYER HIT IT.
        physicsBody.AddForce(new Vector2(Random.Range(-2000, 2000), Random.Range(-2000, 2000)));
        blockSpawner.BlockDestroyed();
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

    #endregion Death Methods
}