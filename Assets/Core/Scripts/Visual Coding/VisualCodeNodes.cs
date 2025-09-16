using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class GeneralNode
{
    public enum ReturnType { Value, Variable, Function, Temp, Preset, None }
    public ReturnType returnType = ReturnType.Value;

    // Temporary Evaluator
    public string tempName;
    
    // Function Evaluators
    public string functionName;
    public string functionDescription;
    public string functionDynamicDescription;
    [SerializeReference] public GeneralNode[] functionEvaluators = new GeneralNode[] { };

    [SerializeReference] 
    public GeneralNode parentNode = null;
    public int parentNodeID = -1;

    public GeneralNode Template
    {
        get
        {
            if (functionName == null || functionName.Length == 0) return null;
            if (!LogicEngine.nodeTemplates.ContainsKey(functionName))
            {
                return null;
            }
            return LogicEngine.nodeTemplates[functionName];
        }
    }

    public int indent = 0;

    public string presetName;
    public string variableName;
    public string stringSuffix = "";
    public string nodeIcon = "";
    public bool allowValue = true;
    public bool allowPreset = true;
    public bool allowFunction = true;

    private const string MISSING_VALUE = "Missing Value";

    public virtual string DefaultTemporaryNodeLabel
    {
        get { return "Temp"; }
    }

    public GeneralNode() {
    
    }

    /// <summary>
    /// Returns the suffix for this node. Note that if this is one of the template nodes, it will instead
    /// query the template. This is done to allow the templates to be updated and have the changes
    /// propagated to existing visual code nodes.
    /// </summary>
    public string GetStringSuffix ()
    {
        if (parentNode == null) return stringSuffix; 
        return LogicEngine.nodeTemplates[parentNode.functionName].functionEvaluators[parentNodeID].stringSuffix;
    }

    /// <summary>
    /// Returns the temporary name for this node. Note that if this is one of the template nodes, it will instead
    /// query the template. This is done to allow the templates to be updated and have the changes
    /// propagated to existing visual code nodes.
    /// </summary>
    public string GetTempName ()
    {
        if (parentNode == null)
        {
            if (tempName != null && tempName.Length > 0) 
                return tempName;
            else
                return DefaultTemporaryNodeLabel;
        }
        if (!LogicEngine.nodeTemplates.ContainsKey(parentNode.functionName)) Debug.Log($"Error: {parentNode.functionName} doesn't exist.");

        string templateTemporaryName = LogicEngine.nodeTemplates[parentNode.functionName].functionEvaluators[parentNodeID].tempName;

        if (templateTemporaryName == null || templateTemporaryName.Length == 0)
            templateTemporaryName = LogicEngine.nodeTemplates[parentNode.functionName].functionEvaluators[parentNodeID].DefaultTemporaryNodeLabel;
        return templateTemporaryName;
    }

    /// <summary>
    /// Returns whether a specific value is a valid option for this node. Note that if this is 
    /// one of the template nodes, it will instead query the template. This is done to allow the 
    /// templates to be updated and have the changes propagated to existing visual code nodes.
    /// </summary>
    public bool GetAllowValue ()
    {
        if (parentNode == null) return allowValue;
        if (!LogicEngine.nodeTemplates.ContainsKey(parentNode.functionName)) Debug.Log($"Error: {parentNode.functionName} doesn't exist.");
        return LogicEngine.nodeTemplates[parentNode.functionName].functionEvaluators[parentNodeID].allowValue;
    }

    /// <summary>
    /// Returns whether a preset is a valid option for this node. Note that if this is 
    /// one of the template nodes, it will instead query the template. This is done to allow the 
    /// templates to be updated and have the changes propagated to existing visual code nodes.
    /// </summary>
    public bool GetAllowPreset ()
    {
        if (parentNode == null) return allowPreset;
        if (!LogicEngine.nodeTemplates.ContainsKey(parentNode.functionName)) Debug.Log($"Error: {parentNode.functionName} doesn't exist.");
        return LogicEngine.nodeTemplates[parentNode.functionName].functionEvaluators[parentNodeID].allowPreset;
    }

    /// <summary>
    /// Returns whether a function is a valid option for this node. Note that if this is 
    /// one of the template nodes, it will instead query the template. This is done to allow the 
    /// templates to be updated and have the changes propagated to existing visual code nodes.
    /// </summary>
    public bool GetAllowFunction ()
    {
        if (parentNode == null) return allowFunction;
        if (!LogicEngine.nodeTemplates.ContainsKey(parentNode.functionName)) Debug.Log($"Error: {parentNode.functionName} doesn't exist.");
        return LogicEngine.nodeTemplates[parentNode.functionName].functionEvaluators[parentNodeID].allowFunction;
    }

    /// <summary>
    /// Returns true if this node can have child nodes, otherwise false.
    /// </summary>
    public virtual bool CanHaveChildren ()
    {
        return false;
    }
    
    /// <summary>
    /// Returns true if the user is currently hovering over this *exact* node, otherwise false.
    /// Note: If the user is hovering over a child of this node, it will return false.
    /// </summary>
    public virtual bool IsHovered (Rect parent)
    {
        return (parent.Contains(Event.current.mousePosition));
    }

    public override string ToString()
    {
        if (GetValue() == null)
        {
            //returnType = ReturnType.Temp;
            return "Undefined";
        }
        else
        {
            if (GetValue() is string)
            {
                string text = GetValue().ToString();
                if (text.Length < 70)
                    return text + GetStringSuffix();
                else
                    return text.Substring(0, 66) + GetStringSuffix() + " ...";
            }

            else if (GetValue() is Color)
            {
                string text = $"#{ColorUtility.ToHtmlStringRGB((Color)GetValue())}";
                return $"<color={text}>{text}</color>";
            }

            else if (GetValue() is Object)
            {
                Object obj = (Object)GetValue();
                if (obj == null)
                {
                    returnType = ReturnType.Temp;
                    return MISSING_VALUE;
                }
                else
                {
                    return obj.name;
                }
            }

            return GetValue().ToString() + GetStringSuffix();
        }
    }

    /// <summary>
    /// This method returns the specific resolved value for the node (e.g., if this node was using a function to
    /// returen a unit with a specific tag, it would run the functions with necessary arguments to return the exact
    /// unit).
    /// 
    /// If resolving a function, it will recursively resolve any children nodes needed to run the methods.
    /// </summary>
    /// <param name="presets">The preset dictionary that the node will use to resolve.</param>
    /// <param name="engine">The visual coding engine used to resolve the node (could be used for local variable resolution).</param>
    /// <param name="script">The script that contains the node.</param>
    /// <returns>Returns the data associated with the node (or nothing if the node doesn't return any data).</returns>
    public object Resolve(Dictionary<string, object> presets, LogicEngine engine, VisualCodeScript script)
    {
        LogicEngine.current = engine;
        LogicEngine.currentScript = script;
        LogicEngine.currentNode = this;
        if (presets == null || !presets.ContainsKey("CurrentLineID"))
            LogicEngine.currentLine = 0;
        else
            LogicEngine.currentLine = (int)presets["CurrentLineID"];
        LogicEngine.currentPresets = presets;

        // If the node is a value node, the stored value can be returned directly.
        if (returnType == ReturnType.Value)
        {
            return GetValue();
        }

        // If the node is a temporary node, throw an exception.
        else if (returnType == ReturnType.Temp)
        {
            throw new VisualCodeException("Tried to resolve a temporary value.");
        }

        // This is legacy code - a node should not have a variable type.
        else if (returnType == ReturnType.Variable)
        {
            throw new VisualCodeException("Nodes should not have a variable type.");
        }

        // If this is a preset node, return the preset.
        else if (returnType == ReturnType.Preset)
        {
            // If the preset dictionary contains this preset, return it.
            if (presets.ContainsKey(presetName))
            {
                return presets[presetName];
            }

            // The preset may need to be dynamically evaluated, try to evaluate it.
            else
            {
                MethodInfo methodInfo = (typeof(VisualCodeScript)).GetMethod(LogicEngine.DYNAMIC_PRESETS[presetName]);
                if (methodInfo == null) Debug.Log($"No method named {LogicEngine.DYNAMIC_PRESETS[presetName]} found!");
                return methodInfo.Invoke(script, null);
            }
        }

        // If this is a function node, run the function to evaluate the result.
        else if (returnType == ReturnType.Function)
        {
            // TODO: This is likely legacy code that can now be removed.
            if (presets != null && presets.ContainsKey(functionName))
            {
                return presets[functionName];
            }

            // Check that the method exists.
            MethodInfo methodInfo = (typeof(VisualCodeScript)).GetMethod(functionName);
            if (methodInfo == null) Debug.Log($"No method named {functionName} found!");

            // Resolve all of the arguments.
            object[] resolvedArgs = new object[functionEvaluators.Length];
            for (int i = 0; i < resolvedArgs.Length; i++)
            {
                resolvedArgs[i] = functionEvaluators[i].Resolve(presets, engine, script);
            }

            // Invoke the method to run.
            return methodInfo.Invoke(script, resolvedArgs);
        }

        return null;
    }

    /// <summary>
    /// Returns true if the node is valid, otherwise false. A node is invalid if it contains
    /// inappropriate values for execution (e.g., temporary nodes still exist).
    /// </summary>
    public virtual bool Validate ()
    {
        if (returnType == ReturnType.Value)
        {
            return true;
        }
        else if (returnType == ReturnType.Temp)
        {
            return false;
        }
        else if (returnType == ReturnType.Variable)
        {
            return false;
        }
        else if (returnType == ReturnType.Function)
        {
            bool valid = true;
            foreach (GeneralNode value in functionEvaluators)
            {
                if (!value.Validate())
                {
                    valid = false;
                }
            }
            return valid;
        }
        return true;
    }
     
    /// <summary>
    /// Returns a deep copy of the node.
    /// </summary>
    public GeneralNode Copy ()
    {
        GeneralNode node = New();
        node.CopyFrom(this);
        return node;
    }

    /// <summary>
    /// Deep copies data from another node into this node.
    /// </summary>
    public virtual GeneralNode CopyFrom(GeneralNode toCopy)
    {
        returnType = toCopy.returnType; 
        tempName = toCopy.tempName;
        functionDescription = toCopy.functionDescription;
        functionDynamicDescription = toCopy.functionDynamicDescription;
        functionName = toCopy.functionName;
        functionEvaluators = toCopy.functionEvaluators;
        presetName = toCopy.presetName;
        variableName = toCopy.variableName;
        indent = toCopy.indent;
        stringSuffix = toCopy.stringSuffix;
        nodeIcon = toCopy.nodeIcon;

        allowValue = toCopy.allowValue;
        allowPreset = toCopy.allowPreset;
        allowFunction = toCopy.allowFunction;
        SetValue(toCopy.GetValue());

        functionEvaluators = new GeneralNode[toCopy.functionEvaluators.Length];
        for (int i = 0; i < functionEvaluators.Length; i++)
        {
            functionEvaluators[i] = toCopy.functionEvaluators[i].Copy();
            functionEvaluators[i].parentNode = this;
            functionEvaluators[i].parentNodeID = i;
        }

        return this;
    }
    

    public virtual GeneralNode New() { return new GeneralNode(); }
    public virtual object GetValue () { return null; }
    public virtual void SetValue(object value) { }
}

