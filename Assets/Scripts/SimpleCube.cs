using UnityEngine;

public class SimpleCube : Cube
{
    [SerializeField] Texture2D cubeText;
    public override void Draw(Rect position)
    {
        Color cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(color);
        CustomRenderer.RenderTexture(cubeMesh, cubeMat, cubeText, cubeColor, position.position, position.size);
    }
}
