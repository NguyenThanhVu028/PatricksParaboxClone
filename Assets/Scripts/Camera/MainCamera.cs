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
    [SerializeField] float exposure = 0.0f;

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
    private Coroutine transitionCoroutine = null;

    public static MainCamera Instance { get => instance; }

    public bool IsPlayingTrasition { get => transitionCoroutine != null; }
    public MainCameraRenderMode RenderMode { get => renderMode; }
    public float Exposure { get => exposure; set => exposure = value; }
    public ContainerCube TargetCube { get => targetCube; }
    public Rect RenderPosition { get => renderPosition; }
    public float OrthographicSize { get => mainCamera.orthographicSize; set => mainCamera.orthographicSize = value; }

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }
    private void OnEnable()
    {
        //RenderPipelineManager.endCameraRendering += OnRender;
        CustomRendererFeature.CustomRenderPass.OnExecuteCmd.AddListener(OnRender);
    }

    private void OnDisable()
    {
        //RenderPipelineManager.endCameraRendering -= OnRender;
        CustomRendererFeature.CustomRenderPass.OnExecuteCmd.RemoveListener(OnRender);
    }
    private void OnRender()
    {
        Render();
    }

    public void Render(float depth = 0)
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
            cubeRenderDetail.TargetCube.Draw(cubeRenderDetail.RenderPosition, depth, exposure, GetScreenRect());
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
                newTargetCube.Draw(newRenderPosition, depth, exposure, GetScreenRect());
            }
        }
        else targetCube.Draw(renderPosition, depth, exposure, GetScreenRect());
    }

    public void SetNewTargetCube(ContainerCube newTarget)
    {
        if (newTarget == null || newTarget == this) return;
        targetCube = newTarget;
        //FocusOnTargetCube();
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

    public void PlayTransition(ICameraTransition transition)
    {
        transitionCoroutine = StartCoroutine(TransitionWrapper(transition.ExecuteCoroutine(this)));
    }

    public void StopTransition()
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = null;
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

    private IEnumerator TransitionWrapper(IEnumerator transition)
    {
        if (transition != null)
            yield return transition;
        transitionCoroutine = null;
    }

    public Rect GetScreenRect()
    {
        Rect screenRect = new();
        screenRect.position = mainCamera.transform.position;
        screenRect.height = mainCamera.orthographicSize * 2.0f;
        screenRect.width = screenRect.height * ((float)mainCamera.pixelWidth / mainCamera.pixelHeight);
        return screenRect;
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
