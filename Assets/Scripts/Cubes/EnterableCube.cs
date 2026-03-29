using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class EnterableCube : Cube
{
    [Header("Enterable cube properties")]
    [HideInInspector]
    [Min(1)]
    [SerializeField] int tiling = 9;
    [Min(1)]
    [SerializeField] int wallSubdivision = 2; // Wall texture might be smaller than a tile
    [SerializeField] CustomRuleTile wallRuleTile;
    [SerializeField] Texture2D floorTexture;
    [Header("Enterable cube rendering")]
    [SerializeField] int tileTextureSize = 16;
    [SerializeField] int staticTilesRTDepth = 16;

    [HideInInspector]
    [SerializeField]
    [Min(0)]
    ChildCubeInitDetail[] childCubesInitDetails = {new()};

    private List<Cube> emptyCubes = new(); // Seperate empty cubes from other cubes, empty cubes are only be rendered but ignore checking for interactions by other cubes
    private Cube[,] childCubes;

    public int Tiling { get => tiling; }
    public ChildCubeInitDetail[] ChildCubesInitDetails { get => childCubesInitDetails; }
    public ChildCubeInitDetail GetChildCubeInitDetails(int row, int col)
    {
        int index = row * tiling + col;
        if (index >= childCubesInitDetails.Length) return null;
        return childCubesInitDetails[index];
    }
    public Cube[,] CubesGrid { get => childCubes; }

    // Used by the Editor
    public void SetChildCubeInitDetails(int row, int col, ChildCubeInitDetail details)
    {
        int index = row * tiling + col;
        if (index >= childCubesInitDetails.Length) return;
        childCubesInitDetails[index] = details;
    }

    // This dictionary stores calculated static textures and their positions in grid
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
         * - Loop through cubes grid to find wall cubes
         * - Having the position of a wall cube in the cubes grid -> Find suitable texture for each tile at the coresponding positions in walls grid of it
         * - If this wall cube is not or can't be player -> Store that texture with its position in grid to the static textures list.
         * */

        var wallsGrid = CalculateWallsGrid();

        for (int cubesGridRow = 0; cubesGridRow < childCubes.GetLength(0); cubesGridRow++)
        {
            for (int cubesGridColumn = 0; cubesGridColumn < childCubes.GetLength(1); cubesGridColumn++)
            {
                if (childCubes[cubesGridRow, cubesGridColumn] == null) continue;
                if (!(childCubes[cubesGridRow, cubesGridColumn] is WallCube)) continue;
                (childCubes[cubesGridRow, cubesGridColumn] as WallCube).SetSubdivision(wallSubdivision);
                for (int subdivisionRow = 0; subdivisionRow < wallSubdivision; subdivisionRow++)
                {
                    for (int subDivisionColumn = 0; subDivisionColumn < wallSubdivision; subDivisionColumn++)
                    {
                        int wallsGridRow = cubesGridRow * wallSubdivision + subdivisionRow;
                        int wallsGridColumn = cubesGridColumn * wallSubdivision + subDivisionColumn;
                        var wallTex = wallRuleTile.GetTexture(wallsGrid, wallsGridRow, wallsGridColumn);

                        // If this wall cube is or can be a player -> only calculate texture without drawing on RT
                        if (childCubes[cubesGridRow, cubesGridColumn].CanBePlayer || childCubes[cubesGridRow, cubesGridColumn].IsPlayer)
                        {
                            (childCubes[cubesGridRow, cubesGridColumn] as WallCube).SetTexture(wallTex, subdivisionRow, subDivisionColumn);
                            continue;
                        }

                        // Tell the staticTextures dictionary that this wall texture will be drawn in wallsGrid at (wallsGridRow, wallsGridColumn) 
                        if (!staticTextures.ContainsKey(wallTex)) staticTextures.Add(wallTex, new());
                        staticTextures[wallTex].Add(new(wallsGrid.GetLength(0), wallsGrid.GetLength(1), new Vector2Int(wallsGridRow, wallsGridColumn)));
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
        if (childCubes == null) return null;

        int[,] wallsGrid = new int[tiling * wallSubdivision, tiling * wallSubdivision];

        for (int cubesGridRow = 0; cubesGridRow < childCubes.GetLength(0); cubesGridRow++)
        {
            for (int cubesGridColumn = 0; cubesGridColumn < childCubes.GetLength(1); cubesGridColumn++)
            {
                if (!(childCubes[cubesGridRow, cubesGridColumn] is WallCube)) continue;

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
        childCubes = new Cube[tiling, tiling];
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
                if (spawnedCube.CubeType == CubeTypes.Empty) emptyCubes.Add(spawnedCube);
                else childCubes[row, column] = spawnedCube;
                ModifyChildCube(spawnedCube);
                spawnedCube.Init();
            }
        }
    }
    private void ModifyChildCube(Cube childCube)
    {
        if (childCube == null) return;
    }
    private void UnModifyChildCube(Cube childCube)
    {
        if (childCube == null) return;
    }
    
    // Draw functions
    protected override void DrawCube(Rect position, float depth = 0)
    {
        DrawFloor(position, depth);
        DrawWalls(position, depth);
        DrawChildCubes(position, depth);
    }   
    private void DrawFloor(Rect position, float depth)
    {
        Color cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(base.cubeColor);
        CustomTextureRenderer2D.RenderMesh(cubeMesh, cubeMat, floorTexture, cubeColor, position.position, position.size, depth);
    }
    public void DrawWalls(Rect position, float depth)
    {
        Color cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(base.cubeColor);
        if (staticTexturesRT != null) CustomTextureRenderer2D.RenderMesh(cubeMesh, cubeMat, staticTexturesRT, cubeColor, position.position, position.size, depth);
    }
    private void DrawChildCubes(Rect position, float depth)
    {
        // Draw empty cubes first
        foreach (var emptyCube in emptyCubes)
        {
            emptyCube.Draw(Relativity.CRectFromPRect(position, emptyCube.RelativeScale, emptyCube.RelativePosition), depth);
        }

        List<Cube> movingCubes = new();
        // Draw static cubes
        foreach (var childCube in childCubes)
        {
            if (childCube == null) continue;
            if (childCube is WallCube) // Ignore walls that aren't or can't potentially be a player
            {
                if (!(childCube.CanBePlayer || childCube.IsPlayer)) continue;
            }
            if (childCube.GetComponent<CubeMovement>() != null && childCube.GetComponent<CubeMovement>().IsMoving)
            {
                movingCubes.Add(childCube);
                continue;
            }
            childCube.Draw(Relativity.CRectFromPRect(position, childCube.RelativeScale, childCube.RelativePosition), depth);
        }

        // Drawing moving cubes on top of other cubes to avoid being covered
        foreach (var movingCube in movingCubes)
        {
            movingCube.Draw(Relativity.CRectFromPRect(position, movingCube.RelativeScale, movingCube.RelativePosition), depth - 0.1f);
        }
    }

    // Child cubes actions
    public float RequestToMove(Vector2 cRPos, Vector2 cRScl, Cube requestedCube, PlayerInputsManager.MovementInputs direction, int requestedRow, int requestedColumn, bool external = false, bool specialMove = false)
    {
        var requestedCubeMovement = requestedCube.GetComponent<CubeMovement>();
        if (requestedCubeMovement == null) return 0;

        Vector2Int requestedCubePosition = Relativity.GridPosFromRPos(tiling, tiling, requestedCube.RelativePosition);
        
        // If the requested cube is a child of this cube -> Pick it up from its position
        if (!external && CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) CubesGrid[requestedCubePosition.x, requestedCubePosition.y] = null;
        // If the requested cube is not a child of this cube -> Modify it
        if (external)
        {
            ModifyChildCube(requestedCube);
            requestedCube.Parent = this;
        }

        //if (external) Debug.Log($"{requestedCube.name} requests to enter {gameObject.name}");
        //else Debug.Log($"{requestedCube.name} requests to move within {gameObject.name}");

        float targetTime = (specialMove) ? requestedCubeMovement.SpecialMoveTime : requestedCubeMovement.NormalMoveTime;

        // if the requested position is out of range -> try to move the requested cube outside
        if (!CheckValidGridPosition(requestedRow, requestedColumn))
        {
            //Debug.Log($"{requestedCube.name} try to move out of {gameObject.name}");
            if (Parent == null)
            {
                //Debug.Log($"{requestedCube.name} cant move out of {gameObject.name} because there is no parent cube!");
                if (!external && CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) CubesGrid[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
                return 0;
            }
            if (external && Parent == this)
            {
                // Inifite loop -> teleport to the void instead
                // This is just a placeholder code for testing
                //Debug.Log($"{requestedCube.name} cant move out of {gameObject.name} because of infinite loop!");
                if (!external && CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) CubesGrid[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
                return 0;
            }

            /* Moving out logic:
             * - Caculate target position in the outter cube
             * - Calculate outter cube relative values to this cube
             * - Calculate child cube relative values to the outter cube
             * - Let the outter cube modify the child cube (reverse, ...), if fail -> unmodify
             * - Let the outter cube handle the movement of the requested cube
             */

            Vector2Int enterableCubePos = Relativity.GridPosFromRPos(Parent.Tiling, Parent.tiling, RelativePosition);
            //PlayerInputsManager.MovementInputs pushDirection = (isReversed)? PlayerInputsManager.FilpMovementInput(direction, true) : direction;
            Vector2Int targetPositionToPushTo = enterableCubePos + PlayerInputsManager.ConvertMovementInputToGridDirection(direction);
            
            Vector2 outterCubeRPos = Relativity.PRPosToAChild(relativePosition, relativeScale);
            Vector2 outterCubeRScl = Relativity.PRSclToAChild(relativeScale);

            Vector2 childCubeRPosToOutterCube = Relativity.SRPosFromSameParent(outterCubeRPos, outterCubeRScl, cRPos);
            Vector2 childCubeRSclToOutterCube = Relativity.SRSclFromSameParent(outterCubeRScl, cRScl);

            targetTime = parent.RequestToMove(childCubeRPosToOutterCube, childCubeRSclToOutterCube, requestedCube, direction, targetPositionToPushTo.x, targetPositionToPushTo.y, true, true);
            if (targetTime > 0) return targetTime;

            // Fail to move out -> unmodify, set requested cube's parent back to this cube
            parent.UnModifyChildCube(requestedCube);
            requestedCube.Parent = this;
            // Place the requested cube back to its original position if the requested cube is this cube's child
            //Debug.Log($"{requestedCube.name} cant move out of {gameObject.name} because its parent rejected!");
            if (!external && CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) CubesGrid[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
            return 0;
        }

        // If there is a movable cube -> Try to move that cube away first
        if (childCubes[requestedRow, requestedColumn] != null)
        {
            //Debug.Log($"{requestedCube.name} try to push {childCubes[requestedRow, requestedColumn].name}");
            var blockageCubeMovement = childCubes[requestedRow, requestedColumn].GetComponent<CubeMovement>();
            if (blockageCubeMovement != null && blockageCubeMovement.Movable)
            {
                if (blockageCubeMovement.IsMoving)
                {
                    //Debug.Log($"{requestedCube.name} try to push {blockageCubeMovement.gameObject.name} but it's moving!");
                    if (!external && CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) CubesGrid[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
                    return 0;
                }
                if (childCubes[requestedRow, requestedColumn].GetComponent<CubeMovement>().IsMoving) return 0;
                Vector2Int targetPositionToPushTo = new Vector2Int(requestedRow, requestedColumn) + PlayerInputsManager.ConvertMovementInputToGridDirection(direction);
                targetTime = RequestToMove(childCubes[requestedRow, requestedColumn].RelativePosition, childCubes[requestedRow, requestedColumn].RelativeScale, childCubes[requestedRow, requestedColumn], direction, targetPositionToPushTo.x, targetPositionToPushTo.y, false, specialMove);
                // If move successful, the position at [row, column] should be empty, update target time based on how long it takes for the blockage cube to move
            }
        }

        // If target position is empty (either originally or after moving the blockage cube) -> Move to that position
        if (childCubes[requestedRow, requestedColumn] == null)
        {
            //Debug.Log($"{requestedCube.name} successfully move to an empty position {requestedRow}, {requestedColumn} of {gameObject.name}!");
            Vector2 childCubeTargetRPos = Relativity.RPosFromGridTile(tiling, tiling, requestedRow, requestedColumn);
            Vector2 childCubeTargetRScl = new Vector2(1.0f / tiling, 1.0f / tiling);
            childCubes[requestedRow, requestedColumn] = requestedCube;
            return requestedCubeMovement.StartMoving(cRPos, cRScl, childCubeTargetRPos, childCubeTargetRScl, targetTime);
        }
        // If there is another cube that is trying to move into this position
        else if (childCubes[requestedRow, requestedColumn].GetComponent<CubeMovement>().IsMoving)
        {
            //Debug.Log($"{requestedCube.name} fail to move because another cube is entering {requestedRow}, {requestedColumn} of {gameObject.name}!");
            if (!external && CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) CubesGrid[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
            return 0;
        }

        // If that cube is blocked or not movable -> Try to enter
        if (childCubes[requestedRow, requestedColumn] is EnterableCube enterableCube)
        {
            Vector2 childCubeRPosToBlockageCube = Relativity.SRPosFromSameParent(enterableCube.RelativePosition, enterableCube.RelativeScale, cRPos);
            Vector2 childCubeRSclToBlockageCube = Relativity.SRSclFromSameParent(enterableCube.RelativeScale, cRScl);

            Vector2Int targetPositionToPushTo = enterableCube.GetEnterPosition(direction, childCubeRPosToBlockageCube);

            targetTime = enterableCube.RequestToMove(childCubeRPosToBlockageCube, childCubeRSclToBlockageCube, requestedCube, direction, targetPositionToPushTo.x, targetPositionToPushTo.y, true, true);
            if (targetTime > 0) return targetTime;

            // Fail to move out -> unmodify, set requested cube's parent back to this cube
            enterableCube.UnModifyChildCube(requestedCube);
            requestedCube.Parent = this;
            //Debug.Log($"{requestedCube.name} fail to enter {childCubes[requestedRow, requestedColumn].gameObject.name}!");
            // Place the requested cube back to its original position if the requested cube is this cube's child
            if (!external && CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) CubesGrid[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
            return 0;
        }

        // If all above fail, try to possess the cube
        if (requestedCube.StartPossessing(childCubes[requestedRow, requestedColumn]))
        {
            // If possess successfully -> return child cube to its original position in the cubes grid
            if (!external && CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) CubesGrid[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
            return 0;
        }

        // Fail to move -> return child cube to its original position in the cubes grid
        if (!external && CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) CubesGrid[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
        return 0;
    }

    private Vector2Int GetEnterPosition(PlayerInputsManager.MovementInputs direction, Vector2 rPos)
    {
        switch (direction)
        {
            case PlayerInputsManager.MovementInputs.Up:
                return new Vector2Int(tiling - 1, Mathf.RoundToInt((tiling - 1) * (rPos.x + 1) * 0.5f));
            case PlayerInputsManager.MovementInputs.Down:
                return new Vector2Int(0, Mathf.RoundToInt((tiling - 1) * (rPos.x + 1) * 0.5f));
            case PlayerInputsManager.MovementInputs.Right:
                return new Vector2Int(Mathf.RoundToInt((tiling - 1) * (1 - rPos.y) * 0.5f), 0);
            case PlayerInputsManager.MovementInputs.Left:
                return new Vector2Int(Mathf.RoundToInt((tiling - 1) * (1 - rPos.y) * 0.5f), tiling - 1);
        }
        return Vector2Int.zero;
    }

    protected bool CheckValidGridPosition(int row, int column)
    {
        if (row < 0 || column < 0 || row >= tiling || column >= tiling) return false;
        return true;
    }

    [Serializable]
    public class ChildCubeInitDetail
    {
        [SerializeField] int cubeID = 0;
        [SerializeField] bool isPlayer = false;
        [SerializeField] bool canBePlayer = false;

        public int CubeID { get => cubeID; set => cubeID = value; }
        public bool IsPlayer { get => isPlayer; set => isPlayer = value; }
        public bool CanBePlayer { get => canBePlayer; set => canBePlayer = value; }

        public ChildCubeInitDetail()
        {
            cubeID = 0;
            isPlayer = false;
            canBePlayer = false;
        }
    }
}
