using MyUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using Random = UnityEngine.Random;

public partial class VisualCodeScript
{
    [VisualScriptingFunction(
        dropdownDescription = "Player/Player Level",
        dynamicDescription = "Player Level")]
    public float GetPlayerLevel()
    {
        Error(GameManager.player == null, "Player could not be found.");
        return GameManager.player.currentLevel;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Player Gold",
        dynamicDescription = "Player Gold")]
    public float GetPlayerGold()
    {
        Error(GameManager.player == null, "Player could not be found.");
        return GameManager.player.currentGold;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Items Equipped",
        dynamicDescription = "Count equipped items on player")]
    public float GetPlayerItemsEquipped()
    {
        Error(GameManager.player == null, "Player could not be found.");
        return GameManager.player.equipment.GetFilledSlots();
    }

    // TODO: IS THIS NEEDED?
    public float GetPlayerExperience()
    {
        Error(GameManager.player == null, "Player could not be found.");
        return GameManager.player.currentExperience;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Time/Time Since Level Start",
        dynamicDescription = "Time Since Level Start")]
    public float TimeSinceLevelStart()
    {
        return Time.timeSinceLevelLoad;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Time/Delta Time",
        dynamicDescription = "Delta Time")]
    public float DeltaTime()
    {
        return Time.deltaTime;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Time/Fixed Delta Time",
        dynamicDescription = "Fixed Delta Time")]
    public float FixedDeltaTime()
    {
        return Time.fixedDeltaTime;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Math/Addition",
        dynamicDescription = "$ + $")]
    [NumberArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public float Addition(float num1, float num2)
    {
        return num1 + num2;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Math/Subtraction",
        dynamicDescription = "$ - $")]
    [NumberArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public float Subtraction(float num1, float num2)
    {
        return num1 - num2;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Math/Multiplication",
        dynamicDescription = "$ x $")]
    [NumberArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public float Multiplication(float num1, float num2)
    {
        return num1 * num2;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Math/Division",
        dynamicDescription = "$ / $")]
    [NumberArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public float Division(float num1, float num2)
    {
        return num1 / num2;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Random/Random Number",
        dynamicDescription = "Random Number Between $ and $")]
    [NumberArg(argType = ArgType.Value, defaultValue = 0)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public float RandomNumberBetween(float num1, float num2)
    {
        return Random.Range(num1, num2);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Health",
        dynamicDescription = "$ Health")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public float UnitHealth(Unit unit)
    {
        Error(unit == null, "Specified unit is invalid.");
        return unit.health;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Max Health",
        dynamicDescription = "$ Max Health")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public float UnitMaxHealth(Unit unit)
    {
        Error(unit == null, "Specified unit is invalid.");
        return unit.stats.GetValue(Stat.MaxHealth);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Health Percent",
        dynamicDescription = "$ Health Percent")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public float HealthPercent(Unit unit)
    {
        Error(unit == null, "Specified unit is invalid.");
        return unit.GetCurrentHealthPercent();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Resource",
        dynamicDescription = "$ Resource")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public float UnitResource(Unit unit)
    {
        Error(unit == null, "Specified unit is invalid.");
        return unit.resource;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Max Resource",
        dynamicDescription = "$ Max Resource")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public float UnitMaxResource(Unit unit)
    {
        Error(unit == null, "Specified unit is invalid.");
        return unit.stats.GetValue(Stat.MaxResource);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Resource Percent",
        dynamicDescription = "$ Resource Percent")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public float ResourcePercent(Unit unit)
    {
        Error(unit == null, "Specified unit is invalid.");
        return unit.GetCurrentResourcePercent();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Math/Distance Between Points",
        dynamicDescription = "Distance between $ and $")]
    [VectorArg(argType = ArgType.Temp)]
    [VectorArg(argType = ArgType.Temp)]
    public float DistanceBetweenPoints(Vector3 vec1, Vector3 vec2)
    {
        return Vector3.Distance(vec1, vec2);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Math/Distance Between Units",
        dynamicDescription = "Distance between $ and $")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public float DistanceBetweenUnits(Unit unit1, Unit unit2)
    {
        Error(unit1 == null || unit2 == null, "Specified unit is invalid.");
        return Vector3.Distance(unit1.transform.position, unit2.transform.position);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Math/Vector Component",
        dynamicDescription = "$ Component of $")]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.VectorComponents, allowFunction = false, allowPreset = false)]
    [VectorArg(argType = ArgType.Temp)]
    public float VectorComponent(string component, Vector3 vector)
    {
        switch (component)
        {
            case "X":
                return vector.x;
            case "Y":
                return vector.y;
            case "Z":
                return vector.z;
            default:
                return 0;
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Count Units in Unit Group",
        dynamicDescription = "Number of units in $")]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    public float CountUnitsInUnitGroup(UnitGroup unitGroup)
    {
        Error(unitGroup == null, "Specified unit group is invalid.");
        return unitGroup.Count();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Number Variable",
        dynamicDescription = "Number: $")]
    [StringArg(argType = ArgType.Temp, allowFunction = false, allowPreset = false)]
    public float GetNumberVariable(string name)
    {
        return LogicEngine.current.GetLocalVariable<float>(name);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Number Variable",
        dynamicDescription = "Global Number: $")]
    [StringArg(argType = ArgType.Temp, allowFunction = false, allowPreset = false)]
    public float GetGlobalNumberVariable(string name)
    {
        return LogicEngine.GetGlobalVariable<float>(name);
    }
}
