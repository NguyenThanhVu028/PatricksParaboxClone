using System.Collections.Generic;
using UnityEngine;

public class CloneCube : ContainerCube
{
    [SerializeField] ContainerCube mainContainerCube;

    [SerializeField] Cube requestedCube;

    //public override void Draw(Rect position, float depth = 0)
    //{
    //    if (mainContainerCube != null) mainContainerCube.Draw(position, depth);
    //}

    private void Update()
    {
        if(requestedCube != null)
        {
            var requestedCubeMovement = requestedCube.GetComponent<CubeMovement>();
            if (requestedCubeMovement == null) requestedCube = null;
            if (!requestedCubeMovement.IsMoving || !requestedCubeMovement.IsExternal || requestedCube.Parent != mainContainerCube) requestedCube = null;
        }
    }

    protected override void DrawFloor(Rect position, float depth)
    {
        if (mainContainerCube == null || mainContainerCube.FloorTexture == null || mainContainerCube.FloorTexture.GetTexture() == null) return;
        CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, mainContainerCube.FloorTexture.GetTexture(), mainContainerCube.RealCubeColor, position.position, position.size, depth);
    }

    protected override void DrawWalls(Rect position, float depth)
    {
        if (mainContainerCube == null || mainContainerCube.StaticTexturesRT == null) return;
        CustomTextureRenderer2D.RenderMesh(cubeMesh, outlineMat, mainContainerCube.StaticTexturesRT, mainContainerCube.RealCubeColor, position.position, position.size, depth);
    }

    protected override void DrawChildCubes(Rect position, float depth)
    {
        if (mainContainerCube == null) return;

        // Draw empty cubes first
        foreach (var emptyCube in mainContainerCube.EmptyCubes)
        {
            emptyCube.Draw(Relativity.CRectFromPRect(position, emptyCube.RelativeScale, emptyCube.RelativePosition), depth);
        }

        List<Cube> movingCubes = new();
        // Draw static cubes
        foreach (var childCube in mainContainerCube.ChildCubes)
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
            Vector2 idealRScl = movingCube.RelativeScale;
            Vector2 idealRPos = movingCube.RelativePosition;
            var movingCubeMovement = movingCube.GetComponent<CubeMovement>();
            if (movingCubeMovement != null && movingCubeMovement.IsExternal)
            {
                if (movingCube != requestedCube)
                {
                    idealRScl = new(1.0f / mainContainerCube.Tiling.y, 1.0f / mainContainerCube.Tiling.x);
                    Vector2 normalizedRPos = (movingCube.RelativePosition).normalized;
                    idealRPos = new(movingCube.RelativePosition.x - normalizedRPos.x * movingCube.RelativeScale.x + normalizedRPos.x * idealRScl.x,
                                    movingCube.RelativePosition.y - normalizedRPos.y * movingCube.RelativeScale.y + normalizedRPos.y * idealRScl.y);
                }

            }
            movingCube.Draw(Relativity.CRectFromPRect(position, idealRScl, idealRPos), depth - 0.1f);
        }
    }

    public override float RequestToMove(Vector2 cRPos, Vector2 cRScl, Cube requestedCube, PlayerInputsManager.MovementInputs direction, bool external = false, bool specialMove = false)
    {
        if (mainContainerCube == null || mainContainerCube.Parent == null) return 0;

        // Change the requested cube's relative position and scale to properly update the camera
        //Vector2 oldRPos = requestedCube.RelativePosition;
        //Vector2 oldRScl = requestedCube.RelativeScale;

        //Vector2 rPosMainParentToMain = Relativity.PRPosToAChild(mainContainerCube.RelativePosition, mainContainerCube.RelativeScale);
        //Vector2 rSclMainParentToMain = new(1.0f / mainContainerCube.RelativeScale.x, 1.0f / mainContainerCube.RelativeScale.y);
        //Vector2 rPosReqCubeToMainParent = Relativity.SRPosFromSameParent(rPosMainParentToMain, rSclMainParentToMain, cRPos);
        //Vector2 rSclReqCubeToMainParent = new(cRScl.x * mainContainerCube.RelativeScale.x, cRScl.y * mainContainerCube.RelativeScale.y);

        //requestedCube.RelativePosition = rPosReqCubeToMainParent;
        //requestedCube.RelativeScale = rSclReqCubeToMainParent;

        float res = mainContainerCube.RequestToMove(cRPos, cRScl, requestedCube, direction, external, specialMove);
        if (res <= 0)
        {
            //requestedCube.RelativePosition = oldRPos;
            //requestedCube.RelativeScale = oldRScl;
            return 0;
        }
        this.requestedCube = requestedCube;
        return res;
    }
}
