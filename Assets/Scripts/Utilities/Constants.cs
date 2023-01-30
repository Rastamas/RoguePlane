using UnityEngine;

public static class Constants
{
    public const string EnemyTag = "Enemy";
    public const string PlayerTag = "Player";

    public const int PlayerLayer = 3;
    public const int EnemiesLayer = 7;
    public const int TerrainLayer = 8;
    public const int ImmuneLayer = 9;

    public static string SaveFilePath = Application.persistentDataPath + "/save.json";

    public static string AdConsentPlayerPrefKey = "UserConsent";
    public static string SoundPlayerPrefKey = "Sound";
    public static string MusicVolumePlayerPrefKey = "Music";
    public static string SfxVolumePlayerPrefKey = "Sfx";
    public static string VibratePlayerPrefKey = "Vibrate";
}
