using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MyCamera : MonoBehaviour
{
    public enum MyCameraRenderMode { SingleCube, MultiCubes };

    private static MyCamera _instance = null;

    [Header("Render settings")]
    [SerializeField] bool _isRendering = true;
    [SerializeField] MyCameraRenderMode _renderMode = MyCameraRenderMode.SingleCube;
    [SerializeField] float _defaultExposure = 0.0f;

    [Header("Single Cube mode")]
    [SerializeField] BaseCube _targetCube;
    [SerializeField] Rect _renderRect;
    [SerializeField] bool _renderParents = true;
    [SerializeField] int _numberOfParentsToTraverse = 3;
    private bool _isHorizFlipped = false;

    [Header("Multiple Cubes mode")]
    [SerializeField] List<CubeRenderDetails> _cubesToRender = new();

    [Header("Gizmos")]
    [SerializeField] bool _useGizmos = true;
    [SerializeField] Color _cubeRectColor = Color.yellow;

    private Camera _mainCamera;
    private float _currentExposure = 0f;
    private bool _useDebug = false;

    public static MyCamera Instance
    {
        get => _instance;
    }

    public Camera MainCamera
    {
        get => _mainCamera;
    }

    public bool IsHorizFlipped
    {
        get => _isHorizFlipped;
        set => _isHorizFlipped = value;
    }
    public MyCameraRenderMode RenderMode
    {
        get => _renderMode;
    }
    public float Exposure
    {
        get => _currentExposure;
        set => _currentExposure = value;
    }
    public BaseCube TargetCube
    {
        get => _targetCube;
    }
    public Rect RenderRect
    {
        get
        {
            Rect realRenderRect = _renderRect;
            if (_isHorizFlipped) realRenderRect.size = new(-_renderRect.size.x, _renderRect.size.y);
            return realRenderRect;
        }
    }

    private void Awake()
    {
        _mainCamera = GetComponent<Camera>();

        // Keep the camera of this very scene
        if (_instance != null && _instance != this) Destroy(_instance);
        _instance = this;
    }

    private void OnEnable()
    {
        _currentExposure = _defaultExposure;

        Vux.Core.OnBeginRender += HandleBeginRender;
        Vux.Core.OnFinishRender += HandleFinishRender;
    }

    private void HandleBeginRender()
    {
        if (!_isRendering)
        {
            return;
        }

        switch (_renderMode)
        {
            case MyCameraRenderMode.SingleCube:
                RenderSingleCube();
                break;
            case MyCameraRenderMode.MultiCubes:
                RenderMultiCubes();
                break;
        }
    }

    private void RenderSingleCube()
    {
        _targetCube.Renderer.Render(_renderRect);
    }

    private void RenderMultiCubes()
    {
        foreach (var cubeRenderDetail in _cubesToRender)
        {
            if (cubeRenderDetail.TargetCube == null) return;
            Rect renderPos = cubeRenderDetail.RenderRect;
            // if (isHorizFlipped)
            // {
            //     renderPos.size = new(-renderPos.size.x, renderPos.size.y);
            //     renderPos.position = new(-renderPos.position.x, renderPos.position.y);
            // }
            // if (cubeRenderDetail.TargetCube.IsHorizFlipped) renderPos.size = new(-renderPos.size.x, renderPos.size.y);
            cubeRenderDetail.TargetCube.Renderer.Render(renderPos);
            //cubeRenderDetail.TargetCube.Draw(renderPos, RenderingUtils.DEFAULT_PRIORITY, depth, exposure, GetScreenRect());
        }
    }

    private void HandleFinishRender()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _cubeRectColor;
        switch (_renderMode)
        {
            case MyCameraRenderMode.SingleCube:
                Gizmos.DrawCube(_renderRect.position, _renderRect.size);
                break;
            case MyCameraRenderMode.MultiCubes:
                foreach (var renderDetails in _cubesToRender)
                {
                    Gizmos.DrawCube(renderDetails.RenderRect.position, renderDetails.RenderRect.size);
                }
                break;
        }
    }

    [Serializable]
    private struct CubeRenderDetails
    {
        public BaseCube TargetCube;
        public Rect RenderRect;
    }
}
