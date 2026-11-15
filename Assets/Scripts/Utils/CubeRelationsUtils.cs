using System;
using UnityEngine;

/// <summary>
/// Utility class for managing cube relations through a singleton instance.
/// Other classes do not need to directly interact with the CubeRelations instance.
/// </summary>
public class CubeRelationsUtils
{
    private static CubeRelationTree _cubeRelationTree;

    public static void Init(CubeRelationTree cubeRelations)
    {
        _cubeRelationTree = cubeRelations;
    }

    public void SetParent(BaseCube childCube, BaseCube parentCube)
    {
        _cubeRelationTree.EstablishRelation(childCube, parentCube);
    }
}


