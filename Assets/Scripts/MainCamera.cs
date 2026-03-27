using System;
using System.Collections;
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

    private Camera mainCamera;
    private Coroutine zoomCoroutine = null;
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }
    private void Start()
    {
        FocusOnTargetCube();
    }
    private void Update()
    {
        Render();
    }
    private void Render()
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

            if (newTargetCube != null)
            {
                newTargetCube.Draw(newRenderPosition);
            }
        }
        else targetCube.Draw(renderPosition);
    }

    // This function can only be used in SingleCube mode
    [ContextMenu("Focus On Target Cube")]
    public void FocusOnTargetCube()
    {
        if (renderMode != MainCameraRenderMode.SingleCube) return;
        if (targetCube == null) return;

        transform.position = new Vector3 (renderPosition.position.x, renderPosition.position.y, transform.position.z);
        mainCamera.orthographicSize = GetIdealOrthographicSize(renderPosition, targetCube);
    }

    public enum CameraMovements { ZoomIn, ZoomOut }
    public void ChangeTarget(Vector2 rPos, Vector2 rScl, EnterableCube newTarget)
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
        if (rScl.x < 1 || rScl.y < 1) zoomCoroutine = StartCoroutine(ZoomOutCoroutine(rPos, rScl, newTarget));
        // Zooming in
        else zoomCoroutine = StartCoroutine(ZoomInCoroutine(rPos, rScl, newTarget));
    }
    [ContextMenu("Demo zoom out")]
    public void DemoZoomOut()
    {
        ChangeTarget(targetCube.RelativePosition, targetCube.RelativeScale, targetCube.Parent);
    }
    [SerializeField] EnterableCube demoZoomInCube;
    [ContextMenu("Demo zoom in")]
    public void DemoZoomIn()
    {
        if (demoZoomInCube == null) return;
        Vector2 rScl = new(); rScl.x = 1.0f / demoZoomInCube.RelativeScale.x; rScl.y = 1.0f / demoZoomInCube.RelativeScale.y;
        Vector2 rPos = Relativity.PRPosFromCRPosAndCRScl(demoZoomInCube.RelativePosition, demoZoomInCube.RelativeScale);

        Debug.Log($"Demo cube: {demoZoomInCube.name}, childRPos: {demoZoomInCube.RelativePosition}, childRScl: {demoZoomInCube.RelativeScale}, parentRPos: {rPos}, parentRScl {rScl}");
        ChangeTarget(rPos, rScl, demoZoomInCube);
    }

    public float GetIdealOrthographicSize(Rect renderPosition, EnterableCube target)
    {
        if (renderMode != MainCameraRenderMode.SingleCube) return mainCamera.orthographicSize;
        if (targetCube == null) return mainCamera.orthographicSize;

        if (Screen.width > Screen.height)
        {
            int tiling = target.Tiling;
            return renderPosition.height * 0.5f + ((float)renderPosition.height / tiling);
        }
        else
        {
            int tiling = target.Tiling;
            float horizontalOrthographicSize = renderPosition.width * 0.5f + ((float)renderPosition.width / tiling);
            return horizontalOrthographicSize * (Screen.height / Screen.width);
        }
    }

    private IEnumerator ZoomInCoroutine(Vector2 rPos, Vector2 rScl, EnterableCube newTarget, float time = 0.5f)
    {
        /* Zoom in logic:
         * - Target cube is still the old target as first
         * - Keep old ortho and transform
         * - Calculate new target Rect based on old target Rect
         * - Calculate new ortho and new position based on new target Rect
         * - Start lerping toward new ortho and new position in "time"
         * - Set new target and focus on new target
         */
        Vector2 newTargetRScl = new Vector2(1.0f / rScl.x, 1.0f / rScl.y);
        Vector2 newTargetRPos = Relativity.PRPosFromCRPosAndCRScl(rPos, rScl);
        Rect newTargetRect = Relativity.CRectFromPRect(renderPosition, newTargetRScl, newTargetRPos);

        float elapsedTime = -1;
        float oldOrthoSize = mainCamera.orthographicSize;
        float targetOrthoSize = GetIdealOrthographicSize(newTargetRect, newTarget);
        Vector3 oldPos = transform.position;
        Vector3 targetPos = new Vector3(newTargetRect.x, newTargetRect.y, -10);
        while (elapsedTime < time)
        {
            if (elapsedTime < 0) elapsedTime = 0; // Make sure the zooming in process starts at elapsed time = 0, not deltaTime
            else elapsedTime += Time.deltaTime;

            mainCamera.orthographicSize = Mathf.Lerp(oldOrthoSize, targetOrthoSize, elapsedTime / time);
            transform.position = Vector3.Lerp(oldPos, targetPos, elapsedTime / time);

            Debug.Log(mainCamera.orthographicSize + " elapsed time: " + elapsedTime + " delta: " + Time.deltaTime);

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
    private IEnumerator ZoomOutCoroutine(Vector2 rPos, Vector2 rScl, EnterableCube newTarget, float time = 0.5f)
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
        // Calculate old target new Rect
        Rect oldTargetNewRect = Relativity.CRectFromPRect(renderPosition, rScl, rPos);
        // Calculate camera relative values from the old target
        float cameraRelativeOrthoSize = mainCamera.orthographicSize / renderPosition.height;
        Vector2 cameraRPosFromOldTarget = Relativity.CRPosFromCRealPos(renderPosition, transform.position);

        float elapsedTime = -1;
        var oldOrthoSize = cameraRelativeOrthoSize * oldTargetNewRect.height;
        var targetOrthoSize = GetIdealOrthographicSize(renderPosition, targetCube);
        Vector3 oldPos = Relativity.CRealPosFromCRPos(oldTargetNewRect, cameraRPosFromOldTarget); ; oldPos.z = -10;
        Vector3 targetPos = renderPosition.position; targetPos.z = -10;

        while (elapsedTime < time)
        {
            if (elapsedTime < 0) elapsedTime = 0;
            else elapsedTime += Time.deltaTime;

            mainCamera.orthographicSize = Mathf.Lerp(oldOrthoSize, targetOrthoSize, elapsedTime / time);
            transform.position = Vector3.Lerp(oldPos, targetPos, elapsedTime / time);
            Debug.Log(mainCamera.orthographicSize + " " + Time.deltaTime);
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
