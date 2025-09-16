using MyUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class VisualCodeScript
{
    [VisualScriptingFunction(
        dropdownDescription = "Empty Unit Group",
        dynamicDescription = "Empty Unit Group")]
    public UnitGroup EmptyUnitGroup()
    {
        return UnitGroup.Empty();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit Group Variable",
        dynamicDescription = "Unit Group: $")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name")]
    public UnitGroup GetUnitGroupVariable(string name)
    {
        if (!LogicEngine.current.localVariables.ContainsKey(name)) return UnitGroup.Empty();
        return LogicEngine.current.GetLocalVariable<UnitGroup>(name);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Unit Group Variable",
        dynamicDescription = "Global Unit Group: $")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name")]
    public UnitGroup GetGlobalUnitGroupVariable(string name)
    {
        if (!LogicEngine.globalVariables.ContainsKey(name)) return UnitGroup.Empty();
        return LogicEngine.GetGlobalVariable<UnitGroup>(name);
    }

    [VisualScriptingFunction(
        dropdownDescription = "All Enemies In Arc From Unit",
        dynamicDescription = "All enemies in $ arc from $ extending $")]
    [NumberArg(argType = ArgType.Value, defaultValue = 90, suffix = "�")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Temp, suffix = "m")]
    public UnitGroup AllEnemiesInArcFromUnit(float arc, Unit unit, float distance)
    {
        Error(unit == null, "The specified unit is invalid.");
        return AllEnemiesInArc(unit, arc, unit.transform.position, unit.transform.position + unit.transform.forward * distance);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Other/Alls Enemies In Arc",
        dynamicDescription = "All enemies of $ in $ degree arc from $ to $")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 90, suffix = "�")]
    [VectorArg(argType = ArgType.Temp)]
    [VectorArg(argType = ArgType.Temp)]
    public UnitGroup AllEnemiesInArc(Unit factionCheck, float arc, Vector3 from, Vector3 to)
    {
        Error(factionCheck == null, "The specified unit is invalid.");
        float distance = Vector3.Distance(from, to);
        List<Unit> allNearbyUnits = Utilities.GetAllWithinRange<Unit>(from, distance);
        List<Unit> matches = new List<Unit>();

        Vector3 compare = (to - from).normalized;
        float threshold = (arc - 180) / -180;

        foreach (Unit possibleMatch in allNearbyUnits)
        {
            if (possibleMatch.GetFaction() != factionCheck.GetFaction())
            {
                float dot = Vector3.Dot(compare, (possibleMatch.transform.position - from).normalized);
                if (dot > threshold)
                {
                    matches.Add(possibleMatch);
                }
            }
        }
        return new UnitGroup(matches);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Other/Alls Units Near Point",
        dynamicDescription = "All units within $ of $")]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "m")]
    [VectorArg(argType = ArgType.Temp)]
    public UnitGroup AllUnitsWithinRangeOfPoint(float distance, Vector3 point)
    {
        List<Unit> units = Utilities.GetAllWithinRange<Unit>(point, distance);
        return new UnitGroup(units);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Other/Alls Units Near Unit",
        dynamicDescription = "All units within $ of $")]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "m")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public UnitGroup AllUnitsWithinRangeOfUnit(float distance, Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        List<Unit> units = Utilities.GetAllWithinRange<Unit>(unit.transform.position, distance);
        return new UnitGroup(units);
    }

    // TODO: REMOVE
    public UnitGroup AllEnemiesWithinRangeOfPoint(Unit unit, float distance, Vector3 point)
    {
        Error(unit == null, "The specified unit is invalid.");
        List<Unit> allNearbyUnits = Utilities.GetAllWithinRange<Unit>(point, distance);
        List<Unit> enemies = new List<Unit>();
        foreach (Unit possibleMatch in allNearbyUnits)
            if (unit.GetFaction() != possibleMatch.GetFaction())
                enemies.Add(possibleMatch);
        return new UnitGroup(enemies);
    }

    // TODO: REMOVE
    public UnitGroup AllEnemiesWithinRangeOfUnit(Unit factionToCheck, float distance, Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        Error(factionToCheck == null, "The specified unit to faction check is invalid.");
        List<Unit> allNearbyUnits = Utilities.GetAllWithinRange<Unit>(unit.transform.position, distance);
        List<Unit> enemies = new List<Unit>();
        foreach (Unit possibleMatch in allNearbyUnits)
            if (factionToCheck.GetFaction() != possibleMatch.GetFaction())
                enemies.Add(possibleMatch);
        return new UnitGroup(enemies);
    }

    // TODO: REMOVE
    public UnitGroup RandomUnitsWithinRangeOfPoint(int count, float distance, Vector3 point)
    {
        List<Unit> units = Utilities.GetAllWithinRange<Unit>(point, distance);
        List<Unit> returnUnits = new List<Unit>();
        for (int i = 0; i < count; i++)
        {
            Unit u = units[Random.Range(0, units.Count)];
            units.Remove(u);
            returnUnits.Add(u);
        }
        return new UnitGroup(returnUnits);
    }

    [VisualScriptingFunction(
        dropdownDescription = "All Enemies Near Point",
        dynamicDescription = "All enemies within $ of $")]
    [NumberArg(argType = ArgType.Temp, suffix = "m")]
    [VectorArg(argType = ArgType.Temp, allowValue = false)]
    public UnitGroup AllEnemiesWithinRangeOfPoint2(float distance, Vector3 point)
    {
        Unit factionToCheck = LogicEngine.current.GetOwner();
        List<Unit> allNearbyUnits = Utilities.GetAllWithinRange<Unit>(point, distance);
        List<Unit> enemies = new List<Unit>();
        foreach (Unit possibleMatch in allNearbyUnits)
            if (factionToCheck.GetFaction() != possibleMatch.GetFaction())
                enemies.Add(possibleMatch);
        return new UnitGroup(enemies);
    }

    [VisualScriptingFunction(
        dropdownDescription = "All Enemies Near Unit",
        dynamicDescription = "All enemies within $ of $")]
    [NumberArg(argType = ArgType.Temp, suffix = "m")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public UnitGroup AllEnemiesWithinRangeOfUnit2(float distance, Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        Unit factionToCheck = LogicEngine.current.GetOwner();
        List<Unit> allNearbyUnits = Utilities.GetAllWithinRange<Unit>(unit.transform.position, distance);
        List<Unit> enemies = new List<Unit>();
        foreach (Unit possibleMatch in allNearbyUnits)
            if (factionToCheck.GetFaction() != possibleMatch.GetFaction())
                enemies.Add(possibleMatch);
        return new UnitGroup(enemies);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Random/Random Units Near Unit",
        dynamicDescription = "$ random units within $ of $")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    [NumberArg(argType = ArgType.Value, defaultValue = 3, suffix = "m")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public UnitGroup RandomUnitsWithinRangeOfUnit(float count, float distance, Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        List<Unit> units = Utilities.GetAllWithinRange<Unit>(unit.transform.position, distance);
        List<Unit> returnUnits = new List<Unit>();
        for (int i = 0; i < (int)count; i++)
        {
            Unit u = units[Random.Range(0, units.Count)];
            units.Remove(u);
            returnUnits.Add(u);
        }
        return new UnitGroup(returnUnits);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Units with Tag",
        dynamicDescription = "Units with Tag $")]
    [StringArg(argType = ArgType.Temp, allowPreset = false, allowFunction = false)]
    public UnitGroup GetUnitsWithTag(string tag)
    {
        List<Unit> units = new List<Unit>();
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objs)
        {
            Unit unit = obj.GetComponent<Unit>();
            if (unit != null)
            {
                units.Add(unit);
            }
        }
        return new UnitGroup(units);
    }
}
