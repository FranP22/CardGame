using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLevel
{
    public static int GetRequiredExp(int level)
    {
        switch (level)
        {
            case 1:
                return 100;
            case 2:
                return 200;
            case 3:
                return 400;
            case 4:
                return 600;
        }
        return -1;
    }

    public static int GetPlayerLevelUp(int currentLevel, int exp)
    {
        int reqExp = GetRequiredExp(currentLevel);
        if (exp > reqExp && reqExp != -1)
        {
            return reqExp;
        }

        return 0;
    }
}
