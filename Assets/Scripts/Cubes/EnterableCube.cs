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
    [HideInInspector]
    [SerializeField]
    [Min(0)]
    ChildCubeInitDetail[] childCubesInitDetails = {new()};

    private Cube[,] cubesGrid;

    public int Tiling { get => tiling; }
    public ChildCubeInitDetail[] ChildCubesInitDetails { get => childCubesInitDetails; }
    public ChildCubeInitDetail GetChildCubeInitDetails(int row, int col)
    {
        int index = row * tiling + col;
        if (index >= childCubesInitDetails.Length) return null;
        return childCubesInitDetails[index];
    }
    public void SetChildCubeInitDetails(int row, int col, ChildCubeInitDetail details)
    {
        int index = row * tiling + col;
        if (index >= childCubesInitDetails.Length) return;
        childCubesInitDetails[index] = details;
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

    // Init functions
    public void ReGenerateCubesIDGrid()
    {
        childCubesInitDetails = new ChildCubeInitDetail[tiling * tiling];
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
        cubesGrid = new Cube[tiling, tiling];
        for(int row = 0; row < tiling; row++)
        {
            for(int column = 0; column < tiling; column++)
            {
                var cubeToSpawnDetails = GetChildCubeInitDetails(row, column);
                if (cubeToSpawnDetails == null) continue;

                var spawnedCube = cubesManager.GetCube(cubeToSpawnDetails.CubeID);
                if (spawnedCube == null) continue;

                spawnedCube.RelativeScale = new Vector2(1.0f / tiling, 1.0f / tiling);
                spawnedCube.RelativePosition = Relativity.RPosFromGridTile(tiling, tiling, row, column);
                spawnedCube.Parent = this;
                spawnedCube.IsPlayer = cubeToSpawnDetails.IsPlayer;
                spawnedCube.CanBePlayer = cubeToSpawnDetails.CanBePlayer;

                childCubes.Add(spawnedCube);
                cubesGrid[row, column] = spawnedCube;

                if (spawnedCube is WallCube)
                {
                    spawnedCube.CubeColor = cubeColor;
                }
            }
        }
    }
    
    // Draw functions
    protected override void DrawCube(Rect position)
    {
        DrawFloor(position);
        DrawWalls(position);
        DrawChildCubes(position);
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

    // Child cube actions
    public float RequestToMove(CubeMovement childCubeMovement, Vector2 direction)
    {
        if (childCubeMovement.TargetCube.Parent != this) return 0;
        Vector2Int childCubePosInGrid = Relativity.GridPosFromRPos(tiling, tiling, childCubeMovement.TargetCube.RelativePosition);
        Vector2Int targetPos = childCubePosInGrid + Vector2Int.RoundToInt(direction.normalized);
        if (targetPos.x < 0 || targetPos.x >= tiling || targetPos.y < 0 || targetPos.y >= tiling)
        {
            // Moving out logic
            return 0; // Testing purpose
        }

        //// Moving internally
        //Rect targetRect = Relativity.CRectFromGridTile(tiling, tiling, targetPos.x, targetPos.y);

        return 0;
    }

    [Serializable]
    public class ChildCubeInitDetail
    {
        [SerializeField] int cubeID = 0;
        [SerializeField] bool isPlayer = false;
        [SerializeField] bool canBePlayer = false;
        [SerializeField] bool isReversed = false;

        public int CubeID { get => cubeID; set => cubeID = value; }
        public bool IsPlayer { get => isPlayer; set => isPlayer = value; }
        public bool CanBePlayer { get => canBePlayer; set => canBePlayer = value; }

        public ChildCubeInitDetail()
        {
            cubeID = 0;
            isPlayer = false;
            canBePlayer = false;
            isReversed = false;
        }
    }
}
