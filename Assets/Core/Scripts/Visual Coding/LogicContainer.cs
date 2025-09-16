using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "General Script Handler", menuName = "Data/General Script Handler", order = 1)]
public class LogicContainer : ScriptableObject, IVisualCodeHandler
{
    public LogicEngine engine = new LogicEngine();

    public LogicContainer Copy ()
    {
        LogicContainer copy = ScriptableObject.CreateInstance<LogicContainer>();
        copy.engine = engine.Copy();
        return copy;
    }

    public Object GetData()
    {
        return this;
    }

    public LogicEngine GetEngine()
    {
        return engine;
    }

    public Unit GetOwner()
    {
        return GameManager.player;
    }

    public void SetOwner(Unit owner)
    {
        
    }

    public string GetTag ()
    {
        return "";
    }
}
