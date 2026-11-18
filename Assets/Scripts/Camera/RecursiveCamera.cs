using System;
using System.Collections.Generic;
using UnityEngine;

public class RecursiveCamera
{
    private const string DEBUG_PREFIX = "[Recursive Camera]";
    private const float SortingOrderMin = 0;
    private const float SortingOrderMax = 1;

    public enum CameraRenderMode { SingleCube, MultiCubes };

    private static RecursiveCamera _instance = null;

    // Render Settings
    private bool _isRendering = true;
    private CameraRenderMode _renderMode = CameraRenderMode.SingleCube;
    private float _defaultExposure = 0.0f;
    private Rect _renderRect = new(0, 0, 1, 1);
    private Vector2 _renderRectPaddingProportion = new(1 / 8f, 1 / 8f);

    // Single Cube Mode
    private BaseCube _targetCube;
    private bool _renderParents = true;
    private int _numberOfParentsToTraverse = 3;
    private bool _isHorizFlipped = false;

    // Multiple Cubes Mode
    private List<BaseCube> _cubesToRender = new();
    private Vector2 _cubeRectSpacingProportion = new(1 / 8f, 1 / 8f);

    private Camera _mainCamera;
    private float _currentExposure = 0f;
    private bool _useDebug = false;

    private List<RenderCall> _renderCalls = new();

    public static RecursiveCamera Instance
    {
        get
        {
            _instance ??= new();
            return _instance;
        }
    }

    public Camera MainCamera
    {
        get => _mainCamera;
    }

    public CameraRenderMode RenderMode
    {
        get => _renderMode;
        set => _renderMode = value;
    }

    public bool IsHorizFlipped
    {
        get => _isHorizFlipped;
        set => _isHorizFlipped = value;
    }

    public float Exposure
    {
        get => _currentExposure;
        set => _currentExposure = value;
    }

    public BaseCube SingleTargetCube
    {
        get => _targetCube;
        set => _targetCube = value;
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

    public void AddCubeToRender(BaseCube cube)
    {
        _cubesToRender ??= new();
        _cubesToRender.Add(cube);
    }

    public void RemoveCubeToRender(BaseCube cube)
    {
        _cubesToRender?.Remove(cube);
    }

    public void ClearAllCubesToRender()
    {
        _cubesToRender?.Clear();
    }

    public void StartRendering()
    {
        StopRendering(); // Clear previous render session

        _mainCamera = Camera.main;

        _currentExposure = _defaultExposure;

        Vux.Core.OnPreRender += HandlePreRender;
        Vux.Core.OnRender += HandleRender;
    }

    public void StopRendering()
    {
        Vux.Core.OnPreRender -= HandlePreRender;
        Vux.Core.OnRender -= HandleRender;
    }

    private void HandlePreRender()
    {
        if (!_isRendering)
        {
            return;
        }

        AdjustCamera();

        _renderCalls ??= new();
        _renderCalls.Clear();

        switch (_renderMode)
        {
            case CameraRenderMode.SingleCube:
                RenderSingleCube();
                break;
            case CameraRenderMode.MultiCubes:
                RenderMultiCubes();
                break;
        }
    }

    private void AdjustCamera()
    {
        _mainCamera.orthographicSize = Mathf.Max(
            _renderRect.size.y * 0.5f + _renderRectPaddingProportion.y * _renderRect.size.y,
            (_renderRect.size.x * 0.5f + _renderRectPaddingProportion.x * _renderRect.size.x) * ((float)_mainCamera.pixelHeight / _mainCamera.pixelWidth)
        );
    }

    private void RenderSingleCube()
    {
        _targetCube.Renderer.Render(SortingOrderMin, SortingOrderMax, RenderRect);
    }

    private void RenderMultiCubes()
    {
        int columnCount = Mathf.CeilToInt(Mathf.Sqrt(_cubesToRender.Count));
        int rowCount = Mathf.CeilToInt(_cubesToRender.Count / (float)columnCount);

        Vector2 subRectSize = new()
        {
            x = _renderRect.size.x / (float)(columnCount + _cubeRectSpacingProportion.x * (columnCount - 1)),
            y = _renderRect.size.y / (float)(rowCount + _cubeRectSpacingProportion.y * (rowCount - 1))
        };

        Vector2 subRectSpacing = new()
        {
            x = subRectSize.x * _cubeRectSpacingProportion.x,
            y = subRectSize.y * _cubeRectSpacingProportion.y
        };

        if (columnCount > rowCount)
        {
            subRectSize.y = subRectSize.x * (_renderRect.size.y / _renderRect.size.x);
        }

        Vector2 topLeftSubRectPosition = new()
        {
            x = _renderRect.position.x - (subRectSize.x * columnCount + subRectSpacing.x * (columnCount - 1)) * 0.5f + subRectSize.x * 0.5f,
            y = _renderRect.position.y + (subRectSize.y * rowCount + subRectSpacing.y * (rowCount - 1)) * 0.5f - subRectSize.y * 0.5f
        };

        for (int i = 0; i < _cubesToRender.Count; i++)
        {
            int row = Mathf.FloorToInt((float)i / columnCount);
            int column = i % columnCount;
            Rect subRect = new()
            {
                size = subRectSize,
                position = new()
                {
                    x = topLeftSubRectPosition.x + column * (subRectSpacing.x + subRectSize.x),
                    y = topLeftSubRectPosition.y - row * (subRectSpacing.y + subRectSize.y)
                }
            };

            _cubesToRender[i].Renderer.Render(SortingOrderMin, SortingOrderMax, subRect);
        }
    }

    public void AddRenderCall(RenderCall renderCall)
    {
        _renderCalls.Add(renderCall);
    }

    private void HandleRender()
    {
        _renderCalls.Sort((renderCall1, renderCall2) =>
        {
            return renderCall1.SortingOrder.CompareTo(renderCall2.SortingOrder);
        });

        foreach (var renderCall in _renderCalls)
        {
            renderCall.Callback?.Invoke();
        }
    }

    public struct RenderCall
    {
        public float SortingOrder;
        public Action Callback;
    }
}
