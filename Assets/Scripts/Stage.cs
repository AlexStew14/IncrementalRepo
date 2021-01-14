using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stage
{
    public string blockSpritesPath;

    public string animatorName;

    public int currentCount;

    public int maxCount;

    public bool completed = false;

    public int stageKey;

    public int KilledBlock()
    {
        if (currentCount >= maxCount)
        {
            completed = true;
            return maxCount;
        }

        return ++currentCount;
    }
}