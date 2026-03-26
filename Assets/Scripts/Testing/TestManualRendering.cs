using UnityEngine;
using UnityEngine.Rendering;

public class TestManualRendering : MonoBehaviour
{
    [SerializeField] Cube cubeToFocus;
    //[SerializeField] Texture2D textureToRender;
    [SerializeField] Texture2D defaultTile;
    [SerializeField] Texture2D wallTile;
    [SerializeField] Texture2D floorTile;
    [SerializeField] Material tileMat;
    [SerializeField] Mesh tileMesh;
    [Header("Render zone")]
    [SerializeField] Vector2 renderZoneSize = new Vector2(9, 9);
    [SerializeField] Vector2 renderZonePos = Vector2.zero;

    private MaterialPropertyBlock matPropBlock;
    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    private int[,] grid = new int[3, 3]
    {
        {1, 1, 0 },
        {0, 0, 1 },
        {1, 0, 0 }
    };

    private void Start()
    {
        matPropBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        Render();
    }

    private void Render()
    {
        Vector2 tileSize = new Vector2(renderZoneSize.x / grid.GetLength(0), renderZoneSize.y / grid.GetLength(0));
        Vector2 startingPoint = new Vector2(renderZonePos.x - renderZoneSize.x * 0.5f + tileSize.x * 0.5f, renderZonePos.y + renderZoneSize.y * 0.5f - tileSize.y * 0.5f);
        

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                Vector2 renderPos = new Vector2(startingPoint.x + (j * tileSize.x), startingPoint.y - i * tileSize.y);

                if (grid[i, j] == 1)
                {
                    matPropBlock.SetTexture(MainTexID, wallTile);
                }
                else
                {
                    matPropBlock.SetTexture(MainTexID, floorTile);
                }

                matPropBlock.SetColor(ColorID, Color.yellow);
                Matrix4x4 matrix = Matrix4x4.TRS(renderPos, Quaternion.identity, tileSize);
                RenderParams rp = new RenderParams(tileMat);
                rp.matProps = matPropBlock;
                Graphics.RenderMesh(rp, tileMesh, 0, matrix);
            }
        }


    }
}
