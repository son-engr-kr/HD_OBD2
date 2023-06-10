using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager
{
    public static bool RectangularPulse(float highDurSec, float lowDurSec)
    {
        long highDurMillis = (long)(highDurSec * 1000);
        long lowDurMillis = (long)(lowDurSec * 1000);
        long totalDurMillis = highDurMillis + lowDurMillis;
        long curTime = GetNowTimeMillis();
        if(curTime % totalDurMillis < highDurMillis)
        {
            return true;
        }
        return false;
    }
    public static long GetNowTimeMillis()
    {
        return System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

        //if (Global.IsPLCRealMode)
        //{
        //    return System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        //}
        //else
        //{
        //    return (long)(Time.time * 1000);
        //}
    }
}
