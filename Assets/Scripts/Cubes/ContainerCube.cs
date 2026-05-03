using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ContainerCube : Cube
{
    public const float floorDepthOffset = 0f;
    public const float wallDepthOffset = 0f;
    public const float childCubesDepthOffset = 0f;
    public const float movingChildCubesDepthOffset = -0.1f;

    [SerializeField] protected bool isEnterable = true;
    [SerializeField] protected bool isLeavable = true;
    [SerializeField] protected CustomGrid<ChildCubeDetails> childGrid = new();
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
    protected List<Cube> cullingCubesAll = new(); // These cubes wont be rendered if they are children of this cube, apply to all cube
    protected List<Cube> cullingCubesOne = new(); // Same as culling cubes all but only apply to one specific instance of a cube
    protected bool useDebug = false;

    // Delegates and events
    public delegate void BeginDrawingChildCube(Cube childCube, Rect parentRect, ref Rect childRect, ref float depth, ref float exposure, ref Rect? scissorRect);
    protected event BeginDrawingChildCube onBeginDrawingChildCube;
    protected Action<Cube> onFinishedDrawingChildCube;

    public bool IsEnterable { get => isEnterable; set => isEnterable = value; }
    public bool IsLeavable { get => isLeavable; set => isLeavable = value; }
    public Vector2Int Tiling { get => childGrid.Tiling; }
    public CustomTexture FloorTexture { get => floorTexture; }
    public ChildCubeInitDetail[] ChildCubesInitDetails { get => childCubesInitDetails; }
    public ChildCubeInitDetail GetChildCubeInitDetails(int row, int col)
    {
        int index = row * childGrid.Tiling.y + col;
        if (index >= childCubesInitDetails.Length) return null;
        return childCubesInitDetails[index];
    }
    public CustomGrid<ChildCubeDetails> ChildGrid { get => childGrid; }
    public ChildCubeDetails[,] ChildCubes { get => childGrid.Children; }
    public RenderTexture StaticTexturesRT { get => staticTexturesRT; }
    public List<Cube> EmptyCubes { get => emptyCubes; }
    public List<Cube> CullingCubesAll { get => cullingCubesAll; }
    public List<Cube> CullingCubesOne { get => cullingCubesOne; }
    public event BeginDrawingChildCube OnBeginDrawingChildCube
    {
        add
        {
            onBeginDrawingChildCube += value;
        }
        remove
        {
            onBeginDrawingChildCube -= value;
        }
    }
    public Action<Cube> OnFinishedDrawingChildCUbe { get => onFinishedDrawingChildCube; }

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
                else
                {
                    childGrid.Children[row, column].Cube = spawnedCube;
                    childGrid.Children[row, column].MovingDirection = CubeMovement.GridDirections.None;
                }
                //ModifyChildCube(spawnedCube);
                spawnedCube.Init();
            }
        }
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
                if (childGrid.Children[cubesGridRow, cubesGridColumn].Cube == null) continue;
                if (childGrid.Children[cubesGridRow, cubesGridColumn].Cube is not WallCube) continue;
                (childGrid.Children[cubesGridRow, cubesGridColumn].Cube as WallCube).SetSubdivision(wallSubdivision);
                for (int subdivisionRow = 0; subdivisionRow < wallSubdivision; subdivisionRow++)
                {
                    for (int subDivisionColumn = 0; subDivisionColumn < wallSubdivision; subDivisionColumn++)
                    {
                        int wallsGridRow = cubesGridRow * wallSubdivision + subdivisionRow;
                        int wallsGridColumn = cubesGridColumn * wallSubdivision + subDivisionColumn;
                        var wallTex = wallRuleTile.GetTexture(wallsGrid, wallsGridRow, wallsGridColumn);

                        // If this wall cube is or can be a player -> only calculate texture without drawing on RT
                        if (childGrid.Children[cubesGridRow, cubesGridColumn].Cube.CanBePlayer || childGrid.Children[cubesGridRow, cubesGridColumn].Cube.IsPlayer)
                        {
                            (childGrid.Children[cubesGridRow, cubesGridColumn].Cube as WallCube).SetTexture(wallTex, subdivisionRow, subDivisionColumn);
                            continue;
                        }

                        // Tell the staticTextures dictionary that this wall texture will be drawn in wallsGrid at (wallsGridRow, wallsGridColumn) 
                        if (!staticTextures.ContainsKey(wallTex)) staticTextures.Add(wallTex, new());
                        staticTextures[wallTex].Add(new(wallsGrid.GetLength(1), wallsGrid.GetLength(0), new Vector2Int(wallsGridRow, wallsGridColumn), childGrid.Children[cubesGridRow, cubesGridColumn].Cube.NormalMat));
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
                if (!(childGrid.Children[cubesGridRow, cubesGridColumn].Cube is WallCube)) continue;

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

    // Modification functions
    public virtual void ModifyChildCubeEnter(Cube childCube)
    {
        if (childCube == null) return;
        if (isHorizFlipped) childCube.IsHorizFlipped = !childCube.IsHorizFlipped;
    }
    public virtual void ModifyChildCubeExit(Cube childCube)
    {
        if (childCube == null) return;
        if (isHorizFlipped) childCube.IsHorizFlipped = !childCube.IsHorizFlipped;
    }

    // Draw functions
    public override void DrawCube(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        DrawFloor(position, depth + floorDepthOffset, exposure, scissorRect);
        DrawWalls(position, depth + wallDepthOffset, exposure, scissorRect);
        DrawChildCubes(position, depth + childCubesDepthOffset, exposure, scissorRect);
    }   
    public virtual void DrawFloor(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (floorTexture == null || floorTexture.GetTexture() == null) return;
        CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, floorTexture.GetTexture(), RealCubeColor, exposure, position.position, position.size, depth, scissorRect);
    }
    public virtual void DrawWalls(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (staticTexturesRT != null) CustomTextureRenderer2D.RenderMesh(cubeMesh, outlineMat, staticTexturesRT, RealCubeColor, exposure, position.position, position.size, depth, scissorRect);
    }
    protected virtual void DrawChildCubes(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        // Draw empty cubes first
        foreach (var emptyCube in emptyCubes)
        {
            Rect childRect = Relativity.CRectFromPRect(position, emptyCube.RelativeScale, emptyCube.RelativePosition);
            onBeginDrawingChildCube?.Invoke(emptyCube, position, ref childRect, ref depth, ref exposure, ref scissorRect);
            emptyCube.Draw(childRect, depth, exposure, scissorRect);
            onFinishedDrawingChildCube?.Invoke(emptyCube);
        }

        List<Cube> movingCubes = new();

        // Draw static cubes
        foreach (var childCube in childGrid.Children)
        {
            if (childCube.Cube == null || (cullingCubesOne != null && (cullingCubesOne.Contains(childCube.Cube) || cullingCubesAll.Contains(childCube.Cube)))) continue;
            if (childCube.Cube is WallCube) // Ignore walls that aren't or can't potentially be a player
            {
                if (!(childCube.Cube.CanBePlayer || childCube.Cube.IsPlayer)) continue;
            }
            if (childCube.Cube.GetComponent<CubeMovement>() != null && childCube.Cube.GetComponent<CubeMovement>().IsMoving)
            {
                movingCubes.Add(childCube.Cube);
                continue;
            }
            List<Cube> tempCullingCubeOne = null;
            if (childCube.Cube == this)
            {
                tempCullingCubeOne = cullingCubesOne;
                cullingCubesOne = null;
            }
            Rect childRect = Relativity.CRectFromPRect(position, childCube.Cube.RelativeScale, childCube.Cube.RelativePosition);
            onBeginDrawingChildCube?.Invoke(childCube.Cube, position, ref childRect, ref depth, ref exposure, ref scissorRect);
            childCube.Cube.Draw(childRect, depth, exposure, scissorRect);
            onFinishedDrawingChildCube?.Invoke(childCube.Cube);
            if (childCube.Cube == this && tempCullingCubeOne != null)
            {
                cullingCubesOne = tempCullingCubeOne;
            }
        }

        // Drawing moving cubes on top of other cubes to avoid being covered
        foreach (var movingCube in movingCubes)
        {
            List<Cube> tempCullingCubeOne = null;
            if (movingCube == this)
            {
                tempCullingCubeOne = cullingCubesOne;
                cullingCubesOne = null;
            }
            Rect childRect = Relativity.CRectFromPRect(position, movingCube.RelativeScale, movingCube.RelativePosition);
            onBeginDrawingChildCube?.Invoke(movingCube, position, ref childRect, ref depth, ref exposure, ref scissorRect);
            movingCube.Draw(childRect, depth + movingChildCubesDepthOffset, exposure, scissorRect);
            onFinishedDrawingChildCube?.Invoke(movingCube);
            if (movingCube == this && tempCullingCubeOne != null)
            {
                cullingCubesOne = tempCullingCubeOne;
            }
        }
    }
    public override void DrawSurfaceEffects(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        base.DrawSurfaceEffects(position, depth, exposure, scissorRect);
        if (!isEnterable) CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, Texture2D.whiteTexture, unenterableColor, exposure, position.position, position.size, depth);
        if (!isLeavable)
        {
            if (materialPropertyBlock == null) materialPropertyBlock = new();
            else materialPropertyBlock.Clear();
            materialPropertyBlock.SetColor(CustomTextureRenderer2D.colorID, Color.clear);
            materialPropertyBlock.SetColor(CustomTextureRenderer2D.borderHightlightColorID, unleavableColor);
            materialPropertyBlock.SetTexture(CustomTextureRenderer2D.mainTexID, Texture2D.whiteTexture);
            materialPropertyBlock.SetFloat(CustomTextureRenderer2D.isHighlightedID, 1);

            CustomTextureRenderer2D.RenderMesh(cubeMesh, outlineMat, materialPropertyBlock, position.position, position.size, depth, scissorRect);
        }
    }

    // This functions if call when a cube is trying to move within / entering this cube
    public virtual float RequestToMove(Vector2 cRPos, Vector2 cRScl, Cube requestedCube, CubeMovement.GridDirections direction, bool external = false, bool specialMove = false)
    {
        // Check if the requested cube can move
        if (requestedCube.CubeType == CubeTypes.Static || requestedCube.CubeType == CubeTypes.Empty) return 0;
        var requestedCubeMovement = requestedCube.GetComponent<CubeMovement>();
        if (requestedCubeMovement == null) return 0;

        // Check for infinite loop possibility
        if (requestedCube.PreviousParents.Count > 1)
        {
            int entriesLoopCount = 0;
            int exitsLoopCount = 0;
            InfinityCube foundInfinityCube = null;
            EpsilonCube foundEpsilonCube = null;
            for (int i = requestedCube.PreviousParents.Count - 1; i >= 0 ; i--)
            {
                if (requestedCube.PreviousParents[i].Cube == null) continue;
                if (requestedCube.PreviousParents[i].Cube is InfinityCube)
                {
                    foundInfinityCube = (InfinityCube) requestedCube.PreviousParents[i].Cube;
                    break;
                }
                if (requestedCube.PreviousParents[i].Cube is EpsilonCube)
                {
                    foundEpsilonCube = (EpsilonCube) requestedCube.PreviousParents[i].Cube;
                    break;
                }

                if (requestedCube.PreviousParents[i].Cube == this)
                {
                    if (requestedCube.PreviousParents[i].Direction == CubeMovement.LayerDirections.In) entriesLoopCount++;
                    else if (requestedCube.PreviousParents[i].Direction == CubeMovement.LayerDirections.Out) exitsLoopCount++;
                }
            }
            Debug.Log($"Exit count: {exitsLoopCount}, entry count: {entriesLoopCount}");
            // If the cube keep trying to exit for more than 3 times -> Teleport to infinity cube
            if (exitsLoopCount >= 3)
            {
                if (useDebug) Debug.Log($"{requestedCube.name} is in infinite exits!");
                int infinityLevel = (foundInfinityCube != null)? foundInfinityCube.Level + 1 : InfinityCube.minLevel;
                var infinityCube = CubesManager.Instance.GetInfinityCube(requestedCube.Parent, infinityLevel);
                if (infinityCube != null)
                {
                    return infinityCube.RequestToMove(requestedCube, direction, true, specialMove);
                }
                return 0;
            }
            // If the cube keep trying to enter for more than 3 times -> Teleport to epsilon cube
            else if (entriesLoopCount >= 3)
            {
                if (useDebug) Debug.Log($"{requestedCube.name} is in infinite entries!");
                int epsilonLevel = (foundEpsilonCube != null) ? foundEpsilonCube.Level + 1 : 1;

                return 0;
            }

        }

        // Set up camera transition
        if (MainCamera.Instance != null && requestedCube.IsPlayer)
        {
            ZoomingTransition zoomingTransition = new();
            MainCamera.Instance.SetTransition(zoomingTransition);
        }

        // Flip the requested cube if it is trying to move into a horizontally flipped cube from the outside
        Vector2Int requestedCubePosition = Relativity.GridPosFromRPos(childGrid.Tiling.y, childGrid.Tiling.x, cRPos);
        if (useDebug) Debug.Log($"Requested cube: {requestedCube.name} rPos: {cRPos}, grid pos: {requestedCubePosition}");
        if (isHorizFlipped)
        {
            if (external && !childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y))
            {
                cRPos.x = -cRPos.x;
                requestedCubePosition = Relativity.GridPosFromRPos(childGrid.Tiling.y, childGrid.Tiling.x, cRPos);
                direction = CubeMovement.FlipMovementInput(direction, true);
            }
        }

        // Calculate requested position based on whether the requested cube is from the outside or inside
        Vector2Int requestedPosition;
        if (childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y))
        {
            requestedPosition = requestedCubePosition + CubeMovement.ConvertMovementInputToGridDirection(direction);
        }
        else
        {
            if (!isEnterable)
            {
                return 0;
            }
            requestedPosition = childGrid.GetEnterPosition(direction, cRPos);
        }

        // If the requested cube is not a child of this cube -> Add to previous parents list
        if (external)
        {
            if (childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y))
            {
                requestedCube.PreviousParents.Add(new(CubeMovement.LayerDirections.Out, this));
            }
            else
            {
                requestedCube.PreviousParents.Add(new(CubeMovement.LayerDirections.In, this));
            }
        }
        // If the requested cube is a child of this cube -> Pick it up from its position
        else
        {
            if (childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y))
            {
                childGrid.Children[requestedCubePosition.x, requestedCubePosition.y].Cube = null;
                childGrid.Children[requestedCubePosition.x, requestedCubePosition.y].MovingDirection = direction;
            }
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
            targetTime = TryPushRequestedCubeOutside(cRPos, cRScl, requestedCube, direction);
            if (targetTime <= 0)
            {
                HandleFailToMove(requestedCube, requestedCubePosition, external);
            }
            else
            {
                if (!external) childGrid.Children[requestedCubePosition.x, requestedCubePosition.y].MovingDirection = CubeMovement.GridDirections.None;
            }
            return targetTime;
        }

        // If there is a movable cube -> Try to move that cube away first
        if (childGrid.Children[requestedPosition.x, requestedPosition.y].Cube != null)
        {
            float tempTargetTime = TryPushBlockageCubeAway(childGrid.Children[requestedPosition.x, requestedPosition.y].Cube, requestedPosition.x, requestedPosition.y, direction, specialMove);
            if (tempTargetTime > 0) targetTime = tempTargetTime;
        }

        // If there is another cube that is trying to move into this position first
        if (childGrid.Children[requestedPosition.x, requestedPosition.y].Cube != null && 
            childGrid.Children[requestedPosition.x, requestedPosition.y].Cube.GetComponent<CubeMovement>() != null && 
            childGrid.Children[requestedPosition.x, requestedPosition.y].Cube.GetComponent<CubeMovement>().IsMoving)
        {
            if (useDebug) Debug.Log($"{requestedCube.name} fail to move because another cube is entering {requestedPosition.x}, {requestedPosition.y} of {gameObject.name}!");
            HandleFailToMove(requestedCube, requestedCubePosition, external);
            return 0;
        }

        // If that blockage cube has not been moved -> Let the requested cube try to enter it
        if (childGrid.Children[requestedPosition.x, requestedPosition.y].Cube != null && childGrid.Children[requestedPosition.x, requestedPosition.y].Cube is ContainerCube enterableCube)
        {
            var tempTargetTime = TryLetRequestedCubeEnterBlockageCube(cRPos, cRScl, enterableCube, requestedCube, direction);
            if (tempTargetTime > 0)
            {
                if (!external) childGrid.Children[requestedCubePosition.x, requestedCubePosition.y].MovingDirection = CubeMovement.GridDirections.None;
                return tempTargetTime;
            }
        }

        // If the requested cube cannot move the blockage cube nor enter it -> Try to let the blockage cube enter the requested cube instead
        if (childGrid.Children[requestedPosition.x, requestedPosition.y].Cube != null && requestedCube is ContainerCube enterable)
        {
            Cube blockageCube = childGrid.Children[requestedPosition.x, requestedPosition.y].Cube;
            childGrid.Children[requestedPosition.x, requestedPosition.y].Cube = null;
            childGrid.Children[requestedPosition.x, requestedPosition.y].MovingDirection = CubeMovement.GridDirections.None;

            var tempTargetTime = TryLetBlockageCubeEnterRequestedCube(blockageCube, enterable, cRPos, cRScl, direction);

            // Fail to move the blockage cube -> return the blockage cube
            if (tempTargetTime <= 0)
            {
                HandleFailToMove(blockageCube, requestedPosition, false);
            }
            else
            {
                targetTime = tempTargetTime;
            }

            // If move successfully, the position at [row, column] should be empty by now
        }

        // If target position is empty (either originally or after moving the blockage cube) -> Move to that position
        if (childGrid.Children[requestedPosition.x, requestedPosition.y].Cube == null)
        {
            if (CubeMovement.CheckOppositeMovementInputs(direction, childGrid.Children[requestedPosition.x, requestedPosition.y].MovingDirection))
            {
                if (useDebug) Debug.Log($"There is a cube moving {childGrid.Children[requestedPosition.x, requestedPosition.y].MovingDirection} while this cube tries to move {direction}");
                HandleFailToMove(requestedCube, requestedCubePosition, external);
                return 0;
            }
            if (!external) childGrid.Children[requestedCubePosition.x, requestedCubePosition.y].MovingDirection = CubeMovement.GridDirections.None;
            return LetRequestedCubeEnter(requestedCube, cRPos, cRScl, requestedPosition.x, requestedPosition.y, targetTime, external);
        }

        // If all above fail, try to possess the cube
        requestedCube.StartPossessing(childGrid.Children[requestedPosition.x, requestedPosition.y].Cube);

        // Fail to move or possess successfully -> Return child cube to its original position in the cubes grid
        HandleFailToMove(requestedCube, requestedCubePosition, external);
        return 0;
    }

    private void HandleFailToMove(Cube requestedCube, Vector2Int requestedCubePosition, bool external)
    {
        if (!external)
        {
            if (childGrid.CheckValidGridPosition(requestedCubePosition.x, requestedCubePosition.y))
            {
                childGrid.Children[requestedCubePosition.x, requestedCubePosition.y].Cube = requestedCube;
                childGrid.Children[requestedCubePosition.x, requestedCubePosition.y].MovingDirection = CubeMovement.GridDirections.None;
            }
            if (!requestedCube.IsPossessing)
            {
                requestedCube.PreviousParents.Clear();
                requestedCube.PreviousParents.Add(new(CubeMovement.LayerDirections.Out, this));
            }
        }
        else if (!requestedCube.IsPossessing) requestedCube.RemovePreviousParent(this);
        // Dont clear previous parents list if the requested cube is possessing another cube, which is later used to play camera transition
    }

    private float TryPushRequestedCubeOutside(Vector2 cRPos, Vector2 cRScl, Cube requestedCube, CubeMovement.GridDirections direction)
    {
        if (!isLeavable) return 0;
        if (Parent == null)
        {
            // Teleport to the infitive cube
            if (useDebug) Debug.Log($"{requestedCube.name} cant move out of {gameObject.name} because there is no parent cube!");
            return 0;
        }

        /* Moving out logic:
         * - Caculate target position in the outter cube
         * - Calculate outter cube relative values to this cube
         * - Calculate the requested cube relative values to the outter cube
         * - Let the outter cube handle the movement of the requested cube
         */

        // Flip the requested cube if it is trying to move outside of a horizontally flipped cube
        if (isHorizFlipped)
        {
            cRPos.x = -cRPos.x;
            direction = CubeMovement.FlipMovementInput(direction, true);
        }

        Vector2 outterCubeRPos = Relativity.PRPosToAChild(relativePosition, relativeScale);
        Vector2 outterCubeRScl = Relativity.PRSclToAChild(relativeScale);

        Vector2 childCubeRPosToOutterCube = Relativity.SRPosFromSameParent(outterCubeRPos, outterCubeRScl, cRPos);
        Vector2 childCubeRSclToOutterCube = Relativity.SRSclFromSameParent(outterCubeRScl, cRScl);

        var targetTime = parent.RequestToMove(childCubeRPosToOutterCube, childCubeRSclToOutterCube, requestedCube, direction, true, true);
        if (targetTime > 0) return targetTime;

        if (useDebug) Debug.Log($"{requestedCube.name} cant move out of {gameObject.name} because its parent rejected!");

        return 0;
    }

    private float TryPushBlockageCubeAway(Cube blockageCube, int row, int column, CubeMovement.GridDirections direction, bool specialMove)
    {
        if (useDebug) Debug.Log($"Try to push {blockageCube.name} away");
        var blockageCubeMovement = blockageCube.GetComponent<CubeMovement>();
        if (blockageCubeMovement != null && blockageCubeMovement.Movable)
        {
            if (blockageCubeMovement.IsMoving)
            {
                if (useDebug) Debug.Log($"Try to push {blockageCubeMovement.gameObject.name} but it's moving!");
                return 0; //  Immediately stop, not continue to try other options
            }
            return RequestToMove(blockageCube.RelativePosition, blockageCube.RelativeScale, blockageCube, direction, false, specialMove);

            // If move successful, the position at [row, column] should be empty, update target time based on how long it takes for the blockage cube to move
        }
        return 0;
    }

    private float TryLetRequestedCubeEnterBlockageCube(Vector2 requestedCubeRPos, Vector2 requestedCubeRScl, ContainerCube blockageCube, Cube requestedCube, CubeMovement.GridDirections direction)
    {
        Vector2 childCubeRPosToBlockageCube = Relativity.SRPosFromSameParent(blockageCube.RelativePosition, blockageCube.RelativeScale, requestedCubeRPos);
        Vector2 childCubeRSclToBlockageCube = Relativity.SRSclFromSameParent(blockageCube.RelativeScale, requestedCubeRScl);

        var tempTargetTime = blockageCube.RequestToMove(childCubeRPosToBlockageCube, childCubeRSclToBlockageCube, requestedCube, direction, true, true);

        // If move successfully
        if (tempTargetTime > 0) return tempTargetTime;

        if (useDebug) Debug.Log($"{requestedCube.name} fail to enter {blockageCube.gameObject.name}!");
        return 0;
    }

    private float TryLetBlockageCubeEnterRequestedCube(Cube blockageCube, ContainerCube requestedCube, Vector2 requestedCubeRPos, Vector2 requestedCubeRScl, CubeMovement.GridDirections direction)
    {
        if (useDebug) Debug.Log($"Try to let {blockageCube.name} enter {requestedCube.name}");

        var blockageCubeMovement = blockageCube.GetComponent<CubeMovement>();
        if (blockageCubeMovement != null && blockageCubeMovement.Movable)
        {
            Vector2 blockageCubeRPosToRequestedCube = Relativity.SRPosFromSameParent(requestedCubeRPos, requestedCubeRScl, blockageCube.RelativePosition);
            Vector2 blockageCubeRSclToRequestedCube = Relativity.SRSclFromSameParent(requestedCubeRScl, blockageCube.RelativeScale);

            if (useDebug) Debug.Log(blockageCubeRPosToRequestedCube + " " + blockageCubeRSclToRequestedCube);

            return requestedCube.RequestToMove(blockageCubeRPosToRequestedCube, blockageCubeRSclToRequestedCube, blockageCube, CubeMovement.ReverseMovementInput(direction), true, true);
        }
        return 0;
    }

    private float LetRequestedCubeEnter(Cube requestedCube, Vector2 cRPos, Vector2 cRScl, int row, int column, float targetTime, bool external)
    {
        if (useDebug) Debug.Log($"{requestedCube.name} successfully move to an empty position {row}, {column} of {gameObject.name}!");
        
        Vector2 childCubeTargetRPos = Relativity.RPosFromGridTile(childGrid.Tiling.y, childGrid.Tiling.x, row, column);
        Vector2 childCubeTargetRScl = new Vector2(1.0f / childGrid.Tiling.y, 1.0f / childGrid.Tiling.x);
        if (useDebug) Debug.Log($"{requestedCube.name} new relative vallues: {childCubeTargetRPos}, {childCubeTargetRScl}, targetTime: {targetTime}");
        childGrid.Children[row, column].Cube = requestedCube;
        childGrid.Children[row, column].MovingDirection = CubeMovement.GridDirections.None;
        var requestedCubeMovement = requestedCube.GetComponent<CubeMovement>();
        if (requestedCubeMovement == null) return 0;

        Vector2 requestedCubeOldRPos = requestedCube.RelativePosition;
        Vector2 requestedCubeOldRScl = requestedCube.RelativeScale;

        float finalTargetTime = requestedCubeMovement.StartMoving(cRPos, cRScl, childCubeTargetRPos, childCubeTargetRScl, targetTime, this, external);

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
    public class ChildCubeDetails
    {
        private Cube cube;
        private CubeMovement.GridDirections movingDirection;

        public Cube Cube { get => cube; set => cube = value; }
        public CubeMovement.GridDirections MovingDirection { get => movingDirection; set => movingDirection = value; }
        public ChildCubeDetails()
        {
            cube = null;
            movingDirection = CubeMovement.GridDirections.None;
        }
        public ChildCubeDetails(Cube cube = null, CubeMovement.GridDirections movingDirection = CubeMovement.GridDirections.None)
        {
            this.cube = cube;
            this.movingDirection = movingDirection;
        }
    }
}
