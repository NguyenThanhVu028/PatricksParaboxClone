using UnityEngine;

public class EpsilonCube : ContainerCube
{
    [SerializeField] ContainerCube mainContainerCube;
    [Min(1)]
    [SerializeField] int level = 1;
    [SerializeField] CustomTexture epsilonTexture;
    [SerializeField] float epsilonTextureHeightRatio = 0.5f;

    public ContainerCube MainContainerCube { get => mainContainerCube; set => mainContainerCube = value; }
    public int Level { get => level; set => level = value; }

    public override void DrawSurfaceEffects(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        base.DrawSurfaceEffects(position, depth, exposure, scissorRect);

        if (level <= 0 || epsilonTexture == null) return;
        // Draw the epsilon texture
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
            CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, epsilonTexture.GetTexture(), Color.white, exposure, iconRect.position, iconRect.size, depth, CustomTextureRenderer2D.GetOverlapRect(scissorRect, iconScissorRect));
        }
    }
}
