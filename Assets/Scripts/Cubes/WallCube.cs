using UnityEngine;

public class WallCube : Cube
{
    [SerializeField] int wallSubdivision = 2;
    [SerializeField] Texture2D[,] cubeTex;

    private void OnEnable()
    {
        if (GetComponent<CubeMovement>() != null)
        {
            GetComponent<CubeMovement>().OnMoveStart.AddListener(OnMoveStart);
        }

        onParentChanged.AddListener(OnParentChanged);
    }
    private void OnDisable()
    {
        if (GetComponent<CubeMovement>() != null)
        {
            GetComponent<CubeMovement>().OnMoveStart.RemoveListener(OnMoveStart);
        }

        onParentChanged.RemoveListener(OnParentChanged);
    }

    protected void OnMoveStart()
    {
        if (parent != null)
        {
            parent.CalculateStaticTextures();
        }
    }

    protected void OnParentChanged()
    {
        cubeColor = parent.CubeColor;
    }

    public override void DrawCube(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        Vector2 texSize = new((float)position.width / wallSubdivision, (float)position.height / wallSubdivision);
        Vector2 startingPos = new(position.x - position.width * 0.5f + texSize.x * 0.5f, position.y + position.height * 0.5f - texSize.y * 0.5f);

        for (int row = 0; row < wallSubdivision; row++)
        {
            for (int col = 0; col < wallSubdivision; col++)
            {
                Vector2 texPos = new Vector2(startingPos.x + col * texSize.x, startingPos.y - row * texSize.y);
                CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, cubeTex[row, col], RealCubeColor, exposure, texPos, texSize, depth, scissorRect);
            }
        }
    }

    public void SetSubdivision(int subdivision)
    {
        wallSubdivision = subdivision;
        cubeTex = new Texture2D[subdivision, subdivision];
    }
    public void SetTexture(Texture2D texture, int row, int col)
    {
        if (cubeTex == null || texture == null) return;
        if (row > cubeTex.GetLength(0) || col > cubeTex.GetLength(1)) return;
        cubeTex[row, col] = texture;
    }
}
