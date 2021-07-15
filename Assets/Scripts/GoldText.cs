using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldText : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float journeyLength;
    private float startTime;
    private float speed = 10.0f;

    private TextMeshPro goldText;
    private Color startColor;

    private bool moving;

    // Start is called before the first frame update
    private void Start()
    {
        startPosition = transform.position;
        targetPosition = Camera.main.ScreenToWorldPoint(GameObject.FindGameObjectWithTag("CurrentMoney").transform.position);
        journeyLength = Vector3.Distance(transform.position, targetPosition);

        goldText = transform.GetChild(0).GetComponent<TextMeshPro>();
        startColor = goldText.color;

        moving = true;

        startTime = Time.time;
    }

    // Update is called once per frame
    private void Update()
    {
        if (moving)
        {
            float distCovered = (Time.time - startTime) * speed;

            float fractionOfJourney = distCovered / journeyLength;

            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney * fractionOfJourney);

            Color newColor = startColor;
            newColor.a = Mathf.Lerp(1, .25f, fractionOfJourney * fractionOfJourney);

            goldText.color = newColor;

            if (fractionOfJourney >= .99)
            {
                moving = false;
                EventManager.TriggerEvent("GoldArrived");
                Destroy(transform.gameObject);
            }
        }
    }
}