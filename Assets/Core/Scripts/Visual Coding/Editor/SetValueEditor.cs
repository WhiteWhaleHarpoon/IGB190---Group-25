using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering.PostProcessing;
using UnityEngine;

/// <summary>
/// The SetValueEditor allows the user to enter in the data associated with a node.
/// There are two common types that can be entered:
///   (1): A Value - which is a primative type or link to a specific object.
///   (2): A Function - Runs code to determin the appropriate value (e.g. closest unit to a location).
/// </summary>
public class SetValueEditor : EditorWindow
{
    // Store a copy of this window for easy access.
    public static SetValueEditor window;

    // The node that is being edited.
    public GeneralNode nodeBackup;
    
    // A copy of the node being edited. Changes are made to this object and do not propogate
    // to the main node until the user confirms the changes.
    public GeneralNode nodeCopy;

    // The cached list of available function nodes.
    public static GeneralNode[] functionNodes;
    public static string[] functionNodeStrings;

    // The cached list of available preset nodes.
    public static string[] presetStrings;


    public static string[] valueOptionStrings;
    public static MonoBehaviour[] valueOptions;


    public int nodeValueID;

    

    // Keep a copy of the 
    private static EditorWindow parentWindow;

    public static bool hasFocused = false;

    public static Vector2 pos;

    private IVisualCodeHandler engineHandler;

    private static bool allowValues;
    private static bool allowPresets;
    private static bool allowFunctions;

    public static SetValueEditor OpenWindow (EditorWindow parent, Vector2 position, GeneralNode node, IVisualCodeHandler engineHandler)
    {
        // Build the data if it hasn't already been done.
        if (LogicEngineEditor.allNodes.Count == 0) LogicEngineEditor.BuildAllData();

        // Setup the window data.
        window = Editor.CreateInstance<SetValueEditor>();
        window.nodeCopy = node;
        window.nodeBackup = node.New().CopyFrom(node);
        window.engineHandler = engineHandler;
        parentWindow = parent;

        // Cache the data which should be shown for this specific node.
        CachePresets(node, engineHandler);
        CacheFunctions(node); 

        valueOptions = null;
        valueOptionStrings = null;
        if (node is UnitNode) CacheValueOptions<Unit>();
        else if (node is ProjectileNode) CacheValueOptions_Projectiles();
        else if (node is EffectNode) CacheValueOptions_Feedback();

       

        allowValues = window.nodeCopy.GetAllowValue();
        allowPresets = window.nodeCopy.GetAllowPreset();
        allowFunctions = window.nodeCopy.GetAllowFunction();
      

        if (functionNodes == null || functionNodes.Length == 0) 
            allowFunctions = false;
        if (presetStrings == null || presetStrings.Length == 0)
            allowPresets = false;

        // If this was a temporary node, make it a value node.
        if (window.nodeCopy.returnType == GeneralNode.ReturnType.Temp && window.nodeCopy.GetAllowValue())
        {
            window.nodeCopy.returnType = GeneralNode.ReturnType.Value;
        }
        else if (window.nodeCopy.returnType == GeneralNode.ReturnType.Temp && window.nodeCopy.GetAllowPreset() && presetStrings.Length > 0)
        {
            window.nodeCopy.returnType = GeneralNode.ReturnType.Preset;
            window.nodeCopy.presetName = presetStrings[0];
        }
        else if (window.nodeCopy.returnType == GeneralNode.ReturnType.Temp && window.nodeCopy.GetAllowFunction())
        {
            window.nodeCopy.returnType = GeneralNode.ReturnType.Function;
            window.nodeCopy.functionDescription = functionNodes[0].Template.functionDescription;
            window.nodeCopy.functionDynamicDescription = functionNodes[0].Template.functionDynamicDescription;
            window.nodeCopy.functionName = functionNodes[0].Template.functionName;
            window.nodeCopy.functionEvaluators = functionNodes[0].Template.functionEvaluators;
        }

        // Show the window just below the mouse.
        //window.ShowPopup();
        //window.ShowAuxWindow();
        window.ShowAsDropDown(new Rect(100, 100, 100, 100), new Vector2(400, GetTotalHeight(allowValues, allowPresets, allowFunctions)));
        //window.ShowModalUtility();
        //window.ShowTab();
        //window.ShowUtility();
        position.y += 5;
        pos = position;
        window.position = new Rect(pos.x, pos.y + 100, 50, GetTotalHeight(allowValues, allowPresets, allowFunctions));
        hasFocused = false;
        
        // Return the instance of this window.
        return window;
    }

