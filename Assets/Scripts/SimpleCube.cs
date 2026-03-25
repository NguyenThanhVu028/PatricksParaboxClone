using UnityEngine;

public class SimpleCube : Cube
{
    [SerializeField] Texture2D cubeText;
    protected override void DrawCube(Rect position)
    {
        Color cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(color);
        CustomTextureRenderer2D.RenderTexture(cubeMesh, cubeMat, cubeText, cubeColor, position.position, position.size);
    }
}
