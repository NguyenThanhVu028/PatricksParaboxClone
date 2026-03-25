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
        { 0, 1, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 2, 5, 6, 0, 0, 0, 0, 0 },
        { 0, 3, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 4, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
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
    protected int[,] subdividedWallsGrid;

    /* This dictionary stores precalculated texture and grid positions for each static tile
     * Group static tiles that share the same texture
     */
    Dictionary<Texture2D, List<Vector2Int>> staticTiles = new();

    // This Render Texture is used to draw static tiles
    [SerializeField] RenderTexture staticTilesRT;

    public int Tiling { get => tiling; }

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

    // Need to subdivide the grid if wall textures are smaller than a tile
    private void SubdivideWallsGrid()
    {
        subdividedWallsGrid = new int[tiling * wallSubdivision, tiling * wallSubdivision];

        for(int rawGridRow = 0; rawGridRow < rawWallsGrid.GetLength(0); rawGridRow++)
        {
            for(int rawGridColumn = 0; rawGridColumn < rawWallsGrid.GetLength(1); rawGridColumn++)
            {
                for(int subdivisionRow = 0; subdivisionRow < wallSubdivision; subdivisionRow++)
                {
                    for (int subDivisionColumn = 0; subDivisionColumn < wallSubdivision; subDivisionColumn++)
                    {
                        subdividedWallsGrid[rawGridRow * wallSubdivision + subdivisionRow, rawGridColumn * wallSubdivision + subDivisionColumn] = rawWallsGrid[rawGridRow, rawGridColumn];
                    }
                }
            }
        }
    }
    public void CalculateStaticTiles()
    {
        SubdivideWallsGrid();

        if (staticTilesRT == null)
        {
            int renderTextureSize = tiling * wallSubdivision * tileTextureSize;
            staticTilesRT = new RenderTexture(renderTextureSize, renderTextureSize, staticTilesRTDepth);
            staticTilesRT.filterMode = FilterMode.Point;

        }

        staticTiles.Clear();

        for (int row = 0; row < subdividedWallsGrid.GetLength(0); row++)
        {
            for (int column = 0; column < subdividedWallsGrid.GetLength(1); column++)
            {
                // Calculate texture positions for every wall tiles
                if (subdividedWallsGrid[row, column] == 1)
                {
                    var wallTex = wallRuleTile.GetTexture(subdividedWallsGrid, row, column);

                    if (!staticTiles.ContainsKey(wallTex)) staticTiles.Add(wallTex, new());
                    staticTiles[wallTex].Add(new Vector2Int(row, column));
                }
            }
        }

        CustomTextureRenderer2D.DrawGridToRenderTexture(ref staticTilesRT, subdividedWallsGrid.GetLength(0), subdividedWallsGrid.GetLength(1), staticTiles, cubeMat);
    }

    private void InitChildCubes()
    {
        CubesManager cubesManager = CubesManager.Instance;
        if (cubesManager == null ) { Debug.LogWarning("No cubes manager is found in this scene to spawn cubes!"); return; }

        // Init support cubes
        for(int row = 0; row < supportCubesGrid.GetLength(0); row++)
        {
            for(int column = 0; column < supportCubesGrid.GetLength(1); column++)
            {
                var supportCube = cubesManager.GetCubeInScene(supportCubesGrid[row, column]);
                if (supportCube == null) continue;

                supportCube.RelativeSize = new Vector2(1.0f / supportCubesGrid.GetLength(1), 1.0f / supportCubesGrid.GetLength(0));
                supportCube.RelativePosition = Relativity.RPosFromGridTile(supportCubesGrid.GetLength(1), supportCubesGrid.GetLength(0), row, column);

                supportCube.Parent = this;
                childCubes.Add(supportCube);
            }
        }
    }

    private void DrawFloor(Rect position)
    {
        Color cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(color);
        CustomTextureRenderer2D.RenderMesh(cubeMesh, cubeMat, floorTexture, cubeColor, position.position, position.size);
    }

    public void DrawWalls(Rect position)
    {
        if (subdividedWallsGrid == null) return;
        if (wallRuleTile == null || floorTexture == null)
        {
            Debug.LogWarning($"Enterable cube: {name} is missing wall or floor tile reference.");
            return;
        }

        Color cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(color);
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
            childCube.Draw(Relativity.CRectFromPRect(position, childCube.RelativeSize, childCube.RelativePosition));
        }
    }
}
