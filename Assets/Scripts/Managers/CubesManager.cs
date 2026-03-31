using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CubesManager : MonoBehaviour
{
    private static CubesManager instance = null;

    [SerializeField] List<CubeDetails> allCubes = new();

    public static CubesManager Instance { get => instance; }

    public List<CubeDetails> AllCubes { get => allCubes; }

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
