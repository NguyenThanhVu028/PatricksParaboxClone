using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class RenderingUtils
{
    public static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    public static readonly int ColorID = Shader.PropertyToID("_Color");
    public static readonly int IsHighlightedID = Shader.PropertyToID("_IsHighlighted");
    public static readonly int IsHorizFlippedID = Shader.PropertyToID("_IsHorizFlipped");
    public static readonly int BorderHighlightColorID = Shader.PropertyToID("_BorderHighlightColor");
    public static readonly int ExposureID = Shader.PropertyToID("_Exposure");

    public static void RenderMesh(RenderRequestData renderRequestData)
    {
        // If occulusionCulling is on, then the texture won't be rendered outside of camera's view
        if (renderRequestData.OcclusionCulling && !CheckVisibility(renderRequestData.Rect)) return;

        MaterialPropertyBlock matProps = new();
        matProps.SetTexture(MainTexID, renderRequestData.Texture);
        matProps.SetColor(ColorID, renderRequestData.Color);
        matProps.SetFloat(ExposureID, renderRequestData.Exposure);
        if (renderRequestData.Rect.size.x < 0) matProps.SetFloat(IsHorizFlippedID, 1);
        else matProps.SetFloat(IsHorizFlippedID, 0);

        RenderMesh(new RenderData()
        {
            Mesh = renderRequestData.Mesh,
            Material = renderRequestData.Material,
            MatProps = matProps,
            Rect = renderRequestData.Rect,
            WorldSpaceScissorRect = renderRequestData.worldSpaceScissorRect,
            OcclusionCulling = renderRequestData.OcclusionCulling,
            Depth = renderRequestData.Depth
        });
    }

    public static void RenderMesh(RenderData renderData)
    {
        Vector3 positionToRender = new(renderData.Rect.position.x, renderData.Rect.position.y, renderData.Depth);
        Matrix4x4 matrix = Matrix4x4.TRS(positionToRender, Quaternion.identity, renderData.Rect.size); //  Calculate position

        RasterCommandBuffer cmd = CustomRendererFeature.CustomRenderPass.CommandBuffer;
        if (renderData.WorldSpaceScissorRect != null)
        {
            cmd.EnableScissorRect(ScreenspaceRectFromWorldspace(renderData.WorldSpaceScissorRect.Value));
        }
        cmd.DrawMesh(
            mesh: renderData.Mesh,
            material: renderData.Material,
            matrix: matrix,
            submeshIndex: 0,
            shaderPass: -1,
            properties: renderData.MatProps
            );
        cmd.DisableScissorRect();
    }

    public static Rect ScreenspaceRectFromWorldspace(Rect? worldspaceRect)
    {
        if (Camera.main == null || worldspaceRect == null) return new Rect();
        Vector2 worldBottomLeft = new Vector2(worldspaceRect.Value.x - worldspaceRect.Value.width * 0.5f, worldspaceRect.Value.y - worldspaceRect.Value.height * 0.5f);
        Vector2 worldTopRight = new Vector2(worldspaceRect.Value.x + worldspaceRect.Value.width * 0.5f, worldspaceRect.Value.y + worldspaceRect.Value.height * 0.5f);
        Vector2 screenBottomLeft = Camera.main.WorldToScreenPoint(worldBottomLeft);
        Vector2 screenTopRight = Camera.main.WorldToScreenPoint(worldTopRight);
        return new Rect(position: screenBottomLeft, size: new(screenTopRight.x - screenBottomLeft.x, screenTopRight.y - screenBottomLeft.y));
    }

    public static bool CheckVisibility(Rect rect)
    {
        if (RecursiveCamera.Instance == null || RecursiveCamera.Instance.MainCamera) return false;

        rect.size = new(Mathf.Abs(rect.size.x), Mathf.Abs(rect.size.y));

        Vector2 topLeftPos = new Vector2(rect.position.x - rect.size.x * 0.5f, rect.position.y + rect.size.y * 0.5f);
        Vector2 bottomRightPos = new Vector2(topLeftPos.x + rect.size.x, topLeftPos.y - rect.size.y);

        Vector2 topLeftViewportPos = RecursiveCamera.Instance.MainCamera.WorldToViewportPoint(topLeftPos);
        Vector2 bottomRightViewportPos = RecursiveCamera.Instance.MainCamera.WorldToViewportPoint(bottomRightPos);

        if (topLeftViewportPos.x >= 1 || topLeftViewportPos.y <= 0) return false;
        if (bottomRightViewportPos.x <= 0 || bottomRightViewportPos.y >= 1) return false;

        return true;
    }

    public static Rect GetOverlapRect(Rect? inputRect1, Rect? inputRect2)
    {
        if (inputRect1 == null || inputRect2 == null)
        {
            if (inputRect2 != null) return inputRect2.Value;
            if (inputRect1 != null) return inputRect1.Value;
            return new(0, 0, 0, 0);
        }

        Rect rect1 = new(inputRect1.Value.position, new(Mathf.Abs(inputRect1.Value.size.x), Mathf.Abs(inputRect1.Value.size.y)));
        Rect rect2 = new(inputRect2.Value.position, new(Mathf.Abs(inputRect2.Value.size.x), Mathf.Abs(inputRect2.Value.size.y)));

        Vector2 bottomLeft1 = new(rect1.x - rect1.width * 0.5f, rect1.y - rect1.height * 0.5f);
        Vector2 topRight1 = new(rect1.x + rect1.width * 0.5f, rect1.y + rect1.height * 0.5f);

        Vector2 bottomLeft2 = new(rect2.x - rect2.width * 0.5f, rect2.y - rect2.height * 0.5f);
        Vector2 topRight2 = new(rect2.x + rect2.width * 0.5f, rect2.y + rect2.height * 0.5f);

        Vector2 bottomLeft = new();
        Vector2 topRight = new();

        bottomLeft.x = Mathf.Max(bottomLeft1.x, bottomLeft2.x);
        bottomLeft.y = Mathf.Max(bottomLeft1.y, bottomLeft2.y);
        topRight.x = Mathf.Min(topRight1.x, topRight2.x);
        topRight.y = Mathf.Min(topRight1.y, topRight2.y);

        if (topRight.x < bottomLeft.x || topRight.y < bottomLeft.y)
        {
            return new(0, 0, 0, 0);
        }
        else
        {
            Rect rect = new()
            {
                center = new((topRight.x + bottomLeft.x) * 0.5f, (topRight.y + bottomLeft.y) * 0.5f),
                size = new(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y)
            };
            return rect;
        }
    }

    public static Vector2 WorldSizeToScreenSize(Vector2 scale)
    {
        if (RecursiveCamera.Instance == null || RecursiveCamera.Instance.MainCamera == null) return Vector2.zero;

        int pixelPerUnit = Mathf.RoundToInt(RecursiveCamera.Instance.MainCamera.pixelHeight / (RecursiveCamera.Instance.MainCamera.orthographicSize * 2.0f));
        return new Vector2(Mathf.Abs(scale.x) * pixelPerUnit, Mathf.Abs(scale.y) * pixelPerUnit);
    }

    public struct RenderRequestData
    {
        public Mesh Mesh;
        public Material Material;
        public Texture Texture;
        public Color Color;
        public float Exposure;
        public Rect Rect;
        public Rect? worldSpaceScissorRect;
        public bool OcclusionCulling;
        public float Depth;
    }

    public struct RenderData
    {
        public Mesh Mesh;
        public Material Material;
        public MaterialPropertyBlock MatProps;
        public Rect Rect;
        public Rect? WorldSpaceScissorRect;
        public bool OcclusionCulling;
        public float Depth;
    }
}
