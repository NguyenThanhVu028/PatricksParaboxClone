using System;
using System.Collections.Generic;
using UnityEngine;

public class CubesManager : MonoBehaviour
{
    private static CubesManager instance = null;

    [SerializeField] List<CubeDetails> cubePrefabs = new();
    [SerializeField] List<CubeDetails> cubesInScene = new();

    public static CubesManager Instance { get => instance; }

    private void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
        }
        instance = this;
    }

    public Cube GetCubePrefab(int id)
    {
        foreach(var cube in cubePrefabs)
        {
            if (cube == null) continue;
            if (cube.ID == id)
            {
                return Instantiate(cube.Cube);
            }
        }
        return null;
    }

    public Cube GetCubeInScene(int id)
    {
        foreach(var cube in cubesInScene)
        {
            if (cube == null) continue;
            if (cube.ID == id) return cube.Cube;
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
