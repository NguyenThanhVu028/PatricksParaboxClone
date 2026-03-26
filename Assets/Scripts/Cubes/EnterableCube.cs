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
    [HideInInspector]
    [Min(1)]
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
    [Min(0)]
    int[] cubesIDGrid = {0};

    private Cube[,] cubesGrid;

    public int Tiling { get => tiling; }
    public int[] CubesIDGrid { get => cubesIDGrid; }
    public int GetCubeIDInGrid(int row, int col)
    {
        int index = row * tiling + col;
        if (index >= cubesIDGrid.Length) return -1;
        return cubesIDGrid[index];
    }
    public void SetCubeIDInGrid(int row, int col, int value)
    {
        int index = row * tiling + col;
        if (index >= cubesIDGrid.Length) return;
        cubesIDGrid[index] = value;
    }

    /* This dictionary stores calculated static textures and their positions in grid
     * One texture could be drawn at different positions with different grid sizes.
     * Currently, only walls are considered as static tiles.
     * Clear this list and add new textures to draw on different layers.
     */
    protected Dictionary<Texture2D, List<CustomTextureRenderer2D.TexturePositionInGrid>> staticTextures = new();

    // All static textures are drawn onto this Render Texture which will later be drawn in Update.
    // Use RenderTexture to reduce calculations on static tiles.
    [SerializeField] RenderTexture staticTexturesRT;

    private void Reset()
    {
        needInstantiating = false;
    }
    private void Start()
    {
        InitChildCubes();
        CalculateStaticTextures();
    }

    protected override void DrawCube(Rect position)
    {
        DrawFloor(position);
        DrawWalls(position);
        DrawChildCubes(position);
    }
    public void ReGenerateCubesIDGrid()
    {
        cubesIDGrid = new int[tiling * tiling];
    }
    public void CalculateStaticTextures()
    {
        if (staticTexturesRT == null)
        {
            int renderTextureSize = tiling * wallSubdivision * tileTextureSize;
            staticTexturesRT = new RenderTexture(renderTextureSize, renderTextureSize, staticTilesRTDepth);
            staticTexturesRT.filterMode = FilterMode.Point;
        }

        CustomTextureRenderer2D.ClearRenderTexture(staticTexturesRT);

        staticTextures.Clear();

        /* Calculate walls logic:
         * - Loop through cubes grid to find wall cubes, ignore those that are or can be player.
         * - Having the position of a wall cube in the cubes grid -> Find suitable texture for each tile at the coresponding positions in walls grid of it
         * - Store that texture with its position in grid data.
         * */

        var wallsGrid = CalculateWallsGrid();

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

                        // Tell the staticTextures dictionary that this wall texture will be drawn in wallsGrid at (wallsGridRow, wallsGridColumn) 
                        if (!staticTextures.ContainsKey(wallTex)) staticTextures.Add(wallTex, new());
                        staticTextures[wallTex].Add(new(wallsGrid.GetLength(0), wallsGrid.GetLength(1), new Vector2Int(wallsGridRow, wallsGridColumn)));

                        (cubesGrid[cubesGridRow, cubesGridColumn] as WallCube).SetTexture(wallTex, subdivisionRow, subDivisionColumn);
                    }
                }
            }
        }

        CustomTextureRenderer2D.DrawTexturesToRenderTextureGrid(ref staticTexturesRT, staticTextures, cubeMat);

        /* To draw static textures on different layers:
         * After drawing textures of one layer
         * -> Clear the static textures list
         * -> Calculate new static textures of the next layer
         * -> Draw them onto RenderTexture
         * -> Repeat the process for the remaining layers
         * */
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
        Debug.Log("Tiling: " + Tiling + " tiling: " + tiling);
        cubesGrid = new Cube[tiling, tiling];
        for(int row = 0; row < tiling; row++)
        {
            for(int column = 0; column < tiling; column++)
            {
                var spawnedCube = cubesManager.GetCube(GetCubeIDInGrid(row, column));
                if (spawnedCube == null) continue;

                spawnedCube.RelativeScale = new Vector2(1.0f / tiling, 1.0f / tiling);
                spawnedCube.RelativePosition = Relativity.RPosFromGridTile(tiling, tiling, row, column);

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
        Color cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(base.cubeColor);
        if (staticTexturesRT != null) CustomTextureRenderer2D.RenderMesh(cubeMesh, cubeMat, staticTexturesRT, cubeColor, position.position, position.size);
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
            childCube.Draw(Relativity.CRectFromPRect(position, childCube.RelativeScale, childCube.RelativePosition));
        }
    }
}
