using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualCodeTimer
{
    public VisualCodeScript script;
    public GeneralNode node;
    public bool oneOff;
    public bool hasFinished;
    public float currentTimerValue;

    public VisualCodeTimer (VisualCodeScript script, GeneralNode node, bool oneOff = false)
    {
        this.script = script;
        this.node = node;
        this.oneOff = oneOff;
    }

    public void Update (LogicEngine engine)
    {
        currentTimerValue += Time.deltaTime;
        float timeToReach = (float)node.functionEvaluators[0].Resolve(null, engine, script);
        if (!hasFinished && currentTimerValue >= timeToReach)
        {
            if (oneOff)
            {
                script.RunScript(engine, LogicEngine.EVENT_TIMER_ONE_OFF_FINISHED);
                hasFinished = true;
            }
            else
            {
                script.RunScript(engine, LogicEngine.EVENT_TIMER_CONTINUOUS_FINISHED);
                currentTimerValue = 0;
            }
        }
    }
}
