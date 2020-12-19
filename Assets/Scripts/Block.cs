using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    [SerializeField]
    private float maxHealth = 5.0f;

    private float currentHealth = 5.0f;

    private string blockType;

    private bool colliding = false;

    private Player player;

    [SerializeField]
    private int killReward = 1;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private GameObject killEffect;

    private Canvas healthCanvas;

    private Rigidbody2D physicsBody;

    // Start is called before the first frame update
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        healthCanvas = GameObject.FindGameObjectWithTag("HealthCanvas").GetComponent<Canvas>();
        physicsBody = GetComponent<Rigidbody2D>();
        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        slider.maxValue = maxHealth;
        slider.minValue = 0f;
        slider.value = maxHealth;

        var pos = Camera.main.WorldToScreenPoint(transform.position);
        pos.y += 40.0f;
        slider.transform.position = pos;
        slider.transform.SetParent(healthCanvas.transform, true);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (colliding)
        {
            if (player.CanAttack())
            {
                currentHealth -= player.GetDamage();
                slider.value = currentHealth;
                Debug.Log("Block attacked, health: " + currentHealth);
                player.Attacked();
                if (currentHealth <= 0)
                {
                    player.KilledBlock(this);
                    Destroy(slider.gameObject);
                    Killed();
                }
            }
        }
    }

    private void Killed()
    {
        var effect = Instantiate(killEffect.transform, transform.position, transform.rotation);
        // Destroy(effect, 1.0f);
        physicsBody.bodyType = RigidbodyType2D.Dynamic;
        physicsBody.WakeUp();
        physicsBody.AddForce(new Vector2(Random.Range(-2000, 2000), Random.Range(-2000, 2000)));
        Destroy(transform.gameObject, 1.0f);
    }

    public int GetKillReward()
    {
        return killReward;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject c = collision.gameObject;

        if (c.tag == "Player")
        {
            colliding = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        GameObject c = collision.gameObject;

        if (c.tag == "Player")
        {
            colliding = false;
        }
    }
}