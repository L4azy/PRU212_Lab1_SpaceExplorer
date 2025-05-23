using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions
{
    static readonly Dictionary<float, WaitForSeconds> WaitForSeconds = new();
    public static WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (WaitForSeconds.TryGetValue(seconds, out var wait))
            return wait;
        wait = new WaitForSeconds(seconds);
        WaitForSeconds.Add(seconds, wait);
        return wait;
    }

    public static T SafeGetByIndex<T>(this List<T> list, int index)
    {
        if (index >= 0 && index < list.Count)
            return list[index];

        return default;
	}
}
