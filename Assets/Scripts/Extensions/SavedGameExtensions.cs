using System.Linq;

public static class SavedGameExtensions
{
    public static float GetStat(this SaveGame savedGame, StatType type)
    {
        var statBlock = PermanentProgressionManager.IsChallengeEnabed(Challenge.Baseline)
            ? StatBlock.Empty(type)
            : savedGame.stats?.FirstOrDefault(s => s.type == type) ?? StatBlock.Empty(type);
        var boundaries = StatBlockBoundary.GetBoundary(type);

        return statBlock.value / boundaries.divider;
    }
}
