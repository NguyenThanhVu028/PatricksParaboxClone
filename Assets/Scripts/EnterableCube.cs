using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EnterableCube : Cube
{
    [Header("Enterable cube properties")]
    [SerializeField] bool isReversed = false;
    [SerializeField] List<Cube> childCubes = new();
    [Header("Tiling")]
    [SerializeField] int tiling = 9;
    [Min(1)]
    [SerializeField] int wallFloorSubdivision = 2; // Wall and floor tiles might smaller than a cube
    [SerializeField] CustomRuleTile wallRuleTile;
    [SerializeField] Texture2D floorTexture;


    // This grid is used to mark position of the walls
    //[SerializeField]
    int[,] rawWallsGrid = new int[,]
    {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 0, 1, 1, 0, 0, 1, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 1, 0, 1, 1, 0, 0, 1 },
        { 1, 0, 1, 0, 1, 1, 0, 0, 1 },
        { 1, 0, 1, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 1, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1 }
    };

    // This grid contains id of cubes that can be instantiated by cube manager (normal cube, ...)
    [SerializeField]
    int[,] supportCubesGrid = new int[,]
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 1, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 1, 0, 0, 1, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 1, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
    };

    // This grid contains id of cubes that cannot be instantiated (enterable cubes, ...)
    [SerializeField]
    int[,] cubesGrid = new int[,]
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 1, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
    };

    // This is the actual walls and floors grid that will be drawn on screen
    private int[,] subdividedWallsGrid;

    public int Tiling { get => tiling; }

    private void Start()
    {
        SubdivideWallsGrid();
        InitChildCubes();
    }

    private void Update()
    {
        //Draw(new Rect(0, 0, 10, 10));
    }

    public override void Draw(Rect position)
    {
        DrawWallsAndFloor(position);
        DrawChildCubes(position);
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
    private void InitChildCubes()
    {
        CubesManager cubesManager = CubesManager.Instance;
        if (cubesManager == null ) { Debug.LogWarning("No cubes manager is found in this scene to spawn cubes!"); return; }

        // Init support cubes
        for(int row = 0; row < supportCubesGrid.GetLength(0); row++)
        {
            for(int column = 0; column < supportCubesGrid.GetLength(1); column++)
            {
                var supportCube = cubesManager.GetCubePrefab(supportCubesGrid[row, column]);
                if (supportCube == null) continue;

                supportCube.RelativeSize = new Vector2(1.0f / supportCubesGrid.GetLength(1), 1.0f / supportCubesGrid.GetLength(0));
                supportCube.RelativePosition = Relativity.RPosFromGridTile(supportCubesGrid.GetLength(1), supportCubesGrid.GetLength(0), row, column);

                supportCube.Parent = this;
                childCubes.Add(supportCube);
            }
        }
    }

    public void DrawWallsAndFloor(Rect position)
    {
        if (wallRuleTile == null || floorTexture == null)
        {
            Debug.LogWarning($"Enterable cube: {name} is missing wall or floor tile reference.");
            return;
        }

        Color cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(color);

        // Draw floor
        CustomRenderer.RenderTexture(cubeMesh, cubeMat, floorTexture, cubeColor, position.position, position.size);

        Vector2 tileSize = new Vector2(position.width / (tiling * wallFloorSubdivision), position.height / (tiling * wallFloorSubdivision));
        Vector2 startingPosition = new(position.x - position.width * 0.5f + tileSize.x * 0.5f, position.y + position.height * 0.5f - tileSize.y * 0.5f);

        for (int row = 0; row < subdividedWallsGrid.GetLength(0); row++)
        {
            for (int column = 0; column < subdividedWallsGrid.GetLength(1); column++)
            {
                Vector2 tilePosition = new(startingPosition.x + tileSize.x * column, startingPosition.y - tileSize.y * row);

                // Draw walls
                if (subdividedWallsGrid[row, column] == 1)
                {
                    var wallTex = wallRuleTile.GetTexture(subdividedWallsGrid, row, column);
                    CustomRenderer.RenderTexture(cubeMesh, cubeMat, wallTex, cubeColor, tilePosition, tileSize);
                }
            }
        }
    }
    private void DrawChildCubes(Rect position)
    {
        foreach(var childCube in childCubes)
        {
            if (childCube == null) continue;
            childCube.Draw(Relativity.CRectFromPRect(position, childCube.RelativeSize, childCube.RelativePosition));
        }
    }
}
