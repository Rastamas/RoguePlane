using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefExtensions
{
    public static bool GetBool(string key, bool defaultValue = false)
        => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;

    public static void SetBool(string key, bool value)
        => PlayerPrefs.SetInt(key, value ? 1 : 0);
}
