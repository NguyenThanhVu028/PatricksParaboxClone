using System;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCube : Cube
{
    [SerializeField] protected bool isEnterable = true;
    [SerializeField] protected bool isLeavable = true;
    [SerializeField] protected CustomGrid<Cube> childGrid = new();
    [Min(1)]
    [SerializeField] protected int wallSubdivision = 2; // Wall texture might be smaller than a tile
    [SerializeField] protected CustomRuleTile wallRuleTile;
    [SerializeField] protected CustomTexture floorTexture;
    [SerializeField] protected int tileTextureSize = 16;
    [SerializeField] protected int staticTilesRTDepth = 16;
    [SerializeField] Color unenterableColor = new(0, 0, 0, 0.9f);
    [SerializeField] Color unleavableColor = new(1, 0, 0, 0);


    [SerializeField]
    [Min(0)]
    ChildCubeInitDetail[] childCubesInitDetails = {new()};

    protected List<Cube> emptyCubes = new(); // Seperate empty cubes from other cubes, empty cubes are only be rendered but ignore checking for interactions by other cubes
    protected List<Cube> cullingCubes = new(); // These cubes wont be rendered if they are children of this cube
    protected bool useDebug = false;

    public bool IsEnterable { get => isEnterable; set => isEnterable = value; }
    public Vector2Int Tiling { get => childGrid.Tiling; }
    public CustomTexture FloorTexture { get => floorTexture; }
    public ChildCubeInitDetail[] ChildCubesInitDetails { get => childCubesInitDetails; }
    public ChildCubeInitDetail GetChildCubeInitDetails(int row, int col)
    {
        int index = row * childGrid.Tiling.y + col;
        if (index >= childCubesInitDetails.Length) return null;
        return childCubesInitDetails[index];
    }
    public CustomGrid<Cube> ChildGrid { get => childGrid; }
    public Cube[,] ChildCubes { get => childGrid.Children; }
    public RenderTexture StaticTexturesRT { get => staticTexturesRT; }
    public List<Cube> EmptyCubes { get => emptyCubes; }
    public List<Cube> CullingCubes { get => cullingCubes; }

    // Used by the Editor
    public void SetChildCubeInitDetails(int row, int col, ChildCubeInitDetail details)
    {
        int index = row * childGrid.Tiling.y + col;
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

    private void OnEnable()
    {
        childGrid.Init();
    }

    public void Start()
    {
        InitChildCubes();
        Init();
        CalculateStaticTextures();
    }

    // Init functions
    public void ReGenerateCubesIDGrid()
    {
        childCubesInitDetails = new ChildCubeInitDetail[childGrid.Tiling.x * childGrid.Tiling.y];
    }
    public void CalculateStaticTextures()
    {
        if (staticTexturesRT == null)
        {
            //int renderTextureSize = ((tiling.x > tiling.y) ? tiling.x : tiling.y) * wallSubdivision * tileTextureSize;
            staticTexturesRT = new RenderTexture(childGrid.Tiling.y * wallSubdivision * tileTextureSize, childGrid.Tiling.x * wallSubdivision * tileTextureSize, staticTilesRTDepth);
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

        for (int cubesGridRow = 0; cubesGridRow < childGrid.Children.GetLength(0); cubesGridRow++)
        {
            for (int cubesGridColumn = 0; cubesGridColumn < childGrid.Children.GetLength(1); cubesGridColumn++)
            {
                if (childGrid.Children[cubesGridRow, cubesGridColumn] == null) continue;
                if (!(childGrid.Children[cubesGridRow, cubesGridColumn] is WallCube)) continue;
                (childGrid.Children[cubesGridRow, cubesGridColumn] as WallCube).SetSubdivision(wallSubdivision);
                for (int subdivisionRow = 0; subdivisionRow < wallSubdivision; subdivisionRow++)
                {
                    for (int subDivisionColumn = 0; subDivisionColumn < wallSubdivision; subDivisionColumn++)
                    {
                        int wallsGridRow = cubesGridRow * wallSubdivision + subdivisionRow;
                        int wallsGridColumn = cubesGridColumn * wallSubdivision + subDivisionColumn;
                        var wallTex = wallRuleTile.GetTexture(wallsGrid, wallsGridRow, wallsGridColumn);

                        // If this wall cube is or can be a player -> only calculate texture without drawing on RT
                        if (childGrid.Children[cubesGridRow, cubesGridColumn].CanBePlayer || childGrid.Children[cubesGridRow, cubesGridColumn].IsPlayer)
                        {
                            (childGrid.Children[cubesGridRow, cubesGridColumn] as WallCube).SetTexture(wallTex, subdivisionRow, subDivisionColumn);
                            continue;
                        }

                        // Tell the staticTextures dictionary that this wall texture will be drawn in wallsGrid at (wallsGridRow, wallsGridColumn) 
                        if (!staticTextures.ContainsKey(wallTex)) staticTextures.Add(wallTex, new());
                        staticTextures[wallTex].Add(new(wallsGrid.GetLength(1), wallsGrid.GetLength(0), new Vector2Int(wallsGridRow, wallsGridColumn), childGrid.Children[cubesGridRow, cubesGridColumn].NormalMat));
                    }
                }
            }
        }

        CustomTextureRenderer2D.DrawTexturesToRenderTextureGrid(ref staticTexturesRT, staticTextures);

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
        if (childGrid.Children == null) return null;

        int[,] wallsGrid = new int[childGrid.Tiling.x * wallSubdivision, childGrid.Tiling.y * wallSubdivision];

        for (int cubesGridRow = 0; cubesGridRow < childGrid.Children.GetLength(0); cubesGridRow++)
        {
            for (int cubesGridColumn = 0; cubesGridColumn < childGrid.Children.GetLength(1); cubesGridColumn++)
            {
                if (!(childGrid.Children[cubesGridRow, cubesGridColumn] is WallCube)) continue;

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
        //childCubes = new Cube[tiling.x, tiling.y];
        for(int row = 0; row < childGrid.Tiling.x; row++)
        {
            for(int column = 0; column < childGrid.Tiling.y; column++)
            {
                var cubeToSpawnDetails = GetChildCubeInitDetails(row, column);
                if (cubeToSpawnDetails == null) continue;

                var spawnedCube = cubesManager.GetCube(cubeToSpawnDetails.CubeID);
                if (spawnedCube == null) continue;

                spawnedCube.RelativeScale = new Vector2(1.0f / childGrid.Tiling.y, 1.0f / childGrid.Tiling.x);
                spawnedCube.RelativePosition = Relativity.RPosFromGridTile(childGrid.Tiling.y, childGrid.Tiling.x, row, column);
                spawnedCube.Parent = this;
                spawnedCube.IsPlayer = cubeToSpawnDetails.IsPlayer;
                spawnedCube.CanBePlayer = cubeToSpawnDetails.CanBePlayer;
                if (spawnedCube.CubeType == CubeTypes.Empty) emptyCubes.Add(spawnedCube);
                else childGrid.Children[row, column] = spawnedCube;
                //ModifyChildCube(spawnedCube);
                spawnedCube.Init();
            }
        }
    }
    //protected void ModifyChildCube(Cube childCube)
    //{
    //    if (childCube == null) return;
    //}
    //protected void UnModifyChildCube(Cube childCube)
    //{
    //    if (childCube == null) return;
    //}
    
    // Draw functions
    protected override void DrawCube(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        DrawFloor(position, depth, exposure, scissorRect);
        DrawWalls(position, depth, exposure, scissorRect);
        DrawChildCubes(position, depth, exposure, scissorRect);
    }   
    protected virtual void DrawFloor(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (floorTexture == null || floorTexture.GetTexture() == null) return;
        CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, floorTexture.GetTexture(), RealCubeColor, exposure, position.position, position.size, depth, scissorRect);
    }
    protected virtual void DrawWalls(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (staticTexturesRT != null) CustomTextureRenderer2D.RenderMesh(cubeMesh, outlineMat, staticTexturesRT, RealCubeColor, exposure, position.position, position.size, depth, scissorRect);
    }
    protected virtual void DrawChildCubes(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        // Draw empty cubes first
        foreach (var emptyCube in emptyCubes)
        {
            emptyCube.Draw(Relativity.CRectFromPRect(position, emptyCube.RelativeScale, emptyCube.RelativePosition), depth, exposure, scissorRect);
        }

        List<Cube> movingCubes = new();

        // Draw static cubes
        foreach (var childCube in childGrid.Children)
        {
            if (childCube == null || cullingCubes.Contains(childCube)) continue;
            if (childCube is WallCube) // Ignore walls that aren't or can't potentially be a player
            {
                if (!(childCube.CanBePlayer || childCube.IsPlayer)) continue;
            }
            if (childCube.GetComponent<CubeMovement>() != null && childCube.GetComponent<CubeMovement>().IsMoving)
            {
                movingCubes.Add(childCube);
                continue;
            }
            childCube.Draw(Relativity.CRectFromPRect(position, childCube.RelativeScale, childCube.RelativePosition), depth, exposure, scissorRect);
        }

        // Drawing moving cubes on top of other cubes to avoid being covered
        foreach (var movingCube in movingCubes)
        {
            movingCube.Draw(Relativity.CRectFromPRect(position, movingCube.RelativeScale, movingCube.RelativePosition), depth - 0.1f, exposure, scissorRect);
        }
    }
    protected override void DrawSurfaceEffects(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        base.DrawSurfaceEffects(position, depth, exposure, scissorRect);
        if (!isEnterable) CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, Texture2D.whiteTexture, unenterableColor, exposure, position.position, position.size, depth - 0.2f);
        if (!isLeavable)
        {
            if (materialPropertyBlock == null) materialPropertyBlock = new();
            else materialPropertyBlock.Clear();
            materialPropertyBlock.SetColor(CustomTextureRenderer2D.colorID, unleavableColor);
            materialPropertyBlock.SetTexture(CustomTextureRenderer2D.mainTexID, Texture2D.whiteTexture);
            materialPropertyBlock.SetFloat(CustomTextureRenderer2D.isHighlightedID, 1);

            CustomTextureRenderer2D.RenderMesh(cubeMesh, outlineMat, materialPropertyBlock, position.position, position.size, depth - 0.2f, scissorRect);
        }
    }

    // This functions if call when a cube is trying to move within / entering this cube
    public virtual float RequestToMove(Vector2 cRPos, Vector2 cRScl, Cube requestedCube, PlayerInputsManager.MovementInputs direction, bool external = false, bool specialMove = false)
    {
        // Check if the requested cube can move
        if (requestedCube.CubeType == CubeTypes.Static || requestedCube.CubeType == CubeTypes.Empty) return 0;
        var requestedCubeMovement = requestedCube.GetComponent<CubeMovement>();
        if (requestedCubeMovement == null) return 0;

        // Flip the requested cube if it is trying to move into a horizontally flipped cube
        Vector2Int requestedCubePosition = Relativity.GridPosFromRPos(childGrid.Tiling.y, childGrid.Tiling.x, cRPos);
        if (!childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y) && external && isHorizFlipped)
        {
            cRPos.x = -cRPos.x;
            requestedCubePosition = Relativity.GridPosFromRPos(childGrid.Tiling.y, childGrid.Tiling.x, cRPos);
            direction = PlayerInputsManager.FlipMovementInput(direction, true);
        }

        Vector2Int requestedPosition;
        if (!childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y))
        {
            if (!isEnterable)
            {
                if (!external && childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) childGrid.Children[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
                return 0;
            }
            requestedPosition = childGrid.GetEnterPosition(direction, cRPos);
        }
        else requestedPosition = requestedCubePosition + PlayerInputsManager.ConvertMovementInputToGridDirection(direction);

        // If the requested cube is not a child of this cube
        if (external)
        {
            //ModifyChildCube(requestedCube);
            if (childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y))
            {
                requestedCube.PreviousParents.Add(new(PreviousParentDetails.Directions.Out, this));
            }
            else
            {
                requestedCube.PreviousParents.Add(new(PreviousParentDetails.Directions.In, this));
            }
        }
        // If the requested cube is a child of this cube -> Pick it up from its position
        else
        {
            if (childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) childGrid.Children[requestedCubePosition.x, requestedCubePosition.y] = null;
        }

        if (useDebug)
        {
            if (external) Debug.Log($"{requestedCube.name} requests to enter {gameObject.name} from {direction} to {requestedPosition.x}, {requestedPosition.y}");
            else Debug.Log($"{requestedCube.name} requests to move within {gameObject.name} with {direction}");
        }

        float targetTime = (specialMove) ? requestedCubeMovement.EnterTime : requestedCubeMovement.MoveTime;

        // if the requested position is out of range -> try to move the requested cube outside
        if (!childGrid.CheckValidGridPosition(requestedPosition.x, requestedPosition.y))
        {
            if (useDebug) Debug.Log($"{requestedCube.name} try to move out of {gameObject.name}");
            targetTime = TryPushRequestedCubeOutside(cRPos, cRScl, requestedCube, direction, external);
            if (targetTime <= 0) 
                if (!external && childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) childGrid.Children[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
            return targetTime;
        }

        // If there is a movable cube -> Try to move that cube away first
        if (childGrid.Children[requestedPosition.x, requestedPosition.y] != null)
        {
            float tempTargetTime = TryPushBlockageCubeAway(childGrid.Children[requestedPosition.x, requestedPosition.y], requestedPosition.x, requestedPosition.y, direction, specialMove);
            if (tempTargetTime < 0)
            {
                if(!external && childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) childGrid.Children[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
                return 0;
            }
            if (tempTargetTime > 0) targetTime = tempTargetTime;
        }

        // If there is another cube that is trying to move into this position first
        if (childGrid.Children[requestedPosition.x, requestedPosition.y] != null && childGrid.Children[requestedPosition.x, requestedPosition.y].GetComponent<CubeMovement>() != null && childGrid.Children[requestedPosition.x, requestedPosition.y].GetComponent<CubeMovement>().IsMoving)
        {
            if (useDebug) Debug.Log($"{requestedCube.name} fail to move because another cube is entering {requestedPosition.x}, {requestedPosition.y} of {gameObject.name}!");
            if (!external && childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) childGrid.Children[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
            return 0;
        }

        // If that blockage cube has not been moved -> Let the requested cube try to enter it
        if (childGrid.Children[requestedPosition.x, requestedPosition.y] != null && childGrid.Children[requestedPosition.x, requestedPosition.y] is ContainerCube enterableCube)
        {
            var tempTargetTime = TryLetRequestedCubeEnterBlockageCube(cRPos, cRScl, enterableCube, requestedCube, direction);
            if (tempTargetTime > 0) return tempTargetTime;
        }

        // If the requested cube cannot move the blockage cube nor enter it -> Try to let the blockage cube enter the requested cube instead
        if (childGrid.Children[requestedPosition.x, requestedPosition.y] != null && requestedCube is ContainerCube enterable)
        {
            Cube blockageCube = childGrid.Children[requestedPosition.x, requestedPosition.y];
            childGrid.Children[requestedPosition.x, requestedPosition.y] = null;

            var tempTargetTime = TryLetBlockageCubeEnterRequestedCube(blockageCube, enterable, cRPos, cRScl, direction);

            // Fail to move the blockage cube -> return the blockage cube
            if (tempTargetTime <= 0) childGrid.Children[requestedPosition.x, requestedPosition.y] = blockageCube;
            else targetTime = tempTargetTime;

            // If move successfully, the position at [row, column] should be empty by now
        }

        // If target position is empty (either originally or after moving the blockage cube) -> Move to that position
        if (childGrid.Children[requestedPosition.x, requestedPosition.y] == null)
        {
            return LetRequestedCubeEnter(requestedCube, cRPos, cRScl, requestedPosition.x, requestedPosition.y, targetTime, external);
        }

        // If all above fail, try to possess the cube
        requestedCube.StartPossessing(childGrid.Children[requestedPosition.x, requestedPosition.y]);

        // Fail to move or possess successfully -> Return child cube to its original position in the cubes grid
        if (external) requestedCube.RemovePreviousParent(this);
        else if (!external && childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y)) childGrid.Children[requestedCubePosition.x, requestedCubePosition.y] = requestedCube;
        return 0;
    }

    private float TryPushRequestedCubeOutside(Vector2 cRPos, Vector2 cRScl, Cube requestedCube, PlayerInputsManager.MovementInputs direction, bool external)
    {
        if (!isLeavable) return 0;
        if (Parent == null)
        {
            // Teleport to the void
            if (useDebug) Debug.Log($"{requestedCube.name} cant move out of {gameObject.name} because there is no parent cube!");
            return 0;
        }
        if (external && requestedCube.PreviousParents.Count > 0 && Parent == requestedCube.PreviousParents[0].Cube)
        {
            // Inifite loop -> teleport to the void
            // This is just a placeholder code for testing
            if (useDebug) Debug.Log($"{requestedCube.name} cant move out of {gameObject.name} because of infinite loop!");
            return 0;
        }

        /* Moving out logic:
         * - Caculate target position in the outter cube
         * - Calculate outter cube relative values to this cube
         * - Calculate the requested cube relative values to the outter cube
         * - Let the outter cube modify the child cube (reverse, ...), if fail -> unmodify
         * - Let the outter cube handle the movement of the requested cube
         */

        // Flip the requested cube if it is trying to move outside of a horizontally flipped cube
        if (isHorizFlipped)
        {
            cRPos.x = -cRPos.x;
            direction = PlayerInputsManager.FlipMovementInput(direction, true);
        }

        Vector2 outterCubeRPos = Relativity.PRPosToAChild(relativePosition, relativeScale);
        Vector2 outterCubeRScl = Relativity.PRSclToAChild(relativeScale);

        Vector2 childCubeRPosToOutterCube = Relativity.SRPosFromSameParent(outterCubeRPos, outterCubeRScl, cRPos);
        Vector2 childCubeRSclToOutterCube = Relativity.SRSclFromSameParent(outterCubeRScl, cRScl);

        var targetTime = parent.RequestToMove(childCubeRPosToOutterCube, childCubeRSclToOutterCube, requestedCube, direction, true, true);
        if (targetTime > 0) return targetTime;

        // Fail to move out -> unmodify, set requested cube's parent back to this cube
        //parent.UnModifyChildCube(requestedCube);
        requestedCube.Parent = this;

        if (useDebug) Debug.Log($"{requestedCube.name} cant move out of {gameObject.name} because its parent rejected!");

        return 0;
    }

    private float TryPushBlockageCubeAway(Cube blockageCube, int row, int column, PlayerInputsManager.MovementInputs direction, bool specialMove)
    {
        if (useDebug) Debug.Log($"Try to push {blockageCube.name} away");
        var blockageCubeMovement = blockageCube.GetComponent<CubeMovement>();
        if (blockageCubeMovement != null && blockageCubeMovement.Movable)
        {
            if (blockageCubeMovement.IsMoving)
            {
                if (useDebug) Debug.Log($"Try to push {blockageCubeMovement.gameObject.name} but it's moving!");
                return -1; //  Immediately stop, not continue to try other options
            }
            return RequestToMove(blockageCube.RelativePosition, blockageCube.RelativeScale, blockageCube, direction, false, specialMove);

            // If move successful, the position at [row, column] should be empty, update target time based on how long it takes for the blockage cube to move
        }
        return 0;
    }

    private float TryLetRequestedCubeEnterBlockageCube(Vector2 requestedCubeRPos, Vector2 requestedCubeRScl, ContainerCube blockageCube, Cube requestedCube, PlayerInputsManager.MovementInputs direction)
    {
        Vector2 childCubeRPosToBlockageCube = Relativity.SRPosFromSameParent(blockageCube.RelativePosition, blockageCube.RelativeScale, requestedCubeRPos);
        Vector2 childCubeRSclToBlockageCube = Relativity.SRSclFromSameParent(blockageCube.RelativeScale, requestedCubeRScl);

        var tempTargetTime = blockageCube.RequestToMove(childCubeRPosToBlockageCube, childCubeRSclToBlockageCube, requestedCube, direction, true, true);

        // If move successfully
        if (tempTargetTime > 0) return tempTargetTime;

        // Fail to move out -> unmodify, set requested cube's parent back to this cube
        //blockageCube.UnModifyChildCube(requestedCube);
        requestedCube.Parent = this;

        if (useDebug) Debug.Log($"{requestedCube.name} fail to enter {blockageCube.gameObject.name}!");
        return 0;
    }

    private float TryLetBlockageCubeEnterRequestedCube(Cube blockageCube, ContainerCube requestedCube, Vector2 requestedCubeRPos, Vector2 requestedCubeRScl, PlayerInputsManager.MovementInputs direction)
    {
        if (useDebug) Debug.Log($"Try to let {blockageCube.name} enter {requestedCube.name}");

        var blockageCubeMovement = blockageCube.GetComponent<CubeMovement>();
        if (blockageCubeMovement != null && blockageCubeMovement.Movable)
        {
            Vector2 blockageCubeRPosToRequestedCube = Relativity.SRPosFromSameParent(requestedCubeRPos, requestedCubeRScl, blockageCube.RelativePosition);
            Vector2 blockageCubeRSclToRequestedCube = Relativity.SRSclFromSameParent(requestedCubeRScl, blockageCube.RelativeScale);

            if (useDebug) Debug.Log(blockageCubeRPosToRequestedCube + " " + blockageCubeRSclToRequestedCube);

            var tempTargetTime = requestedCube.RequestToMove(blockageCubeRPosToRequestedCube, blockageCubeRSclToRequestedCube, blockageCube, PlayerInputsManager.ReverseMovementInput(direction), true, true);
            
            if (tempTargetTime <= 0)
            {
                //requestedCube.UnModifyChildCube(blockageCube);
                blockageCube.Parent = this;
            }
            return tempTargetTime;
        
        }
        return 0;
    }

    private float LetRequestedCubeEnter(Cube requestedCube, Vector2 cRPos, Vector2 cRScl, int row, int column, float targetTime, bool external)
    {
        if (useDebug) Debug.Log($"{requestedCube.name} successfully move to an empty position {row}, {column} of {gameObject.name}!");
        Vector2 childCubeTargetRPos = Relativity.RPosFromGridTile(childGrid.Tiling.y, childGrid.Tiling.x, row, column);
        Vector2 childCubeTargetRScl = new Vector2(1.0f / childGrid.Tiling.y, 1.0f / childGrid.Tiling.x);
        if (useDebug) Debug.Log($"{requestedCube.name} new relative vallues: {childCubeTargetRPos}, {childCubeTargetRScl}, targetTime: {targetTime}");
        childGrid.Children[row, column] = requestedCube;
        var requestedCubeMovement = requestedCube.GetComponent<CubeMovement>();
        if (requestedCubeMovement == null) return 0;

        Vector2 requestedCubeOldRPos = requestedCube.RelativePosition;
        Vector2 requestedCubeOldRScl = requestedCube.RelativeScale;

        float finalTargetTime = requestedCubeMovement.StartMoving(cRPos, cRScl, childCubeTargetRPos, childCubeTargetRScl, targetTime, this, external);

        // Update camera transition if the requested cube is a player
        if (requestedCube.IsPlayer && MainCamera.Instance != null)
        {
            ZoomingTransition zoomingTransition = new(
                requestedCube.PreviousParents,
                targetTime
                );

            MainCamera.Instance.PlayTransition(zoomingTransition);
        }

        return finalTargetTime;
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
