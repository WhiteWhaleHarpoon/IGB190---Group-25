using System;

/// <summary>
/// Enum representing all valid stat modifiers. Add new modifiers here if needed.
/// </summary>
public enum Stat
{
    Damage,
    Armor,
    DamageTaken,
    AttacksPerSecond,
    MaxHealth,
    MaxResource,
    MovementSpeed,
    CooldownReduction,
    ResourceCostReduction,
    ResourceGeneration,
    CriticalStrikeChance,
    CriticalStrikeDamage
}

public static class StatExtensions
{
    private const string DamageLabel = "Damage";
    private const string ArmorLabel = "Armor";
    private const string AttackSpeedLabel = "Attack Speed";
    private const string MaxHealthLabel = "Max Health";
    private const string MaxResourceLabel = "Max Resource";
    private const string MovementSpeedLabel = "Movement Speed";
    private const string CriticalChanceLabel = "Critical Chance";
    private const string CriticalDamageLabel = "Critical Damage";
    private const string CooldownReductionLabel = "Cooldown Reduction";
    private const string AbilityCostReductionLabel = "Ability Cost Reduction";
    private const string DamageTakenLabel = "Damage Taken";
    private const string ResourceGenerationLabel = "Resource Generation";
    private const string StatNotFoundLabel = "Stat not found.";

    /// <summary>
    /// Converts a string label to its corresponding Stat enum value.
    /// </summary>
    public static Stat LabelToStat(string label)
    {
        return label switch
        {
            DamageLabel => Stat.Damage,
            ArmorLabel => Stat.Armor,
            AttackSpeedLabel => Stat.AttacksPerSecond,
            MaxHealthLabel => Stat.MaxHealth,
            MaxResourceLabel => Stat.MaxResource,
            MovementSpeedLabel => Stat.MovementSpeed,
            CriticalChanceLabel => Stat.CriticalStrikeChance,
            CriticalDamageLabel => Stat.CriticalStrikeDamage,
            CooldownReductionLabel => Stat.CooldownReduction,
            AbilityCostReductionLabel => Stat.ResourceCostReduction,
            DamageTakenLabel => Stat.DamageTaken,
            ResourceGenerationLabel => Stat.ResourceGeneration,
            _ => (Stat)Enum.Parse(typeof(Stat), label)
        };
    }

    /// <summary>
    /// Returns a human-readable label for the stat.
    /// </summary>
    public static string Label(this Stat stat)
    {
        return stat switch
        {
            Stat.Damage => DamageLabel,
            Stat.Armor => ArmorLabel,
            Stat.AttacksPerSecond => AttackSpeedLabel,
            Stat.MaxHealth => MaxHealthLabel,
            Stat.MaxResource => MaxResourceLabel,
            Stat.MovementSpeed => MovementSpeedLabel,
            Stat.CriticalStrikeChance => CriticalChanceLabel,
            Stat.CriticalStrikeDamage => CriticalDamageLabel,
            Stat.CooldownReduction => CooldownReductionLabel,
            Stat.ResourceCostReduction => AbilityCostReductionLabel,
            Stat.DamageTaken => DamageTakenLabel,
            Stat.ResourceGeneration => ResourceGenerationLabel,
            _ => StatNotFoundLabel
        };
    }

    /// <summary>
    /// Determines whether a stat should be represented as a percentage.
    /// </summary>
    public static bool ShowAsPercent(this Stat stat, bool isPercent)
    {
        return stat == Stat.CriticalStrikeChance || stat == Stat.CriticalStrikeDamage || isPercent;
    }

    /// <summary>
    /// Checks if the stat is valid based on whether it is a percentage or a value.
    /// </summary>
    public static bool IsValidStat(this Stat stat, bool isPercent)
    {
        return stat switch
        {
            Stat.CriticalStrikeChance => !isPercent,
            Stat.CriticalStrikeDamage => !isPercent,
            Stat.MovementSpeed => isPercent,
            Stat.AttacksPerSecond => isPercent,
            Stat.ResourceGeneration => isPercent,
            _ => true
        };
    }

    /// <summary>
    /// Determines if a stat is considered a "basic" stat for general usage.
    /// </summary>
    public static bool IsBasicStat(this Stat stat, bool isPercent)
    {
        return stat switch
        {
            Stat.CriticalStrikeChance => !isPercent,
            Stat.CriticalStrikeDamage => !isPercent,
            Stat.MovementSpeed => isPercent,
            Stat.AttacksPerSecond => isPercent,
            Stat.ResourceCostReduction => isPercent,
            Stat.ResourceGeneration => isPercent,
            Stat.CooldownReduction => isPercent,
            Stat.DamageTaken => false,
            _ => true
        };
    }

    /// <summary>
    /// Determines the display modifier for a stat, used for visual representation in UIs.
    /// </summary>
    public static float DisplayModifier(this Stat stat, bool isPercent)
    {
        return isPercent || stat == Stat.CriticalStrikeChance || stat == Stat.CriticalStrikeDamage ? 100.0f : 1.0f;
    }
}