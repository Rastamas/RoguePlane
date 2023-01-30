using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class SaveGame
{
    public int version;
    public int killCount;
    public int coinCount;
    public int trophyCount;
    public float bestTime;
    public bool beatBoss;
    public List<Challenge> challengesActive;
    public List<StatBlock> stats;

    public static SaveGame Empty() => new SaveGame
    {
        version = 0,
        coinCount = 0,
        killCount = 0,
        trophyCount = 0,
        bestTime = float.MaxValue,
        beatBoss = false,
        challengesActive = new List<Challenge>(),
        stats = EnumExtensions.GetValues<StatType>().Select(t => StatBlock.Empty(t)).ToList()
    };
}
