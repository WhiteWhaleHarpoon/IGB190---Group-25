using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

public class ItemEditor : EditorWindow
{
    //public static ItemEditor window;
    private static Item item = null;
    public LogicEngineEditor engineEditor;
    private Color panelColor = new Color(0.3f, 0.3f, 0.3f);

    private const float abilitiesPanelWidth = 200;
    private const float abilityDetailsPanelWidth = 230;

    private const float leftMargin = 3;
    private const float rightMargin = 3;
    private const float topMargin = 2;
    private const float bottomMargin = 4;
    private const float spaceBetweenPanels = 2;
    private const float panelHeaderHeight = 25;
    private const float itemHeight = 30;
    private const float itemPadding = 1;

    private static Color panelHeaderColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
    private static Color selectedColor = new Color(0.17f, 0.36f, 0.53f, 1f);
    public static Color unselectedColor = new Color(0.15f, 0.15f, 0.15f);

    private static List<Item.StatBlock> toDelete = new List<Item.StatBlock>();

    private Vector2 itemListScrollPosition; 


    public void SetSelectedItem (Item itemToSelect)
    {
        item = itemToSelect;
        engineEditor = new LogicEngineEditor(this, item.engine, item);
    }



    /// <summary>
    /// Draw the complete list of items in the game.
    /// </summary>
    private void DrawItemList (Rect rect)
    {
        // Order the items, first by rarity than by slot type.
        Item[] items = Resources.LoadAll<Item>("Items").OrderBy(
            x => 1 * (int)x.itemType - 1000 * (int)x.itemRarity).ToArray();
        
        // Draw the background panel color.
        EditorGUI.DrawRect(rect, panelColor);

        // Draw the header.
        Rect area = new Rect(rect.x, rect.y, rect.width, panelHeaderHeight);
        EditorGUI.DrawRect(area, panelHeaderColor);
        EditorGUI.LabelField(area, "  Items ", LogicEngineEditor.windowStyle_HeaderText);

        // Draw the "Create Item" button.
        if (GUI.Button(new Rect(area.xMax - panelHeaderHeight, area.y - 1, 
            panelHeaderHeight, panelHeaderHeight), "+", LogicEngineEditor.windowStyle_AddButton))
        {
            CreateItem();
        }

        // Build the scrollbar if needed.
        Rect scrollRect = new Rect(rect);
        scrollRect.y += panelHeaderHeight;
        scrollRect.height -= panelHeaderHeight;
        Rect requiredSize = new Rect(scrollRect);
        requiredSize.height = (itemHeight + 1) * items.Length;
        itemListScrollPosition = GUI.BeginScrollView(scrollRect, itemListScrollPosition, 
            requiredSize, false, false, GUIStyle.none, GUIStyle.none);

        // Draw each item.
        float yOffset = rect.y + panelHeaderHeight;
        for (int i = 0; i < items.Length; i++)
        {
            // Draw the item.
            Rect itemRect = new Rect(rect.x, yOffset, rect.width, itemHeight);
            EditorGUI.DrawRect(itemRect, items[i] == item ? selectedColor : unselectedColor);

            // Draw the item icon.
            EditorGUI.DrawRect(new Rect(rect.x, yOffset, itemHeight, itemHeight), Color.black);
            if (items[i].itemIcon != null)
                GUI.DrawTexture(new Rect(rect.x, yOffset, itemHeight, itemHeight), items[i].itemIcon.texture);

            // Draw the item label.
            Rect r = new Rect(rect.x + 35, yOffset, rect.width - 35, itemHeight);
            if (EditorUtility.IsDirty(items[i]))
                GUI.Label(r, "*" + SetStringColor(items[i].itemName, items[i].GetItemColor()), LogicEngineEditor.windowStyle_BodyText);
            else    
                GUI.Label(r, SetStringColor(items[i].itemName, items[i].GetItemColor()), LogicEngineEditor.windowStyle_BodyText);

            // Check for mouse events.
            Event current = Event.current;
            if (itemRect.Contains(current.mousePosition))
            {
                // On mouse down, select this item.
                if (current.type == EventType.MouseDown || current.type == EventType.ContextClick)
                {
                    SetSelectedItem(items[i]);
                }

                // On right click, create an options window.
                if (current.type == EventType.ContextClick)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Rename"), false, () => { RenameItem(item); });
                    menu.AddItem(new GUIContent("Copy"), false, () => { CopyItem(item); });
                    menu.AddItem(new GUIContent("Delete"), false, () => { DeleteItem(item); });
                    menu.ShowAsContext();
                    current.Use();
                }
            }
            yOffset += (itemHeight + itemPadding);
        }

