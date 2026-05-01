using UnityEngine;

public class VoidCube : ContainerCube
{
    public enum VoidType { Infinity, Epsilon }
    [Min(3)]

    private ContainerCube mainCube;

    public ContainerCube MainCube { get => mainCube; set => mainCube = value; }

    public override void Init()
    {
        isPlayer = false;
        canBePlayer = false;
        isLeavable = false;

        childGrid.Init();

        onInit.Invoke();
    }

    public override void Draw(Rect position, float depth = 0, float exposure = 0, Rect? scissorRect = null)
    {
        if (!CustomTextureRenderer2D.CheckVisibility(position.position, position.size) && enableOcclusionCulling) return;

        Vector2 rectSizeInPixel = CustomTextureRenderer2D.ConvertScaleToPixel(position.size);
        if (rectSizeInPixel.x < minPixelToRender || rectSizeInPixel.y < minPixelToRender) return; // Don't draw if the requested rectangle is too small (To avoid infinite rendering)

        DrawCube(position, depth, exposure, scissorRect);
        DrawMainCube(position, depth, exposure, scissorRect);

        onFinishedDrawing.Invoke(position, depth, exposure, scissorRect);
    }
    public override void DrawCube(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (mainCube != null) cullingCubesAll.Add(mainCube);
        DrawChildCubes(position, depth, exposure, scissorRect);
        if (mainCube != null) cullingCubesAll.Remove(mainCube);
        DrawSurfaceEffects(position, depth, exposure, scissorRect);
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

        Vector2Int center = new Vector2Int(childGrid.Tiling.x / 2, childGrid.Tiling.y / 2);
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
            containerCube.IsEnterable = false;
        }
    }

    public override void ModifyChildCubeExit(Cube childCube)
    {
        base.ModifyChildCubeExit(childCube);
    }
}
