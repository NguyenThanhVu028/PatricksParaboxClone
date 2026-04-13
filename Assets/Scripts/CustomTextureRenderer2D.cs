using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomTextureRenderer2D
{
    public static readonly int mainTexID = Shader.PropertyToID("_MainTex");
    public static readonly int colorID = Shader.PropertyToID("_Color");
    public static readonly int isHighlightedID = Shader.PropertyToID("_IsHighlighted");
    public static readonly int exposureID = Shader.PropertyToID("_Exposure");

    public static void RenderMesh(Mesh mesh,
                                    Material material,
                                    Texture texture,
                                    Color color,
                                    float exposure,
                                    Vector2 position,
                                    Vector2 size,
                                    float z = 0,
                                    Rect? worldSpaceScissorRect = null)
    {
        // If occulusionCulling is on, then the texture won't be rendered outside of camera's view
        //if (occlusionCulling && !CheckVisibility(position, size)) return;

        MaterialPropertyBlock matProps = new();
        matProps.SetTexture(mainTexID, texture);
        matProps.SetColor(colorID, color);
        matProps.SetFloat(exposureID, exposure);

        RenderMesh(mesh, material, matProps, position, size, z, worldSpaceScissorRect);
    }

    public static void RenderMesh(Mesh mesh,
                                    Material material, 
                                    MaterialPropertyBlock matProps, 
                                    Vector2 position, 
                                    Vector2 size,
                                    float z = 0,
                                    Rect? worldSpaceScissorRect = null)
    {
        // If occulusionCulling is on, then the texture won't be rendered outside of camera's view
        //if (occlusionCulling && !CheckVisibility(position, size)) return;

        Vector3 positionToRender = new(position.x, position.y, z);
        Matrix4x4 matrix = Matrix4x4.TRS(positionToRender, Quaternion.identity, size); //  Calculate position

        RenderParams rp = new RenderParams(material); // Calculate render parameters
        rp.matProps = matProps;

        CommandBuffer cmd = new();
        if (worldSpaceScissorRect != null)
        {
            cmd.EnableScissorRect(ScreenspaceRectFromWorldspace(worldSpaceScissorRect.Value));
            Debug.Log(ScreenspaceRectFromWorldspace(worldSpaceScissorRect.Value));
        }
        cmd.DrawMesh(
            mesh: mesh,
            material: material,
            matrix: matrix,
            submeshIndex: 0,
            shaderPass: -1,
            properties: matProps
            );
        Graphics.ExecuteCommandBuffer(cmd);
        cmd.Release();
        //Graphics.RenderMesh(rp, mesh, 0, matrix);
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

    //public static Rect GetOverlapRect(Rect rect1, Rect rect2)
    //{
    //    Vector2 bottomLeft1 = new Vector2(rect1.x - rect1.width * 0.5f, rect1.y - rect1.height * 0.5f);
    //    Vector2 topRight1 = new Vector2(rect1.x + rect1.width * 0.5f, rect1.y + rect1.height * 0.5f);

    //    Vector2 bottomLeft2 = new Vector2(rect2.x - rect2.width * 0.5f, rect2.y - rect2.height * 0.5f);
    //    Vector2 topRight2 = new Vector2(rect2.x + rect2.width * 0.5f, rect2.y + rect2.height * 0.5f);

    //    Vector2 bottomLeft = new();
    //    Vector2 topRight = new();

    //    if (bottomLeft1.x > bottomLeft2.x && bottomLeft1.y > bottomLeft2.y) bottomLeft = bottomLeft1;
    //    else if (bottomLeft2.x > bottomLeft1.x && bottomLeft2.y > bottomLeft1.y) bottomLeft = bottomLeft2;
    //    else
    //    {
    //        if (bottomLeft1.y > bottomLeft2.y)
    //        {
    //            bottomLeft.x = bottomLeft2.x;
    //            bottomLeft.y = bottomLeft1.y;
    //        }
    //        else if (bottomLeft2.y >= bottomLeft1.y)
    //        {
    //            bottomLeft.x = bottomLeft1.x;
    //            bottomLeft.y = bottomLeft2.y;
    //        }
    //    }

    //    if (topRight1.x < topRight2.x && topRight1.y < topRight2.y) topRight = topRight1;
    //    else if (topRight2.x < topRight1.x && topRight2.y < topRight1.y) topRight = topRight2;
    //    else
    //    {
    //        if (topRight1.y > topRight2.y)
    //        {
    //            topRight.x = topRight1.x;
    //            topRight.y = topRight2.y;
    //        }
    //        else if (topRight2.y >= topRight1.y)
    //        {
    //            topRight.x = topRight2.x;
    //            topRight.y = topRight1.y;
    //        }
    //    }

    //    if (topRight.x < bottomLeft.x || topRight.y < bottomLeft.y) return new(0, 0, 0, 0);
    //    else
    //    {
    //        Rect rect = new();
    //        rect.center = new((topRight.x + bottomLeft.x) * 0.5f, (topRight.y + bottomLeft.y) * 0.5f);
    //        rect.size = new(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
    //        return rect;
    //    }
    //}

    public static Rect GetOverlapRect(Rect? rect1, Rect? rect2)
    {
        if (rect1 == null || rect2 == null)
        {
            if (rect2 != null) return rect2.Value;
            if (rect1 != null) return rect1.Value;
            return new(0, 0, 0, 0);
        }
        Vector2 bottomLeft1 = new Vector2(rect1.Value.x - rect1.Value.width * 0.5f, rect1.Value.y - rect1.Value.height * 0.5f);
        Vector2 topRight1 = new Vector2(rect1.Value.x + rect1.Value.width * 0.5f, rect1.Value.y + rect1.Value.height * 0.5f);

        Vector2 bottomLeft2 = new Vector2(rect2.Value.x - rect2.Value.width * 0.5f, rect2.Value.y - rect2.Value.height * 0.5f);
        Vector2 topRight2 = new Vector2(rect2.Value.x + rect2.Value.width * 0.5f, rect2.Value.y + rect2.Value.height * 0.5f);

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

            foreach(var positionInGrid in entry.Value)
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

        Vector2 topLeftPos = new Vector2(position.x - size.x * 0.5f, position.y + size.y * 0.5f);
        Vector2 bottomRightPos = new Vector2(topLeftPos.x + size.x, topLeftPos.y - size.y);

        Vector2 topLeftViewportPos = Camera.main.WorldToViewportPoint(topLeftPos);
        Vector2 bottomRightViewportPos = Camera.main.WorldToViewportPoint(bottomRightPos);

        if (topLeftViewportPos.x >= 1 || topLeftViewportPos.y <= 0) return false;
        if (bottomRightViewportPos.x <= 0 || bottomRightViewportPos.y >= 1) return false;

        return true;
    }

    public static Vector2 ConvertScaleToPixel(Vector2 scale)
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
}
