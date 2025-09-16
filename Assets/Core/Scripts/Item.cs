using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using MyUtilities;
using static Item;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Item", menuName = "Data/Item", order = 1)]
public class Item : ScriptableObject, IVisualCodeHandler
{
    // Item properties
    public string itemName;
    public string itemDescription;
    public string itemTag;
    public string itemFlavourText;
    public string classRequirement;
    public Sprite itemIcon;

    // Item properties.
    public ItemType itemType;
    public ItemRarity itemRarity;
    public int itemCost;

    // Stat rolling.
    public int randomStatCount;
    public List<StatBlock> guaranteedStats = new List<StatBlock>();
    public List<StatBlock> randomisableStats = new List<StatBlock>();

    // The final item stat rolls
    [NonSerialized] public bool hasRolledStats = false;
    [NonSerialized] public List<RolledStatValue> rolledStatValues = new List<RolledStatValue>();

    // Drop requirements.
    public int minimumDropLevel = 0;
    public bool canPurchaseInShop = true;
    public bool canDropOffMonster = true;

    // Keep track of the raw item template from before stats are rolled.
    public Item template;

    // Keep track of who "owns" the item (e.g., has it equipped).
    private Unit owner;

    // Handles all logic for the item from the visual engine.
    public LogicEngine engine = new LogicEngine();


    private static Dictionary<ItemRarity, List<Item>> itemLootTable;
    private static Item[] allItems;

