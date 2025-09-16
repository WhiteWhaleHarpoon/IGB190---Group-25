using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class VisualCodeScript
{
    [VisualScriptingFunction(
        dropdownDescription = "Region/Region Exists",
        dynamicDescription = "Region labeled $ exists",
        icon = regionIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Region Name")]
    public bool RegionExists(string regionName)
    {
        return Region.RegionExists(regionName);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Region/Unit Is In Region",
        dynamicDescription = "$ is in region $",
        icon = regionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Region Name")]
    public bool UnitIsInRegion(Unit unit, string regionName)
    {
        Error(unit == null, "The specified unit does not exist.");
        return Region.UnitIsInRegion(unit, regionName);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Input/Key is Held",
        dynamicDescription = "$ is Held",
        icon = inputIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Key")]
    public bool KeyIsHeld(string keyString)
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (key.ToString().ToUpper() == keyString.ToUpper() && Input.GetKey(key))
            {
                return true;
            }
        }
        return false;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit Group/Unit Group is Empty",
        dynamicDescription = "$ is empty",
        icon = conditionIcon)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    public bool UnitGroupIsEmpty(UnitGroup unitGroup)
    {
        Error(unitGroup == null, "The specified unit group is invalid.");
        return (unitGroup.Count() == 0);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit Group/Unit Group is not Empty",
        dynamicDescription = "$ is not empty",
        icon = conditionIcon)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    public bool UnitGroupIsNotEmpty(UnitGroup unitGroup)
    {
        Error(unitGroup == null, "The specified unit group is invalid.");
        return (unitGroup.Count() > 0);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit Group/Unit is in Unit Group",
        dynamicDescription = "$ is in $",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    public bool UnitIsInUnitGroup(Unit unit, UnitGroup unitGroup)
    {
        Error(unit == null, "The specified unit is invalid.");
        Error(unitGroup == null, "The specified unit group is invalid.");
        return (unitGroup.Contains(unit));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit Group/Unit is not in Unit Group",
        dynamicDescription = "$ is not in $",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    public bool UnitIsNotInUnitGroup(Unit unit, UnitGroup unitGroup)
    {
        Error(unit == null, "The specified unit is invalid.");
        Error(unitGroup == null, "The specified unit group is invalid.");
        return (!unitGroup.Contains(unit));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Comparisons/Or Comparison",
        dynamicDescription = "$ or $",
        icon = conditionIcon)]
    [BoolArg(argType = ArgType.Temp)]
    [BoolArg(argType = ArgType.Temp)]
    public bool OrComparison(bool bool1, bool bool2)
    {
        return bool1 || bool2;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Comparisons/And Comparison",
        dynamicDescription = "$ and $",
        icon = conditionIcon)]
    [BoolArg(argType = ArgType.Temp)]
    [BoolArg(argType = ArgType.Temp)]
    public bool AndComparison(bool bool1, bool bool2)
    {
        return (bool1 && bool2);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Comparisons/Bool Comparison",
        dynamicDescription = "$ $ $",
        icon = conditionIcon)]
    [BoolArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.BoolComparators, allowPreset = false, allowFunction = false)]
    [BoolArg(argType = ArgType.Temp)]
    public bool BoolComparison(bool bool1, string comparator, bool bool2)
    {
        switch (comparator)
        {
            case "Equal To":
                return bool1 == bool2;
            case "Not Equal To":
                return bool1 != bool2;
            default:
                return false;
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Comparisons/Number Comparison",
        dynamicDescription = "$ $ $",
        icon = conditionIcon)]
    [NumberArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.NumberComparators, allowPreset = false, allowFunction = false)]
    [NumberArg(argType = ArgType.Temp)]
    public bool NumberComparison(float num1, string comparator, float num2)
    {
        switch (comparator)
        {
            case "Equal To":
                return num1 == num2;
            case "Not Equal To":
                return num1 != num2;
            case "Less Than":
                return num1 < num2;
            case "Less Than or Equal To":
                return num1 <= num2;
            case "Greater Than":
                return num1 > num2;
            case "Greater Than or Equal To":
                return num1 >= num2;
            default:
                return false;
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Comparisons/Vector Comparison",
        dynamicDescription = "$ $ $",
        icon = conditionIcon)]
    [VectorArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.BoolComparators, allowPreset = false, allowFunction = false)]
    [VectorArg(argType = ArgType.Temp)]
    public bool VectorComparison(Vector3 vec1, string comparator, Vector3 vec2)
    {
        switch (comparator)
        {
            case "Equal To":
                return vec1 == vec2;
            case "Not Equal To":
                return vec1 != vec2;
            default:
                return false;
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Comparisons/Vector Comparison",
        dynamicDescription = "$ $ $",
        icon = conditionIcon)]
    [AbilityArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.BoolComparators, allowPreset = false, allowFunction = false)]
    [AbilityArg(argType = ArgType.Temp)]
    public bool AbilityComparison(Ability ability1, string comparator, Ability ability2)
    {
        Error(ability1 == null, "The specified ability is invalid.");
        Error(ability2 == null, "The specified ability is invalid.");
        switch (comparator)
        {
            case "Equal To":
                return Ability.AbilitiesShareTemplate(ability1, ability2);
            case "Not Equal To":
                return !Ability.AbilitiesShareTemplate(ability1, ability2);
            default:
                return false;
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Comparisons/String Comparison",
        dynamicDescription = "$ $ $",
        icon = conditionIcon)]
    [StringArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.BoolComparators, allowPreset = false, allowFunction = false)]
    [StringArg(argType = ArgType.Temp)]
    public bool StringComparison(string string1, string comparator, string string2)
    {
        switch (comparator)
        {
            case "Equal To":
                return string1 == string2;
            case "Not Equal To":
                return string1 != string2;
            default:
                return false;
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Comparisons/Unit Comparison",
        dynamicDescription = "$ $ $",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.BoolComparators, allowPreset = false, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp)]
    public bool UnitComparison(Unit unit1, string comparator, Unit unit2)
    {
        Error(unit1 == null, "The specified unit is invalid.");
        Error(unit2 == null, "The specified unit is invalid.");
        switch (comparator)
        {
            case "Equal To":
                return unit1 == unit2;
            case "Not Equal To":
                return unit1 != unit2;
            default:
                return false;
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Bool Variable",
        dynamicDescription = "Bool: $",
        icon = conditionIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    public bool GetBoolVariable(string name)
    {
        return LogicEngine.current.GetLocalVariable<bool>(name);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Bool Variable",
        dynamicDescription = "Global Bool: $",
        icon = conditionIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    public bool GetGlobalBoolVariable(string name)
    {
        return LogicEngine.GetGlobalVariable<bool>(name);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Matches Template",
        dynamicDescription = "$ type matches $",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp)]
    [UnitArg(argType = ArgType.Temp)]
    public bool UnitTypeMatch(Unit unit1, Unit unit2)
    {
        Error(unit1 == null, "The specified unit is invalid.");
        Error(unit2 == null, "The specified unit is invalid.");
        return unit1.unitName == unit2.unitName;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Is Moving",
        dynamicDescription = "$ is moving",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public bool UnitIsMoving(Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        return unit.IsMoving();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Is Stationary",
        dynamicDescription = "$ is stationary",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public bool UnitIsStationary(Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        return !unit.IsMoving();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Is Casting",
        dynamicDescription = "$ is casting",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public bool UnitIsCasting(Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        return !unit.IsCasting();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Can Move",
        dynamicDescription = "$ can move",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public bool UnitCanMove(Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        return unit.CanMove();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Has Buff",
        dynamicDescription = "$ has buff labeled $",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Buff Name")]
    public bool UnitHasBuff(Unit unit, string buff)
    {
        Error(unit == null, "The specified unit is invalid.");
        Error(buff.Length == 0, "The buff name cannot be empty.");
        return (unit.HasBuff(buff));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit is Stunned",
        dynamicDescription = "$ is stunned",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public bool UnitIsStunned(Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        return (unit.IsStunned());
    }


    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Ability Is On Cooldown",
        dynamicDescription = "$ is on cooldown for $",
        icon = conditionIcon)]
    [AbilityArg(argType = ArgType.Temp)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public bool UnitAbilityIsOnCooldown(Ability ability, Unit unit)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        return (ability.GetRemainingCooldown(unit) > 0);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Can Cast ability",
        dynamicDescription = "$ can cast $",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [AbilityArg(argType = ArgType.Temp)]
    public bool UnitCanCastAbility(Unit unit, Ability ability)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        return ability.IsValidToCast(unit, null, unit.transform.position);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Is Empowered",
        dynamicDescription = "$ is empowered",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public bool UnitIsEmpowered(Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        if (unit is Monster monster) return monster.isEmpowered;
        return false;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Quest/Quest Is Active",
        dynamicDescription = "Quest named $ is currently active",
        icon = conditionIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Quest Name")]
    public bool QuestIsActive(string label)
    {
        Error(label.Length == 0, "The quest name cannot be empty.");
        return GameManager.quests.QuestIsActive(label);
    }

    // TODO: THIS IS BROKEN?
    [VisualScriptingFunction(
        dropdownDescription = "Quest/Quest Is Completed",
        dynamicDescription = "Quest named $ has been completed",
        icon = conditionIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Quest Name")]
    public bool QuestIsCompleted(string label)
    {
        Error(label.Length == 0, "The quest name cannot be empty.");
        return GameManager.quests.QuestIsCompleted(label);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Unit Has Tag",
        dynamicDescription = "$ Has Tag $",
        icon = conditionIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [StringArg(argType = ArgType.Temp, allowPreset = false, allowFunction = false, tempLabel = "Tag")]
    public bool UnitHasTag(Unit unit, string tag)
    {
        Error(unit == null, "The specified unit is invalid.");
        return (unit.CompareTag(tag));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Player Has Item Equipped",
        dynamicDescription = "Player has $ equipped",
        icon = conditionIcon)]
    [ItemArg(argType = ArgType.Temp)]
    public bool PlayerHasItemEquipped(Item item)
    {
        Error(item == null, "The specified item is invalid.");
        return GameManager.player.HasItemEquipped(item);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Player Has Item In Inventory",
        dynamicDescription = "Player has $ in their inventory",
        icon = conditionIcon)]
    [ItemArg(argType = ArgType.Temp)]
    public bool PlayerHasItemInInventory(Item item)
    {
        Error(item == null, "The specified item is invalid.");
        return GameManager.player.HasItemInInventory(item);
    }
}