    /// <summary>
    /// 
    /// </summary>
    private static void CacheFunctions (GeneralNode node)
    {
        List<GeneralNode> functionNodesList = new List<GeneralNode>();
        List<string> functionNodeStringsList = new List<string>();
        foreach (GeneralNode current in LogicEngineEditor.allNodes)
        {
            /*
            // Unit Group nodes are special and can allow single units as well.
            if (node.GetType() == typeof(UnitGroupNode))
            {
                if (current.GetType() == typeof(UnitGroupNode))
                {
                    functionNodesList.Add(current);
                    functionNodeStringsList.Add("Group of Units/" + current.functionDescription);
                    //functionNodeStringsList.Add(current.functionDescription);
                }
                else if (current.GetType() == typeof(UnitNode))
                {
                    functionNodesList.Add(current);
                    functionNodeStringsList.Add("Single Unit/" + current.functionDescription);
                    //functionNodeStringsList.Add(current.functionDescription);
                }
            }
            else */
            if (current.GetType() == node.GetType())
            {
                functionNodesList.Add(current);
                functionNodeStringsList.Add(current.functionDescription);
            }
        }
        functionNodes = functionNodesList.ToArray();
        functionNodeStrings = functionNodeStringsList.ToArray();
    } 

    /// <summary>
    /// 
    /// </summary>
    private static void CachePresets (GeneralNode node, IVisualCodeHandler engineHandler)
    {
        List<string> presetNodeStringsList = new List<string>();

        

        // Generate a list of ALL presets.
        List<string> allPresets = new List<string>();
        VisualCodeScript script = engineHandler.GetEngine().selectedScript;
        foreach (GeneralNode n in script.eventNodes)
        {
            if (n is EventNode && ((EventNode)n).presets != null)
            {
                foreach (string preset in ((EventNode)n).presets)
                {
                    allPresets.Add(preset);
                }
            }
        }

        // Filter the list so only the presets of the correct type are shown.
        foreach (string preset in allPresets)
        {
            if ((node is UnitNode && LogicEngine.UNIT_PRESETS.Contains(preset)) ||
                (node is NumberNode && LogicEngine.NUMBER_PRESETS.Contains(preset)) ||
                (node is ProjectileNode && LogicEngine.PROJECTILE_PRESETS.Contains(preset)) ||
                (node is ItemNode && LogicEngine.ITEM_PRESETS.Contains(preset)) ||
                (node is AbilityNode && LogicEngine.ABILITY_PRESETS.Contains(preset)) ||
                (node is VectorNode && LogicEngine.VECTOR_PRESETS.Contains(preset)) ||
                (node is BoolNode && LogicEngine.BOOL_PRESETS.Contains(preset)))
            {
                presetNodeStringsList.Add(preset);
            }
        }

        if (node is NumberNode)
        {
            //presetNodeStringsList.Add(LogicEngine.PRESET_PLAYER_LEVEL);
            //presetNodeStringsList.Add(LogicEngine.PRESET_TIME_SINCE_START);
        }
        if (node is UnitNode)
        {
            presetNodeStringsList.Add(LogicEngine.PRESET_UNIT_PLAYER);
            presetNodeStringsList.Add(LogicEngine.PRESET_UNIT_LAST_CREATED);
        }
        if (node is ProjectileNode)
        {
            presetNodeStringsList.Add(LogicEngine.PRESET_PROJECTILE_LAST_CREATED);
        }
        if (parentWindow is AbilityEditor)
        {
            if (node is UnitNode) 
            {
                presetNodeStringsList.Add(LogicEngine.PRESET_ABILITY_OWNER);
            }
            else if (node is AbilityNode)
            {
                presetNodeStringsList.Add(LogicEngine.PRESET_ABILITY_THIS);
            }
        }
        else if (parentWindow is ItemEditor)
        {
            if (node is UnitNode)
            {
                presetNodeStringsList.Add(LogicEngine.PRESET_ITEM_OWNER);
            }
            else if (node is ItemNode)
            {
                presetNodeStringsList.Add(LogicEngine.PRESET_ITEM_THIS);
            }
        }

        presetStrings = presetNodeStringsList.ToArray();
    }

