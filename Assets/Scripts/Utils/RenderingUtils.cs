using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderingUtils
{
    public static readonly int MAIN_TEX_ID = Shader.PropertyToID("_MainTex");
    public static readonly int COLOR_ID = Shader.PropertyToID("_Color");
    public static readonly int IS_HIGHLIGHTED_ID = Shader.PropertyToID("_IsHighlighted");
    public static readonly int IS_HORIZONTAL_FLIPPED_ID = Shader.PropertyToID("_IsHorizFlipped");
    public static readonly int BORDER_HIGHLIGHT_COLOR_ID = Shader.PropertyToID("_BorderHighlightColor");
    public static readonly int EXPOSURE_ID = Shader.PropertyToID("_Exposure");
    public static readonly int DEFAULT_PRIORITY = 0;

    private static List<RenderingCall> renderingCalls = new();

    public static void RenderMesh(Mesh mesh,
                                    Material material,
                                    Texture texture,
                                    Color color,
                                    float exposure,
                                    Vector2 position,
                                    Vector2 size,
                                    float z = 0,
                                    Rect? worldSpaceScissorRect = null,
                                    int priority = 0)
    {
        // If occulusionCulling is on, then the texture won't be rendered outside of camera's view
        //if (occlusionCulling && !CheckVisibility(position, size)) return;

        MaterialPropertyBlock matProps = new();
        matProps.SetTexture(MAIN_TEX_ID, texture);
        matProps.SetColor(COLOR_ID, color);
        matProps.SetFloat(EXPOSURE_ID, exposure);
        if (size.x < 0) matProps.SetFloat(IS_HORIZONTAL_FLIPPED_ID, 1);
        else matProps.SetFloat(IS_HORIZONTAL_FLIPPED_ID, 0);

        RenderMesh(mesh, material, matProps, position, size, z, worldSpaceScissorRect, priority);
    }

    public static void RenderMesh(Mesh mesh,
                                    Material material,
                                    MaterialPropertyBlock matProps,
                                    Vector2 position,
                                    Vector2 size,
                                    float z = 0,
                                    Rect? worldSpaceScissorRect = null,
                                    int priority = 0)
    {
        renderingCalls.Add(
            new(
                priority,
                renderingCalls.Count,
                mesh,
                material,
                matProps,
                position,
                size,
                z,
                worldSpaceScissorRect
            ));
    }

    public static void OnRenderOnScreen()
    {
        renderingCalls ??= new();
        renderingCalls.Sort();

        foreach (var renderMeshCall in renderingCalls)
        {
            Vector3 positionToRender = new(renderMeshCall.Position.x, renderMeshCall.Position.y, renderMeshCall.Z);
            Matrix4x4 matrix = Matrix4x4.TRS(positionToRender, Quaternion.identity, renderMeshCall.Size); //  Calculate position

            RasterCommandBuffer cmd = CustomRendererFeature.CustomRenderPass.CommandBuffer;
            if (renderMeshCall.WorldSpaceScissorRect != null)
            {
                cmd.EnableScissorRect(ScreenspaceRectFromWorldspace(renderMeshCall.WorldSpaceScissorRect.Value));
            }
            cmd.DrawMesh(
                mesh: renderMeshCall.Mesh,
                material: renderMeshCall.Material,
                matrix: matrix,
                submeshIndex: 0,
                shaderPass: -1,
                properties: renderMeshCall.MatProps
                );
            cmd.DisableScissorRect();
        }
        renderingCalls.Clear();
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

    public static Rect GetOverlapRect(Rect? rect1, Rect? rect2)
    {
        if (rect1 == null || rect2 == null)
        {
            if (rect2 != null) return rect2.Value;
            if (rect1 != null) return rect1.Value;
            return new(0, 0, 0, 0);
        }

        Rect realRect1 = new(rect1.Value.position, new(Mathf.Abs(rect1.Value.size.x), Mathf.Abs(rect1.Value.size.y)));
        Rect realRect2 = new(rect2.Value.position, new(Mathf.Abs(rect2.Value.size.x), Mathf.Abs(rect2.Value.size.y)));

        Vector2 bottomLeft1 = new Vector2(realRect1.x - realRect1.width * 0.5f, realRect1.y - realRect1.height * 0.5f);
        Vector2 topRight1 = new Vector2(realRect1.x + realRect1.width * 0.5f, realRect1.y + realRect1.height * 0.5f);

        Vector2 bottomLeft2 = new Vector2(realRect2.x - realRect2.width * 0.5f, realRect2.y - realRect2.height * 0.5f);
        Vector2 topRight2 = new Vector2(realRect2.x + realRect2.width * 0.5f, realRect2.y + realRect2.height * 0.5f);

        Vector2 bottomLeft = new();
        Vector2 topRight = new();

        bottomLeft.x = Mathf.Max(bottomLeft1.x, bottomLeft2.x);
        bottomLeft.y = Mathf.Max(bottomLeft1.y, bottomLeft2.y);
        topRight.x = Mathf.Min(topRight1.x, topRight2.x);
        topRight.y = Mathf.Min(topRight1.y, topRight2.y);

        if (topRight.x < bottomLeft.x || topRight.y < bottomLeft.y) return new(0, 0, 0, 0);
        else
        {
            Rect rect = new();
            rect.center = new((topRight.x + bottomLeft.x) * 0.5f, (topRight.y + bottomLeft.y) * 0.5f);
            rect.size = new(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
            return rect;
        }
    }

    public static List<Rect> SubtractRect(Rect? mainRect, Rect? subtractRect)
    {
        List<Rect> result = new List<Rect>();
        if (mainRect == null) return result;
        if (subtractRect == null)
        {
            result.Add(mainRect.Value);
            return result;
        }

        Rect cutMainRect = mainRect.Value;
        Rect overlapRect = GetOverlapRect(cutMainRect, subtractRect.Value);

        float overlapRectTop = overlapRect.y + Mathf.Abs(overlapRect.height) * 0.5f;
        float overlapRectBot = overlapRectTop - Mathf.Abs(overlapRect.height);
        float overlapRectRight = overlapRect.x + Mathf.Abs(overlapRect.width) * 0.5f;
        float overlapRectLeft = overlapRectRight - Mathf.Abs(overlapRect.width);

        float cutMainRectTop = cutMainRect.y + Mathf.Abs(cutMainRect.height) * 0.5f;
        float cutMainRectBot = cutMainRectTop - Mathf.Abs(cutMainRect.height);
        float cutMainRectRight = cutMainRect.x + Mathf.Abs(cutMainRect.width) * 0.5f;
        float cutMainRectLeft = cutMainRectRight - Mathf.Abs(cutMainRect.width);

        if (overlapRectTop < cutMainRectTop)
        {
            result.Add(CreateRect(cutMainRectTop, overlapRectTop, cutMainRectLeft, cutMainRectRight, (cutMainRect.width < 0)));
            cutMainRectTop = overlapRectTop;
        }
        if (overlapRectRight < cutMainRectRight)
        {
            result.Add(CreateRect(cutMainRectTop, cutMainRectBot, overlapRectRight, cutMainRectRight, (cutMainRect.width < 0)));
            cutMainRectRight = overlapRectRight;
        }
        if (overlapRectBot > cutMainRectBot)
        {
            result.Add(CreateRect(overlapRectBot, cutMainRectBot, cutMainRectLeft, cutMainRectRight, (cutMainRect.width < 0)));
            cutMainRectBot = overlapRectBot;
        }
        if (overlapRectLeft > cutMainRectLeft)
        {
            result.Add(CreateRect(cutMainRectTop, cutMainRectBot, cutMainRectLeft, overlapRectLeft, (cutMainRect.width < 0)));
            cutMainRectLeft = overlapRectLeft;
        }

        return result;
    }

    public static Rect CreateRect(float top, float bottom, float left, float right, bool isHorizFlipped)
    {
        Rect rect = new Rect();
        rect.height = top - bottom;
        rect.width = right - left;
        rect.y = (top + bottom) * 0.5f;
        rect.x = (right + left) * 0.5f;
        if (isHorizFlipped) rect.width = -Mathf.Abs(rect.width);
        else rect.width = Mathf.Abs(rect.width);
        return rect;
    }

    public static void ClearRenderTexture(RenderTexture renderTexture)
    {
        if (renderTexture == null) return;

        RenderTexture.active = renderTexture;

        GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);
        GL.Clear(true, true, Color.clear);

        RenderTexture.active = null;
    }

    public static void DrawTexturesToRenderTextureGrid(ref RenderTexture targetRenderTexture, Dictionary<Texture2D, List<TexturePositionInGrid>> texturesInGrid)
    {
        if (targetRenderTexture == null) { Debug.LogWarning("Trying to render into an invalid Render Texture!"); return; }

        RenderTexture.active = targetRenderTexture;

        GL.LoadPixelMatrix(0, targetRenderTexture.width, targetRenderTexture.height, 0);
        //GL.Clear(true, true, Color.clear);

        Vector2 tileSize = new();
        Rect rectToDraw = new();

        foreach (var entry in texturesInGrid)
        {
            if (entry.Key == null) continue; //  No texture
            if (entry.Value == null) continue; //  No positions in grid

            foreach (var positionInGrid in entry.Value)
            {
                tileSize.x = (float)targetRenderTexture.width / positionInGrid.GridWidth;
                tileSize.y = (float)targetRenderTexture.height / positionInGrid.GridHeight;

                rectToDraw.x = positionInGrid.PositionInGrid.y * tileSize.x;
                rectToDraw.y = positionInGrid.PositionInGrid.x * tileSize.y;
                rectToDraw.size = tileSize;

                Graphics.DrawTexture(rectToDraw, entry.Key, positionInGrid.Material);
            }
        }

        RenderTexture.active = null;

    }

    // Use this function to check if the specified position and size is visible on camera
    public static bool CheckVisibility(Vector2 position, Vector2 size)
    {
        if (Camera.main == null) return false;

        size.x = Mathf.Abs(size.x);
        size.y = Mathf.Abs(size.y);

        Vector2 topLeftPos = new Vector2(position.x - size.x * 0.5f, position.y + size.y * 0.5f);
        Vector2 bottomRightPos = new Vector2(topLeftPos.x + size.x, topLeftPos.y - size.y);

        Vector2 topLeftViewportPos = Camera.main.WorldToViewportPoint(topLeftPos);
        Vector2 bottomRightViewportPos = Camera.main.WorldToViewportPoint(bottomRightPos);

        if (topLeftViewportPos.x >= 1 || topLeftViewportPos.y <= 0) return false;
        if (bottomRightViewportPos.x <= 0 || bottomRightViewportPos.y >= 1) return false;

        return true;
    }

    public static Vector2 ScaleToPixel(Vector2 scale)
    {
        if (Camera.main == null) return Vector2.zero;

        int pixelPerUnit = Mathf.RoundToInt((float)Camera.main.pixelHeight / (Camera.main.orthographicSize * 2.0f));
        return new Vector2(Mathf.Abs(scale.x) * pixelPerUnit, Mathf.Abs(scale.y) * pixelPerUnit);
    }

    [Serializable]
    public class TexturePositionInGrid
    {
        [SerializeField] int gridWidth = 1;
        [SerializeField] int gridHeight = 1;
        [SerializeField] Vector2Int positionInGrid = new();
        [SerializeField] Material material;

        public TexturePositionInGrid(int gridWidth, int gridHeight, Vector2Int positionInGrid, Material material)
        {
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.positionInGrid = positionInGrid;
            this.material = material;
        }

        public int GridWidth { get => gridWidth; }
        public int GridHeight { get => gridHeight; }
        public Vector2Int PositionInGrid { get => positionInGrid; }
        public Material Material { get => material; }
    }

    [Serializable]
    public struct RenderingCall : IComparable<RenderingCall>
    {
        public int Priority;
        public int OriginalIndex;
        public Mesh Mesh;
        public Material Material;
        public MaterialPropertyBlock MatProps;
        public Vector2 Position;
        public Vector2 Size;
        public float Z;
        public Rect? WorldSpaceScissorRect;

        public RenderingCall(int priority, int index, Mesh mesh, Material material, MaterialPropertyBlock matProps, Vector2 position, Vector2 size, float z, Rect? worldSpaceScissorRect)
        {
            Priority = priority;
            OriginalIndex = index;
            Mesh = mesh;
            Material = material;
            MatProps = matProps;
            Position = position;
            Size = size;
            Z = z;
            WorldSpaceScissorRect = worldSpaceScissorRect;
        }

        public int CompareTo(RenderingCall other)
        {
            if (Priority < other.Priority) return -1;
            if (Priority > other.Priority) return 1;

            if (OriginalIndex < other.OriginalIndex) return -1;
            if (OriginalIndex > other.OriginalIndex) return 1;

            return 0;
        }
    }
}
