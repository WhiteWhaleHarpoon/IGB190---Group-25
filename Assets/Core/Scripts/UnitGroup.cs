using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitGroup : IEnumerable<Unit>
{
    public List<Unit> unitList = new List<Unit>();

    public UnitGroup ()
    {

    }

    public int Count ()
    {
        return unitList.Count;
    }

    public Unit PeekNextUnit ()
    {
        if (unitList.Count == 0) return null;
        return unitList[0];
    }

    public Unit PopNextUnit ()
    {
        if (unitList.Count == 0) return null;
        Unit unit = unitList[0];
        unitList.RemoveAt(0);
        return unit;
    }

    public bool Contains (Unit unit)
    {
        return unitList.Contains(unit);
    }


    public Unit GetUnit ()
    {
        if (unitList.Count == 0)
        {
            return null;
        }
        else
        {
            return unitList[0];
        }
    }

    public List<Unit> GetUnits ()
    {
        return unitList;
    }

    public UnitGroup (Unit unit)
    {
        unitList = new List<Unit>();
        AddUnit(unit);
    }

    public UnitGroup (List<Unit> units)
    {
        unitList = new List<Unit>();
        foreach (Unit unit in units)
        {
            unitList.Add(unit);
        }
    }

    public UnitGroup(Unit[] units)
    {
        unitList = new List<Unit>();
        foreach (Unit unit in units)
        {
            unitList.Add(unit);
        }
    }

    public IEnumerator<Unit> GetEnumerator()
    {
        foreach (var item in unitList)
        {
            yield return item;
        }
    }

    // Explicit implementation of the non-generic IEnumerator
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator(); 
    }



    public void AddUnit (Unit unit)
    {
        unitList.Add(unit);
    }

    public void RemoveUnit (Unit unit)
    {
        unitList.Remove(unit);
    }

    public Unit GetUnit (int index)
    {
        return unitList[index];
    }

    public Unit GetRandomUnit ()
    {
        return unitList[Random.Range(0, unitList.Count)];
    }

    public static UnitGroup Empty ()
    {
        return new UnitGroup();
    }

    public static UnitGroup AllUnits ()
    {
        return new UnitGroup(GetAllUnits());
    }

    private static Unit[] GetAllUnits ()
    {
        return GameObject.FindObjectsByType<Unit>(FindObjectsSortMode.None);
    }

    public static UnitGroup AllAlliesOfUnit (Unit unit) 
    {
        List<Unit> allies = new List<Unit>();
        Unit[] units = GetAllUnits();
        foreach (Unit possibleUnit in units)
        {
            if (unit.GetFaction() == possibleUnit.GetFaction())
            {
                allies.Add(possibleUnit);
            }
        }
        return new UnitGroup(allies);
    }

    public static UnitGroup AllEnemiesOfUnit (Unit unit)
    {
        List<Unit> enemies = new List<Unit>();
        Unit[] units = GetAllUnits();
        foreach (Unit possibleUnit in units)
        {
            if (unit.GetFaction() != possibleUnit.GetFaction())
            {
                enemies.Add(possibleUnit);
            }
        }
        return new UnitGroup(enemies);
    }

    public static UnitGroup FilterUnitsWithTag (string tag, UnitGroup filterGroup = null)
    {
        if (filterGroup == null) filterGroup = AllUnits();

        List<Unit> filteredUnits = new List<Unit>();
        foreach (Unit unit in filterGroup)
        {
            if  (unit.tag == tag)
            {
                filteredUnits.Add(unit);
            }
        }
        return new UnitGroup(filteredUnits);
    }

    public static UnitGroup FilterUnitsInRange (Unit unit, float range, UnitGroup possibleUnits = null)
    {
        if (possibleUnits == null) possibleUnits = AllEnemiesOfUnit(unit);
        List<Unit> filtered = new List<Unit>();
        foreach (Unit possibleUnit in possibleUnits)
        {
            if (Vector3.Distance(unit.transform.position, possibleUnit.transform.position) <= range)
            {
                filtered.Add(possibleUnit);
            }
        }
        return new UnitGroup(filtered);
    }

    public static UnitGroup FilterUnitsInArc (Unit unit, float arc, float distance, UnitGroup filterGroup = null)
    {
        if (filterGroup == null) filterGroup = AllEnemiesOfUnit(unit);
        List<Unit> filtered = new List<Unit>();
        return new UnitGroup(filtered);
    }

    public static UnitGroup FilterUnitsInLine (Unit unit, float lineWidth, float lineLength, UnitGroup filterGroup = null)
    {
        if (filterGroup == null) filterGroup = AllEnemiesOfUnit(unit);
        List<Unit> filtered = new List<Unit>();
        return new UnitGroup(filtered);
    }
}
