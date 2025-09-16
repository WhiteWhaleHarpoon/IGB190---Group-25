using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class LogicEngineEditor
{
    public static List<GeneralNode> allNodes = new List<GeneralNode>();

    public static GUIStyle unselectedScriptStyle;
    public static GUIStyle unselectedScriptStyle2;
    public static GUIStyle selectedScriptStyle;

    public static GUIStyle windowStyle_SmallText;
    public static GUIStyle windowStyle_SmallCenteredText;
    public static GUIStyle windowStyle_HeaderText;
    public static GUIStyle windowStyle_BaseText;
    public static GUIStyle windowStyle_BodyText;
    public static GUIStyle windowStyle_TempText;
    public static GUIStyle windowStyle_ValueText;
    public static GUIStyle windowStyle_PresetText;
    public static GUIStyle windowStyle_VariableText;

    public static GUIStyle windowStyle_OddNode;
    public static GUIStyle windowStyle_EvenNode;
    public static GUIStyle windowStyle_HoveredNode;

    public static Color valueColor = new Color(0.5f, 1.0f, 0.5f);
    public static Color tempColor = new Color(1.0f, 0.5f, 0.5f);
    public static Color varColor = new Color(0.75f, 0.75f, 1.0f);
    public static Color presetColor = Color.yellow;


    public static GUIStyle windowStyle_AddButton;
    public static GUIStyle windowStyle_AddButtonSmall;
    public static GUIStyle windowStyle_TextField;

    public static float indentWidth = 30;

    private static string[] increaseDecrease = new string[] { "Increase", "Decrease" };
    private static string[] boolComparators = new string[] { "Equal To", "Not Equal To" };
    private static string[] numberComparators = new string[] { "Equal To", "Not Equal To", "Less Than",
            "Less Than or Equal To", "Greater Than", "Greater Than or Equal To" };


    public static Color hoveredNodeColor = new Color(0.35f, 0.35f, 0.35f);
    public static Color oddNodeColor = new Color(0.25f, 0.25f, 0.25f, 1.0f);
    public static Color evenNodeColor = new Color(0.15f, 0.15f, 0.15f, 1.0f);

    public const float itemHeight = 50;
    public const float heightIncreasePerDepth = 12;

    public IVisualCodeHandler engineHandler;
    public LogicEngine engine;
    public EditorWindow window;

    public static GeneralNode hoveredNode;
    public static int hoveredNodeDepth;

    public static bool isDragging = false;
    public static GeneralNode dragStartedAt = null;

    public static bool isDraggingScript = false;
    public static VisualCodeScript scriptBeingDragged = null;

    public static Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();

    public static List<GeneralNode> nodesInClipboard = new List<GeneralNode>();

    public const string unitIcon = "Unit";
    public const string projectileIcon = "Projectile";
    public const string effectIcon = "Effect";
    public const string questIcon = "QuestNew";
    public const string soundIcon = "Audio";
    public const string uiIcon = "UI"; 
    public const string variableIcon = "Variable";
    public const string conditionIcon = "Condition";
    public const string eventIcon = "Event";
    public const string loopIcon = "Loop";
    public const string timerIcon = "Timer";
    public const string waitIcon = "Timer"; 
    public const string gameIcon = "Game";
    public const string regionIcon = "Region";
    public const string cancelIcon = "Cancel";
    public const string pickupIcon = "Pickup";
    public const string inputIcon = "Keyboard";

    private Sprite scriptIconSprite = null;

    

    public LogicEngineEditor (EditorWindow window, LogicEngine engine, IVisualCodeHandler engineHandler)
    {
        this.window = window;
        this.engine = engine;
        this.engineHandler = engineHandler;
    }

    public void SetEngine (LogicEngine engine)
    {
        this.engine = engine;
        engine.selectedScript = engine.scripts[0];
    }

    public void SetEngine (IVisualCodeHandler engineHandler)
    {
        this.engine = engineHandler.GetEngine();
        this.engineHandler = engineHandler;
        if (engine.scripts.Count == 0)
        {
            engine.scripts.Add(new VisualCodeScript("Main"));
        }
        engine.selectedScript = engine.scripts[0];
    }

    public void SetSelectedScript (VisualCodeScript script)
    {
        engine.selectedScript = script;
        EditorUtility.SetDirty(engineHandler.GetData());
    }

    public static GeneralNode Node_UnitStartsCastingThisAbility;
    public static GeneralNode Node_UnitFinishesCastingThisAbility;
    public static GeneralNode Node_OwnerNode;

    private static GeneralNode BuildEventNodeFromMethod (MethodInfo method)
    {
        EventNode node = new EventNode();

        // Generate the list of all presets based on the method attributes.
        List<string> presets = new List<string>();
        foreach (var preset in method.GetCustomAttributes<EventPreset>())
        {
            presets.Add(preset.presetName);
        }
        node.presets = presets.ToArray();

        // Update the description to include the presets.
        if (node.presets.Length > 0)
        {
            string extraDesc = "";
            for (int i = 0; i < node.presets.Length; i++)
            {
                string end = (i != node.presets.Length - 1) ? ", " : "";
                extraDesc += $"<color=yellow>{node.presets[i]}</color>{end}";
            }
            node.functionDynamicDescription += $" (Presets: {extraDesc})";
        }

        return node;
    }


    /// <summary>
    /// Constructs the list of all template visual nodes. These are identified using attribute tags.
    /// </summary>
    public static void BuildNodesFromAttributes ()
    {
        var methods = typeof(VisualCodeScript).GetMethods();
        foreach (var method in methods)
        {
            VisualScriptingFunction visualScripting = method.GetCustomAttribute<VisualScriptingFunction>();

            
            if (visualScripting != null)
            {
                GeneralNode node = null;
                if (visualScripting is VisualScriptingEvent)
                    node = BuildEventNodeFromMethod(method);
                else
                    node = ToNode(method.ReturnParameter.ParameterType, visualScripting.allowsChildren);

                // Populate the node data from the attribute information.
                node.functionName = method.Name;
                node.functionDescription = visualScripting.dropdownDescription;
                node.functionDynamicDescription = visualScripting.dynamicDescription + node.functionDynamicDescription;
                node.nodeIcon = visualScripting.icon;
                node.returnType = GeneralNode.ReturnType.Function;
                node.functionEvaluators = new GeneralNode[method.GetParameters().Length];

                // The number of parameters for the method should match the number of attributes given.
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != method.GetCustomAttributes<Arg>().Count()) 
                    Debug.Log($"Parameter Count Mismatch: {method.Name}");

                int id = 0;
                foreach (var arg in method.GetCustomAttributes<Arg>())
                {
                    // The parameter types for each method should match the arguments listed in the attributes.
                    if (parameters[id].ParameterType != arg.GetStoredType())
                        Debug.Log($"Parameter Type Mismatch: {method.Name}, Parameter {id}");

                    // Generate the child node.
                    GeneralNode child = ToNode(arg.GetStoredType(), false);
                    child.allowFunction = arg.allowFunction;
                    child.allowPreset = arg.allowPreset;
                    child.allowValue = arg.allowValue;
                    child.stringSuffix = arg.suffix;
                    if (arg.tempLabel.Length > 0) 
                        child.tempName = arg.tempLabel;
                    child.parentNode = node;
                    child.parentNodeID = id;
                    child.presetName = arg.preset;
                    child.returnType = arg.argType.ToNodeReturnType();
                    child.SetValue(arg.GetValue());

                    if (child is StringNode)
                    {
                        StringNode stringNode = (StringNode)child;
                        string[] options = StringArg.GetOptions(((StringArg)arg).choicePreset);
                        stringNode.options = options;
                        if (options.Length > 0) stringNode.SetValue(options[0]);
                    }

                    node.functionEvaluators[id] = child;
                    id++;

                    /*
                    if (child.returnType == GeneralNode.ReturnType.Value)
                        child.SetValue(arg.GetValue());

                    if (arg.argType == ArgType.Temp)
                    {
                        child.returnType = GeneralNode.ReturnType.Temp;
                    }
                    else if (arg.argType == ArgType.Preset)
                    {
                        child.returnType = GeneralNode.ReturnType.Preset;
                    }
                    else if (arg.argType == ArgType.Function)
                    {
                        child.returnType = GeneralNode.ReturnType.Function;
                    }
                    else if (arg.argType == ArgType.Value)
                    {
                        child.returnType = GeneralNode.ReturnType.Value;
                        child.SetValue(arg.GetValue());
                    }

                    node.functionEvaluators[id] = child;
                    id++;
                    */
                }
                allNodes.Add(node);
                LogicEngine.nodeTemplates[node.functionName] = node;
            }
        }
    }

    public static GeneralNode ToNode (Type returnParam, bool allowsChildren)
    {
        //Debug.Log($"{returnParam} : {typeof(Unit)}");
        if (allowsChildren) return new NestingActionNode();
        if (returnParam.Equals(typeof(Unit))) return new UnitNode();
        if (returnParam.Equals(typeof(string))) return new StringNode();
        if (returnParam == typeof(float)) return new NumberNode();
        if (returnParam == typeof(bool)) return new BoolNode();
        if (returnParam == typeof(AudioClip)) return new AudioNode();
        if (returnParam == typeof(Projectile)) return new ProjectileNode();
        if (returnParam == typeof(UnitGroup)) return new UnitGroupNode();
        if (returnParam == typeof(Color)) return new ColorNode();
        if (returnParam == typeof(Vector3)) return new VectorNode();
        if (returnParam == typeof(Item)) return new ItemNode();
        if (returnParam == typeof(GameObject)) return new GameObjectNode();
        if (returnParam == typeof(CustomVisualEffect)) return new EffectNode();
        if (returnParam == typeof(GameFeedback)) return new GameFeedbackNode();
        if (returnParam == typeof(Ability)) return new AbilityNode();
        return new ActionNode();
    }
    public static void BuildValueNodes()
    {
        BuildNodesFromAttributes();
    }

    private static void BuildStyles ()
    { 
        GUIStyle s = null;
        int defaultFontSize = 14;

        windowStyle_HeaderText = new GUIStyle(EditorStyles.boldLabel);
        windowStyle_HeaderText.fontSize = defaultFontSize;
        windowStyle_HeaderText.richText = true;

        windowStyle_BaseText = new GUIStyle(EditorStyles.boldLabel);
        windowStyle_BaseText.fontSize = defaultFontSize;
        windowStyle_BaseText.richText = true;

        windowStyle_BodyText = new GUIStyle(EditorStyles.boldLabel);
        windowStyle_BodyText.fontSize = 13;
        windowStyle_BodyText.richText = true;

        windowStyle_PresetText = new GUIStyle(EditorStyles.boldLabel);
        windowStyle_PresetText.fontSize = defaultFontSize;
        windowStyle_PresetText.normal.textColor = Color.yellow;

        windowStyle_ValueText = new GUIStyle(GUI.skin.button);
        windowStyle_ValueText.fontSize = defaultFontSize;
        windowStyle_ValueText.normal.textColor = new Color(0.5f, 1.0f, 0.5f);
        windowStyle_ValueText.richText = true;

        windowStyle_TempText = new GUIStyle(GUI.skin.button);
        windowStyle_TempText.fontSize = defaultFontSize;
        windowStyle_TempText.normal.textColor = new Color(1.0f, 0.5f, 0.5f);

        s = new GUIStyle(EditorStyles.label);
        s.fontSize = defaultFontSize;
        s.fontStyle = FontStyle.Italic;
        s.normal.textColor = s.hover.textColor = s.focused.textColor = s.active.textColor = new Color(1.0f, 1.0f, 1.0f, 0.6f);
        s.richText = true;
        windowStyle_SmallText = s;

        s = new GUIStyle(EditorStyles.label);
        s.fontStyle = FontStyle.Italic;
        s.alignment = TextAnchor.MiddleCenter;
        s.normal.textColor = s.hover.textColor = s.focused.textColor = s.active.textColor = new Color(1.0f, 1.0f, 1.0f, 0.6f);
        s.richText = true;
        windowStyle_SmallCenteredText = s;

        // Unselected Script Style
        s = new GUIStyle(EditorStyles.boldLabel);
        s.padding = new RectOffset(5, 0, 0, 0);
        s.fontSize = 13;
        s.fontStyle = FontStyle.Bold;
        s.alignment = TextAnchor.MiddleLeft;
        s.normal = s.hover = s.focused = s.active = s.onNormal;
        s.normal.background = s.hover.background = s.focused.background = s.active.background = MakeTexture(new Color(0.15f, 0.15f, 0.15f));
        s.active.background = s.focused.background = MakeTexture(new Color(0.15f, 0.15f, 0.15f));
        s.active.background = s.focused.background = MakeTexture(new Color(0.15f, 0.15f, 0.15f));
        unselectedScriptStyle = s;

        // Unselected Script Style
        s = new GUIStyle(EditorStyles.boldLabel);
        s.padding = new RectOffset(5, 0, 0, 0);
        s.fontSize = 13;
        s.fontStyle = FontStyle.Bold;
        s.alignment = TextAnchor.MiddleLeft;
        s.normal = s.hover = s.focused = s.active = s.onNormal;
        s.normal.background = s.hover.background = s.focused.background = s.active.background = MakeTexture(new Color(0.3f, 0.3f, 0.3f));
        s.active.background = s.focused.background = MakeTexture(new Color(0.3f, 0.3f, 0.3f));
        s.active.background = s.focused.background = MakeTexture(new Color(0.3f, 0.3f, 0.3f));
        unselectedScriptStyle2 = s; 

        // Selected Script Style
        s = new GUIStyle(EditorStyles.boldLabel);
        s.padding = new RectOffset(5, 0, 0, 0);
        s.fontSize = 13;
        s.fontStyle = FontStyle.Bold;
        s.alignment = TextAnchor.MiddleLeft;
        s.normal = s.hover = s.focused = s.active = s.onNormal;
        s.normal.background = s.hover.background = s.focused.background = s.active.background = MakeTexture(new Color(.17f, .36f, .53f, 1f));
        selectedScriptStyle = s;

        // 
        s = new GUIStyle(GUI.skin.button);
        s.fontSize = 20;
        s.fontStyle = FontStyle.Bold;
        s.normal = s.hover = s.focused = s.active = s.onNormal;
        s.normal.textColor = s.hover.textColor = s.focused.textColor = s.active.textColor = Color.white;
        s.normal.background = s.hover.background = s.focused.background = s.active.background = MakeTexture(new Color(1, 1, 1, 0.1f));
        s.hover.background = MakeTexture(new Color(1, 1, 1, 0.3f));
        s.active.background = s.focused.background = MakeTexture(new Color(1, 1, 1, 0.4f));
        s.hover.textColor = s.active.textColor = s.normal.textColor;
        s.normal.scaledBackgrounds = s.hover.scaledBackgrounds = s.active.scaledBackgrounds = null;
        windowStyle_AddButton = s;

        //
        s = new GUIStyle(GUI.skin.button);
        s.fontSize = 14;
        s.fontStyle = FontStyle.Bold;
        s.normal = s.hover = s.focused = s.active = s.onNormal;
        s.normal.textColor = s.hover.textColor = s.focused.textColor = s.active.textColor = new Color(0.9f, 0.9f, 0.9f);
        s.normal.background = s.hover.background = s.focused.background = s.active.background = MakeTexture(new Color(0.2f, 0.2f, 0.2f, 1.0f));
        s.hover.background = MakeTexture(new Color(1, 1, 1, 0.3f));
        s.active.background = s.focused.background = MakeTexture(new Color(1, 1, 1, 0.4f));
        s.hover.textColor = s.active.textColor = s.normal.textColor;
        s.normal.scaledBackgrounds = s.hover.scaledBackgrounds = s.active.scaledBackgrounds = null;
        windowStyle_AddButtonSmall = s;

        //
        s = new GUIStyle(GUI.skin.textField);
        s.normal = s.hover = s.focused = s.active = s.onNormal;
        s.normal.background = s.hover.background = s.focused.background = s.active.background = MakeTexture(new Color(0.2f, 0.2f, 0.2f, 1.0f));
          s.active.background = s.focused.background = MakeTexture(new Color(0.2f, 0.2f, 0.2f, 1.0f));
        s.hover.textColor = s.active.textColor = s.normal.textColor;
        s.alignment = TextAnchor.MiddleCenter;
        windowStyle_TextField = s;

        // Variable Nodes
        s = new GUIStyle(GUI.skin.button);
        s.fontSize = defaultFontSize;
        s.normal = s.hover = s.focused = s.active = s.onNormal;
        s.normal.textColor = s.hover.textColor = s.focused.textColor = s.active.textColor = new Color(0.5f, 0.5f, 1.0f);
        s.normal.background = s.hover.background = s.focused.background = s.active.background = MakeTexture(oddNodeColor);
        s.hover.textColor = s.active.textColor = s.normal.textColor;
        s.normal.scaledBackgrounds = s.hover.scaledBackgrounds = s.active.scaledBackgrounds = null;
        windowStyle_VariableText = s;

        // Value Nodes
        s = new GUIStyle(GUI.skin.button);
        s.fontSize = defaultFontSize;
        s.normal = s.hover = s.focused = s.active = s.onNormal;
        s.normal.textColor = s.hover.textColor = s.focused.textColor = s.active.textColor = new Color(0.1f, 1.0f, 0.5f);
        s.normal.background = s.hover.background = s.focused.background = s.active.background = MakeTexture(oddNodeColor);
        s.hover.textColor = s.active.textColor = s.normal.textColor;
        s.richText = true;
        s.normal.scaledBackgrounds = s.hover.scaledBackgrounds = s.active.scaledBackgrounds = null;
        windowStyle_ValueText = s;

        // Temp Nodes
        s = new GUIStyle(GUI.skin.button);
        s.fontSize = defaultFontSize;
        s.normal = s.hover = s.focused = s.active = s.onNormal;
        s.normal.textColor = s.hover.textColor = s.focused.textColor = s.active.textColor = new Color(1.0f, 0.5f, 0.5f);
        s.normal.background = s.hover.background = s.focused.background = s.active.background = MakeTexture(oddNodeColor);
        s.hover.textColor = s.active.textColor = s.normal.textColor;
        s.normal.scaledBackgrounds = s.hover.scaledBackgrounds = s.active.scaledBackgrounds = null;
        windowStyle_TempText = s;

        // Skin for odd nodes.
        GUIStyle odd = new GUIStyle(GUI.skin.button);
        odd.fontSize = defaultFontSize;
        odd.fontStyle = FontStyle.Bold;
        odd.normal = odd.hover = odd.focused = odd.active = odd.onNormal;
        odd.normal.textColor = odd.hover.textColor = odd.focused.textColor = odd.active.textColor = Color.white;
        odd.normal.background = odd.hover.background = odd.focused.background = odd.active.background = MakeTexture(oddNodeColor);
        odd.hover.textColor = odd.active.textColor = odd.normal.textColor;
        odd.normal.scaledBackgrounds = odd.hover.scaledBackgrounds = odd.active.scaledBackgrounds = null;
        windowStyle_OddNode = odd;

        // Skin for even nodes.
        GUIStyle even = new GUIStyle(GUI.skin.button);
        even.fontSize = defaultFontSize;
        even.fontStyle = FontStyle.Bold;
        even.normal = even.hover = even.focused = even.active = even.onNormal;
        even.normal.textColor = even.hover.textColor = even.focused.textColor = even.active.textColor = Color.white;
        even.normal.background = even.hover.background = even.focused.background = even.active.background = MakeTexture(evenNodeColor);
        even.hover.textColor = even.active.textColor = even.normal.textColor;
        even.normal.scaledBackgrounds = even.hover.scaledBackgrounds = even.active.scaledBackgrounds = null;
        windowStyle_EvenNode = even;

        // Skin for hovered nodes.
        GUIStyle hov = new GUIStyle(GUI.skin.button);
        hov.fontSize = defaultFontSize;
        hov.fontStyle = FontStyle.Bold;
        hov.normal = odd.hover = hov.focused = hov.active = hov.onNormal;
        hov.normal.textColor = hov.hover.textColor = hov.focused.textColor = hov.active.textColor = Color.white;
        hov.normal.background = hov.hover.background = hov.focused.background = hov.active.background = MakeTexture(hoveredNodeColor);
        hov.hover.textColor = hov.active.textColor = hov.normal.textColor;
        hov.normal.scaledBackgrounds = hov.hover.scaledBackgrounds = hov.active.scaledBackgrounds = null;
        windowStyle_HoveredNode = hov;
    }

    private static Texture2D MakeTexture(Color col)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }


    public float DrawHorizontalScriptPanel (float currentX, float currentY, float width)
    {
        //string[] toolbarOptions = new string[] { "Option 1", "Option 2", "Option 3" };
        //GUILayout.BeginArea(new Rect(area.x, area.y, 1000, 30));
        //int toolbarIndex = GUILayout.Toolbar(0, toolbarOptions); 
        //GUILayout.EndArea();

        EditorGUI.DrawRect(new Rect(currentX, currentY, width, 34), new Color(0.15f, 0.15f, 0.15f, 1));

        if (engine.scripts.Count == 0)
        {
            engine.scripts.Add(new VisualCodeScript("Main"));
        }
        if (engine.selectedScript == null) engine.selectedScript = engine.scripts[0];

        //return;

        Rect rect = new Rect(currentX + 2, currentY + 2, 0, 30);

        Event current = Event.current;
        for (int i = 0; i < engine.scripts.Count; i++)
        //for (int i = 0; i < 3; i++)
        {
            rect.width = LogicEngineEditor.windowStyle_BodyText.CalcSize(new GUIContent(engine.scripts[i].scriptName)).x + 31;

            //Rect rect = new Rect(area.x + i * (140 + 2) + 2, area.y + 2, 140, 30);
            if (engine.scripts[i].scriptName == engine.selectedScript.scriptName)
                GUI.Label(rect, "", selectedScriptStyle);
            else
                GUI.Label(rect, "", unselectedScriptStyle2);

            if (scriptIconSprite == null)
                scriptIconSprite = Resources.Load<Sprite>("Icons/Script");
            if (scriptIconSprite != null)
                GUI.DrawTexture(new Rect(rect.x + 3, rect.y + 5, 20, 20), scriptIconSprite.texture);



            Rect textRect = new Rect(rect);
            textRect.x += 25;
            textRect.width -= 25;
            GUI.Label(textRect, engine.scripts[i].scriptName, LogicEngineEditor.windowStyle_BodyText);

            if (rect.Contains(current.mousePosition))
            {

                if (current.type == EventType.MouseDown)
                {
                    engine.selectedNodes.Clear();
                    SetSelectedScript(engine.scripts[i]);
                    isDraggingScript = true;
                    scriptBeingDragged = engine.scripts[i];
                }
                if (current.type == EventType.ContextClick)
                {
                    engine.selectedScript = engine.scripts[i];
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Rename"), false, () => { RenameScript(engine.selectedScript); });
                    menu.AddItem(new GUIContent("Delete"), false, () => { DeleteScript(); });
                    menu.ShowAsContext(); 
                    current.Use();
                }
            }
            rect.x += rect.width + 2;
        }
        rect.width = 92;
        if (GUI.Button(rect, "Create New", unselectedScriptStyle2))
        {
            CreateScript();
        }

        return 34;
    }

    /// <summary>
    /// Draw the script panel for the logic engine.
    /// </summary>
    public void DrawScriptPanel(Rect area)
    {
        EditorGUI.DrawRect(area, new Color(0f, 0f, 0f)); 

        float totalHeight = area.height;
        bool needToDrawScriptDragGuide = false;
        Rect scriptDragGuideLocation = new Rect();
        bool dragIsInTopHalf = true;



        // Draw the header
        area.height = 25;
        Rect labelRect = new Rect(area);
        labelRect.x += 5;
        EditorGUI.DrawRect(area, new Color(0.1f, 0.1f, 0.1f, 1));

        Rect bodyStart = new Rect(area);
        bodyStart.y += 25;

        GUIStyle transparentBtn = new GUIStyle(GUI.skin.box);

        // Draw the button that allows users to create scripts.
        if (GUI.Button(new Rect(area.xMax - 25, area.y - 1, 25, 24), "+", windowStyle_AddButton))
        {
            CreateScript();
        }

        // Draw the title Text
        EditorGUI.LabelField(labelRect, "Logic", windowStyle_HeaderText);

        // Draw the background panel
        area.y += area.height;
        area.height = totalHeight - area.height;
        //EditorGUI.DrawRect(area, new Color(0.5f, 0.5f, 0.5f, .2f));
        EditorGUI.DrawRect(area, new Color(0.3f, 0.3f, 0.3f));
        //bodyStart.y += 1;
        bodyStart.height = 30;
        for (int i = 0; i < engine.scripts.Count; i++)
        {
            if (engine.scripts[i].scriptName == engine.selectedScript.scriptName)
                GUI.Label(bodyStart, engine.scripts[i].scriptName, selectedScriptStyle);
            else
                GUI.Label(bodyStart, engine.scripts[i].scriptName, unselectedScriptStyle); 



            Event current = Event.current;
            if (bodyStart.Contains(current.mousePosition))
            {
                if (isDraggingScript && scriptBeingDragged != engine.scripts[i])
                {
                    Rect topHalf = new Rect(bodyStart);
                    topHalf.height = bodyStart.height / 2.0f;

                    Rect lineRect = new Rect(bodyStart);
                    lineRect.height = 2;

                    

                    if (!topHalf.Contains(current.mousePosition))
                    {
                        dragIsInTopHalf = false;
                        lineRect.y += bodyStart.height;
                    }
                    needToDrawScriptDragGuide = true;
                    scriptDragGuideLocation = lineRect;
                }

                if (current.type == EventType.MouseUp && isDraggingScript)
                {
                    VisualCodeScript target = engine.scripts[i];
                    if (scriptBeingDragged != target)
                    {
                        
                        engine.scripts.Remove(scriptBeingDragged);
                        if (dragIsInTopHalf)
                            engine.scripts.Insert(engine.scripts.IndexOf(target), scriptBeingDragged);
                        else
                            engine.scripts.Insert(engine.scripts.IndexOf(target) + 1, scriptBeingDragged);

                    }
                    isDraggingScript = false;
                    scriptBeingDragged = null;
                }

                if (current.type == EventType.MouseDown)
                {
                    engine.selectedNodes.Clear();
                    SetSelectedScript(engine.scripts[i]);
                    isDraggingScript = true;
                    scriptBeingDragged = engine.scripts[i];
                }
                if (current.type == EventType.ContextClick)
                {
                    engine.selectedScript = engine.scripts[i];
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Rename"), false, () => { RenameScript(engine.selectedScript); });
                    menu.AddItem(new GUIContent("Delete"), false, () => { DeleteScript(); });
                    menu.ShowAsContext();
                    current.Use();
                }
            }
            bodyStart.y += 31;
        }

        Rect unfocusRect = new Rect(area);
        unfocusRect.y = bodyStart.y;
        unfocusRect.height = (area.height - bodyStart.y + area.y);

 
        if (needToDrawScriptDragGuide)
            EditorGUI.DrawRect(scriptDragGuideLocation, Color.grey);

        if (GUI.Button(unfocusRect, "", GUIStyle.none))
            GUI.FocusControl(null);
    }



    /// <summary>
    /// Create a new script for the current engine.
    /// </summary>
    public void CreateScript()
    {
        string scriptName = "Script " + Random.Range(10000, 99999);
        if ((scriptName = EditorInputDialog.Show("Enter Script Name", "", scriptName)) != null)
        {
            engine.scripts.Add(new VisualCodeScript(scriptName));
            engine.selectedScript = engine.scripts[engine.scripts.Count - 1];
            EditorUtility.SetDirty(engineHandler.GetData());

            
            engine.selectedNodes.Clear();
        }
    }

    /// <summary>
    /// Prompt for a rename of the given script.
    /// </summary>
    public void RenameScript(VisualCodeScript script)
    {
        string newText = EditorInputDialog.Show("Enter Script Name", "", script.scriptName);
        if (newText != null)
        {
            script.scriptName = newText;
            EditorUtility.SetDirty(engineHandler.GetData());
            AssetDatabase.SaveAssetIfDirty(engineHandler.GetData());
        }
    }

    /// <summary>
    /// Delete the selected script from the given engine.
    /// </summary>
    public void DeleteScript()
    {
        engine.scripts.Remove(engine.selectedScript);
        if (engine.scripts.Count == 0)
            engine.scripts.Add(new VisualCodeScript("Main"));
        engine.selectedScript = engine.scripts[0];
        engine.selectedNodes.Clear();
        EditorUtility.SetDirty(engineHandler.GetData());
        AssetDatabase.SaveAssetIfDirty(engineHandler.GetData());
    }



    public static Vector2 mousePosition;



    private List<GeneralNode> GetNodeList (GeneralNode node)
    {
        List<GeneralNode> nodeList = null;
        if (engine.selectedScript.eventNodes.Contains(node))
            nodeList = engine.selectedScript.eventNodes;
        else if (engine.selectedScript.conditionNodes.Contains(node))
            nodeList = engine.selectedScript.conditionNodes;
        else if (engine.selectedScript.actionNodes.Contains(node)) 
            nodeList = engine.selectedScript.actionNodes;
        return nodeList;
    }

    private List<GeneralNode> GetAllChildNodes (GeneralNode node)
    {
        List<GeneralNode> childNodes = new List<GeneralNode>();
        List<GeneralNode> nodeList = GetNodeList(node);
        int startID = nodeList.IndexOf(node) + 1;
        for (int i = startID; i < nodeList.Count; i++)
        {
            if (nodeList[i].indent > node.indent)
                childNodes.Add(nodeList[i]);
            else
                break;
        }
        return childNodes;
    }

    private bool IsParentChild (GeneralNode child, GeneralNode parent)
    {
        List<GeneralNode> nodeList = GetNodeList(parent);
        int startID = nodeList.IndexOf(parent) + 1;
        for (int i = startID; i < nodeList.Count; i++)
        {
            if (nodeList[i].indent <= parent.indent)
            {
                return false;
            }
            else if (nodeList[i] == child)
            {
                return true;
            }
        }
        return false;
    }

    private void MoveSelectedToNode (GeneralNode moveToNode, bool insertBefore)
    {
        if (GetNodeList(moveToNode) != GetNodeList(engine.selectedNodes[0])) return;
        if (engine.selectedNodes.Count == 0) return;
        if (IsParentChild(moveToNode, engine.selectedNodes[0])) return;

        GeneralNode selectedNode = engine.selectedNodes[0];
        List<GeneralNode> nodeList = GetNodeList(selectedNode);
        List<GeneralNode> nodesToMove = new List<GeneralNode> { selectedNode };
        nodesToMove.AddRange(GetAllChildNodes(selectedNode));

        // Update the node indents.
        int change = moveToNode.indent - selectedNode.indent;
        if (!insertBefore && moveToNode is NestingActionNode) change++;
        foreach (GeneralNode node in nodesToMove)
            node.indent += change;

        // Remove the nodes from the current list.
        foreach (GeneralNode node in nodesToMove)
            nodeList.Remove(node);

        // Insert the nodes at the correct location.
        int insertID = nodeList.IndexOf(moveToNode);
        if (!insertBefore) insertID++;
        nodeList.InsertRange(insertID, nodesToMove);
    }

    private void DeleteAllSelectedNodes()
    {
        // Add the node and all children.
        HashSet<GeneralNode> toDeleteSet = new HashSet<GeneralNode>(engine.selectedNodes);
        foreach (GeneralNode node in engine.selectedNodes)
            toDeleteSet.AddRange(GetAllChildNodes(node));

        // Delete all the nodes.
        foreach (GeneralNode node in toDeleteSet)
        {
            engine.selectedScript.eventNodes.Remove(node);
            engine.selectedScript.conditionNodes.Remove(node);
            engine.selectedScript.actionNodes.Remove(node);
        }
        EditorUtility.SetDirty(engineHandler.GetData());
    }

    public void AddNode(List<GeneralNode> nodeList, GeneralNode nodeToAdd)
    {
        GeneralNode copy = nodeToAdd.Copy();
        nodeList.Add(copy);
        engine.selectedNodes = new List<GeneralNode>() { copy };
        EditorUtility.SetDirty(engineHandler.GetData());
    } 

    public void AddNode(List<GeneralNode> nodeList, GeneralNode nodeToAdd, int insertID, int indent)
    {
        GeneralNode node = nodeToAdd.Copy();
        node.indent = indent;
        nodeList.Insert(insertID, node);
        EditorUtility.SetDirty(engineHandler.GetData());
    }

    public static void BuildAllData()
    {
        allNodes.Clear();
        BuildStyles();
        BuildValueNodes();
    }

    public void Process ()
    {
        if (allNodes.Count == 0 || windowStyle_AddButton == null || windowStyle_AddButton.normal == null || windowStyle_AddButton.normal.background == null || windowStyle_AddButton.normal.background.width > 1)
            BuildAllData();

        mousePosition = Event.current.mousePosition;

        if (Event.current.isKey && Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Delete)
        {
            DeleteAllSelectedNodes();
        }

        if (engine == null)
            Debug.Log("Engine is null");

        if (engine.selectedScript == null)
        {
            if (engine.scripts.Count == 0)
            {
                engine.scripts.Add(new VisualCodeScript("Main"));
            }
            engine.selectedScript = engine.scripts[0];
        }
    }

    public void DrawNodes ()
    {
        float width = window.position.width - 180;
        float currentX = 170;
        float currentY = 10;

        //DrawScriptPanel(new Rect(10, 10, 150, window.position.height - 20));

        hoveredNodeDepth = -1;
        hoveredNode = null;


        Color eventColor = new Color(0.2f, 0.1f, 0.1f, 1);
        Color conditionColor = new Color(0.1f, 0.2f, 0.1f, 1);
        Color actionColor = new Color(0.1f, 0.1f, 0.2f, 1);
        currentY += DrawHorizontalScriptPanel(currentX, currentY, width);
        currentY += 3;
        currentY += DrawNodeList(currentX, currentY, width, "Events: ", engine.selectedScript.eventNodes, eventColor, typeof(EventNode));
        currentY += 10;
        currentY += DrawNodeList(currentX, currentY, width, "Conditions: ", engine.selectedScript.conditionNodes, conditionColor, typeof(BoolNode));
        currentY += 10;
        currentY += DrawNodeList(currentX, currentY, width, "Actions: ", engine.selectedScript.actionNodes, actionColor, typeof(ActionNode));
        currentY += 10;
    }

    public float contentHeight = 0;
    public Vector2 scrollPos;

    public void DrawNodes(Rect rect)
    {
        //float width = window.position.width - 180;
        //float currentX = 170;
        //float currentY = 10;
        float width = rect.width;
        float currentX = rect.x;
        float currentY = rect.y;

        //DrawScriptPanel(new Rect(10, 10, 150, window.position.height - 20));

        if (engine.selectedScript == null)
            engine.selectedScript = engine.scripts[0];

        float maxWidth = 0;
        foreach (GeneralNode node in engine.selectedScript.actionNodes)
        {
            maxWidth = Mathf.Max(maxWidth, GetDrawWidth(node) + (node.indent-1) * indentWidth); 
        }


        scrollPos = GUI.BeginScrollView(rect, scrollPos, new Rect(rect.x, rect.y, Mathf.Max(maxWidth + 50, rect.width - 15), contentHeight));

        if (contentHeight > rect.height)
            width -= 10;

        Event e = Event.current;

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.C && (e.control || e.command))
        {
            CopySelectedNode();
            e.Use();
        }
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.V && (e.control || e.command))
        {
            PasteNodesInClipboard();
            e.Use();
        }

        Color eventColor = new Color(0.2f, 0.1f, 0.1f, 1);
        Color conditionColor = new Color(0.1f, 0.2f, 0.1f, 1);
        Color actionColor = new Color(0.1f, 0.1f, 0.2f, 1);
        currentY += DrawHorizontalScriptPanel(currentX, currentY, width);
        currentY += 2;
        currentY += DrawNodeList(currentX, currentY, width, "Events: ", engine.selectedScript.eventNodes, eventColor, typeof(EventNode));
        currentY += 2;
        currentY += DrawNodeList(currentX, currentY, width, "Conditions: ", engine.selectedScript.conditionNodes, conditionColor, typeof(BoolNode));
        currentY += 2;
        currentY += DrawNodeList(currentX, currentY, width, "Actions: ", engine.selectedScript.actionNodes, actionColor, typeof(ActionNode));
        currentY += 2;

        GUI.EndScrollView();
        contentHeight = currentY;
    }

    private int MaxChildDepth (GeneralNode node)
    {
        if (node.returnType != GeneralNode.ReturnType.Function)
        {
            return 1;
        }
        int depth = 0;
        for (int i = 0; i < node.functionEvaluators.Length; i++)
        {
            depth = Mathf.Max(depth, MaxChildDepth(node.functionEvaluators[i]));          
        }
        return depth + 1;
    }

    public float DrawNodeList(float x, float y, float width, string title, List<GeneralNode> nodes, Color headerColor, Type type)
    {

        Color[] colors = new Color[2] { new Color(0.5f, 0.5f, 0.5f, .2f), new Color(0.5f, 0.5f, 0.5f, .1f) };
        //float itemHeight = 40;

        //Rect r = new Rect(x, y, width, 0);
        Rect r = new Rect(x, y, width + scrollPos.x, 0); 

        // Draw the header
        r.height = 25;
        Rect labelRect = new Rect(r);
        labelRect.x += 5;
        EditorGUI.DrawRect(r, headerColor);
        EditorGUI.LabelField(labelRect, title, windowStyle_HeaderText);


        if (GUI.Button(new Rect(r.xMax - 25, r.y - 1, 25, 24), "+", windowStyle_AddButton))
        {
            GenericMenu menu = new GenericMenu();
            foreach (GeneralNode node in allNodes)
            {
                if (type.IsAssignableFrom(node.GetType()))
                {
                    menu.AddItem(new GUIContent(node.Template.functionDescription), false, () => { AddNode(nodes, node); });
                }
            }
            menu.ShowAsContext();
        }
        r.y += r.height;

        //EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.Link);
        //EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.ArrowPlus);

        bool needToDrawDragGuide = false;
        
        Rect dragGuideLocation = new Rect();
        

        // Draw each node.
        Event current = Event.current;
        int currNode = 0;
        GeneralNode hovered = null;

        

        for (int i = 0; i < nodes.Count; i++)
        {
            r.height = Mathf.Max(itemHeight, (MaxChildDepth(nodes[i]) + 1) * heightIncreasePerDepth);
            
            Rect edge = new Rect(r);
            edge.x += nodes[i].indent * indentWidth;
            edge.width -= nodes[i].indent * indentWidth;
            if (engine.selectedNodes.Contains(nodes[i]))
                EditorGUI.DrawRect(edge, (new Color(.17f, .36f, .53f, 1f)));
            else
                EditorGUI.DrawRect (edge, colors[currNode % 2]);
            //nodes[i].Draw(window, edge);

            Rect iconRect = new Rect(edge.x + 5, edge.y + (r.height - 20) / 2.0f, 20, 20);
            //edge.x += 40;

            if (!icons.ContainsKey(nodes[i].nodeIcon)) 
            {
                if (nodes[i].nodeIcon == "")
                {
                    icons.Add(nodes[i].nodeIcon, (Resources.Load<Sprite>("Icons/DefaultIcon")).texture); 
                    //icons.Add(nodes[i].nodeIcon, Resources.Load("Icons/" +  EditorGUIUtility.FindTexture($"{defaultIconPath}/DefaultIcon.png"));
                }
                else
                {
                    //icons.Add(nodes[i].nodeIcon, EditorGUIUtility.FindTexture($"{defaultIconPath}/{nodes[i].nodeIcon}.png"));
                    Sprite sprite = Resources.Load<Sprite>($"Icons/{nodes[i].nodeIcon}");
                    if (sprite != null)
                    {
                        icons.Add(nodes[i].nodeIcon, sprite.texture);
                    }
                    else
                    {
                        Debug.Log(nodes[i].nodeIcon + " Can't be found!");
                    }

                    

                    //icons.Add(nodes[i].nodeIcon, EditorGUIUtility.FindTexture($"{defaultIconPath}/{nodes[i].nodeIcon}.png"));
                }
            }

            //if (nodes[i].nodeIcon == "")
            //    icon = EditorGUIUtility.FindTexture($"{defaultIconPath}/DefaultIcon.png");
            //else
            //    icon = EditorGUIUtility.FindTexture($"{defaultIconPath}/{nodes[i].nodeIcon}.png");

            GUI.DrawTexture(iconRect, icons[nodes[i].nodeIcon]);

            //EditorGUI.DrawTextureTransparent(iconRect, icon, ScaleMode.ScaleToFit, 0, 1, UnityEngine.Rendering.ColorWriteMask.All);

            //EditorGUI.DrawPreviewTexture(iconRect, icon);

            Rect newEdge = new Rect(edge);
            newEdge.x += 25;
            Draw(newEdge, nodes[i]);
            


            

            

            if (edge.Contains(current.mousePosition))
            {
                hovered = nodes[i];
                if (isDragging && (engine.selectedNodes.Count > 1 || engine.selectedNodes[0] != nodes[i]))
                {
                    Rect topHalf = new Rect(edge);
                    topHalf.height = edge.height / 2.0f;

                    Rect dragGuide = new Rect(edge);
                    dragGuide.height = 2;

                    if (topHalf.Contains(current.mousePosition))
                    {
                    }
                    else
                    {
                        dragGuide.y += 50;
                    }

                    if (current.type == EventType.MouseUp && isDragging)
                    {
                        isDragging = false;
                        MoveSelectedToNode(nodes[i], topHalf.Contains(current.mousePosition));
                    }

                    //Rect dragGuide = new Rect(edge);
                    //dragGuide.height = 2;
                    needToDrawDragGuide = true;
                    dragGuideLocation = dragGuide;
                    
                }

                /*
                if (current.type == EventType.MouseUp && isDragging)
                {
                    isDragging = false;
                    if (dragStartedAt != nodes[i])
                    {
                        // Store the target.
                        GeneralNode target = nodes[i];

                        // If one of the selected items is the drop target, remove it from the list.
                        engine.selectedNodes.Remove(nodes[i]);

                        // Remove all items from the node list.
                        foreach (GeneralNode node in engine.selectedNodes)
                        {
                            nodes.Remove(node);
                        }

                        // Find the ID of the target again now that items have been removed.
                        int insertID = nodes.IndexOf(target);

                        Rect above = new Rect(edge);
                        above.height = edge.height / 2.0f;
                        if (above.Contains(current.mousePosition))
                        {
                            nodes.InsertRange(insertID, engine.selectedNodes);
                            foreach (GeneralNode node in engine.selectedNodes)
                            {
                                node.indent = nodes[insertID].indent;
                            }
                        }
                        else
                        {
                            nodes.InsertRange(insertID + 1, engine.selectedNodes);
                            foreach (GeneralNode node in engine.selectedNodes)
                            {
                                node.indent = (nodes[insertID].CanHaveChildren() ? node.indent = nodes[insertID].indent + 1 : node.indent = nodes[insertID].indent);
                            }
                        }
                    }
                }
                */
                if (current.type == EventType.MouseDown)
                {
                    GUI.FocusControl(null);
                    isDragging = true;
                    dragStartedAt = nodes[i];
                    if (current.control)
                    {
                        if (engine.selectedNodes.Contains(nodes[i]))
                            engine.selectedNodes.Remove(nodes[i]);
                        else
                            engine.selectedNodes.Add(nodes[i]);
                    }
                    else
                    {
                        engine.selectedNodes.Clear();
                        engine.selectedNodes.Add(nodes[i]);
                        //DragAndDrop.StartDrag("NodeDragging");

                        //DragAndDrop.SetGenericData("NodeDragging", selectedNodes);
                        //DragAndDrop.visualMode = DragAndDropVisualMode.Move;

                    }
                }
                if (current.type == EventType.ContextClick)
                {
                    GUI.FocusControl(null);
                    engine.selectedNodes.Clear();
                    engine.selectedNodes.Add(nodes[i]);
                    window.Repaint();
                    GenericMenu menu = new GenericMenu();
                    Vector2 temp = current.mousePosition + window.position.position;
                    //menu.AddItem(new GUIContent("Edit"), false, () => {
                    //    EditNode(engine.selectedNodes[0]);
                    //});
                    menu.AddItem(new GUIContent("Copy"), false, () => { CopySelectedNode(); });
                    menu.AddItem(new GUIContent("Delete"), false, () => { DeleteAllSelectedNodes(); });
                    menu.ShowAsContext();
                    current.Use();
                }

            }
            

            r.y += r.height;
            currNode++;

            if (nodes[i] is NestingActionNode && (i == nodes.Count - 1 || nodes[i + 1].indent != nodes[i].indent + 1))
            {
                edge.y += r.height;
                edge.x += indentWidth;
                edge.width -= indentWidth;
                EditorGUI.DrawRect(edge, colors[currNode % 2]);
                labelRect = new Rect(edge);
                labelRect.x += 15;

                EditorGUI.LabelField(labelRect, "No actions specified", windowStyle_SmallText);
                currNode++;
                r.y += r.height;
            }
        }

        if (current.type == EventType.MouseDown)
        {
            //isDragging = false;
        }

        if (current.type == EventType.MouseUp && isDragging && nodes.Contains(engine.selectedNodes[0]))
        {
            isDragging = false;
        }

        if (needToDrawDragGuide)
            EditorGUI.DrawRect(dragGuideLocation, Color.grey);

        // If no nodes exist, draw a temporary node.
        if (nodes.Count == 0)
        {
            r.height = itemHeight;
            labelRect = new Rect(r);
            labelRect.x += 15;
            EditorGUI.DrawRect(r, colors[0]);
            EditorGUI.LabelField(labelRect, "None specified", windowStyle_SmallText);
            r.y += r.height;
        }

        

        Rect focusRect = new Rect(x, y, width, r.y);
        Event current2 = Event.current;
        if (focusRect.Contains(current2.mousePosition) && current2.type == EventType.MouseDown)
            GUI.FocusControl(null);

        return r.y - y;
    }

    private void PasteNodesInClipboard ()
    {
        if (nodesInClipboard.Count == 0) return;

        foreach (GeneralNode node in nodesInClipboard)
        {
            AddNode(engine.selectedScript.actionNodes, node);
            //engine.selectedNodes = new List<GeneralNode>() { copy }; 
        }
    }

    private void CopySelectedNode ()
    {
        // No nodes to copy.
        if (engine.selectedNodes.Count == 0) return;

        // Not an action node or 
        if (!engine.selectedScript.actionNodes.Contains(engine.selectedNodes[0])) return;

        nodesInClipboard = new List<GeneralNode>();

        GeneralNode nodeToCopy = engine.selectedNodes[0];
        int offsetChange = 0;
        bool foundNode = false;
        foreach (GeneralNode node in engine.selectedScript.actionNodes)
        {
            if (foundNode && node.indent > nodeToCopy.indent)
            {
                GeneralNode copy = node.Copy();
                copy.indent += offsetChange;
                nodesInClipboard.Add(copy);
            }
            else if (foundNode && node.indent <= nodeToCopy.indent)
            {
                break;
            }
            else if (node == nodeToCopy)
            {
                foundNode = true;
                GeneralNode copy = node.Copy();
                offsetChange = -copy.indent;
                copy.indent = 0;
                nodesInClipboard.Add(copy);
            }
        }
    }

    public virtual float GetDrawWidth(GeneralNode node)
    {
        GUIContent content = null;
        if (node.returnType == GeneralNode.ReturnType.Function)
        {
            float width = 0;
            string[] parts = node.Template.functionDynamicDescription.Split('$');
            foreach (string part in parts)
            {
                if (part.Length > 0)
                {
                    content = new GUIContent(part);
                    width += windowStyle_BaseText.CalcSize(content).x;
                }
            }
            foreach (GeneralNode n in node.functionEvaluators)
            {
                width += GetDrawWidth(n);
            }
            return width + 10;
        }
        if (node.returnType == GeneralNode.ReturnType.Value) { content = new GUIContent(node.ToString()); }
        if (node.returnType == GeneralNode.ReturnType.Temp) { content = new GUIContent(node.GetTempName()); }
        if (node.returnType == GeneralNode.ReturnType.Preset) { content = new GUIContent(node.presetName); }
        return windowStyle_HoveredNode.CalcSize(content).x;
    }

    public virtual float Draw(Rect rect, GeneralNode node, int depth = 1)
    {
        // Determine the correct style for the node.
        GUIStyle style = null;
        if (node == hoveredNode)
            style = windowStyle_HoveredNode;
        else if (depth % 2 == 1)
            style = windowStyle_OddNode;
        else
            style = windowStyle_EvenNode;

        // Determine the correct width for the node.
        Rect temp = new Rect(rect);
        temp.width = GetDrawWidth(node);
        temp.height -= depth * 2;
        temp.y += depth * 1;

        GUIContent content = null;
        if (node is EventNode || (node.returnType == GeneralNode.ReturnType.Function && (node.functionEvaluators.Length > 0 || depth == 1)))


        //if (node is EventNode || (node.returnType == GeneralNode.ReturnType.Function))
        {
            if (depth == 1)
            {
                temp.y -= 1;
            }

            // Draw the function node button (but not if it is a base node).
            if (depth != 1)
            {
                EditorGUIUtility.AddCursorRect(temp, MouseCursor.Link);

                // Check to see if the mouse is currently hovering over this node. If so,
                // it should be drawn in a different colour.
                if (temp.Contains(Event.current.mousePosition))
                {
                    if (hoveredNode == null || depth >= hoveredNodeDepth)
                    {
                        hoveredNode = node;
                        hoveredNodeDepth = depth;
                    }
                }
                else
                {
                    if (node == hoveredNode)
                    {
                        hoveredNodeDepth = -1;
                        hoveredNode = null;
                    }
                }

                // Draw the button to allow for edits.    
                if (hoveredNode == node)
                {
                    if (GUI.Button(temp, "", style))
                    {
                        SetValueEditor.OpenWindow(window,  window.position.min - scrollPos + new Vector2(temp.xMin, temp.yMin + temp.height * 1.5f), node, engineHandler);
                    }
                }
                else
                {
                    GUI.Label(temp, "", style);
                }
            }

            // If the node can have children actions, create the add button.
            if (node.CanHaveChildren())
            {
                if (GUI.Button(new Rect(rect.x + rect.width - 75, rect.y - 1, 49, 49), "+", windowStyle_AddButton))
                {
                    GenericMenu menu = new GenericMenu();
                    foreach (GeneralNode n in allNodes)
                    {
                        if (n is ActionNode)
                        {
                            menu.AddItem(new GUIContent(n.Template.functionDescription), false, () => { AddNodeAsChild(node, n); });
                        }
                    }
                    menu.ShowAsContext();
                }
            }

            // Draw each part of the function node.
            temp.x += 5;
            string[] parts = node.Template.functionDynamicDescription.Split('$');
            int next = 0;
            foreach (string part in parts)
            {
                content = new GUIContent(part);
                temp.width = windowStyle_BaseText.CalcSize(content).x;
                GUI.Label(temp, part, windowStyle_BaseText);
                temp.x += temp.width;
                if (next < node.functionEvaluators.Length && node.functionEvaluators.Length > 0)
                {
                    // temp.x += node.functionEvaluators[next].Draw(window, temp, depth + 1);

                    temp.x += Draw(temp, node.functionEvaluators[next], depth + 1);
                    next++;
                }
            }
            return GetDrawWidth(node);
        }

        // Assign the appropriate text and color for the node.
        if (node.returnType == GeneralNode.ReturnType.Function && node.functionEvaluators.Length == 0)
            temp.width = CalcContentWidth(style, content = new GUIContent(node.Template.functionDynamicDescription), varColor);
        else if (node.returnType == GeneralNode.ReturnType.Value)
            temp.width = CalcContentWidth(style, content = new GUIContent(node.ToString()), valueColor);
        else if (node.returnType == GeneralNode.ReturnType.Preset)
            temp.width = CalcContentWidth(style, content = new GUIContent(node.presetName), presetColor);
        else if (node.returnType == GeneralNode.ReturnType.Temp)
            temp.width = CalcContentWidth(style, content = new GUIContent(node.GetTempName()), tempColor);
        EditorGUIUtility.AddCursorRect(temp, MouseCursor.Link);

        style.richText = true;

        // Check to see if the mouse is currently hovering over this node. If so,
        // it should be drawn in a different colour.
        if (temp.Contains(Event.current.mousePosition))
        {
            if (hoveredNode == null || depth >= hoveredNodeDepth)
            {
                hoveredNode = node;
                hoveredNodeDepth = depth;
            }
        }
        else
        {
            if (node == hoveredNode)
            {
                hoveredNodeDepth = -1;
                hoveredNode = null;
            }
        }

        // Draw the node as a button (if it is hovered), or a label which looks like a button if not.
        // This is needed to get correct clicks on the window. 
        if (hoveredNode == node)
        {
            if (GUI.Button(temp, content, style))
            {
                SetValueEditor.OpenWindow(window, window.position.min - scrollPos + new Vector2(temp.xMin, temp.yMin + temp.height * 1.5f), node, engineHandler);
            }
        }
        else
        {
            GUI.Label(temp, content, style);
        }

        


        return temp.width;


    }

    private float CalcContentWidth(GUIStyle style, GUIContent content, Color color)
    {
        style.normal.textColor = style.hover.textColor = style.focused.textColor = style.active.textColor = color;
        return style.CalcSize(content).x;
    }

    public void AddNodeAsChild(GeneralNode node, GeneralNode childTemplate)
    {
        int insertID = -1;
        GeneralNode child = childTemplate.Copy();
        child.indent = node.indent + 1;
        int startID = engine.selectedScript.actionNodes.IndexOf(node);
        for (int i = startID + 1; i < engine.selectedScript.actionNodes.Count; i++)
        {
            GeneralNode toCompare = engine.selectedScript.actionNodes[i];
            if (toCompare.indent <= node.indent)
            {
                insertID = i;
                break;
            }
        }
        if (insertID >= 0)
            engine.selectedScript.actionNodes.Insert(insertID, child);
        else
            engine.selectedScript.actionNodes.Add(child);
        EditorUtility.SetDirty(engineHandler.GetData());
    }
}
