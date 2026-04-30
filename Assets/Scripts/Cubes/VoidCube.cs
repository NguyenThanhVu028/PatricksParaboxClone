using UnityEngine;

public class VoidCube : ContainerCube
{
    public enum VoidType { Infinity, Epsilon }
    [Min(3)]
    [SerializeField] Vector2Int tiling = new Vector2Int(5, 5);

    private ContainerCube mainCube;

    public ContainerCube MainCube { get => mainCube; set => mainCube = value; }

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

    public override void Draw(Rect position, float depth = 0, float exposure = 0, Rect? scissorRect = null)
    {
        DrawCube(position, depth, exposure, scissorRect);
        DrawMainCube(position, depth, exposure, scissorRect);
    }
    public override void DrawCube(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        DrawSurfaceEffects(position, depth + surfaceEffectsDepthOffset, exposure, scissorRect);
        if (mainCube != null) cullingCubesAll.Add(mainCube);
        DrawChildCubes(position, depth, exposure, scissorRect);
        if (mainCube != null) cullingCubesAll.Remove(mainCube);
    }
    protected void DrawMainCube(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (mainCube == null) return;
        Rect mainCubeRect = Relativity.CRectFromPRect(position, mainCube.RelativePosition, mainCube.RelativeScale);
        mainCube.Draw(mainCubeRect, depth, exposure, scissorRect);
    }

    public void SetCentralCube (ContainerCube cube)
    {
        if (cube == null) return;

        Vector2Int center = new Vector2Int(tiling.x / 2, tiling.y / 2);
        if (childGrid.Children[center.x, center.y].Cube != null)
        {
            childGrid.Children[center.x, center.y].Cube.Parent = null;
            childGrid.RemoveChild(center.x, center.y, (ref ChildCubeDetails cubeDetails) => { cubeDetails.Cube = null; cubeDetails.MovingDirection = CubeMovement.MovementDirections.None; });
        }

        cube.Parent = this;
        cube.RelativePosition = Vector2.zero;
        cube.RelativeScale = new(1.0f / childGrid.Tiling.y, 1.0f / childGrid.Tiling.x);
        cube.IsLeavable = false;
        cube.Init();
        childGrid.Children[center.x, center.y].Cube = cube;
        childGrid.Children[center.x, center.y].MovingDirection = CubeMovement.MovementDirections.None;
        mainCube = cube;
    }

    public override void ModifyChildCubeEnter(Cube childCube)
    {
        base.ModifyChildCubeEnter(childCube);
        if (childCube is ContainerCube containerCube)
        {
            containerCube.IsLeavable = false;
        }
    }

    public override void ModifyChildCubeExit(Cube childCube)
    {
        base.ModifyChildCubeExit(childCube);
    }
}
