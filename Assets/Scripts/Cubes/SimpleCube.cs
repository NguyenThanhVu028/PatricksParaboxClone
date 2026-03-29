using UnityEngine;

public class SimpleCube : Cube
{
    [SerializeField] Texture2D cubeText;
    protected override void DrawCube(Rect position, float depth = 0)
    {
        Color cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(base.cubeColor);
        CustomTextureRenderer2D.RenderMesh(cubeMesh, cubeMat, cubeText, cubeColor, position.position, position.size, depth);
    }
}
