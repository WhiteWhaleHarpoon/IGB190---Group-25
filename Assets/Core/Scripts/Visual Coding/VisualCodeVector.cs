using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class VisualCodeScript
{
    [VisualScriptingFunction(
        dropdownDescription = "Unit/Position of Unit",
        dynamicDescription = "Position of $")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public Vector3 PositionOfUnit(Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        return unit.transform.position;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Position of Projectile",
        dynamicDescription = "Position of $")]
    [ProjectileArg(argType = ArgType.Temp, allowValue = false)]
    public Vector3 PositionOfProjectile(Projectile projectile)
    {
        Error(projectile == null, "The specified projectile is invalid.");
        return projectile.transform.position;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Cast Point of Unit",
        dynamicDescription = "Cast Point of $")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public Vector3 CastPointOfUnit(Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        return unit.GetCastPoint();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Attack Point of Unit",
        dynamicDescription = "Attack Point of $")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public Vector3 AttackPointOfUnit(Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        return unit.GetAttackPoint();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Math/Vector Addition",
        dynamicDescription = "$ + $")]
    [VectorArg(argType = ArgType.Temp)]
    [VectorArg(argType = ArgType.Temp)]
    public Vector3 VectorAddition(Vector3 vec1, Vector3 vec2)
    {
        return vec1 + vec2;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Math/Vector Subtraction",
        dynamicDescription = "$ - $")]
    [VectorArg(argType = ArgType.Temp)]
    [VectorArg(argType = ArgType.Temp)]
    public Vector3 VectorSubtraction(Vector3 vec1, Vector3 vec2)
    {
        return vec1 - vec2;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Math/Vector Multiplication",
        dynamicDescription = "$ x $")]
    [VectorArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Temp)]
    public Vector3 VectorMultiplication(Vector3 vec1, float value)
    {
        return vec1 * value;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Math/Vector Division",
        dynamicDescription = "$ / $")]
    [VectorArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Temp)]
    public Vector3 VectorDivision(Vector3 vec1, float value)
    {
        return vec1 / value;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Vector Variable",
        dynamicDescription = "Vector: $")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name")]
    public Vector3 GetVectorVariable(string name)
    {
        return LogicEngine.current.GetLocalVariable<Vector3>(name);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Vector Variable",
        dynamicDescription = "Global Vector: $")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name")]
    public Vector3 GetGlobalVectorVariable(string name)
    {
        return LogicEngine.GetGlobalVariable<Vector3>(name);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Random/Random Point Near Unit",
        dynamicDescription = "Random point within $ of $")]
    [NumberArg(argType = ArgType.Temp, tempLabel = "Range", suffix = "m")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public Vector3 RandomPointNearUnit(float distance, Unit unit)
    {
        Error(unit == null, "The specified unit is invalid.");
        Vector3 offset = Random.insideUnitSphere * distance;
        offset.y = 0;
        return unit.transform.position + offset;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Random/Random Point Near Point",
        dynamicDescription = "Random point within $ of $")]
    [NumberArg(argType = ArgType.Temp, tempLabel = "Range", suffix = "m")]
    [VectorArg(argType = ArgType.Temp)]
    public Vector3 RandomPointNearPoint(float distance, Vector3 point)
    {
        Vector3 offset = Random.insideUnitSphere * distance;
        offset.y = 0;
        return point + offset;
    }
}
