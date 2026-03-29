using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CubesManager : MonoBehaviour
{
    private static CubesManager instance = null;

    [SerializeField] List<CubeDetails> allCubes = new();
    [SerializeField] float gizmosSpacing = 0.5f;

    public static CubesManager Instance { get => instance; }

    private void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
        }
        instance = this;
    }

    public Cube GetCube(int id)
    {
        foreach(var cube in allCubes)
        {
            if (cube == null) continue;
            if (cube.ID == id)
            {
                if (cube.Cube == null) return null;
                if (cube.Cube.NeedInstantiating)
                    return Instantiate(cube.Cube);
                else return cube.Cube;
            }
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        Handles.Label(transform.position, "Cubes manager details:");
        for (int i = 0; i < allCubes.Count; i++)
        {
            CubeDetails cubeDetails = allCubes[i];
            if (cubeDetails == null) continue;
            Vector3 gizmosPos = transform.position + Vector3.down * (i + 1) * gizmosSpacing;
            Handles.Label(gizmosPos, $"ID: {cubeDetails.ID} | Cube: {(cubeDetails.Cube != null ? cubeDetails.Cube.name : "null")}");
        }
    }
}

[Serializable]
public class CubeDetails
{
    [Min(1)]
    [SerializeField] int id = 1;
    [SerializeField] Cube cube;

    public int ID { get => id; }
    public Cube Cube { get => cube; }
}
