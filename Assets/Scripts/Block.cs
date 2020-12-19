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

    private Canvas healthCanvas;

    // Start is called before the first frame update
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        healthCanvas = GameObject.FindGameObjectWithTag("HealthCanvas").GetComponent<Canvas>();

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
                    Destroy(this.gameObject);
                }
            }
        }
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