/// <summary>
/// Stores the data for an event node - should only store a function to run.
/// </summary>
[System.Serializable]
public class EventNode : GeneralNode
{
    public string[] presets;

    public override GeneralNode New() { return new EventNode(); }

    public override GeneralNode CopyFrom(GeneralNode toCopy)
    {
        GeneralNode copy = base.CopyFrom(toCopy);
        EventNode copyEvent = (EventNode)copy;
        EventNode originalEvent = (EventNode)toCopy;
        if (originalEvent.presets == null)
            copyEvent.presets = null;
        else
        {
            copyEvent.presets = new string[originalEvent.presets.Length];
            for (int i = 0; i < originalEvent.presets.Length; i++)
                copyEvent.presets[i] = originalEvent.presets[i];
        }
        
        return copyEvent;
    }
}

/// <summary>
/// Stores the data for an action node - should only store a function to run.
/// </summary>
[System.Serializable]
public class NestingActionNode : ActionNode
{
    public override GeneralNode New() { return new NestingActionNode(); }
    public override bool CanHaveChildren()
    {
        return true;
    }
}

/// <summary>
/// Stores the data for an action node - should only store a function to run.
/// </summary>
[System.Serializable]
public class ActionNode : GeneralNode
{
    public override GeneralNode New() { return new ActionNode(); }
}

