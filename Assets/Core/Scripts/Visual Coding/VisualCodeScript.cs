using MyUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using Random = UnityEngine.Random;

[System.Serializable]
public partial class VisualCodeScript
{
    #region Script Execution Logic

    public string scriptName;
    public int scriptUID;
    public bool hasWait;

    public static Unit lastCreatedUnit;
    public static Unit triggeringUnit;
    public static Unit damagingUnit;
    public static Unit killingUnit;
    public static Unit healingUnit;
    public static Ability lastAbilityCast;
    public static Ability triggeringAbility;

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

    public VisualCodeScript(string scriptName)
    {
        this.scriptName = scriptName;
        scriptUID = Random.Range(0, int.MaxValue - 1);
    }


    [SerializeReference]
    public List<GeneralNode> eventNodes = new List<GeneralNode>();


    [SerializeReference]
    public List<GeneralNode> conditionNodes = new List<GeneralNode>();


    [SerializeReference]
    public List<GeneralNode> actionNodes = new List<GeneralNode>();

    public VisualCodeScript Copy()
    {
        VisualCodeScript script = new VisualCodeScript(scriptName);
        foreach (GeneralNode node in eventNodes)
            script.eventNodes.Add(node.Copy());
        foreach (GeneralNode node in conditionNodes)
            script.conditionNodes.Add(node.Copy());
        foreach (GeneralNode node in actionNodes)
            script.actionNodes.Add(node.Copy());
        return script;
    }

