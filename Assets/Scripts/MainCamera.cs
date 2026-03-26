using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour
{
    public enum MainCameraRenderMode { SingleCube, MultipleCubes };

    [SerializeField] MainCameraRenderMode renderMode = MainCameraRenderMode.SingleCube;

    [Header("Single Cube mode")]
    [SerializeField] EnterableCube targetCube;
    [SerializeField] Rect renderPosition;
    [SerializeField] bool renderParents = true; // Used for SingleCube mode
    [SerializeField] int numberOfParentsToTraverse = 3;

    [Header("Multiple Cubes mode")]
    [SerializeField] List<CubeRenderDetail> cubesToRender = new();

    [Header("Gizmos")]
    [SerializeField] bool useGizmos = true;
    [SerializeField] Color cubeRenderPositionColor = Color.yellow;

    private void Update()
    {
        switch (renderMode)
        {
            case MainCameraRenderMode.SingleCube:
                RenderSingleCube();
                break;
            case MainCameraRenderMode.MultipleCubes:
                RenderMultipleCubes();
                break;
        }
    }

    private void RenderMultipleCubes()
    {
        // Not render parents by default
        foreach (var cubeRenderDetail in cubesToRender)
        {
            if (cubeRenderDetail == null || cubeRenderDetail.TargetCube == null) return;
            cubeRenderDetail.TargetCube.Draw(cubeRenderDetail.RenderPosition);
        }
    }

    private void RenderSingleCube()
    {
        if (targetCube == null) return;
        if (renderParents)
        {
            // Traverse through the target cube's parents
            EnterableCube newTargetCube = targetCube;
            Rect newRenderPosition = renderPosition;
            int parentCount = 0;
            while(parentCount < numberOfParentsToTraverse)
            {
                if (newTargetCube.Parent == null) break;
                newRenderPosition = Relativity.PRectFromCRect(newRenderPosition, newTargetCube.RelativeScale, newTargetCube.RelativePosition);
                newTargetCube = newTargetCube.Parent;
                parentCount++;
            }

            if (newTargetCube != null) newTargetCube.Draw(newRenderPosition);
        }
        else targetCube.Draw(renderPosition);
    }

    // This function can only be used in SingleCube mode
    [ContextMenu("Focus On Target Cube")]
    public void FocusOnTargetCube()
    {
        if (renderMode != MainCameraRenderMode.SingleCube) return;
        if (targetCube == null) return;

        renderPosition.position = Vector2.zero;
        if (targetCube is EnterableCube enterableCube)
        {
            int tiling = enterableCube.Tiling;
            float halfRenderZoneSize = (Camera.main.orthographicSize / (tiling * 0.5f + 1)) * (tiling * 0.5f);
            renderPosition.width = renderPosition.height = halfRenderZoneSize * 2;
        }
        else
        {
            renderPosition.width = renderPosition.height = Camera.main.orthographicSize / 3.0f;
        }
    }

    private void OnDrawGizmos()
    {
        if (!useGizmos) return;
        Gizmos.color = cubeRenderPositionColor;
        switch (renderMode)
        {
            case MainCameraRenderMode.SingleCube:
                Gizmos.DrawWireCube(renderPosition.position, renderPosition.size);
                break;
            case MainCameraRenderMode.MultipleCubes:
                foreach(var cube in cubesToRender)
                {
                    if (cube == null) continue;
                    Gizmos.DrawWireCube(cube.RenderPosition.position, cube.RenderPosition.size);
                }
                break;
        }
        
    }

    [Serializable]
    public class CubeRenderDetail
    {
        [SerializeField] Cube targetCube;
        [SerializeField] Rect renderPosition;

        public Cube TargetCube { get => targetCube; }
        public Rect RenderPosition { get => renderPosition; }
    }
}
