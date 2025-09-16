using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;

public class GeneralScriptEditor : EditorWindow
{
    private LogicContainer selectedLogicBlock;
    private LogicEngineEditor engineEditor;
    private Sprite itemIcon;
    private const float spacer = 4; 
    private const float scriptPanelWidth = 200;
    private const float scriptBlockHeaderHeight = 25;
    private const string resourceFolder = "General Scripts";
    private const string fullFolderPath = "Assets/Resources/General Scripts";
    private const string iconFolder = "Icons/Code Folder";
    private Color scriptBlockBackgroundColor = new Color(0.3f, 0.3f, 0.3f);
    private Color scriptBlockHeaderColor = new Color(0.1f, 0.1f, 0.1f);
    
    public void SetSelectedLogicBlock (LogicContainer logicBlock)
    {
        selectedLogicBlock = logicBlock;
        engineEditor = new LogicEngineEditor(this, logicBlock.engine, logicBlock);
    }

    /// <summary>
    /// Create a new script block.
    /// </summary>
    private void CreateNewGeneralScript()
    {
        string blockName = "";
        blockName = EditorInputDialog.Show("Enter New Name", "", "Logic Block");
        if (blockName != null && blockName.Length > 0)
        {
            selectedLogicBlock = ScriptableObject.CreateInstance<LogicContainer>();
            selectedLogicBlock.name = blockName;
            string path = $"{fullFolderPath}/{blockName}.asset";
            AssetDatabase.CreateAsset(selectedLogicBlock, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            engineEditor.SetEngine(selectedLogicBlock);
        }
    }

    /// <summary>
    /// Delete the specified script block.
    /// </summary>
    private void DeleteGeneralScript (LogicContainer block)
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(block));
        selectedLogicBlock = Resources.LoadAll<LogicContainer>(resourceFolder)[0];
        engineEditor.SetEngine(selectedLogicBlock);
    }
    
    /// <summary>
    /// Rename the specified script block.
    /// </summary>
    private void RenameGeneralScript (LogicContainer block)
    {
        string blockName = "";
        blockName = EditorInputDialog.Show("Enter New Name", "", block.name);
        if (blockName != null && blockName.Length > 0)
        {
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(block), blockName);
        }
    }

    /// <summary>
    /// Copies the specified script block, and selects the new copy.
    /// </summary>
    private void CopyGeneralScript (LogicContainer block)
    {
        string blockName = "";
        blockName = EditorInputDialog.Show("Enter New Name", "", block.name);
        if (blockName != null && blockName.Length > 0)
        {
            LogicContainer copy = block.Copy();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{fullFolderPath}/{blockName}.asset");
            AssetDatabase.CreateAsset(copy, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            selectedLogicBlock = copy;
            engineEditor.SetEngine(selectedLogicBlock);
        }
    }

    /// <summary>
    /// Draw the list of logic blocks.
    /// </summary>
    public void DrawLogicBlockList (Rect rect)
    {
        const float itemHeight = 30;
        const float itemPadding = 1;
        const float iconPadding = 5;

        if (itemIcon == null)
            itemIcon = Resources.Load<Sprite>(iconFolder);

        // Draw the background.
        EditorGUI.DrawRect(rect, scriptBlockBackgroundColor);

        // Draw the header.
        Rect area = new Rect(rect.x, rect.y, rect.width, scriptBlockHeaderHeight);
        rect.y += scriptBlockHeaderHeight;
        rect.height -= scriptBlockHeaderHeight;
        EditorGUI.DrawRect(area, scriptBlockHeaderColor);
        if (GUI.Button(new Rect(area.xMax - scriptBlockHeaderHeight, area.y - 1, 
            scriptBlockHeaderHeight, scriptBlockHeaderHeight), "+", LogicEngineEditor.windowStyle_AddButton))
        {
            CreateNewGeneralScript();
        }
        EditorGUI.LabelField(area, "  Game Logic ", LogicEngineEditor.windowStyle_HeaderText);
        LogicContainer[] items = Resources.LoadAll<LogicContainer>(resourceFolder);

        // Draw buttons for all of the logic blocks.
        for (int i = 0; i < items.Length; i++)
        {
            // Calculate the item rect.
            Rect itemRect = new Rect(rect);
            itemRect.height = itemHeight;
            itemRect.y += (i * (itemHeight + itemPadding));
             
            // Draw the correct background style.
            if (items[i] == selectedLogicBlock)
                GUI.Label(itemRect, "", LogicEngineEditor.selectedScriptStyle);
            else
                GUI.Label(itemRect, "", LogicEngineEditor.unselectedScriptStyle);

            // Draw the name of the block.
            Rect textRect = new Rect(itemRect);
            textRect.x += itemHeight;
            textRect.width -= itemHeight;
            //GUI.Label(textRect, items[i].name, LogicEngineEditor.windowStyle_BodyText);

            if (EditorUtility.IsDirty(items[i]))
                GUI.Label(textRect, "*" + items[i].name, LogicEngineEditor.windowStyle_BodyText);
            else
                GUI.Label(textRect, items[i].name, LogicEngineEditor.windowStyle_BodyText);


            // Draw the texture of the block.
            Rect iconRect = new Rect(itemRect);
            iconRect.x += iconPadding;
            iconRect.y += iconPadding;
            iconRect.height = itemHeight - 2 * iconPadding;
            iconRect.width = itemHeight - 2 * iconPadding;
            GUI.DrawTexture(iconRect, itemIcon.texture);

            // Handle Input Checks.
            Event current = Event.current;
            if (itemRect.Contains(current.mousePosition))
            {
                // On mouse down, select this code block.
                if (current.type == EventType.MouseDown || current.type == EventType.ContextClick)
                {
                    SetSelectedLogicBlock(items[i]);
                }

                // On right click, select the code block and show available context options.
                if (current.type == EventType.ContextClick)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Copy"), false, () => { CopyGeneralScript(selectedLogicBlock); });
                    menu.AddItem(new GUIContent("Rename"), false, () => { RenameGeneralScript(selectedLogicBlock); });
                    menu.AddItem(new GUIContent("Delete"), false, () => { DeleteGeneralScript(selectedLogicBlock); });
                    menu.ShowAsContext();
                    current.Use();
                }
            }
        }
    }

    /// <summary>
    /// Handle all drawing for the UI panel.
    /// </summary>
    private void OnGUI()
    {
        // Try to load a script if none is selected.
        if (engineEditor == null)
        {
            selectedLogicBlock = Resources.LoadAll<LogicContainer>(resourceFolder)[0];
            engineEditor = new LogicEngineEditor(this, selectedLogicBlock.GetEngine(), selectedLogicBlock);
        } 

        // If no script is found - return.
        if (engineEditor == null) return;

        // Record the state before any changes
        Undo.RecordObject(selectedLogicBlock, "Changed Ability Block " + selectedLogicBlock.name);

        // Set up initial positions.
        float posX = spacer;
        float posY = spacer;
        float height = this.position.height - spacer * 2;

        // Draw all of the panels.
        engineEditor.Process();
        DrawLogicBlockList(new Rect(posX, posY, scriptPanelWidth, height));
        posX += scriptPanelWidth + spacer;
        engineEditor.DrawNodes(new Rect(posX, posY, this.position.width - posX - spacer, height));
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
