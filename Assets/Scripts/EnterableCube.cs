using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EnterableCube : Cube
{
    [Header("Cube properties")]
    [SerializeField] bool isReversed = false;
    [SerializeField] int tiling = 4;
    [Header("Rendering")]
    [SerializeField] Material tileMat;
    [SerializeField] Mesh tileMesh;
    [Header("Rule Tiles")]
    [SerializeField] List<RuleTileDetail> ruleTiles = new();

    private MaterialPropertyBlock matPropBlock;
    private readonly int MainTexID = Shader.PropertyToID("_MainTex");
    private readonly int ColorID = Shader.PropertyToID("_Color");

    /* Tile grid conventions:
     * <0 : tiles
     * 0: floor
     * >0: cubes
     */

    // This array is used to draw walls and floors, each element contains ID of a tile
    [SerializeField]
    int[,] wallFloorGrid = new int[,]
    {
        { -1, -1, -1, -1 },
        { -1,  0, -1, -1 },
        { -1,  0,  0, -1 },
        { -1, -1, -1, -1 },
    };

    // This array is used to contain position of cubes that can be instantiated by cube manager
    // Each element contains ID of the cube
    [SerializeField]
    int[,] supportCubeGrid = new int[,]
    {
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 0, 0, 0 }
    };

    // This array is used to contain position of cubes that cannot be instantiated (enterable cubes, ...)
    // Each element contains the ID of the cube
    [SerializeField]
    int[,] cubeGrid = new int[,]
{
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 0, 0 }
};

    private void Start()
    {
        matPropBlock = new();
        InitChildrenCubes();
    }

    private void Update()
    {
        Draw(new Rect(0, 0, 10, 10));
    }

    public override void Draw(Rect position)
    {
        //base.Draw(position);
        DrawTiles(position);
        DrawChildrenCubes(position);
    }

    // This function is used to filter all the cubes out of the grid
    private void InitChildrenCubes()
    {
        CubesManager cubesManager = CubesManager.Instance;
        if (cubesManager == null)
        {
            Debug.LogWarning("Can't init children cubes of " + name + ", CubesManager not found!");
            return;
        }

        float cubeRelativeSize = 1.0f / tiling;
        Vector2 topLeftRelativePosition = new Vector2((-tiling * 0.5f + 0.5f) / tiling, (tiling * 0.5f - 0.5f) / tiling);
        
        // Init cubes that need to be instantiated
        for (int i = 0; i < supportCubeGrid.GetLength(0); i++){
            for(int j = 0; j < supportCubeGrid.GetLength(1); j++)
            {
                var cube = cubesManager.GetCubePrefab(supportCubeGrid[i, j]);
                if (cube == null) continue;

                cube.RelativeSize = new(cubeRelativeSize, cubeRelativeSize);
                cube.RelativePosition = new(topLeftRelativePosition.x + i * cubeRelativeSize, topLeftRelativePosition.y - j * cubeRelativeSize);

                cube.transform.SetParent(this.gameObject.transform);
            }
        }

        // Init other cubes
        for (int i = 0; i < cubeGrid.GetLength(0); i++)
        {
            for (int j = 0; j < cubeGrid.GetLength(1); j++)
            {
                var cube = cubesManager.GetCubeInScene(cubeGrid[i, j]);
                if (cube == null) continue;

                cube.RelativeSize = new(cubeRelativeSize, cubeRelativeSize);
                cube.RelativePosition = new(topLeftRelativePosition.x + i * cubeRelativeSize, topLeftRelativePosition.y - j * cubeRelativeSize);

                cube.transform.SetParent(this.gameObject.transform);
            }
        }
    }

    private void DrawTiles(Rect position)
    {
        Vector2 tileSize = new Vector2(position.width / tiling, position.height / tiling);
        Vector2 startingPosition = new(position.x - position.width * 0.5f + tileSize.x * 0.5f, position.y + position.height * 0.5f - tileSize.y * 0.5f);

        for (int i = 0; i < wallFloorGrid.GetLength(0); i++)
        {
            for (int j = 0; j < wallFloorGrid.GetLength(1); j++)
            {
                foreach(var ruleTileDetail in ruleTiles)
                {
                    if (ruleTileDetail == null) continue;
                    if (ruleTileDetail.ID == wallFloorGrid[i, j])
                    {
                        var tileTex = ruleTileDetail.RuleTile.GetTile(wallFloorGrid, new Vector2(i, j));

                        Vector2 tilePosition = new(startingPosition.x + tileSize.x * j, startingPosition.y - tileSize.y * i);
                        Debug.Log($"i: {i}, j: {j}, pos: {tilePosition}, id: {wallFloorGrid[i, j]}");
                        matPropBlock.SetTexture(MainTexID, tileTex);
                        matPropBlock.SetColor(ColorID, Color.green);
                        Matrix4x4 matrix = Matrix4x4.TRS(tilePosition, Quaternion.identity, tileSize);
                        RenderParams rp = new RenderParams(tileMat);
                        rp.matProps = matPropBlock;
                        Graphics.RenderMesh(rp, tileMesh, 0, matrix);

                        break;
                    }
                }
            }
        }
    }

    private void DrawChildrenCubes(Rect position)
    {

    }

    [Serializable]
    public class RuleTileDetail
    {
        [SerializeField] int id;
        [SerializeField] CustomRuleTile ruleTile;

        public int ID { get => id; }
        public CustomRuleTile RuleTile { get => ruleTile; }
    }
}
