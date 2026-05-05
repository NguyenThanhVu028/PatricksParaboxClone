using UnityEngine;

public class InfinityCube : CloneCube
{
    public const int minLevel = 1;
    [Min(minLevel)]
    [SerializeField] int level = 1;
    [SerializeField] CustomTexture infinityTexture;
    [SerializeField] float infinityTextureHeightRatio = 0.5f;

    public ContainerCube MainContainerCube { get => mainContainerCube; set => mainContainerCube = value; }
    public int Level { get => level; set => level = value; }

    public override void Init()
    {
        base.Init();

        isPlayer = false;
        canBePlayer = false;
        isEnterable = false;
        isLeavable = false;
    }

    public override void Draw(Rect position, float depth = 0, float exposure = 0, Rect? scissorRect = null)
    {
        if (mainContainerCube != null) isHorizFlipped = mainContainerCube.IsHorizFlipped;
        base.Draw(position, depth, exposure, scissorRect);
    }

    public override void DrawSurfaceEffects(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        base.DrawSurfaceEffects(position, depth, exposure, scissorRect);

        if (level <= 0 || infinityTexture == null) return;
        // Draw the infinity texture
        position.width = Mathf.Abs(position.width);
        Vector2 iconNormalRScl = new Vector2(1.0f / level, 1.0f / level);
        Vector2 iconAdjustedRScl = iconNormalRScl;
        iconAdjustedRScl.x *= 1.0f / infinityTextureHeightRatio;
        iconAdjustedRScl.y *= 1.0f / infinityTextureHeightRatio;
        if (iconAdjustedRScl.x > 1.0f)
        {
            iconAdjustedRScl.y *= 1.0f / iconAdjustedRScl.x;
            iconAdjustedRScl.x = 1.0f;
        }
        else if (iconAdjustedRScl.y > 1.0f)
        {
            iconAdjustedRScl.x *= 1.0f / iconAdjustedRScl.y;
            iconAdjustedRScl.y = 1.0f;
        }
        float totalHeight = iconAdjustedRScl.x * infinityTextureHeightRatio * level * 2.0f;
        Vector2 iconStartingPointRPos = new Vector2(0, totalHeight * 0.5f - iconAdjustedRScl.x * infinityTextureHeightRatio);

        for (int i = 0; i < level; i++)
        {
            Rect iconRect = Relativity.CRectFromPRect(position, iconAdjustedRScl, iconStartingPointRPos + Vector2.down * i * iconAdjustedRScl.y * 2.0f * infinityTextureHeightRatio);
            Rect iconScissorRect = iconRect; iconScissorRect.height *= infinityTextureHeightRatio;
            CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, infinityTexture.GetTexture(), Color.white, exposure, iconRect.position, iconRect.size, depth, CustomTextureRenderer2D.GetOverlapRect(scissorRect, iconScissorRect));
        }
    }

    public float RequestToMove(Cube requestedCube, CubeMovement.GridDirections direction, bool external = false, bool specialMove = false)
    {
        if (parent == null || mainContainerCube == null) return 0;
        Debug.Log($"Requested cube: {requestedCube.name} requests infinity cube: {name} level: {level}");
        var cRPos = requestedCube.RelativePosition;
        var cRScl = requestedCube.RelativeScale;
        requestedCube.PreviousParents.Add(new(CubeMovement.LayerDirections.None, this));

        // Flip the requested cube if it is trying to move outside of a horizontally flipped cube
        if (mainContainerCube.IsHorizFlipped)
        {
            cRPos.x = -cRPos.x;
            direction = CubeMovement.FlipMovementInput(direction, true);
        }

        Vector2 outterCubeRPos = Relativity.PRPosToAChild(relativePosition, relativeScale);
        Vector2 outterCubeRScl = Relativity.PRSclToAChild(relativeScale);

        Vector2 childCubeRPosToOutterCube = Relativity.SRPosFromSameParent(outterCubeRPos, outterCubeRScl, cRPos);
        Vector2 childCubeRSclToOutterCube = Relativity.SRSclFromSameParent(outterCubeRScl, cRScl);

        float targetTime = parent.RequestToMove(childCubeRPosToOutterCube, childCubeRSclToOutterCube, requestedCube, direction, external, specialMove);
        if (targetTime > 0)
        {
            // Correct the camera transition
            for(int i = requestedCube.PreviousParents.Count - 1; i >= 0; i--)
            {
                if (requestedCube.PreviousParents[i].Cube is not InfinityCube) continue;
                else
                {
                    if (requestedCube.PreviousParents[i].Cube != this) break;
                    var finalCube = requestedCube.PreviousParents[requestedCube.PreviousParents.Count - 1];
                    requestedCube.PreviousParents.Clear();
                    requestedCube.PreviousParents.Add(new(CubeMovement.LayerDirections.Out, this));
                    requestedCube.PreviousParents.Add(finalCube);
                    break;
                }
            }
        }

        return targetTime;
    }
}
