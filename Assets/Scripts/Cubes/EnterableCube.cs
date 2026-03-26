using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class EnterableCube : Cube
{
    [Header("Enterable cube properties")]
    [SerializeField] bool isReversed = false;
    [SerializeField] List<Cube> childCubes = new();
    [Header("Tiling")]
    [SerializeField] int tiling = 9;
    [Min(1)]
    [SerializeField] int wallSubdivision = 2; // Wall texture might be small than a tile
    [SerializeField] CustomRuleTile wallRuleTile;
    [SerializeField] Texture2D floorTexture;
    [Header("Enterable cube rendering")]
    [SerializeField] int tileTextureSize = 16;
    [SerializeField] int staticTilesRTDepth = 16;

    // This grid contains ID of cubes from the CubesManager
    // Enterable cube uses this grid to initiate its children
    [SerializeField]
    int[,] cubesIDGrid = new int[,]
    {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 0, 1, 1, 0, 0, 1, 0, 1 },
        { 1, 2, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 1, 3, 1, 1, 0, 0, 1 },
        { 1, 0, 1, 0, 1, 1, 0, 0, 1 },
        { 1, 4, 1, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 1, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1 }
    };

    private Cube[,] cubesGrid;

    // This grid is used to mark position of the walls
    //[SerializeField]
    //int[,] rawWallsGrid = new int[,]
    //{
    //    { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    //    { 1, 0, 1, 1, 0, 0, 1, 0, 1 },
    //    { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
    //    { 1, 0, 1, 0, 1, 1, 0, 0, 1 },
    //    { 1, 0, 1, 0, 1, 1, 0, 0, 1 },
    //    { 1, 0, 1, 0, 0, 1, 0, 0, 1 },
    //    { 1, 0, 1, 0, 0, 0, 0, 0, 1 },
    //    { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
    //    { 1, 1, 1, 1, 1, 1, 1, 1, 1 }
    //};

    // This grid contains id of cubes that can be instantiated by cube manager (normal cube, ...)
    //[SerializeField]
    //int[,] supportCubesGrid = new int[,]
    //{
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 1, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 2, 5, 6, 0, 0, 0, 0, 0 },
    //    { 0, 3, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 4, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
    //};

    // This grid contains id of cubes that cannot be instantiated (enterable cubes, ...)
    //[SerializeField]
    //int[,] cubesGrid = new int[,]
    //{
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 1, 0, 0, 0 },
    //    { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
    //};

    public int Tiling { get => tiling; }

    // This is the actual walls and floors grid that will be drawn on screen
    //protected int[,] wallsGrid;

    /* This dictionary stores calculated texture and grid positions for each static tile
     * Group static tiles that share the same texture together. */
    Dictionary<Texture2D, List<Vector2Int>> staticTiles = new();

    // All static tiles will be drawn onto this Render Texture which will later be drawn in Update
    // Use RenderTexture to reduce calculation on static tiles
    [SerializeField] RenderTexture staticTilesRT;

    private void Reset()
    {
        instantiable = false;
    }
    private void Start()
    {
        InitChildCubes();
        CalculateStaticTiles();
    }

    protected override void DrawCube(Rect position)
    {
        DrawFloor(position);
        DrawWalls(position);
        DrawChildCubes(position);
    }
    public void CalculateStaticTiles()
    {
        var wallsGrid = CalculateWallsGrid();

        if (staticTilesRT == null)
        {
            int renderTextureSize = tiling * wallSubdivision * tileTextureSize;
            staticTilesRT = new RenderTexture(renderTextureSize, renderTextureSize, staticTilesRTDepth);
            staticTilesRT.filterMode = FilterMode.Point;

        }

        staticTiles.Clear();

        /* Calculate wall tiles logic:
         * - Loop through cubes grid to find wall cubes, ignore those that are or can be player
         * - Having the position in the cubes grid -> Find suitable Texture for each tile at the coresponding positions in walls grid 
         * - Store textures with their positions data
         * */
        
        for (int cubesGridRow = 0; cubesGridRow < cubesGrid.GetLength(0); cubesGridRow++)
        {
            for (int cubesGridColumn = 0; cubesGridColumn < cubesGrid.GetLength(1); cubesGridColumn++)
            {
                if (cubesGrid[cubesGridRow, cubesGridColumn] == null) continue;
                if (!(cubesGrid[cubesGridRow, cubesGridColumn] is WallCube)) continue;
                if (cubesGrid[cubesGridRow, cubesGridColumn].CanBePlayer || cubesGrid[cubesGridRow, cubesGridColumn].IsPlayer) continue;

                (cubesGrid[cubesGridRow, cubesGridColumn] as WallCube).SetSubdivision(wallSubdivision);

                for (int subdivisionRow = 0; subdivisionRow < wallSubdivision; subdivisionRow++)
                {
                    for (int subDivisionColumn = 0; subDivisionColumn < wallSubdivision; subDivisionColumn++)
                    {
                        int wallsGridRow = cubesGridRow * wallSubdivision + subdivisionRow;
                        int wallsGridColumn = cubesGridColumn * wallSubdivision + subDivisionColumn;
                        var wallTex = wallRuleTile.GetTexture(wallsGrid, wallsGridRow, wallsGridColumn);

                        if (!staticTiles.ContainsKey(wallTex)) staticTiles.Add(wallTex, new());
                        staticTiles[wallTex].Add(new Vector2Int(wallsGridRow, wallsGridColumn));

                        (cubesGrid[cubesGridRow, cubesGridColumn] as WallCube).SetTexture(wallTex, subdivisionRow, subDivisionColumn);
                    }
                }
            }
        }

        CustomTextureRenderer2D.DrawGridToRenderTexture(ref staticTilesRT, wallsGrid.GetLength(0), wallsGrid.GetLength(1), staticTiles, cubeMat);
    }

    private int[,] CalculateWallsGrid()
    {
        if (cubesGrid == null) return null;

        int[,] wallsGrid = new int[tiling * wallSubdivision, tiling * wallSubdivision];

        for (int cubesGridRow = 0; cubesGridRow < cubesGrid.GetLength(0); cubesGridRow++)
        {
            for (int cubesGridColumn = 0; cubesGridColumn < cubesGrid.GetLength(1); cubesGridColumn++)
            {
                if (!(cubesGrid[cubesGridRow, cubesGridColumn] is WallCube)) continue;

                for (int subdivisionRow = 0; subdivisionRow < wallSubdivision; subdivisionRow++)
                {
                    for (int subDivisionColumn = 0; subDivisionColumn < wallSubdivision; subDivisionColumn++)
                    {
                        wallsGrid[cubesGridRow * wallSubdivision + subdivisionRow, cubesGridColumn * wallSubdivision + subDivisionColumn] = 1;
                    }
                }
            }
        }

        return wallsGrid;
    }

    private void InitChildCubes()
    {
        CubesManager cubesManager = CubesManager.Instance;
        if (cubesManager == null ) { Debug.LogWarning("No cubes manager is found in this scene to spawn cubes!"); return; }

        // Init cubes
        cubesGrid = new Cube[cubesIDGrid.GetLength(0), cubesIDGrid.GetLength(1)];
        for(int row = 0; row < cubesIDGrid.GetLength(0); row++)
        {
            for(int column = 0; column < cubesIDGrid.GetLength(1); column++)
            {
                var spawnedCube = cubesManager.GetCube(cubesIDGrid[row, column]);
                if (spawnedCube == null) continue;

                spawnedCube.RelativeSize = new Vector2(1.0f / cubesIDGrid.GetLength(1), 1.0f / cubesIDGrid.GetLength(0));
                spawnedCube.RelativePosition = Relativity.RPosFromGridTile(cubesIDGrid.GetLength(1), cubesIDGrid.GetLength(0), row, column);

                spawnedCube.Parent = this;
                childCubes.Add(spawnedCube);
                cubesGrid[row, column] = spawnedCube;

                if (spawnedCube is WallCube)
                {
                    spawnedCube.CubeColor = cubeColor;
                }
            }
        }
    }

    private void DrawFloor(Rect position)
    {
        Color cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(base.cubeColor);
        CustomTextureRenderer2D.RenderMesh(cubeMesh, cubeMat, floorTexture, cubeColor, position.position, position.size);
    }

    public void DrawWalls(Rect position)
    {
        //if (wallsGrid == null) return;
        if (wallRuleTile == null || floorTexture == null)
        {
            Debug.LogWarning($"Enterable cube: {name} is missing wall or floor tile reference.");
            return;
        }

        Color cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(base.cubeColor);
        if (staticTilesRT != null) CustomTextureRenderer2D.RenderMesh(cubeMesh, cubeMat, staticTilesRT, cubeColor, position.position, position.size);

        //Color cubeColor = Color.white;
        //if (colorPalette != null) cubeColor = colorPalette.GetColor(color);
        
        //Vector2 tileSize = new Vector2(position.width / (tiling * wallSubdivision), position.height / (tiling * wallSubdivision));
        //Vector2 startingPosition = new(position.x - position.width * 0.5f + tileSize.x * 0.5f, position.y + position.height * 0.5f - tileSize.y * 0.5f);

        //Dictionary<Texture2D, List<Vector2>> positionsToDraw = new();
        //foreach (var entry in wallTilesTexture)
        //{
        //    foreach (var tile in entry.Value)
        //    {
        //        Vector2 tilePosition = new(startingPosition.x + tileSize.x * tile.y, startingPosition.y - tileSize.y * tile.x);
        //        if (!positionsToDraw.ContainsKey(entry.Key)) positionsToDraw.Add(entry.Key, new());
        //        positionsToDraw[entry.Key].Add(tilePosition);
        //    }
        //}

        //if (customMeshInstancedDrawer != null)
        //{
        //    foreach (var positionToDraw in positionsToDraw)
        //    {
        //        if (positionToDraw.Key == null || positionToDraw.Value == null) continue;
        //        customMeshInstancedDrawer.DrawMeshInstanced(cubeMesh,
        //                                                    cubeMat,
        //                                                    positionToDraw.Key,
        //                                                    cubeColor,
        //                                                    positionToDraw.Value,
        //                                                    tileSize,
        //                                                    true);
        //    }
        //}

        //for (int row = 0; row < subdividedWallsGrid.GetLength(0); row++)
        //{
        //    for (int column = 0; column < subdividedWallsGrid.GetLength(1); column++)
        //    {
        //        Vector2 tilePosition = new(startingPosition.x + tileSize.x * column, startingPosition.y - tileSize.y * row);

        //        // Draw walls
        //        if (subdividedWallsGrid[row, column] == 1)
        //        {
        //            var wallTex = wallRuleTile.GetTexture(subdividedWallsGrid, row, column);
        //            CustomTextureRenderer2D.RenderTexture(cubeMesh, cubeMat, wallTex, cubeColor, tilePosition, tileSize);
        //        }
        //    }
        //}
    }
    private void DrawChildCubes(Rect position)
    {
        foreach(var childCube in childCubes)
        {
            if (childCube == null) continue;
            if (childCube is WallCube) // Ignore walls that aren't or can't potentially be a player
            {
                if (!(childCube.CanBePlayer || childCube.IsPlayer)) continue;
            }
            childCube.Draw(Relativity.CRectFromPRect(position, childCube.RelativeSize, childCube.RelativePosition));
        }
    }

    [Serializable]
    public class StaticTileDetails
    {
        [SerializeField] List<Vector2Int> positions = new();
        [SerializeField] Vector2 size;

        public List<Vector2Int> Positions { get => positions; }
        public Vector2 Size { get => size; }
    } 
}
