using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomTextureRenderer2D
{
    public static readonly int mainTexID = Shader.PropertyToID("_MainTex");
    public static readonly int colorID = Shader.PropertyToID("_Color");

    public static void RenderMesh(Mesh mesh,
                                    Material material,
                                    Texture texture,
                                    Color color,
                                    Vector2 relativePosition,
                                    Vector2 relativeSize,
                                    //Rect? parentRect = null,
                                    bool occlusionCulling = true)
    {
        // If occulusionCulling is on, then the texture won't be rendered outside of camera's view
        if (occlusionCulling && !CheckVisibility(relativePosition, relativeSize)) return;

        MaterialPropertyBlock matProps = new();
        matProps.SetTexture(mainTexID, texture);
        matProps.SetColor(colorID, color);

        RenderMesh(mesh, material, matProps, relativePosition, relativeSize, occlusionCulling);
    }

    public static void RenderMesh(Mesh mesh,
                                    Material material, 
                                    MaterialPropertyBlock matProps, 
                                    Vector2 position, 
                                    Vector2 size,
                                    bool occlusionCulling = true)
    {
        // If occulusionCulling is on, then the texture won't be rendered outside of camera's view
        if (occlusionCulling && !CheckVisibility(position, size)) return;

        Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, size); //  Calculate position

        RenderParams rp = new RenderParams(material); // Calculate render parameters
        rp.matProps = matProps;

        Graphics.RenderMesh(rp, mesh, 0, matrix);
    }

    public static void ClearRenderTexture(RenderTexture renderTexture)
    {
        if (renderTexture == null) return;

        RenderTexture.active = renderTexture;

        GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);
        GL.Clear(true, true, Color.clear);

        RenderTexture.active = null;
    }

    public static void DrawTexturesToRenderTextureGrid(ref RenderTexture targetRenderTexture, Dictionary<Texture2D, List<TexturePositionInGrid>> texturesInGrid, Material material)
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

                Graphics.DrawTexture(rectToDraw, entry.Key, material);
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
        int pixelPerUnit = Mathf.RoundToInt((float)Camera.main.pixelHeight / (Camera.main.orthographicSize * 2.0f));
        return new Vector2(scale.x * pixelPerUnit, scale.y * pixelPerUnit);
    }

    [Serializable]
    public class TexturePositionInGrid
    {
        [SerializeField] int gridWidth = 1;
        [SerializeField] int gridHeight = 1;
        [SerializeField] Vector2Int positionInGrid = new();

        public TexturePositionInGrid(int gridWidth, int gridHeight, Vector2Int positionInGrid)
        {
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.positionInGrid = positionInGrid;
        }

        public int GridWidth { get => gridWidth; }
        public int GridHeight { get => gridHeight; }
        public Vector2Int PositionInGrid { get => positionInGrid; }
    }
}
