using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour
{
    public enum MainCameraRenderMode { SingleCube, MultipleCubes };

    [SerializeField] MainCameraRenderMode renderMode = MainCameraRenderMode.SingleCube;

    [Header("Single Cube mode")]
    [SerializeField] Cube targetCube;
    [SerializeField] Rect renderPosition;
    [SerializeField] bool renderParents = true; // Used for SingleCube mode
    [SerializeField] int numberOfParentsToTraverse = 3;

    [Header("Multiple Cubes mode")]
    [SerializeField] List<CubeRenderDetail> cubesToRender = new();

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
        targetCube.Draw(renderPosition);
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

    [Serializable]
    public class CubeRenderDetail
    {
        [SerializeField] Cube targetCube;
        [SerializeField] Rect renderPosition;

        public Cube TargetCube { get => targetCube; }
        public Rect RenderPosition { get => renderPosition; }
    }
}
