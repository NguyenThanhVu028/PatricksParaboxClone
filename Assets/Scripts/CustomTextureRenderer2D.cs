using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomTextureRenderer2D
{
    public static readonly int mainTexID = Shader.PropertyToID("_MainTex");
    public static readonly int colorID = Shader.PropertyToID("_Color");

    public static void RenderTexture(Mesh mesh,
                                    Material material,
                                    Texture2D texture,
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

        RenderTexture(mesh, material, matProps, relativePosition, relativeSize, occlusionCulling);
    }

    public static void RenderTexture(Mesh mesh,
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

    // Use this function to check if the specified position and size is visible to camera
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

public class CustomMeshInstancedDrawer
{
    public static readonly int maxDrawMeshInstanceCount = 1023;

    private List<Matrix4x4> matrices; // Reuse Matrix4x4 array to prevent garbage, improve performance
    private MaterialPropertyBlock matProps;

    public CustomMeshInstancedDrawer(int matricesCount)
    {
        matricesCount = Mathf.Min(maxDrawMeshInstanceCount, matricesCount);
        matrices = new ();
        //tempMatrices = new();
        matProps = new();
    }

    public void DrawMeshInstanced(  Mesh mesh,
                                    Material material,
                                    Texture2D texture,
                                    Color color,
                                    List<Vector2> positions,
                                    Vector2 size,
                                    bool occlusionCulling = true)
    {
        int positionsListOffset = 0;
        while (positionsListOffset < positions.Count)
        {
            int positionsCount = Mathf.Min(maxDrawMeshInstanceCount, positions.Count - positionsListOffset);
            DrawMeshInstanceWithOffset(mesh,
                                        material,
                                        texture,
                                        color,
                                        positions,
                                        positionsCount,
                                        positionsListOffset,
                                        size,
                                        occlusionCulling);
            //Debug.Log("Prev offset: " + positionsListOffset);
            positionsListOffset += positionsCount;
            //Debug.Log("Post offset: " + positionsListOffset);
            //Debug.Log("Count: " + positions.Count);
        }

        //int totalDrawCount = 0;
        //for (int i = 0; i < positions.Count; i++)
        //{
        //    if (totalDrawCount >= matrices.Length) break;
        //    if (totalDrawCount >= maxDrawMeshInstanceCount) break;
        //    // If occulusionCulling is on, then the texture won't be rendered outside of camera's view
        //    if (occlusionCulling && !CustomTextureRenderer2D.CheckVisibility(positions[i], size)) continue;

        //    // Convert positions into the Matrix4x4 array
        //    matrices[totalDrawCount] = Matrix4x4.TRS(positions[i], Quaternion.identity, size);
        //    totalDrawCount++;
        //}

        //MaterialPropertyBlock matProps = new();
        //matProps.SetTexture(CustomTextureRenderer2D.mainTexID, texture);
        //matProps.SetColor(CustomTextureRenderer2D.colorID, color);

        //Graphics.DrawMeshInstanced(mesh, 0, material, matrices, totalDrawCount, matProps);

    }

    public void DrawMeshInstanceWithOffset( Mesh mesh,
                                            Material material,
                                            Texture2D texture,
                                            Color color,
                                            List<Vector2> positions,
                                            int positionsCount,
                                            int positionsListOffset,
                                            Vector2 size,
                                            bool occlusionCulling = true)
    {
        int totalDrawCount = 0;
        matrices.Clear();
        for (int i = positionsListOffset; i < positionsListOffset + positionsCount; i++)
        {
            //Debug.Log("i: " + i);
            if (i >= positions.Count) break;
            //if (totalDrawCount >= matrices.Length) break;
            if (totalDrawCount >= maxDrawMeshInstanceCount) break;
            // If occulusionCulling is on, then the texture won't be rendered outside of camera's view
            if (occlusionCulling && !CustomTextureRenderer2D.CheckVisibility(positions[i], size)) continue;

            // Convert positions into the Matrix4x4 array
            //Debug.Log($"i: {i}, position: {positions[i]}, size: {size}");
            matrices.Add(Matrix4x4.TRS(positions[i], Quaternion.identity, size));
            //Debug.Log("Matrix: " + Matrix4x4.TRS(positions[i], Quaternion.identity, new Vector3(size.x, size.y, 1)));
            //matrices[totalDrawCount] = Matrix4x4.TRS(positions[i], Quaternion.identity, size);
            //totalDrawCount++;
        }

        matProps.Clear();
        matProps.SetTexture(CustomTextureRenderer2D.mainTexID, texture);
        //matProps.SetColor(CustomTextureRenderer2D.colorID, color);

        //Debug.Log($"Draw: {mesh}, mat: {material}, matrices: {matrices.Count}");
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, matProps);
    }
}
