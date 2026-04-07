using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour
{
    public enum MainCameraRenderMode { SingleCube, MultipleCubes };
    public enum CameraMovements { ZoomIn, ZoomOut }

    private static MainCamera instance;

    [Header("Render settings")]
    [SerializeField] bool isRendering = true;
    [SerializeField] MainCameraRenderMode renderMode = MainCameraRenderMode.SingleCube;

    [Header("Single Cube mode")]
    [SerializeField] ContainerCube targetCube;
    [SerializeField] Rect renderPosition;
    [SerializeField] bool renderParents = true; // Used for SingleCube mode
    [SerializeField] int numberOfParentsToTraverse = 3;

    [Header("Multiple Cubes mode")]
    [SerializeField] List<CubeRenderDetail> cubesToRender = new();

    [Header("Gizmos")]
    [SerializeField] bool useGizmos = true;
    [SerializeField] Color cubeRenderPositionColor = Color.yellow;

    private Camera mainCamera;
    private Coroutine zoomCoroutine = null;

    public static MainCamera Instance { get => instance; }
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }

    private void Update()
    {
        Render();
    }

    private void Render(float depth = 0)
    {
        if (!isRendering) return;
        switch (renderMode)
        {
            case MainCameraRenderMode.SingleCube:
                RenderSingleCube(depth);
                break;
            case MainCameraRenderMode.MultipleCubes:
                RenderMultipleCubes(depth);
                break;
        }
    }

    private void RenderMultipleCubes(float depth)
    {
        // Not render parents by default
        foreach (var cubeRenderDetail in cubesToRender)
        {
            if (cubeRenderDetail == null || cubeRenderDetail.TargetCube == null) return;
            cubeRenderDetail.TargetCube.Draw(cubeRenderDetail.RenderPosition, depth);
        }
    }

    private void RenderSingleCube(float depth)
    {
        if (targetCube == null) return;
        if (renderParents)
        {
            // Traverse through the target cube's parents
            ContainerCube newTargetCube = targetCube;
            Rect newRenderPosition = renderPosition;
            int parentCount = 0;
            while(parentCount < numberOfParentsToTraverse)
            {
                if (newTargetCube.Parent == null) break;
                newRenderPosition = Relativity.PRectFromCRect(newRenderPosition, newTargetCube.RelativeScale, newTargetCube.RelativePosition);
                newTargetCube = newTargetCube.Parent;
                parentCount++;
            }

            if (newTargetCube != null)
            {
                newTargetCube.Draw(newRenderPosition, depth);
            }
        }
        else targetCube.Draw(renderPosition, depth);
    }

    public void SetNewTargetCube(ContainerCube newTarget)
    {
        if (newTarget == null || newTarget == this) return;
        targetCube = newTarget;
        FocusOnTargetCube();
    }

    [ContextMenu("Focus")]
    public void FocusOnTargetCube()
    {
        if (renderMode != MainCameraRenderMode.SingleCube) return;
        if (targetCube == null) return;

        transform.position = new Vector3 (renderPosition.position.x, renderPosition.position.y, transform.position.z);
        mainCamera.orthographicSize = GetIdealOrthographicSize(renderPosition, targetCube);

        //Debug.Log("Ideal ortho size: " + GetIdealOrthographicSize(renderPosition, targetCube));
    }
    public void ChangeTarget(Vector2 prevRPosToNew, Vector2 prevRSclToNew, ContainerCube newTarget, float time = 0.5f)
    {
        // rPos and rScl are relative values of the old target to the new target

        if (newTarget == null || newTarget == targetCube) return;
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        if (targetCube == null)
        {
            targetCube = newTarget;
            FocusOnTargetCube();
            return;
        }
        // Zooming out
        if (prevRSclToNew.x <= 1 || prevRSclToNew.y <= 1) zoomCoroutine = StartCoroutine(ZoomOutCoroutine(prevRPosToNew, prevRSclToNew, newTarget, time));
        // Zooming in
        else zoomCoroutine = StartCoroutine(ZoomInCoroutine(prevRPosToNew, prevRSclToNew, newTarget, time));
    }

    public void StopZooming()
    {
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = null;
    }

    public float GetIdealOrthographicSize(Rect renderPosition, ContainerCube target)
    {
        if (renderMode != MainCameraRenderMode.SingleCube) return mainCamera.orthographicSize;
        if (targetCube == null) return mainCamera.orthographicSize;

        if (Screen.width > Screen.height)
        {
            return renderPosition.height * 0.5f + ((float)renderPosition.height / target.Tiling.x);
        }
        else
        {
            float horizontalOrthographicSize = renderPosition.width * 0.5f + ((float)renderPosition.width / target.Tiling.y);
            return horizontalOrthographicSize * (Screen.height / Screen.width);
        }
    }

    private IEnumerator ZoomInCoroutine(Vector2 prevRPosToNew, Vector2 prevRSclToNew, ContainerCube newTarget, float time)
    {
        /* Zoom in logic:
         * - Target cube is still the old target at first
         * - Keep old ortho and transform
         * - Calculate new target Rect based on old target Rect
         * - Calculate new ortho and new position based on new target Rect
         * - Start lerping toward new ortho and new position in "time"
         * - Set new target and focus on new target
         */
        Vector2 newTargetRScl = new Vector2(1.0f / prevRSclToNew.x, 1.0f / prevRSclToNew.y);
        Vector2 newTargetRPos = Relativity.PRPosToAChild(prevRPosToNew, prevRSclToNew);
        Rect newTargetRect = Relativity.CRectFromPRect(renderPosition, newTargetRScl, newTargetRPos);

        float elapsedTime = -1;
        float oldOrthoSize = mainCamera.orthographicSize;
        float targetOrthoSize = GetIdealOrthographicSize(newTargetRect, newTarget);
        Vector3 oldPos = transform.position;
        Vector3 targetPos = new Vector3(newTargetRect.x, newTargetRect.y, -10);
        //bool skippedFirstFrame = false;
        while (elapsedTime < time)
        {
            //if (!skippedFirstFrame)
            //{
            //    // Skip the first frame to make sure the zooming in process starts at elapsed time = 0, not deltaTime
            //    skippedFirstFrame = true;
            //    yield return null;
            //    continue;
            //}

            mainCamera.orthographicSize = oldOrthoSize;
            transform.position = oldPos;

            if (elapsedTime < 0) elapsedTime = 0; // Make sure the zooming in process starts at elapsed time = 0, not deltaTime
            else elapsedTime += Time.deltaTime;

            mainCamera.orthographicSize = Mathf.Lerp(oldOrthoSize, targetOrthoSize, elapsedTime / time);
            transform.position = Vector3.Lerp(oldPos, targetPos, elapsedTime / time);

            if (elapsedTime >= time)
            {
                // Last frame
                targetCube = newTarget;
            }
            yield return null;
        }

        FocusOnTargetCube();
        zoomCoroutine = null;
    }
    private IEnumerator ZoomOutCoroutine(Vector2 prevRPosToNew, Vector2 prevRSclToNew, ContainerCube newTarget, float time)
    {
        /* Zoom out logic:
         * - Target cube is set to the new target
         * - Camera's ortho and position is set based on old target
         * - Calculate old ortho and old transform based on relative values and current Render Position
         * - Set ortho and transform the old value calculated above
         * - Lerp toward ideal ortho and Render Position's position
         * - Focus on the new target
         */

        targetCube = newTarget;
        Rect oldTargetNewRect = Relativity.CRectFromPRect(renderPosition, prevRSclToNew, prevRPosToNew);
        float cameraRelativeOrthoSize = mainCamera.orthographicSize / renderPosition.height;
        Vector2 cameraRPosToOldTarget = Relativity.CRPosFromCRealPos(renderPosition, transform.position);

        float elapsedTime = -1;
        var oldOrthoSize = cameraRelativeOrthoSize * oldTargetNewRect.height;
        var targetOrthoSize = GetIdealOrthographicSize(renderPosition, targetCube);
        Vector3 oldPos = Relativity.CRealPosFromCRPos(oldTargetNewRect, cameraRPosToOldTarget); ; oldPos.z = -10;
        Vector3 targetPos = renderPosition.position; targetPos.z = -10;

        mainCamera.orthographicSize = oldOrthoSize;
        transform.position = oldPos;

        Render(-0.1f);

        //bool skippedFirstFrame = false;

        while (elapsedTime < time)
        {
            //if (!skippedFirstFrame)
            //{
            //    // Skip the first frame to make sure the zooming in process starts at elapsed time = 0, not deltaTime
            //    skippedFirstFrame = true;
            //    yield return null;
            //    continue;
            //}


            if (elapsedTime < 0) elapsedTime = 0;
            else elapsedTime += Time.deltaTime;

            mainCamera.orthographicSize = Mathf.Lerp(oldOrthoSize, targetOrthoSize, elapsedTime / time);
            transform.position = Vector3.Lerp(oldPos, targetPos, elapsedTime / time);
            yield return null;
        }
        FocusOnTargetCube();
        zoomCoroutine= null;
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
