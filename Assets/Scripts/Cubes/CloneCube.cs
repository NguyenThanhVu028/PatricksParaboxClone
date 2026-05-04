using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CloneCube : ContainerCube
{
    [SerializeField] protected ContainerCube mainContainerCube;

    private Cube requestedCube;

    public override void Init()
    {
        base.Init();
        InitCloneCube();
    }
    private void InitCloneCube()
    {
        if (hasInit) return;
        //if (mainContainerCube != null) mainContainerCube.OnFinishedDrawing.AddListener(OnMainCubeDraw);
        if (mainContainerCube != null)
        {
            mainContainerCube.OnBeginDrawingChildCube += OnMainCubeDrawChildCube;
        }
        hasInit = true;
    }

    private void Update()
    {
        if(requestedCube != null)
        {
            var requestedCubeMovement = requestedCube.GetComponent<CubeMovement>();
            if (requestedCubeMovement == null) requestedCube = null;
            if (!requestedCubeMovement.IsMoving || !requestedCubeMovement.IsExternal || requestedCube.Parent != mainContainerCube)
            {
                requestedCube = null;
            }
        }
    }

    public override void DrawFloor(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (mainContainerCube == null) return;
        mainContainerCube.DrawFloor(position, depth + floorDepthOffset, exposure, scissorRect);
    }

    public override void DrawWalls(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (mainContainerCube == null) return;
        mainContainerCube.DrawWalls(position, depth + wallDepthOffset, exposure, scissorRect);
    }

    protected override void DrawChildCubes(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (mainContainerCube == null) return;

        // Draw empty cubes first
        foreach (var emptyCube in mainContainerCube.EmptyCubes)
        {
            emptyCube.Draw(Relativity.CRectFromPRect(position, emptyCube.RelativeScale, emptyCube.RelativePosition), depth, exposure, scissorRect);
        }

        List<Cube> movingCubes = new();

        // Draw static cubes
        foreach (var childCube in mainContainerCube.ChildCubes)
        {
            if (childCube.Cube == null || childCube.Cube == requestedCube) continue;
            if (childCube.Cube is WallCube) // Ignore walls that aren't or can't potentially be a player
            {
                if (!(childCube.Cube.CanBePlayer || childCube.Cube.IsPlayer)) continue;
            }
            if (childCube.Cube.GetComponent<CubeMovement>() != null && childCube.Cube.GetComponent<CubeMovement>().IsMoving)
            {
                movingCubes.Add(childCube.Cube);
                continue;
            }
            childCube.Cube.Draw(Relativity.CRectFromPRect(position, childCube.Cube.RelativeScale, childCube.Cube.RelativePosition), depth, exposure, scissorRect);
        }

        // Drawing moving cubes on top of other cubes to avoid being covered
        foreach (var movingCube in movingCubes)
        {
            Vector2 idealRScl = movingCube.RelativeScale;
            Vector2 idealRPos = movingCube.RelativePosition;
            var movingCubeMovement = movingCube.GetComponent<CubeMovement>();

            // If the moving cube is entering / exiting container cube -> clamp it
            if (movingCubeMovement.IsExternal)
            {
                idealRScl = new(Mathf.Min(1.0f / mainContainerCube.Tiling.y, idealRScl.x), Mathf.Min(1.0f / mainContainerCube.Tiling.x, idealRScl.y));
                Vector2 normalizedRPos = (movingCube.RelativePosition).normalized;
                idealRPos = new(movingCube.RelativePosition.x - normalizedRPos.x * movingCube.RelativeScale.x + normalizedRPos.x * idealRScl.x,
                                movingCube.RelativePosition.y - normalizedRPos.y * movingCube.RelativeScale.y + normalizedRPos.y * idealRScl.y);
                movingCube.Draw(Relativity.CRectFromPRect(position, idealRScl, idealRPos), depth + movingChildCubesDepthOffset, exposure, CustomTextureRenderer2D.GetOverlapRect(scissorRect, position));
            }
            else movingCube.Draw(Relativity.CRectFromPRect(position, movingCube.RelativeScale, movingCube.RelativePosition), depth + movingChildCubesDepthOffset, exposure, scissorRect);
        }

        // Draw the requested cube
        if (requestedCube != null)
        {
            requestedCube.Draw(Relativity.CRectFromPRect(position, requestedCube.RelativeScale, requestedCube.RelativePosition), depth - 0.1f, exposure, scissorRect);
        }
    }

    public override void ModifyChildCubeEnter(Cube childCube)
    {
        if (childCube == null) return;
        if (isHorizFlipped != mainContainerCube.IsHorizFlipped) childCube.IsHorizFlipped = !childCube.IsHorizFlipped;
    }

    public override void ModifyChildCubeExit(Cube childCube)
    {
        if (childCube == null) return;
        if (isHorizFlipped != mainContainerCube.IsHorizFlipped) childCube.IsHorizFlipped = !childCube.IsHorizFlipped;
    }
    
    // Draw the requested cube on behalf of the main container cube, also clamp the cube
    private void OnMainCubeDrawChildCube(Cube childCube, Rect position, ref Rect childRect, ref float depth, ref float exposure, ref Rect? scissorRect)
    {
        if (childCube != requestedCube) return;

        Vector2 idealRScl = new(1.0f / mainContainerCube.Tiling.y, 1.0f / mainContainerCube.Tiling.x);
        Vector2 normalizedRPos = (requestedCube.RelativePosition).normalized;
        Vector2 idealRPos = new(requestedCube.RelativePosition.x - normalizedRPos.x * requestedCube.RelativeScale.x + normalizedRPos.x * idealRScl.x,
                                requestedCube.RelativePosition.y - normalizedRPos.y * requestedCube.RelativeScale.y + normalizedRPos.y * idealRScl.y);
        childRect = Relativity.CRectFromPRect(position, idealRScl, idealRPos);
        scissorRect = CustomTextureRenderer2D.GetOverlapRect(scissorRect, position);
    }

    public override float RequestToMove(Vector2 cRPos, Vector2 cRScl, Cube requestedCube, CubeMovement.GridDirections direction, bool external = false, bool specialMove = false)
    {
        if (mainContainerCube == null || !isEnterable) return 0;

        // Set up camera transition
        if (requestedCube.IsPlayer && MainCamera.Instance != null)
        {
            FadeTransition fadeTransition = new();
            MainCamera.Instance.SetTransition(fadeTransition);
        }

        if (isHorizFlipped != mainContainerCube.IsHorizFlipped)
        {
            cRPos.x = -cRPos.x;
            direction = CubeMovement.FlipMovementInput(direction, true);
            requestedCube.PreviousParents.Add(new(CubeMovement.LayerDirections.In, this, Vector2.zero, Vector2.one, true));
        }
        else requestedCube.PreviousParents.Add(new(CubeMovement.LayerDirections.In, this, Vector2.zero, Vector2.one, false));

        float finalTargetTime = mainContainerCube.RequestToMove(cRPos, cRScl, requestedCube, direction, external, specialMove);

        if (finalTargetTime <= 0)
        {
            requestedCube.RemovePreviousParent(this);
            return 0;
        }

        this.requestedCube = requestedCube;

        return finalTargetTime;
    }
}