    public override int GetHashCode()
    {
        return scriptUID;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public static bool operator ==(VisualCodeScript s1, VisualCodeScript s2)
    {
        if (s1 is null && s2 is null) return true;
        if (s1 is null && s2 is not null) return false;
        if (s1 is not null && s2 is null) return false;
        return s1.scriptUID == s2.scriptUID;
    }

    public static bool operator !=(VisualCodeScript s1, VisualCodeScript s2)
    {
        if (s1 is null && s2 is null) return false;
        if (s1 is null && s2 is not null) return true;
        if (s1 is not null && s2 is null) return true;
        return s1.scriptUID != s2.scriptUID;
    }

    /// <summary>
    /// Try to run this script with the given trigger (and no specified presets).
    /// </summary>
    public void RunScript(LogicEngine engine, string eventTrigger = "", object reqs = null)
    {
        RunScript(new Dictionary<string, object>(), engine, eventTrigger, reqs);
    }

    /// <summary>
    /// Try to run this script with the given trigger and presets.
    /// </summary>
    public void RunScript(Dictionary<string, object> presets, LogicEngine engine, string eventTrigger = "", object reqs = null)
    {
        if (engine.disabledScripts.ContainsKey(this) && Time.time > engine.disabledScripts[this]) return;
        LogicEngine.current = engine;

        // Handle the events.
        bool areEventsTrue = false;
        int line = 0;
        LogicEngine.currentType = "Event";
        foreach (GeneralNode eventNode in eventNodes)
        {
            presets["CurrentLineID"] = line;
            line++;
            if (eventTrigger == eventNode.functionName)
            {
                if ((eventNode.functionEvaluators.Length == 0 || reqs == null) ||
                    eventNode.functionEvaluators[0].Resolve(presets, engine, this).Equals(reqs))
                {
                    areEventsTrue = true;
                }
            }
        }
        if (!areEventsTrue) return;

        // Handle the conditions.
        LogicEngine.currentType = "Condition";
        bool areConditionsTrue = true;
        for (int i = 0; i < conditionNodes.Count; i++)
        {
            presets["CurrentLineID"] = i;
            if (((bool)conditionNodes[i].Resolve(presets, engine, this)).Equals(false))
            {
                areConditionsTrue = false;
                break;
            }
        }
        if (!areConditionsTrue) return;

        LogicEngine.currentType = "Action";
        // Handle the actions.
        _ = RunAllActions(presets, engine);
    }

    /// <summary>
    /// This method is used to convert an async wait into a time-controlled wait. This is
    /// necessary because the Unity Timescale can be adjusted (e.g. if the game is paused).
    /// </summary>
    public async Task WaitAsync(float duration)
    {
        // Creating a TaskCompletionSource that will be completed after the wait
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        // Starting the coroutine that handles the wait
        GameManager.instance.StartCoroutine(WaitCoroutine(duration, tcs));

        // Awaiting the TaskCompletionSource's task
        await tcs.Task;
    }

    /// <summary>
    /// Coroutine to simulate the wait. It waits for the set amount of time before setting the
    /// task completion source.
    /// </summary>
    private IEnumerator WaitCoroutine(float duration, TaskCompletionSource<bool> tcs)
    {
        if (duration <= 0)
        {
            // If the duration is zero or negative, immediately complete the task.
            tcs.SetResult(true);
        }
        else
        {
            float startTime = Time.time;
            float endTime = startTime + duration;
            // Adjusting for very short durations by ensuring we wait for at least one frame
            // This could be important for consistency in behavior, especially if the action
            // following the wait is expected to happen after some processing has occurred.
            do
            {
                // Waiting for the next frame
                yield return null;
            } while (Time.time < endTime);

            // Marking the Task as complete
            tcs.SetResult(true);
        }
    }

    /// <summary>
    /// Run all actions in the script, using the given presets and execution engine.
    /// </summary>
    public async Task RunAllActions(Dictionary<string, object> presets, LogicEngine engine, int startID = 0, int indent = 0)
    {
        for (int i = startID; i < actionNodes.Count; i++)
        {
            if (presets.ContainsKey("ActionsArePaused"))
                break;


            presets["CurrentLineID"] = i;
            LogicEngine.current = engine;
            LogicEngine.currentType = "Action";

            // Stop and go back to parent. Stop doing stuff.
            if (actionNodes[i].indent < indent) break;

            // There are a number of "special" nodes which need to be treated seperately. These perform special
            // flow logic and will adjust execution of the actions in a non-standard way.
            if (actionNodes[i].indent == indent)
            {
                // A "Wait" node will cause execution of all remaining actions to pause by the specified amount of time.
                if (actionNodes[i].functionName == "Wait")
                {
                    float value = (float)actionNodes[i].functionEvaluators[0].Resolve(presets, engine, this);
                    await WaitAsync(value);
                }


                else if (actionNodes[i].functionName == "DoActionsXTimes") // This is old and should be removed after it is no longer used.
                {
                    int repeatCount = (int)((float)actionNodes[i].functionEvaluators[0].Resolve(presets, engine, this));
                    for (int j = 0; j < repeatCount; j++)
                    {
                        await RunAllActions(presets, engine, i + 1, indent + 1);
                    }
                }

                // A "DoActionXTimes" node will repeat all child nodes the specified number of times.
                else if (actionNodes[i].functionName == "DoActionsXTimesStoringVariable")
                {
                    int repeatCount = (int)((float)actionNodes[i].functionEvaluators[0].Resolve(presets, engine, this));
                    string variableName = (string)actionNodes[i].functionEvaluators[1].Resolve(presets, engine, this);
                    for (int j = 0; j < repeatCount; j++)
                    {
                        engine.localVariables[variableName] = j;
                        await RunAllActions(presets, engine, i + 1, indent + 1);
                    }
                }

                // A "ForEachUnitInGroup" node will repeat the child actions for each unit in the group.
                else if (actionNodes[i].functionName == "ForEachUnitInGroup")
                {
                    List<Unit> unitGroup = (List<Unit>)actionNodes[i].functionEvaluators[0].Resolve(presets, engine, this);
                    string variableName = (string)actionNodes[i].functionEvaluators[1].Resolve(presets, engine, this);
                    foreach (Unit unit in unitGroup)
                    {
                        if (unit != null)
                        {
                            engine.localVariables[variableName] = unit;
                            await RunAllActions(presets, engine, i + 1, indent + 1);
                        }
                    }
                }

                // A "DoActionsWhileBool" node will repeat the child actions while the condition is true. 
                else if (actionNodes[i].functionName == "DoActionsWhileBool")
                {
                    while ((bool)actionNodes[i].functionEvaluators[0].Resolve(presets, engine, this).Equals(true))
                    {
                        await RunAllActions(presets, engine, i + 1, indent + 1);
                    }
                }

                // A "DoActionsIfBool" node will only execute the child actions if the condition is true.  
                else if (actionNodes[i].functionName == "DoActionsIfBool")
                {
                    if ((bool)actionNodes[i].functionEvaluators[0].Resolve(presets, engine, this).Equals(true))
                    {
                        await RunAllActions(presets, engine, i + 1, indent + 1);
                    }
                }

                /*
                // A "DisableScript" node will disable this script, preventing all future execution.
                else if (actionNodes[i].functionName == "DisableScript")
                {
                    engine.disabledScripts[this] = float.MaxValue;
                }
                */

                // Otherwise, execute the node as normal.
                else {
                    try
                    {
                        actionNodes[i].Resolve(presets, engine, this);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.ToString());
                    }
                    await RunAllActions(presets, engine, i, indent + 1);
                }
            }
        }
    }

    public void Error(string message)
    {
        if (GameManager.logicEngine.showErrors)
        {
            if (LogicEngine.current != null && LogicEngine.current.engineHandler != null)
                Debug.Log($"<color=orange>[{LogicEngine.current.engineHandler.GetData().name.Replace("(Clone)", "")}] [{LogicEngine.currentScript.scriptName}] [{LogicEngine.currentType}, Line {LogicEngine.currentLine + 1}]:</color> <color=yellow>{message}</color>");
        }
        if (GameManager.logicEngine.stopScriptExecutionOnError)
        {
            LogicEngine.currentPresets.Add("ActionsArePaused", true);
        }
        //LogicEngine.current = engine;
        //LogicEngine.currentScript = script;
        //LogicEngine.currentNode = this;
    }

    public void Error (bool condition, string errorMessage)
    {
        if (condition)
        {
            Error(errorMessage);
            throw new VisualCodeException(errorMessage);
        }
    }

    #endregion
}