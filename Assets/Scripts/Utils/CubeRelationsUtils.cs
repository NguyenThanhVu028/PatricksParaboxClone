using UnityEngine;

/// <summary>
/// Utility class for managing cube relations through a singleton instance.
/// Other classes do not need to directly interact with the CubeRelations instance.
/// </summary>
public class CubeRelationsUtils
{
    private static CubeRelationsUtils _instance;
    private CubeRelations _cubeRelations;

    public static CubeRelationsUtils Instance
    {
        get => _instance;
    }

    public static void Init(CubeRelations cubeRelations)
    {
        if (_instance == null)
        {
            _instance = new CubeRelationsUtils();
        }
        _instance._cubeRelations = cubeRelations;
    }

    public void SetParent(Cube childCube, Cube parentCube)
    {
        if (_cubeRelations == null)
        {
            Debug.LogWarning("CubeRelations instance is not initialized.");
            return;
        }

        _cubeRelations.EstablishRelation(childCube, parentCube);
    }
}


