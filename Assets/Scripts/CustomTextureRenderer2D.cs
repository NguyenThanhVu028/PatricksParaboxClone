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

    public static void DrawGridToRenderTexture(ref RenderTexture targetRenderTexture, int gridWidth, int gridHeight, Dictionary<Texture2D, List<Vector2Int>> texturesInGrid, Material material)
    {
        if (targetRenderTexture == null) { Debug.LogWarning("Trying to render into an invalid Render Texture!"); return; }

        RenderTexture.active = targetRenderTexture;

        GL.LoadPixelMatrix(0, targetRenderTexture.width, targetRenderTexture.height, 0);
        GL.Clear(true, true, Color.clear);

        foreach (var entry in texturesInGrid)
        {
            if (entry.Key == null) continue;
            if (entry.Value == null) continue;

            Vector2 tileSize = new((float)targetRenderTexture.width / gridWidth, (float)targetRenderTexture.height / gridHeight);
            Rect rectToDraw = new(0, 0, tileSize.x, tileSize.y);
            foreach (var position in entry.Value)
            {
                rectToDraw.x = position.y * tileSize.x;
                rectToDraw.y = position.x * tileSize.y;
                Graphics.DrawTexture(rectToDraw, entry.Key, material);
            }
        }

        RenderTexture.active = null;

    }

    //public static void DrawTextureToRenderTexture(ref RenderTexture )

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
}

//public class CustomMeshInstancedDrawer
//{
//    public static readonly int maxDrawMeshInstanceCount = 1023;

//    private List<Matrix4x4> matrices; // Reuse Matrix4x4 array to prevent garbage, improve performance
//    private MaterialPropertyBlock matProps;

//    public CustomMeshInstancedDrawer()
//    {
//        matrices = new();
//        matProps = new();
//    }

//    public void DrawMeshInstanced(Mesh mesh,
//                                    Material material,
//                                    Texture2D texture,
//                                    Color color,
//                                    List<Vector2> positions,
//                                    Vector2 size,
//                                    bool occlusionCulling = true)
//    {
//        int positionsListOffset = 0;
//        while (positionsListOffset < positions.Count)
//        {
//            int positionsCount = Mathf.Min(maxDrawMeshInstanceCount, positions.Count - positionsListOffset);
//            DrawMeshInstanceWithOffset(mesh,
//                                        material,
//                                        texture,
//                                        color,
//                                        positions,
//                                        positionsCount,
//                                        positionsListOffset,
//                                        size,
//                                        occlusionCulling);
//            positionsListOffset += positionsCount;
//        }
//    }

//    public void DrawMeshInstanceWithOffset(Mesh mesh,
//                                            Material material,
//                                            Texture2D texture,
//                                            Color color,
//                                            List<Vector2> positions,
//                                            int positionsCount,
//                                            int positionsListOffset,
//                                            Vector2 size,
//                                            bool occlusionCulling = true)
//    {
//        int totalDrawCount = 0;
//        matrices.Clear();
//        for (int i = positionsListOffset; i < positionsListOffset + positionsCount; i++)
//        {
//            if (i >= positions.Count) break;
//            if (totalDrawCount >= maxDrawMeshInstanceCount) break;
//            // If occulusionCulling is on, then the texture won't be rendered outside of camera's view
//            if (occlusionCulling && !CustomTextureRenderer2D.CheckVisibility(positions[i], size)) continue;

//            // Convert positions into the Matrix4x4 array
//            matrices.Add(Matrix4x4.TRS(positions[i], Quaternion.identity, size));
//            totalDrawCount++;
//        }

//        matProps.Clear();
//        matProps.SetTexture(CustomTextureRenderer2D.mainTexID, texture);
//        //matProps.SetColor(CustomTextureRenderer2D.colorID, color);

//        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, matProps);
//    }
//}
