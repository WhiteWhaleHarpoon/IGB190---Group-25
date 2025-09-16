using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class VisualCodeScript
{

    [VisualScriptingFunction(
        dropdownDescription = "Name of Unit",
        dynamicDescription = "Name of $")]
    [UnitArg(argType = ArgType.Temp)]
    public string NameOfUnit(Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        return unit.unitName;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Name of Ability",
        dynamicDescription = "Name of $")]
    [AbilityArg(argType = ArgType.Temp)]
    public string NameOfAbility(Ability ability)
    {
        Error(ability == null, "The specified ability is invalid.");
        return ability.abilityName;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Tag of Ability",
        dynamicDescription = "Tag of $")]
    [AbilityArg(argType = ArgType.Temp)]
    public string TagOfAbility(Ability ability)
    {
        Error(ability == null, "The specified ability is invalid.");
        return ability.abilityTag;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Tag of Item",
        dynamicDescription = "Tag of $")]
    [ItemArg(argType = ArgType.Temp)]
    public string TagOfItem(Item item)
    {
        Error(item == null, "The specified item is invalid.");
        return item.itemTag;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Combine Strings",
        dynamicDescription = "$ + $")]
    [StringArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Temp)]
    public string CombineStrings(string str1, string str2)
    {
        return str1 + str2;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/String Variable",
        dynamicDescription = "String: $")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name")]
    public string GetStringVariable(string name)
    {
        return LogicEngine.current.GetLocalVariable<string>(name);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/Global String Variable",
        dynamicDescription = "Global String: $")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name")]
    public string GetGlobalStringVariable(string name)
    {
        return LogicEngine.GetGlobalVariable<string>(name);
    }


}
