using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages all logic for an inventory. Items can be added, removed, and retrieved.
/// </summary>
public class Inventory
{
    // The items stored in this inventory.
    private Item[] items;

    // Unity events to handle any changes to this inventory.
    public UnityEvent<Item> onItemAdded = new UnityEvent<Item>();
    public UnityEvent<Item> onItemRemoved = new UnityEvent<Item>();
    public UnityEvent<int> onSlotUpdated = new UnityEvent<int>();

    /// <summary>
    /// Initializes a new inventory with the specified size.
    /// </summary>
    public Inventory(int size)
    {
        items = new Item[size];
    }

    /// <summary>
    /// Adds an item to the first available slot in the inventory.
    /// </summary>
    public void AddItem(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                onItemAdded.Invoke(item);
                onSlotUpdated.Invoke(i);
                return;
            }
        }
    }

    /// <summary>
    /// Adds an item to the specified slot, replacing any existing item.
    /// </summary>
    public void AddItemAtID(Item item, int id)
    {
        if (items[id] != null)
        {
            onItemRemoved.Invoke(items[id]);
        }
        items[id] = item;
        if (item != null)
        {
            onItemAdded.Invoke(item);
        }
        onSlotUpdated.Invoke(id);
    }

    /// <summary>
    /// Removes the specified item from the inventory.
    /// </summary>
    public void RemoveItem(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                items[i] = null;
                onItemRemoved.Invoke(item);
                onSlotUpdated.Invoke(i);
                return;
            }
        }
    }

    /// <summary>
    /// Removes the item from the specified slot.
    /// </summary>
    public void RemoveItemAtID(int id)
    {
        if (items[id] != null)
        {
            onItemRemoved.Invoke(items[id]);
            items[id] = null;
            onSlotUpdated.Invoke(id);
        }
    }

    /// <summary>
    /// Retrieves the item in the specified slot.
    /// </summary>
    public Item GetItemAtID(int id)
    {
        return items[id];
    }

    /// <summary>
    /// Checks if the specified slot is empty.
    /// </summary>
    public bool IsEmpty(int id)
    {
        return items[id] == null;
    }

    /// <summary>
    /// Returns the number of filled slots in the inventory.
    /// </summary>
    public int GetFilledSlots()
    {
        int count = 0;
        foreach (Item item in items)
        {
            if (item != null) count++;
        }
        return count;
    }

    /// <summary>
    /// Returns the number of empty slots in the inventory.
    /// </summary>
    public int GetEmptySlots()
    {
        return items.Length - GetFilledSlots();
    }

    public int GetSlots ()
    {
        return items.Length;
    }
}