        GUI.EndScrollView();
        Event current2 = Event.current;
        if (rect.Contains(current2.mousePosition) && current2.type == EventType.MouseDown)
            GUI.FocusControl(null);
    }

    /// <summary>
    /// Build a richtext string, turning the text into the specified color.
    /// e.g. "My String" and Red would become "<color=#FF0000>My String</color>".
    /// </summary>
    private string SetStringColor (string text, Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
    }

    /// <summary>
    /// Build a context menu which shows all possible stats that can be added to the item.
    /// If the item already has the stat, it cannot be added again.
    /// </summary>
    private GenericMenu BuildAddStatMenu (List<Item.StatBlock> statBlock)
    {
        GenericMenu menu = new GenericMenu();
        foreach (Stat stat in Enum.GetValues(typeof(Stat)))
        {
            if (stat.IsBasicStat(false) && !statBlock.Exists(x => x.stat == stat && !x.isPercent))
            {
                menu.AddItem(new GUIContent(stat.Label() + (stat.ShowAsPercent(false) ? " (%)" : "")), false, () =>
                {
                    statBlock.Add(new Item.StatBlock(stat, false));
                    EditorUtility.SetDirty(item);
                });
            }
            if (stat.IsBasicStat(true) && !statBlock.Exists(x => x.stat == stat && x.isPercent))
            {
                menu.AddItem(new GUIContent(stat.Label() + (stat.ShowAsPercent(true) ? " (%)" : "")), false, () =>
                {
                    statBlock.Add(new Item.StatBlock(stat, true));
                    EditorUtility.SetDirty(item);
                });
            }
        }
        return menu;
    }

    /// <summary>
    /// Draw all of the details related to a specific item, including the controls which 
    /// allow the user to modify that item.
    /// </summary>
    private void DrawItemDetails (Rect rect)
    {
        if (item == null) return;

        float minorSpacer = 5;
        float majorSpacer = 10;
        float width = rect.width - majorSpacer * 2;
        float posY = rect.y;
        float posX = rect.x + majorSpacer;
        float iconSize = 70;
        float shortWidth = width - iconSize - majorSpacer;
        float shortStart = posX + iconSize + majorSpacer;
        float height = 0;

        // Draw the background panel.
        EditorGUI.DrawRect(rect, panelColor);

        // Draw the header.
        EditorGUI.DrawRect(new Rect(rect.x, posY, rect.width, panelHeaderHeight), panelHeaderColor);
        EditorGUI.LabelField(new Rect(rect.x + 8, posY, rect.width, panelHeaderHeight), SetStringColor(item.itemName, item.GetItemColor()), LogicEngineEditor.windowStyle_HeaderText);
        posY += panelHeaderHeight + majorSpacer;

        // UI Control: Draw the icon for the item.
        item.itemIcon = (Sprite)EditorGUI.ObjectField(new Rect(posX, posY, iconSize, iconSize), item.itemIcon, typeof(Sprite), false);

        // UI Control: Specify the slot of the item.
        GUI.Label(new Rect(shortStart, posY, 100, height = 16), "Item Slot", EditorStyles.boldLabel);
        posY += height;
        item.itemType = (Item.ItemType)EditorGUI.EnumPopup(new Rect(shortStart, posY, shortWidth, height = 20), item.itemType);
        posY += height;

        // UI Control: Specify the rarity of the item. 
        GUI.Label(new Rect(shortStart, posY, 100, height = 16), "Item Rarity", EditorStyles.boldLabel);
        posY += height;
        item.itemRarity = (Item.ItemRarity)EditorGUI.EnumPopup(new Rect(shortStart, posY, shortWidth, height = 20), item.itemRarity);
        posY += height;

        // UI Control: Specify the tooltip of the item.
        GUI.Label(new Rect(posX, posY, width, height = 20), "Item Tooltip", EditorStyles.boldLabel);
        posY += height;
        item.itemDescription = GUI.TextArea(new Rect(posX, posY, width, height = 50), item.itemDescription);
        posY += height;

        // Specify the flavour text of the item.
        GUI.Label(new Rect(posX, posY, width, height = 20), "Item Flavour Text", EditorStyles.boldLabel);
        posY += height;
        item.itemFlavourText = GUI.TextArea(new Rect(posX, posY, width, height = 50), item.itemFlavourText);
        posY += height;

        posY += 5;
        GUI.Label(new Rect(posX, posY, width, 20), "Tag", EditorStyles.boldLabel);
        item.itemTag = GUI.TextField(new Rect(posX + 120, posY, width - 120, 20), item.itemTag);
        posY += 24;
        GUI.Label(new Rect(posX, posY, width, 20), "Class Required", EditorStyles.boldLabel);
        item.classRequirement = GUI.TextField(new Rect(posX + 120, posY, width - 120, 20), item.classRequirement);
        posY += 24;
        GUI.Label(new Rect(posX, posY, width, 20), "Min Drop Level", EditorStyles.boldLabel);
        item.minimumDropLevel = EditorGUI.IntField(new Rect(posX + 120, posY, width - 120, 20), item.minimumDropLevel);
        posY += 24;






        float boxPadding = 5;
        Color boxColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        posY += boxPadding;

        
        EditorGUI.DrawRect(new Rect(posX, posY, width, 50), boxColor);
        
        posY += boxPadding;
        item.canPurchaseInShop = GUI.Toggle(new Rect(posX + boxPadding, posY, width, 22), item.canPurchaseInShop, " Can Purchase in Shop");
        posY += 20;
        item.canDropOffMonster = GUI.Toggle(new Rect(posX + boxPadding, posY, width, 22), item.canDropOffMonster, " Can Drop From Monster");
        posY += 34;

        // UI Control: Specify the number of random stats the item should roll.
        GUI.Label(new Rect(posX, posY, width, height = 20), "Random Stats", EditorStyles.boldLabel);
        item.randomStatCount = EditorGUI.IntField(new Rect(posX, posY + height, 100, height), item.randomStatCount);

        // UI Control: Specify the purchase cost of the item.
        GUI.Label(new Rect(posX + 110, posY, width, height), "Purchase Cost", EditorStyles.boldLabel);
        item.itemCost = EditorGUI.IntField(new Rect(posX + 110, posY + height, 100, height), item.itemCost);
        posY += 2 * height + minorSpacer;

        // UI Control: Draw the header for the guaranteed stats.
        EditorGUI.DrawRect(new Rect(rect.x, posY, rect.width, 22), new Color(0.1f, 0.1f, 0.1f));
        GUI.Label(new Rect(rect.x + minorSpacer, posY, 120, 20), "Guaranteed Stats", EditorStyles.boldLabel);
        if (item.guaranteedStats.Count > 0) GUI.Label(new Rect(posX + 136, posY, 30, 20), "Min", LogicEngineEditor.windowStyle_SmallCenteredText);
        if (item.guaranteedStats.Count > 0) GUI.Label(new Rect(posX + 167, posY, 30, 20), "Max", LogicEngineEditor.windowStyle_SmallCenteredText);
        if (GUI.Button(new Rect(rect.x + rect.width - 21, posY - 1, 21, 21), "+", LogicEngineEditor.windowStyle_AddButtonSmall))
        {
            BuildAddStatMenu(item.guaranteedStats).ShowAsContext();
        }
        posY += 23;

        // UI Control: Draw each guaranteed stat.
        foreach (Item.StatBlock statBlock in item.guaranteedStats)
        {
            DrawStatBlock(item, statBlock, new Rect(rect.x, posY, rect.width, 22));
            posY += 23;
        }
        if (item.guaranteedStats.Count == 0)
        {
            EditorGUI.DrawRect(new Rect(rect.x, posY, rect.width, 22), new Color(0.25f, 0.25f, 0.25f));
            GUI.Label(new Rect(rect.x, posY, rect.width, 22), "No Guaranteed Stats", LogicEngineEditor.windowStyle_SmallCenteredText);
            posY += 23;
        }
        posY += minorSpacer;

        // UI Control: Draw the header for the random stats.
        EditorGUI.DrawRect(new Rect(rect.x, posY, rect.width, 22), new Color(0.1f, 0.1f, 0.1f));
        GUI.Label(new Rect(rect.x + minorSpacer, posY, 150, 20), "Randomisable Stats", EditorStyles.boldLabel);
        if (item.randomisableStats.Count > 0) GUI.Label(new Rect(posX + 136, posY, 30, 20), "Min", LogicEngineEditor.windowStyle_SmallCenteredText);
        if (item.randomisableStats.Count > 0) GUI.Label(new Rect(posX + 167, posY, 30, 20), "Max", LogicEngineEditor.windowStyle_SmallCenteredText);
        if (GUI.Button(new Rect(rect.x + rect.width - 21, posY - 1, 21, 21), "+", LogicEngineEditor.windowStyle_AddButtonSmall))
        {
            BuildAddStatMenu(item.randomisableStats).ShowAsContext();
        }
        posY += 23;

        // UI Control: Draw each randomisable stat.
        foreach (Item.StatBlock statBlock in item.randomisableStats)
        {
            DrawStatBlock(item, statBlock, new Rect(rect.x, posY, rect.width, 22));
            posY += 23;
        }
        if (item.randomisableStats.Count == 0)
        {
            EditorGUI.DrawRect(new Rect(rect.x, posY, rect.width, 22), new Color(0.25f, 0.25f, 0.25f));
            GUI.Label(new Rect(rect.x, posY, rect.width, 22), "No Random Stats", LogicEngineEditor.windowStyle_SmallCenteredText);
            posY += 23;
        }

        // Delete all stats marked for deletion.
        if (toDelete.Count > 0)
        {
            foreach (Item.StatBlock statBlock in toDelete)
            {
                item.guaranteedStats.Remove(statBlock);
                item.randomisableStats.Remove(statBlock);
                EditorUtility.SetDirty(item);
            }
            toDelete = new List<Item.StatBlock>();
        }
    }

    /// <summary>
    /// Draw a single item stat block for the item.
    /// </summary>
    private void DrawStatBlock (Item item, Item.StatBlock statBlock, Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.25f, 0.25f, 0.25f));

        // Draw the delete button.
        Rect deleteRect = new Rect(rect.x + rect.width - 21, rect.y - 1, rect.height - 1, rect.height - 1);
        if (GUI.Button(deleteRect, "�", LogicEngineEditor.windowStyle_AddButtonSmall))
            toDelete.Add(statBlock);

        // Draw the label.
        string label = statBlock.stat.Label();
        if (statBlock.stat.ShowAsPercent(statBlock.isPercent)) label += " %";
        GUI.Label(new Rect(rect.x + 5, rect.y, 140, rect.height), label);

        // Draw the minimum and maximum inputs.
        float mod = statBlock.stat.DisplayModifier(statBlock.isPercent);
        Rect minRect = new Rect(rect.x + rect.width - 85, rect.y, 30, rect.height);
        Rect maxRect = new Rect(rect.x + rect.width - 53, rect.y, 30, rect.height);
        statBlock.minimum = EditorGUI.FloatField(minRect, statBlock.minimum * mod, LogicEngineEditor.windowStyle_TextField) / mod;
        statBlock.maximum = EditorGUI.FloatField(maxRect, statBlock.maximum * mod, LogicEngineEditor.windowStyle_TextField) / mod;
    }

    /// <summary>
    /// Create a new item, by giving the user a list of templates to copy from.
    /// </summary>
    private void CreateItem()
    {
        GenericMenu menu = new GenericMenu();

        Item[] items = Resources.LoadAll<Item>("Templates");
        for (int i = 0; i < items.Length; i++)
        {
            Item it = items[i];
            menu.AddItem(new GUIContent(it.itemName), false, () => { CopyItem(it); });
        }
        menu.ShowAsContext();
    }

    /// <summary>
    /// Start the rename process for the specified item.
    /// </summary>
    private void RenameItem (Item item)
    {
        string itemName = "";
        itemName = EditorInputDialog.Show("Enter New Name", "", item.itemName);
        if (itemName != null && itemName.Length > 0)
        {
            item.itemName = itemName;
            item.name = itemName;
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(item), itemName);
        }
    }

    /// <summary>
    /// Delete the specified item.
    /// </summary>
    private void DeleteItem (Item item)
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(item));
    }

    /// <summary>
    /// Copies the specified script block, and selects the new copy.
    /// </summary>
    private void CopyItem (Item item)
    {
        string itemName = "";
        itemName = EditorInputDialog.Show("Enter Copy Name", "", item.itemName);
        if (itemName != null && itemName.Length > 0)
        {
            Item copy = item.Copy();
            copy.itemName = itemName;
            copy.name = itemName;
            string path = AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/Items/{copy.itemName}.asset");
            AssetDatabase.CreateAsset(copy, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ItemEditor.item = copy;
            engineEditor = new LogicEngineEditor(this, item.engine, item);
        }
    }


    /// <summary>
    /// Handle all drawing for the UI panel.
    /// </summary>
    private void OnGUI()
    {
        // If there is no item selected, select the first item.
        if (item == null)
        {
            item = Resources.LoadAll<Item>("Items")[0];
            if (item == null) return;
            engineEditor = new LogicEngineEditor(this, item.engine, item);
        }

        // Record the state before any changes
        Undo.RecordObject(item, "Changed Item " + item.itemName);

        // Start a change check, so the item can be dirtied if the user makes a change.
        EditorGUI.BeginChangeCheck();

        // Draw the list of items.
        float currentX = leftMargin;
        DrawItemList(new Rect(currentX, topMargin, abilitiesPanelWidth, position.height - bottomMargin));
        
        // Draw the details panel for the currently selected item.
        currentX += abilitiesPanelWidth + spaceBetweenPanels;
        DrawItemDetails(new Rect(currentX, topMargin, abilityDetailsPanelWidth, position.height - bottomMargin));
        
        // Draw the scripting panel for the currently selected item.
        currentX += abilityDetailsPanelWidth + spaceBetweenPanels;
        float scriptsPanelWidth = this.position.width - currentX - rightMargin;
        engineEditor.Process();
        engineEditor.DrawNodes(new Rect(currentX, topMargin, scriptsPanelWidth, position.height - bottomMargin));

        // If a change occured, mark the current item as dirty.
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(item); 
    }

    /// <summary>
    /// Constantly repaint the inspector (OnInspectorUpdate only updates 10 times per second).
    /// </summary>
    private void OnInspectorUpdate() 
    {
        if (this.IsFocused())
            Repaint();
    }
}
