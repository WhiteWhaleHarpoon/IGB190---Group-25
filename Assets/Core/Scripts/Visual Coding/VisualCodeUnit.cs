using MyUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class VisualCodeScript
{
    // TODO: CAN PROBABLY REMOVE.
    public Unit GetOwner()
    {
        return LogicEngine.current.GetOwner();
    }

    // TODO: CAN PROBABLY REMOVE.
    public Unit GetPlayer()
    {
        return GameManager.player;
    }

    // TODO: CAN PROBABLY REMOVE.
    public Unit GetLastCreatedUnit()
    {
        return lastCreatedUnit;
    }

    // TODO: CAN PROBABLY REMOVE.
    public Unit GetTriggeringUnit()
    {
        return triggeringUnit;
    }

    // TODO: CAN PROBABLY REMOVE.
    public Unit GetDamagingUnit()
    {
        return damagingUnit;
    }

    // TODO: CAN PROBABLY REMOVE.
    public Unit GetKillingUnit()
    {
        return killingUnit;
    }

    // TODO: CAN PROBABLY REMOVE.
    public Unit GetHealingUnit()
    {
        return healingUnit;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Distance/Closest Unit Near Point",
        dynamicDescription = "Closest unit within $ distance of $")]
    [NumberArg(argType = ArgType.Temp, suffix = "m")]
    [VectorArg(argType = ArgType.Temp)]
    public Unit ClosestUnitToPoint(float distance, Vector3 point)
    {
        return Utilities.GetClosest<Unit>(point, distance);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Distance/Furthest Unit Near Point",
        dynamicDescription = "Furthest unit within $ distance of $")]
    [NumberArg(argType = ArgType.Temp, suffix = "m")]
    [VectorArg(argType = ArgType.Temp)]
    public Unit FurthestUnitToPoint(float distance, Vector3 point)
    {
        return Utilities.GetFurthest<Unit>(point, distance);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Distance/Closest Unit Near Unit",
        dynamicDescription = "Closest unit within $ distance of $")]
    [NumberArg(argType = ArgType.Temp, suffix = "m")]
    [UnitArg(argType = ArgType.Temp)]
    public Unit ClosestUnitToUnit(float distance, Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        return Utilities.GetClosest<Unit>(unit.transform.position, distance);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Distance/Furthest Unit Near Unit",
        dynamicDescription = "Furthest unit within $ distance of $")]
    [NumberArg(argType = ArgType.Temp, suffix = "m")]
    [UnitArg(argType = ArgType.Temp)]
    public Unit FurthestUnitToUnit(float distance, Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        return Utilities.GetFurthest<Unit>(unit.transform.position, distance);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Random/Random Unit Near Unit",
        dynamicDescription = "Random unit within $ of $")]
    [NumberArg(argType = ArgType.Temp, suffix = "m")]
    [UnitArg(argType = ArgType.Temp)]
    public Unit RandomNearbyUnitToUnit(float distance, Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        List<Unit> units = Utilities.GetAllWithinRange<Unit>(unit.transform.position, distance);
        if (units.Count == 0) return null;
        return units[Random.Range(0, units.Count)];
    }

    [VisualScriptingFunction(
        dropdownDescription = "Random/Random Unit Near Point",
        dynamicDescription = "Random unit within $ distance of $")]
    [NumberArg(argType = ArgType.Temp, suffix = "m")]
    [VectorArg(argType = ArgType.Temp)]
    public Unit RandomNearbyUnitToPoint(float distance, Vector3 point)
    {
        List<Unit> units = Utilities.GetAllWithinRange<Unit>(point, distance);
        if (units.Count == 0) return null;
        return units[Random.Range(0, units.Count)];
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit with Label",
        dynamicDescription = "Unit labeled $")]
    [StringArg(argType = ArgType.Temp)]
    public Unit GetUnitWithLabel(string label)
    {
        Monster[] monsters = GameObject.FindObjectsByType<Monster>(FindObjectsSortMode.None);
        foreach (Monster monster in monsters)
        {
            if (monster.monsterLabel == label) return monster;
        }
        return null;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit Variable",
        dynamicDescription = "Unit: $")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name")]
    public Unit GetUnitVariable(string name)
    {
        return LogicEngine.current.GetLocalVariable<Unit>(name);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Unit Variable",
        dynamicDescription = "Global Unit: $")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name")]
    public Unit GetGlobalUnitVariable(string name)
    {
        return LogicEngine.GetGlobalVariable<Unit>(name);
    }

    // TODO: IS THIS USED??
    public Unit PopUnitFromUnitGroup(UnitGroup unitGroup)
    {
        Error(unitGroup == null, "The specified unit group is invalid.");
        return unitGroup.PopNextUnit();
    }
}
