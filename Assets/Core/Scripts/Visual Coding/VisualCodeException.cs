using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualCodeException : Exception
{
    public VisualCodeException(string errorMessage) : base(errorMessage)
    {

    }

    public VisualCodeException (string errorMessage, IVisualCodeHandler engineHandler, string type, int currentLine) : base(errorMessage)
    {

    }
}
