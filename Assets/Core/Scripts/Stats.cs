using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a collection of stats, allowing modification and retrieval of current values.
/// </summary>
public class Stats
{
    private readonly Dictionary<Stat, StatModifier> stats = new Dictionary<Stat, StatModifier>();

    /// <summary>
    /// Registers a stat with a label and base value to be managed by this class.
    /// </summary>
    public void TrackStat(Stat stat, string label, float baseValue)
    {
        if (!stats.ContainsKey(stat))
        {
            stats.Add(stat, new StatModifier(label, baseValue));
        }
    }

    /// <summary>
    /// Provides indexed access to the StatModifier associated with a stat.
    /// </summary>
    public StatModifier this[Stat stat] => stats.ContainsKey(stat) ? stats[stat] : null;

    /// <summary>
    /// Retrieves the StatModifier associated with a stat.
    /// </summary>
    public StatModifier Get(Stat stat) => this[stat];

    /// <summary>
    /// Removes all timed modifiers from all tracked stats.
    /// </summary>
    public void RemoveAllTimedModifiers()
    {
        foreach (StatModifier statModifier in stats.Values)
        {
            statModifier.RemoveAllTimedModifiers();
        }
    }

    /// <summary>
    /// Returns the current value of the specified stat.
    /// </summary>
    public float GetValue(Stat stat)
    {
        return stats.ContainsKey(stat) ? stats[stat].GetValue() : 0f;
    }

    /// <summary>
    /// Returns the current value of the specified stat, including an offset.
    /// </summary>
    public float GetValue(Stat stat, float offset)
    {
        return stats.ContainsKey(stat) ? stats[stat].GetValue(offset) : 0f;
    }

    /// <summary>
    /// Checks if any stat has a buff with the specified label.
    /// </summary>
    public bool HasBuffWithLabel(string label)
    {
        foreach (StatModifier statModifier in stats.Values)
        {
            if (statModifier.HasBuffWithLabel(label))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Removes all modifiers with the specified label from all stats.
    /// </summary>
    public void RemoveBuffWithLabel(string label)
    {
        foreach (StatModifier statModifier in stats.Values)
        {
            statModifier.RemoveModifiersWithLabel(label);
        }
    }
}