    /// <summary>
    /// List of all possible rarities an item could have.
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Rare,
        Legendary
    }

    /// <summary>
    /// List of all possible types that an item could have.
    /// </summary>
    public enum ItemType
    {
        Ring,
        Amulet,
        Weapon,
        Armor,
        Boots,
        Other
    }

    /// <summary>
    /// Gets the color associated with the item's rarity.
    /// </summary>
    public Color GetItemColor()
    {
        return RarityToColor(itemRarity);
    }

    /// <summary>
    /// A stat block represents a stat that the item *could* have. The stat has a range of 
    /// values it can roll (e.g. 10-20 health).
    /// </summary>
    [Serializable]
    public class StatBlock
    {
        public Stat stat;
        public float minimum;
        public float maximum;
        public bool isPercent;

        public StatBlock(Stat stat, bool isPercent = false)
        {
            this.stat = stat;
            this.isPercent = isPercent;
            minimum = 0.0f;
            maximum = 0.0f;
        }

        /// <summary>
        /// Creates a deep copy of the StatBlock.
        /// </summary>
        public StatBlock Copy()
        {
            return new StatBlock(stat, isPercent)
            {
                minimum = minimum,
                maximum = maximum
            };
        }
    }

    /// <summary>
    /// Creates a deep copy of the item.
    /// </summary>
    public Item Copy()
    {
        Item item = CreateInstance<Item>();
        item.itemName = itemName;
        item.itemDescription = itemDescription;
        item.itemIcon = itemIcon;
        item.itemType = itemType;
        item.itemRarity = itemRarity;
        item.itemCost = itemCost;
        item.randomStatCount = randomStatCount;
        item.canDropOffMonster = canDropOffMonster;
        item.canPurchaseInShop = canPurchaseInShop;
        item.guaranteedStats.AddRange(guaranteedStats.ConvertAll(statBlock => statBlock.Copy()));
        item.randomisableStats.AddRange(randomisableStats.ConvertAll(statBlock => statBlock.Copy()));
        item.engine = engine.Copy();
        return item;
    }

    /// <summary>
    /// Creates a shallow copy of the item.
    /// </summary>
    public Item ShallowCopy()
    {
        Item item = Instantiate(this);
        item.engine = engine.ShallowCopy(item);
        return item;
    }

    /// <summary>
    /// Generates the item's description based on its stats and properties.
    /// </summary>
    public string GetDescription()
    {
        string description = "";

        if (hasRolledStats)
        {
            foreach (RolledStatValue rolledStat in rolledStatValues)
            {
                description += $"{(rolledStat.amount > 0 ? "+" : "")}{rolledStat.amount * rolledStat.stat.DisplayModifier(rolledStat.isPercent)}";
                if (rolledStat.stat.ShowAsPercent(rolledStat.isPercent))
                    description += "%";
                description += $" {rolledStat.stat.Label()}\n";
            }
            description += "\n";
        }
        else
        {
            foreach (StatBlock statBlock in guaranteedStats)
            {
                string perc = statBlock.stat.ShowAsPercent(statBlock.isPercent) ? "%" : "";
                float mod = statBlock.stat.DisplayModifier(statBlock.isPercent);
                description += statBlock.minimum != statBlock.maximum
                    ? $"{(statBlock.minimum > 0 ? "+" : "-")}[{Mathf.Abs(statBlock.minimum * mod)}{perc} - {Mathf.Abs(statBlock.maximum * mod)}{perc}] {statBlock.stat.Label()}\n"
                    : $"{(statBlock.minimum > 0 ? "+" : "-")}{Mathf.Abs(statBlock.minimum * mod)}{perc} {statBlock.stat.Label()}\n";
            }
            description += "<color=#F3EF92>";
            for (int i = 0; i < randomStatCount; i++)
            {
                description += "Random Stat\n";
            }
            description += "</color>\n";
        }

        if (!string.IsNullOrEmpty(itemDescription))
        {
            description += $"<color=orange>Passive</color>: {Regex.Replace(itemDescription, @"\d+(?:\.\d+)?%?", "<color=yellow>$&</color>")}";
        }

        return description;
    }

    /// <summary>
    /// Rolls the item's stats and returns a new instance with those stats.
    /// </summary>
    public Item RollItem()
    {
        Item item = ShallowCopy();
        item.hasRolledStats = true;
        item.SetOwner(GameManager.player);

        // Roll all of the guaranteed stats and add them to the item.
        foreach (StatBlock statBlock in item.guaranteedStats)
        {
            float value = Random.Range(statBlock.minimum, statBlock.maximum);
            value = statBlock.stat.ShowAsPercent(statBlock.isPercent)
                ? Mathf.Round(value * 100.0f) / 100.0f
                : Mathf.Round(value);
            item.rolledStatValues.Add(new RolledStatValue(statBlock.stat, value, statBlock.isPercent));
        }

        // Roll all of the random stats and add them to the item.
        item.randomStatCount = Mathf.Min(item.randomisableStats.Count, item.randomStatCount);
        for (int i = 0; i < item.randomStatCount; i++)
        {
            StatBlock statBlock = item.randomisableStats[Random.Range(0, item.randomisableStats.Count)];
            item.randomisableStats.Remove(statBlock);
            float value = Random.Range(statBlock.minimum, statBlock.maximum);
            value = statBlock.stat.ShowAsPercent(statBlock.isPercent)
                ? Mathf.Round(value * 100.0f) / 100.0f
                : Mathf.Round(value);
            item.rolledStatValues.Add(new RolledStatValue(statBlock.stat, value, statBlock.isPercent));
        }

        return item;
    }

    /// <summary>
    /// Retrieves a random item of the specified rarity from the loot table.
    /// </summary>
    public static Item GetRandomItemOfRarity(ItemRarity rarity)
    {
        GenerateLootTable();
        if (!itemLootTable.ContainsKey(rarity) || itemLootTable[rarity].Count == 0) return null;
        return itemLootTable[rarity].RandomItem();
    }

    /// <summary>
    /// Returns a description combining the item's rarity and type.
    /// </summary>
    public string GetTypeDescription()
    {
        return $"{itemRarity} {itemType}";
    }

    /// <summary>
    /// Generates the loot table if it doesn't already exist.
    /// </summary>
    private static void GenerateLootTable()
    {
        if (allItems == null)
            allItems = Resources.LoadAll<Item>("Items");


        // Add them to the item dictionary based on their rarity.
        if (itemLootTable != null)
            itemLootTable.Clear();
        else
            itemLootTable = new Dictionary<ItemRarity, List<Item>>();
        foreach (Item item in allItems)
        {
            if (item.canDropOffMonster &&
                (item.classRequirement == "" || GameManager.player.unitName == item.classRequirement) &&
                GameManager.player.currentLevel >= item.minimumDropLevel)
            {
                if (!itemLootTable.ContainsKey(item.itemRarity))
                {
                    itemLootTable.Add(item.itemRarity, new List<Item>());
                }
                itemLootTable[item.itemRarity].Add(item);

            }
        }
    }

    /// <summary>
    /// Retrieves all items of a specified type.
    /// </summary>
    public static Item[] GetAllItemsOfType(ItemType type)
    {
        List<Item> items = new List<Item>();
        foreach (Item item in GameManager.items.allItems)
        {
            if (item.itemType == type)
            {
                items.Add(item);
            }
        }
        return items.ToArray();
    }

    /// <summary>
    /// Rolls for a potential item drop based on the given chances for each rarity.
    /// </summary>
    public static Item RollForItemDrop(float commonChance = 0.03f, float rareChance = 0.01f, float legendaryChance = 0.005f)
    {
        GenerateLootTable();
        if (Random.value <= legendaryChance)
            return GetRandomItemOfRarity(ItemRarity.Legendary);
        if (Random.value <= rareChance)
            return GetRandomItemOfRarity(ItemRarity.Rare);
        if (Random.value <= commonChance)
            return GetRandomItemOfRarity(ItemRarity.Common);
        return null;
    }
    
    /// <summary>
    /// Returns the color of an item, given a particular item rarity.
    /// </summary>
    public Color RarityToColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => new Color(0.12f, 1.0f, 0.0f),
            ItemRarity.Rare => Color.yellow,
            ItemRarity.Legendary => new Color(1.0f, 0.5f, 0.0f),
            _ => Color.grey,
        };
    }

    /// <summary>
    /// Returns the current owner of the item, or null if the item is not currently owned.
    /// </summary>
    public Unit GetOwner()
    {
        return owner;
    }

    /// <summary>
    /// Sets the owner of this item to the specified unit.
    /// </summary>
    public void SetOwner(Unit owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// Returns the generic object associated with this item. This is needed as part of the visual
    /// scripting interface.
    /// </summary>
    public UnityEngine.Object GetData()
    {
        return this;
    }

    /// <summary>
    /// Returns the script engine associated with this item (used for the visual scripting interface).
    /// </summary>
    public LogicEngine GetEngine()
    {
        return engine;
    }

    public string GetTag()
    {
        return itemTag;
    }

    public class RolledStatValue
    {
        public Stat stat;
        public float amount;
        public bool isPercent;

        public RolledStatValue(Stat stat, float amount, bool isPercent)
        {
            this.stat = stat;
            this.amount = amount;
            this.isPercent = isPercent;
        }
    }
}