/// <summary>
/// Stores the data for a number node - i.e. a node that is storing a number value,
/// or a function which will resolve into a number.
/// </summary>
[System.Serializable]
public class NumberNode : GeneralNode
{
    public float value;

    public NumberNode() {  }

    public override GeneralNode New() { return new NumberNode(); }
    public override string DefaultTemporaryNodeLabel { get { return "Number"; } }

    public override void SetValue(object value) { this.value = (float)value; }

    public override object GetValue() { return value; }
}

/// <summary>
/// Stores the data for a unit node - i.e. a node that is storing a unit,
/// or a function which will resolve into a unit.
/// </summary>
[System.Serializable]
public class UnitNode : GeneralNode
{
    public Unit value;
    public override string DefaultTemporaryNodeLabel { get { return "Unit"; } }

    public override GeneralNode New() { return new UnitNode(); }

    public override void SetValue(object value) { this.value = (Unit)value; }

    public override object GetValue() { return value; }
}

/// <summary>
/// Stores the data for a vector node - i.e. a node that is storing a vector value,
/// or a function which will resolve into a vector.
/// </summary>
[System.Serializable]
public class VectorNode : GeneralNode
{
    public Vector3 value;
    public override string DefaultTemporaryNodeLabel { get { return "Location"; } }

    public override GeneralNode New() { return new VectorNode(); }

