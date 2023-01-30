using System;
using System.Collections.Generic;

[Serializable]
public class StatBlock
{
    public StatType type;
    public int value;

    public static StatBlock Empty(StatType type)
    {
        return type switch
        {
            StatType.Damage => new StatBlock { type = type, value = 12 },
            StatType.AttackSpeed => new StatBlock { type = type, value = 12 },
            StatType.Health => new StatBlock { type = type, value = 150 },
            StatType.MoveSpeed => new StatBlock { type = type, value = 10 },
            _ => throw new NotImplementedException()
        };
    }
}

public class StatBlockBoundary
{
    public int min;
    public int max;
    public int increment;
    public float divider;

    private static Dictionary<StatType, StatBlockBoundary> _boundaries = new Dictionary<StatType, StatBlockBoundary>()
    {

         {StatType.Damage, new StatBlockBoundary { min = 12, max = 22, increment = 1, divider = 1 }},
         {StatType.AttackSpeed, new StatBlockBoundary { min = 12, max = 22, increment = 1, divider = 5 }},
         {StatType.Health, new StatBlockBoundary { min = 150, max = 250, increment = 10, divider = 1 }},
         {StatType.MoveSpeed, new StatBlockBoundary { min = 10, max = 30, increment = 2, divider = 10 }}
    };

    public static StatBlockBoundary GetBoundary(StatType type) => _boundaries[type];
}

public enum StatType
{
    AttackSpeed,
    Damage,
    Health,
    MoveSpeed,
}
