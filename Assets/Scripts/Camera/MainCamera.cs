using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour
{
    public enum MainCameraRenderMode { SingleCube, MultipleCubes };
    public enum CameraMovements { ZoomIn, ZoomOut }

    private static MainCamera instance;

    [Header("Render settings")]
    [SerializeField] bool isRendering = true;
    [SerializeField] MainCameraRenderMode renderMode = MainCameraRenderMode.SingleCube;
    [SerializeField] float defaultExposure = 0.0f;
    [SerializeField] bool isHorizFlipped = false;

    [Header("Single Cube mode")]
    [SerializeField] ContainerCube targetCube;
    [SerializeField] Rect renderRect;
    [SerializeField] bool renderParents = true; // Used for SingleCube mode
    [SerializeField] int numberOfParentsToTraverse = 3;
    private ContainerCube previousTargetCube;
    private bool isRenderFlipped = false;

    [Header("Multiple Cubes mode")]
    [SerializeField] List<CubeRenderDetail> cubesToRender = new();

    [Header("Gizmos")]
    [SerializeField] bool useGizmos = true;
    [SerializeField] Color cubeRenderPositionColor = Color.yellow;

    private Camera mainCamera;
    private CameraTransition currentTransition = null;
    private Coroutine transitionCoroutine = null;
    private float exposure = 0f;

    public static MainCamera Instance { get => instance; }

    public bool IsPlayingTrasition { get => transitionCoroutine != null; }
    public bool IsHorizFlipped { get => isHorizFlipped; set => isHorizFlipped = value; }
    public MainCameraRenderMode RenderMode { get => renderMode; }
    public float Exposure { get => exposure; set => exposure = value; }
    public ContainerCube TargetCube { get => targetCube; }
    public Rect RenderRect
    {
        get
        {
            Rect realRenderRect = renderRect;
            if (isHorizFlipped) realRenderRect.size = new(-renderRect.size.x, renderRect.size.y);
            return realRenderRect;
        }
    }
    public Rect TargetCubeRect
    {
        get
        {
            if (targetCube == null) return new();
            Rect realRenderPosition = renderRect;
            if (isHorizFlipped) realRenderPosition.width = - realRenderPosition.width;
            if (targetCube.IsHorizFlipped) realRenderPosition.width = -realRenderPosition.width;
            return realRenderPosition;
        }
    }
    public float OrthographicSize { get => mainCamera.orthographicSize; set => mainCamera.orthographicSize = value; }
    
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }
    private void OnEnable()
    {
        exposure = defaultExposure;
        CustomRendererFeature.CustomRenderPass.OnExecuteCmd.AddListener(OnRender);
    }

    private void OnDisable()
    {
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
            Rect renderPos = cubeRenderDetail.RenderPosition;
            if (isHorizFlipped)
            {
                renderPos.size = new(-renderPos.size.x, renderPos.size.y);
                renderPos.position = new(-renderPos.position.x, renderPos.position.y);
            }
            if (cubeRenderDetail.TargetCube.IsHorizFlipped) renderPos.size = new(-renderPos.size.x, renderPos.size.y);
            cubeRenderDetail.TargetCube.Draw(renderPos, depth, exposure, GetScreenRect());
        }
    }

    private void RenderSingleCube(float depth)
    {
        if (targetCube == null) return;

        // Make sure to flip the camera if the target cube is suddenly flipped -> Consistent rendering direction
        if (targetCube == previousTargetCube && (TargetCubeRect.size.x < 0) != isRenderFlipped)
        {
            isHorizFlipped = !isHorizFlipped;
            if (HistoryManager.Instance != null)
            {
                HistoryRecord currentRecord = HistoryManager.Instance.GetCurrentRecord();
                if (currentRecord != null && currentRecord.CameraEvent != null) currentRecord.CameraEvent.IsCamHorizFlipped = isHorizFlipped;
            }
        }
        previousTargetCube = targetCube;
        isRenderFlipped = TargetCubeRect.size.x < 0;

        List<ParentDetails> parents = new();
        if (renderParents)
        {
            // Traverse through the target cube's parents
            ContainerCube newTargetCube = targetCube;
            Rect renderPos = TargetCubeRect;
            int parentCount = 0;
            parents.Add(new(newTargetCube, renderPos));
            while(parentCount < numberOfParentsToTraverse)
            {
                if (newTargetCube.Parent == null) break;
                Vector3 oldRenderPos = renderPos.position;
                renderPos = Relativity.PRectFromCRect(renderPos, newTargetCube.RelativeScale, newTargetCube.RelativePosition);
                if (newTargetCube.IsHorizFlipped)
                {
                    renderPos.width = - renderPos.width;
                    renderPos.x = oldRenderPos.x - (renderPos.x - oldRenderPos.x);
                }
                newTargetCube = newTargetCube.Parent;
                parentCount++;
                parents.Add(new(newTargetCube, renderPos));
            }
            for(int i = parents.Count - 1; i >= 0; i--)
            {
                if (i > 0) parents[i].Cube.CullingCubesOne.Add(parents[i - 1].Cube);
                var renderRect = parents[i].RenderRect;
                parents[i].Cube.DrawCube(renderRect, depth, exposure, GetScreenRect());
                if (i > 0) parents[i].Cube.CullingCubesOne.Remove(parents[i - 1].Cube);
            }
        }
        else
        {
            Rect renderPos = renderRect;
            if (isHorizFlipped) renderPos.size = new(-renderPos.size.x, renderPos.size.y);
            targetCube.Draw(renderPos, depth, exposure, GetScreenRect());
        }
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

        transform.position = new Vector3 (RenderRect.position.x, RenderRect.position.y, transform.position.z);
        mainCamera.orthographicSize = GetIdealOrthographicSize(RenderRect, targetCube);

        //Debug.Log("Ideal ortho size: " + GetIdealOrthographicSize(renderPosition, targetCube));
    }

    public void SetTransition(CameraTransition newTransition)
    {
        Debug.Log("Set transition: " + newTransition);
        if (newTransition == null || newTransition == currentTransition) return;
        if (currentTransition == null)
        {
            currentTransition = newTransition;
            return;
        }

        if (newTransition.IsHigherPriorityThan(currentTransition))
        {
            currentTransition = newTransition;
        }
    }
    public void ClearTransition() { currentTransition = null; }

    public void PlayTransition(List<Cube.PreviousParentDetails> cubesToTraverse, float targetTime)
    {
        if (transitionCoroutine != null || currentTransition == null) return;
        currentTransition.SetTargetTime(targetTime);
        currentTransition.SetCubesToTraverse(cubesToTraverse);
        transitionCoroutine = StartCoroutine(TransitionWrapper(currentTransition.ExecuteCoroutine(this)));
        currentTransition = null;
    }

    public void StopTransition()
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = null;
        currentTransition = null;
        exposure = defaultExposure;
    }

    public float GetIdealOrthographicSize(Rect renderPosition, ContainerCube target)
    {
        if (renderMode != MainCameraRenderMode.SingleCube) return mainCamera.orthographicSize;
        if (targetCube == null) return mainCamera.orthographicSize;

        if (Screen.width > Screen.height)
        {
            return renderPosition.height * 0.5f + (Mathf.Abs((float)renderPosition.height) / target.Tiling.x);
        }
        else
        {
            float horizontalOrthographicSize = renderPosition.width * 0.5f + (Mathf.Abs((float)renderPosition.width) / target.Tiling.y);
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
                Gizmos.DrawWireCube(RenderRect.position, RenderRect.size);
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

    struct ParentDetails
    {
        public ContainerCube Cube;
        public Rect RenderRect;

        public ParentDetails(ContainerCube cube, Rect renderRect)
        {
            this.Cube = cube;
            this.RenderRect = renderRect;
        }
    }
}
