using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class VisualScriptingFunction : Attribute
{
    public string dropdownDescription;
    public string dynamicDescription;
    public string icon;
    public bool allowsChildren = false;
    
    public VisualScriptingFunction()
    {

    }

    public VisualScriptingFunction(string description, string dynamicDescription, string icon)
    {
        this.dropdownDescription = description;
        this.dynamicDescription = dynamicDescription;
        this.icon = icon;
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class VisualScriptingEvent : VisualScriptingFunction
{

}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class EventPreset : Attribute
{
    public string presetName;
    public EventPreset() { }
    public EventPreset(string presetName) { this.presetName = presetName;  }
}

public class Arg : Attribute
{
    public ArgType argType = ArgType.Temp;

    public bool allowValue = true;
    public bool allowPreset = true;
    public bool allowFunction = true;

    public string suffix = "";
    public string tempLabel = "";
    public string preset = "";


    public Arg(ArgType argType, string tempLabel)
    {
        this.tempLabel = tempLabel;
    }

    public Arg()
    {

    }

    public virtual Type GetStoredType() { return null; }
    public virtual object GetValue() { return null; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class NumberArg : Arg
{
    public float defaultValue;
    public NumberArg() { }
    public override Type GetStoredType() { return typeof(float); }
    public override object GetValue() { return defaultValue; }
}

public enum PresetChoices
{
    NoRestriction,
    IncreaseDecrease,
    BoolComparators,
    NumberComparators,
    Buffs,
    PlayOptions,
    VectorComponents,
    Rarities,
    Factions
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class StringArg : Arg
{
    private static string[] increaseDecrease = new string[] { "Increase", "Decrease" };
    private static string[] boolComparators = new string[] { "Equal To", "Not Equal To" };
    private static string[] numberComparators = new string[] { "Equal To", "Not Equal To", "Less Than",
            "Less Than or Equal To", "Greater Than", "Greater Than or Equal To" };
    private static string[] playOptions = new string[] { "Play", "Stop", "Play or Refresh" };
    private static string[] vectorComponents = new string[] { "X", "Y", "Z" };
    private static string[] buffs = null;
    private static string[] rarities = null;
    private static string[] factions = null;
    
    public static string[] GetOptions (PresetChoices choice)
    {
        switch (choice)
        {
            case PresetChoices.IncreaseDecrease:
                return increaseDecrease;

            case PresetChoices.BoolComparators:
                return boolComparators;

            case PresetChoices.NumberComparators:
                return numberComparators;

            case PresetChoices.Buffs:
                if (buffs == null)
                {
                    Stat[] allStats = (Stat[])Enum.GetValues(typeof(Stat));
                    buffs = new string[allStats.Length];
                    for (int i = 0; i < buffs.Length; i++)
                    {
                        buffs[i] = allStats[i].Label();
                    }
                }
                return buffs;

            case PresetChoices.PlayOptions:
                return playOptions;

            case PresetChoices.VectorComponents:
                return vectorComponents;

            case PresetChoices.Rarities:
                if (rarities == null) rarities = Enum.GetNames(typeof(Item.ItemRarity));
                return rarities;

            case PresetChoices.Factions:
                if (factions == null) factions = Enum.GetNames(typeof(Unit.Faction));
                return factions;

            default:
                return new string[] { };
        }
    }

    public PresetChoices choicePreset;
    //public int selectedPresetOption = 0;

    public string defaultValue;
    public StringArg() { }
    public override Type GetStoredType() { return typeof(string); }
    public override object GetValue() { return defaultValue; }

}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class UnitArg : Arg
{
    public Unit defaultValue;
    public UnitArg() { tempLabel = "Unit"; }
    public override Type GetStoredType() { return typeof(Unit); }
    public override object GetValue() { return defaultValue; }
}


[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class UnitGroupArg : Arg
{
    public UnitGroup defaultValue;
    public UnitGroupArg() { tempLabel = "Unit Group"; }
    public override Type GetStoredType() { return typeof(UnitGroup); }
    public override object GetValue() { return defaultValue; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class AudioClipArg : Arg
{
    public AudioClip defaultValue;
    public AudioClipArg() { tempLabel = "Audio Clip"; }
    public override Type GetStoredType() { return typeof(AudioClip); }
    public override object GetValue() { return defaultValue; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class VectorArg : Arg
{
    public Vector3 defaultValue;
    public VectorArg() { tempLabel = "Location"; }
    public override Type GetStoredType() { return typeof(Vector3); }
    public override object GetValue() { return defaultValue; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class BoolArg : Arg
{
    public bool defaultValue;
    public BoolArg() { tempLabel = "Bool"; }
    public override Type GetStoredType() { return typeof(bool); }
    public override object GetValue() { return defaultValue; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class ItemArg : Arg
{
    public Item defaultValue;
    public ItemArg() { tempLabel = "Item"; }
    public override Type GetStoredType() { return typeof(Item); }
    public override object GetValue() { return defaultValue; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class ColorArg : Arg
{
    public Color defaultValue = Color.white;
    public ColorArg() { tempLabel = "Color"; }
    public override Type GetStoredType() { return typeof(Color); }
    public override object GetValue() { return defaultValue; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class ProjectileArg : Arg
{
    public Projectile defaultValue;
    public ProjectileArg() { tempLabel = "Projectile"; }
    public override Type GetStoredType() { return typeof(Projectile); }
    public override object GetValue() { return defaultValue; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class AbilityArg : Arg
{
    public Ability defaultValue;
    public AbilityArg() { tempLabel = "Ability"; }
    public override Type GetStoredType() { return typeof(Ability); }
    public override object GetValue() { return defaultValue; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class EffectArg : Arg
{
    public CustomVisualEffect defaultValue;
    public EffectArg() { tempLabel = "Effect"; }
    public override Type GetStoredType() { return typeof(CustomVisualEffect); }
    public override object GetValue() { return defaultValue; }
}

public enum ArgType
{
    Temp,
    Value,
    Preset,
    Function
}

public static class ArgTypeExtensions
{
    public static GeneralNode.ReturnType ToNodeReturnType(this ArgType sourceType)
    {
        return sourceType switch
        {
            ArgType.Temp => GeneralNode.ReturnType.Temp,
            ArgType.Value => GeneralNode.ReturnType.Value,
            ArgType.Preset => GeneralNode.ReturnType.Preset,
            ArgType.Function => GeneralNode.ReturnType.Function,
            _ => throw new ArgumentOutOfRangeException(nameof(sourceType), $"No mapping exists for {sourceType}")
        };
    }
}