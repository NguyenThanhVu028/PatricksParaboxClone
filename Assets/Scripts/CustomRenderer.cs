using UnityEngine;

public class CustomRenderer : MonoBehaviour
{
    public static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    public static readonly int ColorID = Shader.PropertyToID("_Color");

    public static void RenderTexture(Mesh mesh, 
                                    Material material, 
                                    Texture2D texture, 
                                    Color color, 
                                    Vector2 position, 
                                    Vector2 size)
    {
        MaterialPropertyBlock matProps = new();
        matProps.SetTexture(MainTexID, texture);
        matProps.SetColor(ColorID, color);

        RenderTexture(mesh, material, matProps, position, size);
    }

    public static void RenderTexture(Mesh mesh,
                                    Material material, 
                                    MaterialPropertyBlock matProps, 
                                    Vector2 position, 
                                    Vector2 size)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, size); //  Calculate position

        RenderParams rp = new RenderParams(material); // Calculate render parameters
        rp.matProps = matProps;

        Graphics.RenderMesh(rp, mesh, 0, matrix);
    }
}
