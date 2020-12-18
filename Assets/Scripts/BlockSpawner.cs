using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    // Set in inspector
    public Transform blockPrefab;

    private float timer = 1.0f;

    [SerializeField]
    private float upperSpawnTimerBound = 3.0f;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Vector2 randPos = new Vector2(Random.Range(-11f, 11f), Random.Range(-4.5f, 4.5f));
            var block = Instantiate(blockPrefab, randPos, transform.rotation);
            timer = Random.Range(1.0f, upperSpawnTimerBound);
        }
    }
}