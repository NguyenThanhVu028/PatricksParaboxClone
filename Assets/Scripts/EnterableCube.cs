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
    [Header("Tiles")]
    [SerializeField] int tiling = 9;
    [Min(1)]
    [SerializeField] int wallFloorSubdivision = 2; // Wall and floor tiles might smaller than a cube
    [SerializeField] CustomRuleTile wallRuleTile;
    [SerializeField] Texture2D floorTile;
    [Header("Rendering")]
    [SerializeField] Material tileMat;
    [SerializeField] Mesh tileMesh;

    private MaterialPropertyBlock matPropBlock;
    private readonly int MainTexID = Shader.PropertyToID("_MainTex");
    private readonly int ColorID = Shader.PropertyToID("_Color");

    // This grid contains id of tiles, used to design walls and floors
    // 0: floor
    // 1: wall
    //[SerializeField]
    int[,] rawWallsGrid = new int[,]
    {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 0, 1, 1, 0, 0, 1, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 1, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1 }
    };

    // This grid contains id of cubes that can be instantiated by cube manager
    [SerializeField]
    int[,] supportCubesGrid = new int[,]
    {
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 0, 0, 0 }
    };

    // This grid contains id of cubes that cannot be instantiated (enterable cubes, ...)
    [SerializeField]
    int[,] cubesGrid = new int[,]
    {
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 0, 0 }
    };

    // This is the actual walls and floors grid that will be drawn on screen
    private int[,] subdividedWallsGrid;

    private void Start()
    {
        matPropBlock = new();
        SubdivideWallsGrid();
        InitChildrenCubes();
    }

    private void Update()
    {
        Draw(new Rect(0, 0, 10, 10));
    }

    public override void Draw(Rect position)
    {
        //base.Draw(position);
        DrawWallsAndFloors(position);
        DrawChildrenCubes(position);
    }

    private void SubdivideWallsGrid()
    {
        subdividedWallsGrid = new int[tiling * wallFloorSubdivision, tiling * wallFloorSubdivision];

        for(int rawGridRow = 0; rawGridRow < rawWallsGrid.GetLength(0); rawGridRow++)
        {
            for(int rawGridColumn = 0; rawGridColumn < rawWallsGrid.GetLength(1); rawGridColumn++)
            {
                Debug.Log($"Raw row: {rawGridRow}, raw column: {rawGridColumn}");
                for(int subdivisionRow = 0; subdivisionRow < wallFloorSubdivision; subdivisionRow++)
                {
                    for (int subDivisionColumn = 0; subDivisionColumn < wallFloorSubdivision; subDivisionColumn++)
                    {
                        subdividedWallsGrid[rawGridRow * wallFloorSubdivision + subdivisionRow, rawGridColumn * wallFloorSubdivision + subDivisionColumn] = rawWallsGrid[rawGridRow, rawGridColumn];
                    }
                }
            }
        }
    }

    // Unverified
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
        for (int i = 0; i < supportCubesGrid.GetLength(0); i++){
            for(int j = 0; j < supportCubesGrid.GetLength(1); j++)
            {
                var cube = cubesManager.GetCubePrefab(supportCubesGrid[i, j]);
                if (cube == null) continue;

                cube.RelativeSize = new(cubeRelativeSize, cubeRelativeSize);
                cube.RelativePosition = new(topLeftRelativePosition.x + i * cubeRelativeSize, topLeftRelativePosition.y - j * cubeRelativeSize);

                cube.transform.SetParent(this.gameObject.transform);
            }
        }

        // Init other cubes
        for (int i = 0; i < cubesGrid.GetLength(0); i++)
        {
            for (int j = 0; j < cubesGrid.GetLength(1); j++)
            {
                var cube = cubesManager.GetCubeInScene(cubesGrid[i, j]);
                if (cube == null) continue;

                cube.RelativeSize = new(cubeRelativeSize, cubeRelativeSize);
                cube.RelativePosition = new(topLeftRelativePosition.x + i * cubeRelativeSize, topLeftRelativePosition.y - j * cubeRelativeSize);

                cube.transform.SetParent(this.gameObject.transform);
            }
        }
    }

    private void DrawWallsAndFloors(Rect position)
    {
        if (wallRuleTile == null || floorTile == null)
        {
            Debug.LogWarning($"Cube: {name} is missing wall or floor tile reference.");
            return;
        }

        Vector2 tileSize = new Vector2(position.width / (tiling * wallFloorSubdivision), position.height / (tiling * wallFloorSubdivision));
        Vector2 startingPosition = new(position.x - position.width * 0.5f + tileSize.x * 0.5f, position.y + position.height * 0.5f - tileSize.y * 0.5f);

        for (int row = 0; row < subdividedWallsGrid.GetLength(0); row++)
        {
            for (int column = 0; column < subdividedWallsGrid.GetLength(1); column++)
            {
                Vector2 tilePosition = new(startingPosition.x + tileSize.x * column, startingPosition.y - tileSize.y * row);
                Matrix4x4 matrix = Matrix4x4.TRS(tilePosition, Quaternion.identity, tileSize);

                // Draw floors
                matPropBlock.SetTexture(MainTexID, floorTile);
                matPropBlock.SetColor(ColorID, Color.green);
                RenderParams rp = new RenderParams(tileMat);
                rp.matProps = matPropBlock;
                Graphics.RenderMesh(rp, tileMesh, 0, matrix);

                // Draw walls
                if (subdividedWallsGrid[row, column] == 1)
                {
                    var wallTex = wallRuleTile.GetTile(subdividedWallsGrid, row, column);
                    matPropBlock.SetTexture(MainTexID, wallTex);
                    matPropBlock.SetColor(ColorID, Color.green);
                    rp = new RenderParams(tileMat);
                    rp.matProps = matPropBlock;
                    Graphics.RenderMesh(rp, tileMesh, 0, matrix);
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
