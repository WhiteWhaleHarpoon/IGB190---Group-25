using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class VisualCodeScript
{
    // TODO: CAN REMOVE?
    public Ability LastAbilityCast ()
    {
        return lastAbilityCast; 
    }

    // TODO: CAN REMOVE?
    public Ability ThisAbility()
    {
        return (Ability)LogicEngine.current.engineHandler;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Random Ability on Unit",
        dynamicDescription = "Random Ability by $")]
    [UnitArg(argType = ArgType.Temp)]
    public Ability RandomAbilityOnUnit (Unit unit)
    {
        if (unit == null)
        {
            Error("The specified unit is invalid.");
            return null;
        }
        if (unit.abilities.Count == 0)
        {
            Error("The specified unit has no abilities.");
            return null;
        }

        Ability ability = unit.abilities[Random.Range(0, unit.abilities.Count)];
        if (ability)
        {
            Error("The random ability was undefined.");
            return null;
        }
        return ability;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Last Ability Cast by Unit",
        dynamicDescription = "Last Ability Cast by $")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public Ability LastAbilityCastByUnit(Unit unit)
    {
        if (unit == null)
        {
            Error("The specified unit is invalid.");
            return null;
        }
        return unit.lastAbilityCast;
    }
}
