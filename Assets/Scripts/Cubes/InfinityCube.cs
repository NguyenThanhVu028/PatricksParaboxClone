using UnityEngine;

public class InfinityCube : CloneCube
{
    [Min(1)]
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
}