    public override void SetValue(object value) { this.value = (Vector3)value; }

    public override object GetValue() { return value; }
}

/// <summary>
/// Stores the data for a color node - i.e. a node that is storing a color value,
/// or a function which will resolve into a color.
/// </summary>
[System.Serializable]
public class ColorNode : GeneralNode
{
    public Color value;

    public override GeneralNode New() { return new ColorNode(); }
    public override string DefaultTemporaryNodeLabel { get { return "Color"; } }

    public override void SetValue(object value) { this.value = (Color)value; }

    public override object GetValue() { return value; }
}

/// <summary>
/// Stores the data for a bool node - i.e. a node that is storing a bool,
/// or a function which will resolve into a bool.
/// </summary>
[System.Serializable]
public class BoolNode : GeneralNode
{
    public bool value;

    public override GeneralNode New() { return new BoolNode(); }
    public override string DefaultTemporaryNodeLabel { get { return "Bool"; } }

    public override void SetValue(object value) { this.value = (bool)value; }

    public override object GetValue() { return value; }
}

/// <summary>
/// Stores the data for a ability node - i.e. a node that is storing an ability,
/// or a function which will resolve into a ability.
/// </summary>
[System.Serializable]
public class AbilityNode : GeneralNode
{
    public Ability value;

    public override GeneralNode New() { return new AbilityNode(); }
    public override string DefaultTemporaryNodeLabel { get { return "Ability"; } }

    public override void SetValue(object value) { this.value = (Ability)value; }

    public override object GetValue() { return value; }
}

/// <summary>
/// Stores the data for a ability node - i.e. a node that is storing an ability,
/// or a function which will resolve into a ability.
/// </summary>
[System.Serializable]
public class AudioNode : GeneralNode
{
    public AudioClip value;

