using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource BlockDestroyed;

    [SerializeField]
    private AudioSource Attack;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlayBlockDestroyed()
    {
        BlockDestroyed.Play();
    }

    public void PlayAttack()
    {
        Attack.Play();
    }
}