    /// <summary>
    /// Actions to perform when the user presses the 'Submit' button.
    /// </summary>
    public void Submit ()
    {
        
        Close();
        if (parentWindow != null)
        {
            parentWindow.Repaint();
            parentWindow.Focus();
            LogicEngineEditor.hoveredNode = null;
            LogicEngineEditor.hoveredNodeDepth = -1;
        }
        EditorUtility.SetDirty(engineHandler.GetData());
    }

    /// <summary>
    /// Actions to perform when the user presses the 'Cancel' button.
    /// </summary>
    public void Cancel ()
    {
        nodeCopy.CopyFrom(nodeBackup);
        if (parentWindow != null)
        {
            parentWindow.Repaint();
            parentWindow.Focus();
        }
        Close();
    }

    /// <summary>
    /// When scripts are reloaded, close all windows of this type.
    /// </summary>
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        if (window != null) window.Close();
    }


    private static void CacheValueOptions_Feedback()
    {
        List<MonoBehaviour> components = new List<MonoBehaviour>();
        List<string> componentNames = new List<string>();
        string[] interactionGUIDs = AssetDatabase.FindAssets("t:prefab", new[] { "Assets" });
        foreach (string guid in interactionGUIDs)
        {
            GameObject asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as GameObject;
            CustomVisualEffect component = asset.GetComponent<CustomVisualEffect>();
            if (component != null && !component.isTemplate)
            {
                components.Add(component);
                componentNames.Add(component.subGroup + "/" + component.gameObject.name);
            }
        }
        valueOptionStrings = componentNames.ToArray();
        valueOptions = components.ToArray();
    }

    private static void CacheValueOptions_Projectiles()
    {
        List<MonoBehaviour> components = new List<MonoBehaviour>();
        List<string> componentNames = new List<string>();
        string[] interactionGUIDs = AssetDatabase.FindAssets("t:prefab", new[] { "Assets" });
        foreach (string guid in interactionGUIDs)
        {
            GameObject asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as GameObject;
            Projectile component = asset.GetComponent<Projectile>();
            if (component != null && !component.isTemplate)
            {
                components.Add(component);
                componentNames.Add(component.category + "/" + component.gameObject.name);
            }
        }
        valueOptionStrings = componentNames.ToArray();
        valueOptions = components.ToArray();
    }


    private static void CacheValueOptions<T>() where T : MonoBehaviour
    {
        List<MonoBehaviour> components = new List<MonoBehaviour>();
        List<string> componentNames = new List<string>();
        string[] interactionGUIDs = AssetDatabase.FindAssets("t:prefab", new[] { "Assets" });
        foreach (string guid in interactionGUIDs)
        {
            GameObject asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as GameObject;
            T component = asset.GetComponent<T>();
            if (component != null)
            {
                components.Add(component);
                componentNames.Add(component.gameObject.name);
            }
        }
        valueOptionStrings = componentNames.ToArray();
        valueOptions = components.ToArray();
    }

    private float DrawValuePanel(Rect rect)
    {
        if (nodeCopy.returnType == GeneralNode.ReturnType.Value)
            EditorGUI.DrawRect(new Rect(rect.x, rect.y - 2, rect.width - 2, 27), new Color(0.17f, 0.36f, 0.53f, 0.5f));

        Rect lineRect = new Rect(rect.x, rect.y, rect.width, 1);

        // Handle initial spacing.
        const float spacer = 5;
        rect.x += spacer;
        rect.y += 2;



        // Toggle Block.
        if (EditorGUI.Toggle(new Rect(rect.x, rect.y + 1, 90, 20), nodeCopy.returnType == GeneralNode.ReturnType.Value, EditorStyles.radioButton))
        {
            nodeCopy.returnType = GeneralNode.ReturnType.Value;
            parentWindow.Repaint();
        }

        // Label Block.
        Rect labelRect = new Rect(rect.x + 20, rect.y - 2, rect.width, 20);
        EditorGUI.LabelField(labelRect, "Value: ", nodeCopy.returnType == GeneralNode.ReturnType.Value ? EditorStyles.boldLabel : EditorStyles.label);

        // Value Block.
        Rect valueRect = new Rect(rect.x + 90, rect.y, rect.width - rect.x - 95, 18);
        //floatTest = EditorGUI.FloatField(valueRect, floatTest);

        EditorGUI.BeginChangeCheck();
        if (nodeCopy.returnType == GeneralNode.ReturnType.Value && nodeCopy is not VectorNode)
            GUI.SetNextControlName("InitialFocus");
        if (valueOptions != null)
        {
            int index = Mathf.Max(0, Array.IndexOf(valueOptions, nodeCopy.GetValue()));
            EditorGUI.BeginChangeCheck();
            int newID = EditorGUI.Popup(valueRect, index, valueOptionStrings);
            if (EditorGUI.EndChangeCheck())
            {
                
                nodeCopy.returnType = GeneralNode.ReturnType.Value;
                GUI.FocusControl(null);
            }
            if (nodeCopy.returnType == GeneralNode.ReturnType.Value)
                nodeCopy.SetValue(valueOptions[newID]);
        }
        
        if (nodeCopy is NumberNode)
            nodeCopy.SetValue(EditorGUI.FloatField(valueRect, (float)nodeCopy.GetValue()));
        else if (nodeCopy is VectorNode)
            nodeCopy.SetValue(EditorGUI.Vector3Field(valueRect, "", (Vector3)nodeCopy.GetValue()));
        else if (nodeCopy is BoolNode)
            nodeCopy.SetValue(EditorGUI.Popup(valueRect, (bool)nodeCopy.GetValue() == true ? 0 : 1, new string[] { "True", "False" }) == 0);
        else if (nodeCopy is GameObjectNode)
            nodeCopy.SetValue(EditorGUI.ObjectField(valueRect, (GameObject)nodeCopy.GetValue(), typeof(GameObject), false));
        else if (nodeCopy is AbilityNode)
            nodeCopy.SetValue(EditorGUI.ObjectField(valueRect, (Ability)nodeCopy.GetValue(), typeof(Ability), false));
        else if (nodeCopy is ItemNode)
            nodeCopy.SetValue(EditorGUI.ObjectField(valueRect, (Item)nodeCopy.GetValue(), typeof(Item), false));
        else if (nodeCopy is AudioNode)
            nodeCopy.SetValue(EditorGUI.ObjectField(valueRect, (AudioClip)nodeCopy.GetValue(), typeof(AudioClip), false));
        else if (nodeCopy is GameFeedbackNode)
            nodeCopy.SetValue(EditorGUI.ObjectField(valueRect, (GameFeedback)nodeCopy.GetValue(), typeof(GameFeedback), false));

        //EditorGUI.ColorField()
        else if (nodeCopy is ColorNode)
            nodeCopy.SetValue(EditorGUI.ColorField(valueRect, new GUIContent(), (Color)nodeCopy.GetValue(), true, false, false));


        if (nodeCopy is StringNode)
        {
            string[] options = ((StringNode)nodeCopy).options;
            if (options.Length == 0)
            {
                nodeCopy.SetValue(EditorGUI.TextField(valueRect, (string)nodeCopy.GetValue()));
            }
            else
            {
                int i = Mathf.Max(0, Array.IndexOf(options, nodeCopy.GetValue()));
                nodeCopy.SetValue(options[EditorGUI.Popup(valueRect, i, ((StringNode)nodeCopy).options)]);
            }
        }
        if (nodeCopy is not VectorNode)
            GUI.FocusControl("InitialFocus");
        

        if (EditorGUI.EndChangeCheck()) nodeCopy.returnType = GeneralNode.ReturnType.Value;

        // Draw the line spacer.
        rect.y += 22;
        lineRect.y = rect.y;
        EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f));

        // Return the final height vaue.
        rect.y += 4;
        return rect.y;
    }

    private float DrawPresetPanel (Rect rect)
    {
        if (nodeCopy.returnType == GeneralNode.ReturnType.Preset)
            EditorGUI.DrawRect(new Rect(rect.x, rect.y - 3, rect.width - 2, 27), new Color(0.17f, 0.36f, 0.53f, 0.5f));

        Rect lineRect = new Rect(rect.x, rect.y, rect.width, 1);

        // Handle initial spacing.
        const float spacer = 5;
        rect.x += spacer;
        rect.y += 2;

        // Toggle Block.
        if (EditorGUI.Toggle(new Rect(rect.x, rect.y + 1, 90, 20), nodeCopy.returnType == GeneralNode.ReturnType.Preset, EditorStyles.radioButton))
        {
            nodeCopy.returnType = GeneralNode.ReturnType.Preset;
            parentWindow.Repaint();
        }

        // Label Block.
        Rect labelRect = new Rect(rect.x + 20, rect.y - 2, rect.width, 20);
        EditorGUI.LabelField(labelRect, "Preset: ", nodeCopy.returnType == GeneralNode.ReturnType.Preset ? EditorStyles.boldLabel : EditorStyles.label);



        // Value Block.
        EditorGUI.BeginChangeCheck();
        Rect valueRect = new Rect(rect.x + 90, rect.y, rect.width - rect.x - 95, 20);
        int index = Mathf.Max(0, Array.IndexOf(presetStrings, nodeCopy.presetName));
        int newID = EditorGUI.Popup(valueRect, index, presetStrings);
        if (EditorGUI.EndChangeCheck())
        {
            nodeCopy.returnType = GeneralNode.ReturnType.Preset;
            GUI.FocusControl(null);
        }
        if (nodeCopy.returnType == GeneralNode.ReturnType.Preset)
            nodeCopy.presetName = presetStrings[newID];

        // Draw the line spacer.
        rect.y += 22;
        lineRect.y = rect.y;
        EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f));

        // Return the final height vaue.
        rect.y += 4;
        return rect.y;
    }

    private float DrawFunctionPanel (Rect rect)
    {
        if (nodeCopy.returnType == GeneralNode.ReturnType.Function)
            EditorGUI.DrawRect(new Rect(rect.x, rect.y - 3, rect.width - 2, 27), new Color(0.17f, 0.36f, 0.53f, 0.5f));

        Rect lineRect = new Rect(rect.x, rect.y, rect.width, 1);

        // Handle initial spacing.
        const float spacer = 5; 
        rect.x += spacer;
        rect.y += 2;

        // Toggle Block.
        if (EditorGUI.Toggle(new Rect(rect.x, rect.y + 1, 90, 20), nodeCopy.returnType == GeneralNode.ReturnType.Function, EditorStyles.radioButton))
        {
            nodeCopy.returnType = GeneralNode.ReturnType.Function;
            parentWindow.Repaint();
        }

        // Label Block.
        Rect labelRect = new Rect(rect.x + 20, rect.y - 2, rect.width, 20);
        EditorGUI.LabelField(labelRect, "Function: ", nodeCopy.returnType == GeneralNode.ReturnType.Function ? EditorStyles.boldLabel : EditorStyles.label);

        // Value Block.
        EditorGUI.BeginChangeCheck();

        int index = -1;
        if (nodeCopy.Template != null)
            index = Mathf.Max(0, Array.IndexOf(functionNodeStrings, nodeCopy.Template.functionDescription));

        //int index = Mathf.Max(0, Array.IndexOf(functionNodeStrings, nodeCopy.Template.functionDescription));
        Rect valueRect = new Rect(rect.x + 90, rect.y, rect.width - rect.x - 95, 20);
        int newID = EditorGUI.Popup(valueRect, index, functionNodeStrings);
        if (EditorGUI.EndChangeCheck())
        {
            nodeCopy.returnType = GeneralNode.ReturnType.Function;
            GUI.FocusControl(null);
        }
        if (nodeCopy.returnType == GeneralNode.ReturnType.Function)
        {
            if (newID < 0) newID = 0;
            nodeCopy.functionDescription = functionNodes[newID].Template.functionDescription;
            nodeCopy.functionDynamicDescription = functionNodes[newID].Template.functionDynamicDescription;
            nodeCopy.functionName = functionNodes[newID].functionName;

            nodeCopy.functionEvaluators = new GeneralNode[functionNodes[newID].functionEvaluators.Length];
            for (int i = 0; i < functionNodes[newID].functionEvaluators.Length; i++)
            {
                nodeCopy.functionEvaluators[i] = functionNodes[newID].functionEvaluators[i].Copy();
            }
        }

        // Draw the line spacer.
        rect.y += 22;
        lineRect.y = rect.y;
        EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f));

        // Return the final height vaue.
        rect.y += 4;
        return rect.y;
    }

    public static float GetTotalHeight (bool valueOptionAllowed, bool presetOptionAllowed, bool functionOptionAllowed)
    {
        float size = 49;
        if (valueOptionAllowed) size += 28;
        if (presetOptionAllowed) size += 28;
        if (functionOptionAllowed) size += 28;
        return size;
    }

    private void DrawUI ()
    {
        // Setup default window properties.
        titleContent.text = "Set Value Window";


        EditorGUI.DrawRect(new Rect(0, 0, 400, 1000), new Color(0.5f, 0.5f, 0.5f));
        
        EditorGUI.DrawRect(new Rect(1, 1, 398, GetTotalHeight(allowValues, allowPresets, allowFunctions) - 2), new Color(0.2f, 0.2f, 0.2f));
        EditorGUI.DrawRect(new Rect(0, 0, 400, 27), new Color(0.5f, 0.5f, 0.5f));

        float currentX = 1;
        float currentY = 1;
        float width = 400;
        float height = 25;

        Rect r = new Rect(currentX, currentY, width, height);


        EditorGUI.DrawRect(new Rect(1, 1, width-2, 25), new Color(0.1f, 0.1f, 0.1f));
        r.y += 27;
        EditorGUI.LabelField(new Rect(8, 3, 200, 20), "Set Node Value", EditorStyles.boldLabel);
        if (allowValues) r.y = DrawValuePanel(r);
        if (allowPresets) r.y = DrawPresetPanel(r);
        if (allowFunctions) r.y = DrawFunctionPanel(r);

        //r.y += 30;



        // Draw the buttons.
        Rect leftRect = new Rect(r);
        leftRect.width = width / 2.0f - 1;
        leftRect.x = 7;

        Rect rightRect = new Rect(leftRect);
        rightRect.x += leftRect.width + 2;
        rightRect.width -= 10;

        if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
            Submit();
        if (GUI.Button(leftRect, "Revert", EditorStyles.miniButtonMid))
            Cancel();
        if (GUI.Button(rightRect, "Confirm", EditorStyles.miniButtonMid))
            Submit();

        r.y += 20;
        this.position = new Rect(pos.x, pos.y, width, r.y);
    }




    public void OnGUI()
    {
        DrawUI();
    }
}
