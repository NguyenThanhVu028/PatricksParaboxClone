using System;
using System.Collections.Generic;
using UnityEngine;

public class CubeGrid : MyComponent
{
    public void RemoveChild(BaseCube cubeToRemove)
    {

    }

    public List<BaseCube> GetChildren(Func<BaseCube, bool> condition)
    {
        return new();
    }
}