    public override GeneralNode New() { return new AudioNode(); }
    public override string DefaultTemporaryNodeLabel { get { return "Audio Clip"; } }

    public override void SetValue(object value) { this.value = (AudioClip)value; }

    public override object GetValue() { return value; }
}

/// <summary>
/// Stores the data for a ability node - i.e. a node that is storing an ability,
/// or a function which will resolve into a ability.
/// </summary>
[System.Serializable]
public class GameFeedbackNode : GeneralNode
{
    public GameFeedback value;

    public override GeneralNode New() { return new GameFeedbackNode(); }
    public override string DefaultTemporaryNodeLabel { get { return "Feedback"; } }

    public override void SetValue(object value) { this.value = (GameFeedback)value; }

    public override object GetValue() { return value; }
}

/// <summary>
/// Stores the data for a ability node - i.e. a node that is storing an ability,
/// or a function which will resolve into a ability.
/// </summary>
[System.Serializable]
public class EffectNode : GeneralNode
{
    public CustomVisualEffect value;

    public override GeneralNode New() { return new EffectNode(); }
    public override string DefaultTemporaryNodeLabel { get { return "Effect"; } }

    public override void SetValue(object value) { this.value = (CustomVisualEffect)value; }

    public override object GetValue() { return value; }
}

[System.Serializable]
public class GameObjectNode : GeneralNode
{
    public GameObject value;

    public override GeneralNode New() { return new GameObjectNode(); }
    public override string DefaultTemporaryNodeLabel { get { return "Game Object"; } }
    public override void SetValue(object value) { this.value = (GameObject)value; }

    public override object GetValue() { return value; }
}

public class ProjectileNode : GeneralNode
{
    public Projectile value;

    public override GeneralNode New() { return new ProjectileNode(); }
    public override string DefaultTemporaryNodeLabel { get { return "Projectile"; } }
    public override void SetValue(object value) { this.value = (Projectile)value; }

    public override object GetValue() { return value; }
}

public class ItemNode : GeneralNode
{
    public Item value;

    public override GeneralNode New() { return new ItemNode(); }
    public override string DefaultTemporaryNodeLabel { get { return "Item"; } }

    public override void SetValue(object value) { this.value = (Item)value; }

    public override object GetValue() { return value; }
}

public class GroupNode : GeneralNode { }

/// <summary>
/// Stores the data for a unit group node - i.e. a node that resolves into a group of units.
/// </summary>
[System.Serializable]
public class UnitGroupNode : GroupNode
{
    public override string DefaultTemporaryNodeLabel { get { return "Unit Group"; } }
    public override GeneralNode New() { return new UnitGroupNode(); }
}

/// <summary>
/// Stores the data for a unit group node - i.e. a node that resolves into a group of units.
/// </summary>
[System.Serializable]
public class VectorGroupNode : GroupNode
{
    public override string DefaultTemporaryNodeLabel { get { return "Vector Group"; } }
    public override GeneralNode New() { return new VectorGroupNode(); }
}


/// <summary>
/// Stores the data for a ability node - i.e. a node that is storing an ability,
/// or a function which will resolve into a ability.
/// </summary>
[System.Serializable]
public class StringNode : GeneralNode
{
    public string value;
    public string[] options = new string[] { };
    public override string DefaultTemporaryNodeLabel { get { return "Text"; } }

    public override GeneralNode New() { return new StringNode(); }

    public override void SetValue(object value) { this.value = (string)value; }

    public override object GetValue() { return value; }

    public override GeneralNode CopyFrom(GeneralNode toCopy)
    {
        GeneralNode copy = base.CopyFrom(toCopy);
        StringNode copyString = (StringNode)copy;
        StringNode originalString = (StringNode)toCopy;
        copyString.options = new string[originalString.options.Length];
        for (int i = 0; i < originalString.options.Length; i++)
            copyString.options[i] = originalString.options[i];
        return copyString;
    }
}