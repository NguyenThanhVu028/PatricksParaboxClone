using System;
using UnityEngine;

/// <summary>
/// Utility class for managing cube relations through a singleton instance.
/// Other classes do not need to directly interact with the CubeRelations instance.
/// </summary>
public class CubeRelationsUtils
{
    private static CubeRelationsUtils _instance;
    private CubeRelationTree _cubeRelationTree;

    public static CubeRelationsUtils Instance
    {
        get => _instance;
    }

    public static void Init(CubeRelationTree cubeRelations)
    {
        _instance ??= new CubeRelationsUtils();
        _instance._cubeRelationTree = cubeRelations;
    }

    public void SetParent(Cube childCube, Cube parentCube)
    {
        _cubeRelationTree.EstablishRelation(childCube, parentCube);
    }
}


