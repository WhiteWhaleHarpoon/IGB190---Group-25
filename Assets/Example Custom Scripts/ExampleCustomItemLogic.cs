using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleCustomItemLogic : MonoBehaviour
{
    void Start()
    {
        GameManager.events.OnItemEquipped.AddListener(OnItemEquipped);
        GameManager.events.OnItemUnequipped.AddListener(OnItemUnequipped);
    }

    public void OnItemEquipped (Item item)
    {
        if (item.itemName == "Gold Ring")
        {
            GameManager.events.OnUnitDamaged.AddListener(OnUnitDamaged);
        }
    }

    public void OnItemUnequipped (Item item)
    {
        if (item.itemName == "Gold Ring")
        {
            GameManager.events.OnUnitDamaged.RemoveListener(OnUnitDamaged);
        }
    }

    public void OnUnitDamaged (GameEvents.OnUnitDamagedInfo damageInfo)
    {
        if (damageInfo.damagedUnit == GameManager.player)
        {
            damageInfo.damagingUnit.RemoveHealth(10);
        }
    }
}
