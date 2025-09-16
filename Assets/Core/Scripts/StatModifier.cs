using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles a single stat with both permanent and timed modifiers.
/// Supports applying, removing, and retrieving modified stat values.
/// </summary>
public class StatModifier
{
    private const string NO_BUFF = "None";

    public string Label { get; private set; }
    private float baseValue;
    private List<Modifier> timedModifiers = new List<Modifier>();
    private List<Modifier> permanentModifiers = new List<Modifier>();

    private bool requiresUpdate = true;
    private float cachedValue;

    public StatModifier(string label, float baseValue)
    {
        Label = label;
        this.baseValue = baseValue;
    }

    public void SetBaseValue(float baseValue)
    {
        this.baseValue = baseValue;
        requiresUpdate = true;
    }

    public void ModifyBaseValue(float change)
    {
        this.baseValue += change;
        requiresUpdate = true;
    }

    public void AddTimedValueModifier(float value, float duration, string buff = NO_BUFF, int maxStacks = 99)
    {
        AddModifier(value, duration, false, buff, maxStacks, timedModifiers);
    }

    public void AddValueModifier(float value, string buff = NO_BUFF, int maxStacks = 99)
    {
        permanentModifiers.Add(new Modifier(value, 0, false, buff));
        requiresUpdate = true;
    }

    public void AddTimedPercentageModifier(float value, float duration, string buff = NO_BUFF, int maxStacks = 99)
    {
        AddModifier(value, duration, true, buff, maxStacks, timedModifiers);
    }

    public void AddPercentageModifier(float value, string buff = NO_BUFF, int maxStacks = 99)
    {
        permanentModifiers.Add(new Modifier(value, 0, true, buff));
        requiresUpdate = true;
    }

    public void RemoveModifiersWithLabel(string label)
    {
        timedModifiers.RemoveAll(x => x.Label == label);
        permanentModifiers.RemoveAll(x => x.Label == label);
        requiresUpdate = true;
    }

    public float GetValue()
    {
        RemoveExpiredModifiers();
        if (requiresUpdate)
        {
            cachedValue = CalculateModifiedValue();
            requiresUpdate = false;
        }
        return cachedValue;
    }

    public float GetValue(float additionalValue)
    {
        return CalculateModifiedValue(additionalValue);
    }

    public bool HasBuffWithLabel(string label)
    {
        if (label == NO_BUFF) return false;
        return ContainsModifierWithLabel(label, timedModifiers) || ContainsModifierWithLabel(label, permanentModifiers);
    }

    public void RemoveAllTimedModifiers()
    {
        timedModifiers.Clear();
        requiresUpdate = true;
    }

    private void AddModifier(float value, float duration, bool isPercentage, string buff, int maxStacks, List<Modifier> modifierList)
    {
        RemoveExpiredModifiers();

        Modifier existingModifier = GetModifierWithLabel(buff, modifierList);
        if (existingModifier != null)
        {
            modifierList.Remove(existingModifier);
            existingModifier.RemoveAt = Time.time + duration;
            existingModifier.Stacks = Mathf.Min(existingModifier.Stacks + 1, maxStacks);
        }
        else
        {
            existingModifier = new Modifier(value, duration, isPercentage, buff);
        }

        InsertModifier(existingModifier, modifierList);
        requiresUpdate = true;
    }

    private void InsertModifier(Modifier modifier, List<Modifier> modifierList)
    {
        float removeAt = modifier.RemoveAt;
        int insertIndex = modifierList.FindIndex(m => m.RemoveAt > removeAt);
        if (insertIndex == -1) insertIndex = modifierList.Count;
        modifierList.Insert(insertIndex, modifier);
    }

    private void RemoveExpiredModifiers()
    {
        while (timedModifiers.Count > 0 && Time.time > timedModifiers[0].RemoveAt)
        {
            timedModifiers.RemoveAt(0);
            requiresUpdate = true;
        }
    }

    private float CalculateModifiedValue(float additionalValue = 0)
    {
        float value = baseValue + additionalValue;
        float percentage = 0;

        foreach (Modifier modifier in permanentModifiers)
        {
            ApplyModifier(ref value, ref percentage, modifier);
        }

        foreach (Modifier modifier in timedModifiers)
        {
            ApplyModifier(ref value, ref percentage, modifier);
        }

        return value * (1 + percentage);
    }

    private void ApplyModifier(ref float value, ref float percentage, Modifier modifier)
    {
        if (modifier.IsPercentage)
        {
            percentage += modifier.Value * modifier.Stacks;
        }
        else
        {
            value += modifier.Value * modifier.Stacks;
        }
    }

    private Modifier GetModifierWithLabel(string label, List<Modifier> modifierList)
    {
        return modifierList.Find(m => m.Label == label);
    }

    private bool ContainsModifierWithLabel(string label, List<Modifier> modifierList)
    {
        return modifierList.Exists(m => m.Label == label);
    }

    public class Modifier
    {
        public float Value { get; private set; }
        public float RemoveAt { get; set; }
        public bool IsPercentage { get; private set; }
        public string Label { get; private set; }
        public int Stacks { get; set; }

        public Modifier(float value, float duration, bool isPercentage, string label = "")
        {
            Value = value;
            RemoveAt = Time.time + duration;
            IsPercentage = isPercentage;
            Label = label;
            Stacks = 1;
        }
    }
}