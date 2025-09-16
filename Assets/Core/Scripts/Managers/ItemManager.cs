using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager
{
    public Item[] allItems;
    public Dictionary<Item.ItemType, Item> itemsByRarity = new Dictionary<Item.ItemType, Item>();

    public ItemManager ()
    {
        allItems = Resources.LoadAll<Item>("Items").OrderBy(x => 1 * (int)x.itemType + 1000 * (int)x.itemRarity).ToArray();
    }
}
