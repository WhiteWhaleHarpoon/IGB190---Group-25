using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVisualCodeHandler
{
    LogicEngine GetEngine();
    Unit GetOwner();
    void SetOwner (Unit owner);
    Object GetData();
    string GetTag();
}
