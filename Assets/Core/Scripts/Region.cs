using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Logic for a Region in the game. When the player enters the region, an event
/// will be fired specifying the region that the player has entered.
/// 
/// Usage: Attach this component to any GameObject with a trigger collider. When the
/// player collides with the object, the trigger will be run. 
/// 
/// Make sure you specify the name of the region in the inspector first.
/// </summary>
public class Region : MonoBehaviour
{
    public string regionName;

    private void OnTriggerEnter(Collider other)
    {
        if (regionName == string.Empty)
        {
            Debug.Log($"The Region on {gameObject.name} has not been named. It needs a name to run!");
        }
        else if (other.gameObject == GameManager.player.gameObject)
        {
            GameManager.events.OnPlayerEnteredRegion.Invoke(new GameEvents.OnUnitEnteredRegionInfo(GameManager.player, this, regionName));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (regionName == string.Empty)
        {
            Debug.Log($"The Region on {gameObject.name} has not been named. It needs a name to run!");
        }
        else if (other.gameObject == GameManager.player.gameObject)
        {
            GameManager.events.OnPlayerExitedRegion.Invoke(new GameEvents.OnUnitExitedRegionInfo(GameManager.player, this, regionName));
        }
    }

    public static bool RegionExists (string regionName)
    {
        Region[] regions = GameObject.FindObjectsByType<Region>(FindObjectsSortMode.None);
        foreach (Region region in regions)
        {
            if (region.regionName == regionName) return true;
        }
        return false;
    }

    public static Region GetRegionWithName (string regionName)
    {
        Region[] regions = GameObject.FindObjectsByType<Region>(FindObjectsSortMode.None);
        foreach (Region region in regions)
        {
            if (region.regionName == regionName)
            {
                return region;
            }
        }
        return null;
    }

    public static List<Region> GetRegionsWithName(string regionName)
    {
        List<Region> matchingRegions = new List<Region>();
        Region[] regions = GameObject.FindObjectsByType<Region>(FindObjectsSortMode.None);
        foreach (Region region in regions)
        {
            if (region.regionName == regionName)
            {
                matchingRegions.Add(region);
            }
        }
        return matchingRegions;
    }

    public static bool UnitIsInRegion(Unit unit, string regionName)
    {
        List<Region> regions = Region.GetRegionsWithName(regionName);
        foreach (Region region in regions)
        {
            Collider regionCollider = region.GetComponent<Collider>();
            Collider unitCollider = unit.GetComponent<Collider>();
            if (regionCollider == null)
            {
                Debug.LogError($"A region checked does not have a collider (name = {regionCollider.name}).");
            }
            else if (regionCollider != null && regionCollider.bounds.Intersects(unitCollider.bounds))
            {
                return true;
            }
        }
        return false;
    }

    public static void DestroyAllRegionsWithName (string regionName)
    {
        Region[] regions = GameObject.FindObjectsByType<Region>(FindObjectsSortMode.None);
        foreach (Region region in regions)
        {
            if (region.regionName == regionName)
            {
                GameObject.Destroy(region.gameObject);
            }
        }
    }
}
