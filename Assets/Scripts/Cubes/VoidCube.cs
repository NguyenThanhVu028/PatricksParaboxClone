using UnityEngine;

public class VoidCube : ContainerCube
{
    [Min(3)]
    [SerializeField] Vector2Int tiling = new Vector2Int(5, 5);
    [SerializeField] InfinityCube infinityCubePrefab;
    [SerializeField] EpsilonCube epsilonCubePrefab;

    public override void Init()
    {
        isPlayer = false;
        canBePlayer = false;
        isLeavable = false;

        childGrid.Tiling = new Vector2Int(tiling.x, tiling.y);
        childGrid.Init();

        InitAnimations();

        onInit.Invoke();
    }

    public InfinityCube GetInfinityCube(int level)
    {
        return null;
    }

    public EpsilonCube GetEpsilonCube(int level)
    {
        return null;
    }
}
