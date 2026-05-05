using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CubesManager : MonoBehaviour
{
    private static CubesManager instance = null;
    [SerializeField] VoidCube voidCubePrefab;
    [SerializeField] InfinityCube infinityCubePrefab;
    [SerializeField] EpsilonCube epsilonCubePrefab;

    [SerializeField] List<CubeDetails> allCubes = new();

    private List<VoidCube> voidCubes = new();
    private List<InfinityCube> infinityCubes = new();
    private List<EpsilonCube> epsilonCubes = new();

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

    private void Start()
    {
        PlayerInputsManager playerInputsManager = PlayerInputsManager.Instance;
        if (playerInputsManager != null)
        {
            playerInputsManager.GameplayInputs.OnResetEvent.AddListener(OnReset);
        }

        foreach(var cube in allCubes)
        {
            if (cube == null || cube.Cube == null || !cube.Cube.isActiveAndEnabled) continue;
            if (cube.Cube is VoidCube) voidCubes.Add(cube.Cube as VoidCube);
            else if (cube.Cube is InfinityCube) infinityCubes.Add(cube.Cube as InfinityCube);
            else if (cube.Cube is EpsilonCube) epsilonCubes.Add(cube.Cube as EpsilonCube);
        }
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

    public InfinityCube GetInfinityCube(ContainerCube mainContainer, int level)
    {
        foreach(var cube in infinityCubes)
        {
            if (cube == null) continue;
            if (cube.MainContainerCube == mainContainer && cube.Level == level)
            {
                return cube;
            }
        }
        if (infinityCubePrefab == null || voidCubePrefab == null) return null;
        var newInfinityCube = Instantiate(infinityCubePrefab);
        newInfinityCube.Level = level;
        newInfinityCube.MainContainerCube = mainContainer;
        var newVoid = Instantiate(voidCubePrefab);
        newVoid.Init();
        newVoid.SetCentralCube(newInfinityCube);
        infinityCubes.Add(newInfinityCube);
        return newInfinityCube;
    }

    public EpsilonCube GetEpsilonCube(ContainerCube mainContainer, int level)
    {
        foreach (var cube in epsilonCubes)
        {
            if (cube == null) continue;
            if (cube.MainContainerCube == mainContainer && cube.Level == level)
            {
                return cube;
            }
        }
        if (epsilonCubePrefab == null || voidCubePrefab == null) return null;
        var newEpsilonCube = Instantiate(epsilonCubePrefab);
        newEpsilonCube.Level = level;
        newEpsilonCube.MainContainerCube = mainContainer;
        newEpsilonCube.IsEnterable = false;
        var newVoid = Instantiate(voidCubePrefab);
        newVoid.Init();
        newVoid.SetCentralCube(newEpsilonCube);
        epsilonCubes.Add(newEpsilonCube);
        return newEpsilonCube;
    }

    public void OnReset()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
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
