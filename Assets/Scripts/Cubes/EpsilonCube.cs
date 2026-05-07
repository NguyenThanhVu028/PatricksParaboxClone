using UnityEngine;

public class EpsilonCube : ContainerCube
{
    public const int minLevel = 1;
    [SerializeField] ContainerCube mainContainerCube;
    [Min(minLevel)]
    [SerializeField] int level = 1;
    [SerializeField] CustomTexture epsilonTexture;
    [SerializeField] float epsilonTextureHeightRatio = 0.5f;

    public ContainerCube MainContainerCube { get => mainContainerCube; set => mainContainerCube = value; }
    public int Level { get => level; set => level = value; }

    public override void DrawSurfaceEffects(Rect position, int priority, float depth, float exposure, Rect? scissorRect)
    {
        base.DrawSurfaceEffects(position, priority, depth, exposure, scissorRect);

        if (level <= 0 || epsilonTexture == null) return;
        // Draw the epsilon texture
        position.width  = Mathf.Abs(position.width);
        Vector2 iconNormalRScl = new Vector2(1.0f / level, 1.0f / level);
        Vector2 iconAdjustedRScl = iconNormalRScl;
        iconAdjustedRScl.x *= 1.0f / epsilonTextureHeightRatio;
        iconAdjustedRScl.y *= 1.0f / epsilonTextureHeightRatio;
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
        float totalHeight = iconAdjustedRScl.x * epsilonTextureHeightRatio * level * 2.0f;
        Vector2 iconStartingPointRPos = new Vector2(0, totalHeight * 0.5f - iconAdjustedRScl.x * epsilonTextureHeightRatio);

        for (int i = 0; i < level; i++)
        {
            Rect iconRect = Relativity.CRectFromPRect(position, iconAdjustedRScl, iconStartingPointRPos + Vector2.down * i * iconAdjustedRScl.y * 2.0f * epsilonTextureHeightRatio);
            Rect iconScissorRect = iconRect; iconScissorRect.height *= epsilonTextureHeightRatio;
            CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, epsilonTexture.GetTexture(), Color.white, exposure, iconRect.position, iconRect.size, depth, CustomTextureRenderer2D.GetOverlapRect(scissorRect, iconScissorRect), priority);
        }
    }

    public float RequestToMove(Cube requestedCube, CubeMovement.GridDirections direction, bool external = false, bool specialMove = false)
    {
        // Assume the epsilon cube main container is at the same level as the requested cube
        //if (mainContainerCube == null) return 0;
        //Debug.Log($"Main container : {mainContainerCube.name}, main RPos: {mainContainerCube.RelativePosition}, requestedCube RPos: {requestedCube.RelativePosition}");
        if (requestedCube.PreviousParents.Count < 2) return 0;
        if (requestedCube.PreviousParents[1].Cube == null) return 0;
        Vector2 requestedCubeRPos = Relativity.SRPosFromSameParent(requestedCube.PreviousParents[1].Cube.RelativePosition, requestedCube.PreviousParents[1].Cube.RelativeScale, requestedCube.RelativePosition);
        Vector2 requestedCubeRScl = new Vector2(1.0f / childGrid.Tiling.y, 1.0f / childGrid.Tiling.x);
        bool tempIsEnterable = isEnterable;
        isEnterable = true;
        float targetTime = base.RequestToMove(requestedCubeRPos, requestedCubeRScl, requestedCube, direction, external, specialMove);
        if (targetTime > 0)
        {
            // Correct the camera transition
            var mainCube = requestedCube.PreviousParents[1].Cube;
            if (requestedCube.PreviousParents.Count > 1)
            {
                requestedCube.PreviousParents.RemoveRange(1, (requestedCube.PreviousParents.Count - 1));
            }
            requestedCube.PreviousParents.Add(new(CubeMovement.LayerDirections.In, this, mainCube.RelativePosition, mainCube.RelativeScale, mainCube.IsHorizFlipped));
            if (MainCamera.Instance != null) MainCamera.Instance.SetTransition(new FadeTransition());
        }
        isEnterable = tempIsEnterable;
        return targetTime;
    }
}
