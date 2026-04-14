using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CloneCube : ContainerCube
{
    [SerializeField] ContainerCube mainContainerCube;

    private Cube requestedCube;
    private Vector2Int requestedCubeTargetPos = new();

    //public override void Draw(Rect position, float depth = 0)
    //{
    //    if (mainContainerCube != null) mainContainerCube.Draw(position, depth);
    //}

    public override void Init()
    {
        base.Init();
        if (mainContainerCube != null) mainContainerCube.OnFinishedDrawing.AddListener(OnMainCubeDraw);
        if (AnimationsManager.Instance != null) surfaceEffectsAnimation = AnimationsManager.Instance.GetNormalTextureAnimation("Noise");
    }

    private void Update()
    {
        if(requestedCube != null)
        {
            var requestedCubeMovement = requestedCube.GetComponent<CubeMovement>();
            if (requestedCubeMovement == null) requestedCube = null;
            if (!requestedCubeMovement.IsMoving || !requestedCubeMovement.IsExternal || requestedCube.Parent != mainContainerCube)
            {
                //if (mainContainerCube != null)
                //{
                //    if (mainContainerCube.ChildGrid.Children[requestedCubeTargetPos.x, requestedCubeTargetPos.y] == null)
                //    {
                //        mainContainerCube.ChildGrid.Children[requestedCubeTargetPos.x, requestedCubeTargetPos.y] = requestedCube;
                //    }
                //}
                mainContainerCube.CullingCubes.Remove(requestedCube);
                requestedCube = null;
            }
        }
    }

    protected override void DrawFloor(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (mainContainerCube == null || mainContainerCube.FloorTexture == null || mainContainerCube.FloorTexture.GetTexture() == null) return;
        CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, mainContainerCube.FloorTexture.GetTexture(), mainContainerCube.RealCubeColor, exposure, position.position, position.size, depth, scissorRect);
    }

    protected override void DrawWalls(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (mainContainerCube == null || mainContainerCube.StaticTexturesRT == null) return;
        CustomTextureRenderer2D.RenderMesh(cubeMesh, outlineMat, mainContainerCube.StaticTexturesRT, mainContainerCube.RealCubeColor, exposure, position.position, position.size, depth, scissorRect);
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
            if (childCube == null || childCube == requestedCube) continue;
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
            Vector2 idealRScl = movingCube.RelativeScale;
            Vector2 idealRPos = movingCube.RelativePosition;
            var movingCubeMovement = movingCube.GetComponent<CubeMovement>();

            // If the moving cube is entering / exiting container cube -> clamp it
            if (movingCubeMovement.IsExternal)
            {
                idealRScl = new(1.0f / mainContainerCube.Tiling.y, 1.0f / mainContainerCube.Tiling.x);
                Vector2 normalizedRPos = (movingCube.RelativePosition).normalized;
                idealRPos = new(movingCube.RelativePosition.x - normalizedRPos.x * movingCube.RelativeScale.x + normalizedRPos.x * idealRScl.x,
                                movingCube.RelativePosition.y - normalizedRPos.y * movingCube.RelativeScale.y + normalizedRPos.y * idealRScl.y);
                movingCube.Draw(Relativity.CRectFromPRect(position, idealRScl, idealRPos), depth - 0.1f, exposure, CustomTextureRenderer2D.GetOverlapRect(scissorRect, position));
            }
            else movingCube.Draw(Relativity.CRectFromPRect(position, movingCube.RelativeScale, movingCube.RelativePosition), depth - 0.1f, exposure, scissorRect);
        }

        // Draw the requested cube
        if (requestedCube != null)
        {
            requestedCube.Draw(Relativity.CRectFromPRect(position, requestedCube.RelativeScale, requestedCube.RelativePosition), depth - 0.1f, exposure, scissorRect);
        }
    }

    // Draw the requested cube on behalf of the main container cube, also clamp the cube
    private void OnMainCubeDraw(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (requestedCube != null)
        {
            Vector2 idealRScl = new(1.0f / mainContainerCube.Tiling.y, 1.0f / mainContainerCube.Tiling.x);
            Vector2 normalizedRPos = (requestedCube.RelativePosition).normalized;
            Vector2 idealRPos = new(requestedCube.RelativePosition.x - normalizedRPos.x * requestedCube.RelativeScale.x + normalizedRPos.x * idealRScl.x,
                                    requestedCube.RelativePosition.y - normalizedRPos.y * requestedCube.RelativeScale.y + normalizedRPos.y * idealRScl.y);
            requestedCube.Draw(Relativity.CRectFromPRect(position, idealRScl, idealRPos), depth - 0.1f, exposure, CustomTextureRenderer2D.GetOverlapRect(scissorRect, position));
        }
    }

    public override float RequestToMove(Vector2 cRPos, Vector2 cRScl, Cube requestedCube, PlayerInputsManager.MovementInputs direction, bool external = false, bool specialMove = false)
    {
        if (mainContainerCube == null || mainContainerCube.Parent == null) return 0;

        float finalTargetTime = mainContainerCube.RequestToMove(cRPos, cRScl, requestedCube, direction, external, specialMove);
        if (finalTargetTime <= 0) return 0;

        // Override the current transition of the main camera with fade transition
        if (requestedCube.IsPlayer && MainCamera.Instance != null)
        {
            if (MainCamera.Instance.IsPlayingTrasition)
                MainCamera.Instance.StopTransition();
            FadeTransition fadeTransition = new(mainContainerCube, finalTargetTime);
            MainCamera.Instance.PlayTransition(fadeTransition);
        }

        // Take the requested cube from the main container cube -> Return later
        //requestedCubeTargetPos = mainContainerCube.ChildGrid.FindChild(requestedCube);
        //mainContainerCube.ChildGrid.RemoveChild(requestedCube);
        if (!mainContainerCube.CullingCubes.Contains(requestedCube))
            mainContainerCube.CullingCubes.Add(requestedCube);
        this.requestedCube = requestedCube;

        return finalTargetTime;
    }
}
