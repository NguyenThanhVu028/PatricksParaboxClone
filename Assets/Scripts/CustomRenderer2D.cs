using UnityEngine;

public class CustomRenderer2D : MonoBehaviour
{
    public static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    public static readonly int ColorID = Shader.PropertyToID("_Color");

    public static void RenderTexture(Mesh mesh,
                                    Material material,
                                    Texture2D texture,
                                    Color color,
                                    Vector2 position,
                                    Vector2 size,
                                    bool occlusionCulling = true)
    {
        // If occulusionCulling is on, then the texture won't be rendered outside of camera's view
        if (occlusionCulling && !CheckVisibility(position, size)) return;

        MaterialPropertyBlock matProps = new();
        matProps.SetTexture(MainTexID, texture);
        matProps.SetColor(ColorID, color);

        RenderTexture(mesh, material, matProps, position, size, occlusionCulling);
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
