using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Easing
{
    public static float Lerp(float v0, float v1, float t)
    {
        return v0 + (v1 - v0) * t;
    }

    public static float EaseIn(float t)
    {
        return t * t;
    }

    public static float EaseInCubic(float t)
    {
        return t * t * t;
    }

    static float Flip(float t)
    {
        return 1.0f - t;
    }

    public static float EaseOut(float t)
    {
        return Flip(EaseIn(Flip(t)));
    }

    public static float EaseOutCubic(float t)
    {
        return Flip(EaseInCubic(Flip(t)));
    }

    public static float EaseInOut(float t)
    {
        return Lerp(EaseIn(t), EaseOut(t), t);
    }

    public static float EaseInOutCubic(float t)
    {
        return Lerp(EaseInCubic(t), EaseOutCubic(t), t);
    }
}
