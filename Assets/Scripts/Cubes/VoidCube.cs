using UnityEngine;

public class VoidCube : ContainerCube
{
    public enum VoidType { Infinity, Epsilon }
    [Min(3)]

    private ContainerCube mainCube;

    public ContainerCube MainCube { get => mainCube; set => mainCube = value; }

    public override void Init()
    {
        if (hasInit) return;

        isPlayer = false;
        canBePlayer = false;
        isLeavable = false;

        childGrid.Init();

        onInit.Invoke();

        hasInit = true;
    }

    public override void Draw(Rect position, int priority, float depth = 0, float exposure = 0, Rect? scissorRect = null)
    {
        if (!CustomTextureRenderer2D.CheckVisibility(position.position, position.size) && enableOcclusionCulling) return;

        Vector2 rectSizeInPixel = CustomTextureRenderer2D.ConvertScaleToPixel(position.size);
        if (rectSizeInPixel.x < minPixelToRender || rectSizeInPixel.y < minPixelToRender) return; // Don't draw if the requested rectangle is too small (To avoid infinite rendering)

        DrawCube(position, priority, depth, exposure, scissorRect);

        onFinishedDrawing.Invoke(position, depth, exposure, scissorRect);
    }
    public override void DrawCube(Rect position, int priority, float depth, float exposure, Rect? scissorRect)
    {
        DrawFloor(position, priority, depth + floorDepthOffset, exposure, scissorRect);
        DrawChildCubes(position, priority, depth, exposure, scissorRect);
        DrawSurfaceEffects(position, priority, depth, exposure, scissorRect);
    }

    public void SetCentralCube (ContainerCube cube)
    {
        if (cube == null) return;

        Vector2Int center = new Vector2Int(childGrid.Tiling.x / 2, childGrid.Tiling.y / 2);
        if (childGrid.Children[center.x, center.y].Cube != null)
        {
            childGrid.Children[center.x, center.y].Cube.Parent = null;
            childGrid.RemoveChild(center.x, center.y, (ref ChildCubeDetails cubeDetails) => { cubeDetails.Cube = null; cubeDetails.MovingDirection = CubeMovement.GridDirections.None; });
        }

        cube.Parent = this;
        cube.RelativePosition = Vector2.zero;
        cube.RelativeScale = new(1.0f / childGrid.Tiling.y, 1.0f / childGrid.Tiling.x);
        cube.Init();
        childGrid.Children[center.x, center.y].Cube = cube;
        childGrid.Children[center.x, center.y].MovingDirection = CubeMovement.GridDirections.None;
        mainCube = cube;
    }

    public override void ModifyChildCubeInnerEnter(CubeProperties childCube)
    {
        base.ModifyChildCubeEnter(childCube);
        if (childCube is ContainerCubeProperties containerCube)
        {
            containerCube.IsEnterable = false;
        }
    }
}
