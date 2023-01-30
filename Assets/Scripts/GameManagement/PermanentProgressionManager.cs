using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GooglePlayGames;
using Unity.Mathematics;
using UnityEngine;

public class PermanentProgressionManager : MonoBehaviour
{
    private static SaveGame _savedGame;
    public static SaveGame savedGame
    {
        get
        {
            if (_savedGame == null)
            {
                _savedGame = LoadSaveGame();
            }

            return _savedGame;
        }
    }

    private static PermanentProgressionManager _instance;
    public static PermanentProgressionManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PermanentProgressionManager>();
            }

            return _instance;
        }
    }

    public static bool IsChallengeEnabed(Challenge challenge)
        => savedGame.challengesActive.Contains(challenge);

    public static SaveGame LoadSaveGame()
    {
        if (!File.Exists(Constants.SaveFilePath))
        {
            File.WriteAllText(Constants.SaveFilePath, JsonUtility.ToJson(SaveGame.Empty()));
        }

        var savedGameJson = File.ReadAllText(Constants.SaveFilePath);

        var parsedSavedGame = JsonUtility.FromJson<SaveGame>(savedGameJson);

        CleanupSavedGame(parsedSavedGame);

        return parsedSavedGame;
    }

    private static void CleanupSavedGame(SaveGame parsedSavedGame)
    {
        if (parsedSavedGame.stats == null)
        {
            parsedSavedGame.stats = new List<StatBlock>();
        }

        if (parsedSavedGame.challengesActive == null)
        {
            parsedSavedGame.challengesActive = new List<Challenge>();
        }

        if (parsedSavedGame.bestTime == default)
        {
            parsedSavedGame.bestTime = float.MaxValue;
        }

        var typesWithDuplicateEntries = parsedSavedGame.stats.GroupBy(s => s.type).Where(g => g.Count() > 1).ToList();
        foreach (var type in typesWithDuplicateEntries)
        {
            parsedSavedGame.stats = parsedSavedGame.stats.Where(s => s.type != type.Key).ToList();
        }

        foreach (var statType in EnumExtensions.GetValues<StatType>())
        {
            if (!parsedSavedGame.stats.Any(s => s.type == statType))
            {
                parsedSavedGame.stats.Add(StatBlock.Empty(statType));
            }
        }

        foreach (var stat in parsedSavedGame.stats)
        {
            var boundary = StatBlockBoundary.GetBoundary(stat.type);

            stat.value = Mathf.Clamp(stat.value, boundary.min, boundary.max);
        }
    }

    public static void SaveGameToFile()
    {
        File.WriteAllText(Constants.SaveFilePath, JsonUtility.ToJson(_savedGame));
    }

    public static void ResetSaveGame()
    {
        _savedGame = SaveGame.Empty();
        PermanentProgressionManager.SaveGameToFile();
    }

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);

        _savedGame = LoadSaveGame();
    }

    public static void AddCoin(int number = 1)
    {
        savedGame.coinCount += number;

        SaveGameToFile();

        GooglePlay.SafeIncrementEvent(GPGSIds.event_coin_collected, (uint)number);
    }

    public static void IncreaseKillCount(int number = 1)
    {
        savedGame.killCount += number;

        SaveGameToFile();
    }
}
