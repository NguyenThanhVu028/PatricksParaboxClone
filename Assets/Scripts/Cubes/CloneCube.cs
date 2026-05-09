using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CloneCube : ContainerCube
{
    [SerializeField] protected ContainerCube mainContainerCube;
    [SerializeField] protected float minChildCubePixelToRender = 10;

    public override void Init()
    {
        base.Init();
        InitCloneCube();
    }
    private void InitCloneCube()
    {
        if (hasInit) return;
        hasInit = true;
    }

    public override void DrawFloor(Rect position, int priority, float depth, float exposure, Rect? scissorRect)
    {
        if (mainContainerCube == null) return;
        mainContainerCube.DrawFloor(position, priority, depth + floorDepthOffset, exposure, scissorRect);
    }

    public override void DrawWalls(Rect position, int priority, float depth, float exposure, Rect? scissorRect)
    {
        if (mainContainerCube == null) return;
        mainContainerCube.DrawWalls(position, priority, depth + wallDepthOffset, exposure, scissorRect);
    }

    protected override void DrawChildCubes(Rect position, int priority, float depth, float exposure, Rect? scissorRect, int minSize)
    {
        if (mainContainerCube == null) return;

        childGrid.Tiling = mainContainerCube.ChildGrid.Tiling;

        mainContainerCube.DrawInnerCubes(position, priority, depth, exposure, scissorRect, minChildCubePixelToRender);

        mainContainerCube.DrawExternalCube(position, priority + externalPriority, depth + externalDepthOffset, exposure, scissorRect, minSize);

        if (mainContainerCube != null)
        {
            hideAlterEnterCube = false;
            alterEnterCube = mainContainerCube.AlterEnterCube;
            alterEnterDirection = mainContainerCube.AlterEnterDirection;
            alterEnterOldRPos = mainContainerCube.AlterEnterOldRPos;
        }
        DrawAlterEnterCube(position, priority + alterEnterExitPriority, depth + alterEnterExitDepthOffset, exposure, scissorRect, minSize);
        if (mainContainerCube != null)
        {
            hideExitCube = false;
            exitCube = mainContainerCube.ExitCube;
            exitDirection = mainContainerCube.ExitDirection;
            exitCubeOldRPos = mainContainerCube.ExitCubeOldRPos;
        }
        DrawExitCube(position, priority + alterEnterExitPriority, depth + alterEnterExitDepthOffset, exposure, scissorRect, minSize);

    }

    public override void ModifyChildCubeEnter(CubeProperties childCube)
    {
        if (childCube == null) return;
        if (isHorizFlipped != mainContainerCube.IsHorizFlipped) childCube.IsHorizFlipped = !childCube.IsHorizFlipped;
    }

    public override void ModifyChildCubeExit(CubeProperties childCube)
    {
        if (childCube == null) return;
        if (isHorizFlipped != mainContainerCube.IsHorizFlipped) childCube.IsHorizFlipped = !childCube.IsHorizFlipped;
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

        int currentPreviousParentIndex = requestedCube.PreviousParents.Count;
        if (isHorizFlipped != mainContainerCube.IsHorizFlipped)
        {
            cRPos.x = -cRPos.x;
            direction = CubeMovement.FlipMovementInput(direction, true);
            requestedCube.PreviousParents.Add(new(CubeMovement.LayerDirections.In, this, Vector2.zero, Vector2.one, true));
            //requestedCube.PreviousParents.Add(
            //        //new(CubeMovement.LayerDirections.In, 
            //        //    this, 
            //        //    new(relativePosition.x + mainContainerCube.RelativePosition.x * ((float)mainContainerCube.Parent.Tiling.y / parent.Tiling.y), 
            //        //        relativePosition.y - mainContainerCube.RelativePosition.y * ((float)mainContainerCube.Parent.Tiling.x / parent.Tiling.x)), 
            //        //    new((float)mainContainerCube.Parent.Tiling.y / parent.Tiling.y, 
            //        //        (float)mainContainerCube.Parent.Tiling.x / parent.Tiling.x), true));
            //        new(CubeMovement.LayerDirections.In,
            //        this,
            //        new(relativePosition.x + mainContainerCube.RelativePosition.x * ((float)(1.0f / mainContainerCube.RelativeScale.x) / (1.0f / relativeScale.x)),
            //            relativePosition.y - mainContainerCube.RelativePosition.y * ((float)(1.0f / mainContainerCube.RelativeScale.y) / (1.0f / relativeScale.y))),
            //        new((float)(1.0f / mainContainerCube.RelativeScale.x) / (1.0f / relativeScale.x),
            //            (float)(1.0f / mainContainerCube.RelativeScale.y) / (1.0f / relativeScale.y)), true));
        }
        else requestedCube.PreviousParents.Add(new(CubeMovement.LayerDirections.In, this, Vector2.zero, Vector2.one, false));
        //requestedCube.PreviousParents.Add(
        //        //new(CubeMovement.LayerDirections.In, 
        //        //    this, 
        //        //    new(relativePosition.x - mainContainerCube.RelativePosition.x * ((float)mainContainerCube.Parent.Tiling.y / parent.Tiling.y), 
        //        //        relativePosition.y - mainContainerCube.RelativePosition.y * ((float)mainContainerCube.Parent.Tiling.x / parent.Tiling.x)), 
        //        //    new((float)mainContainerCube.Parent.Tiling.y / parent.Tiling.y, 
        //        //        (float)mainContainerCube.Parent.Tiling.x / parent.Tiling.x), false));
        //        new(CubeMovement.LayerDirections.In,
        //        this,
        //        new(relativePosition.x - mainContainerCube.RelativePosition.x * ((float)(1.0f / mainContainerCube.RelativeScale.x) / (1.0f / relativeScale.x)),
        //            relativePosition.y - mainContainerCube.RelativePosition.y * ((float)(1.0f / mainContainerCube.RelativeScale.y) / (1.0f / relativeScale.y))),
        //        new((float)(1.0f / mainContainerCube.RelativeScale.x) / (1.0f / relativeScale.x),
        //            (float)(1.0f / mainContainerCube.RelativeScale.y) / (1.0f / relativeScale.y)), false));

        requestedCube.PreviousParents.Add(new(CubeMovement.LayerDirections.None, null, Vector2.zero, Vector2.one, false));
        float finalTargetTime = mainContainerCube.RequestToMove(cRPos, cRScl, requestedCube, direction, external, specialMove);

        if (finalTargetTime <= 0)
        {
            requestedCube.RemovePreviousParent(this);
            return 0;
        }
        else
        {
            if (requestedCube.PreviousParents.Count > currentPreviousParentIndex + 2 && requestedCube.PreviousParents[currentPreviousParentIndex].Cube == this && requestedCube.PreviousParents[currentPreviousParentIndex + 2].Cube == mainContainerCube)
            {
                var previousParent = requestedCube.PreviousParents[currentPreviousParentIndex + 2];
                previousParent.RelativeScale = relativeScale;
                previousParent.RelativePosition = new ((isHorizFlipped != mainContainerCube.IsHorizFlipped) ? -relativePosition.x : relativePosition.x, relativePosition.y);
                previousParent.UseDefaultValues = false;
                requestedCube.PreviousParents[currentPreviousParentIndex + 2] = previousParent;

            }
        }


        return finalTargetTime;
    }
}
