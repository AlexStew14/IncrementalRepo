using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapLevel
{
    public int currentCount;

    public int maxCount;

    public bool completed = false;

    public int mapLevelKey;

    public int KilledBlock()
    {
        if (currentCount >= maxCount - 1)
        {
            completed = true;
            currentCount = maxCount;
            return maxCount;
        }

        return ++currentCount;
    }
}