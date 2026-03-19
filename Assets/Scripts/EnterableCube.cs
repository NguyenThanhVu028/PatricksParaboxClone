using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EnterableCube : Cube
{
    [Header("Cube properties")]
    [SerializeField] MainCube mainCube;
    [SerializeField] bool isReversed = false;

    [Header("Cameras")]
    [SerializeField] Camera frontSurroundingCam;
    [SerializeField] Camera backSurroundingCam;

    [Header("Renderings")]
    [SerializeField] int frontSurroundingCamRTexRes = 1080;
    [SerializeField] int frontSurroundingCamRTexDepth = 32;
    [SerializeField] int backSurroundingCamRTexRes = 1080;
    [SerializeField] int backSurroundingCamRTexDepth = 32;

    private Renderer cubeRenderer;

    public RenderTexture FrontSurroundingCamRTex
    {
        get
        {
            if (frontSurroundingCam == null) return null;
            if (frontSurroundingCam.targetTexture == null)
            {
                RenderTexture renderTexture = new RenderTexture(frontSurroundingCamRTexRes, frontSurroundingCamRTexRes, frontSurroundingCamRTexDepth);
                renderTexture.filterMode = FilterMode.Point;
                frontSurroundingCam.targetTexture = renderTexture;
            }
            return frontSurroundingCam.targetTexture;
        }
    }

    public RenderTexture BackSurroundingCamRTex
    {
        get
        {
            if (backSurroundingCam == null) return null;
            if (backSurroundingCam.targetTexture == null)
            {
                RenderTexture renderTexture = new RenderTexture(backSurroundingCamRTexRes, backSurroundingCamRTexRes, backSurroundingCamRTexDepth);
                renderTexture.filterMode = FilterMode.Point;
                backSurroundingCam.targetTexture = renderTexture;
            }
            return backSurroundingCam.targetTexture;
        }
    }

    protected override void AdditionalStart()
    {
        cubeRenderer = GetComponent<Renderer>();
        transform.localScale = new Vector3(1, 1, transform.localScale.z);

        SetUpMaterials();
        SetUpCameras();

        if (mainCube != null)
        {
            mainCube.SetMiniCube(this);
            mainCube.Init();
        }
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnPreRenderCallback;
        RenderPipelineManager.endCameraRendering -= OnPostRenderCallback;
    }

    private void OnValidate()
    {
        if (mainCube == null) Debug.LogWarning("Cube: " + name + " is missing a main map reference");
    }

    //public void SetFrontSurroundingsCamOutput(RenderTexture renderTexture)
    //{
    //    if (frontSurroundingCam != null) frontSurroundingCam.targetTexture = renderTexture;
    //}

    //public void SetBackSurroundingsCamOutput(RenderTexture renderTexture)
    //{
    //    if (backSurroundingCam != null) backSurroundingCam.targetTexture = renderTexture;
    //}

    public bool Enter(CubeMovement targetCube, Vector2 direction)
    {
        /*
         * Enter logic:
         * - Calculate the position the target cube should be placed in if it enters successfully.
         * - Check if there are any object in that position.
         *   + If there is an obstacle, the target cube cannot enter.
         *   + If there is another cube, try to push it. 
         *   + If failed, try to sub-enter it. If both failed, the target cube cannot enter.
         *   + If there is nothing or only ignorable objects are detected, the target cube can enter directly.
         * - If enter successfullt, the function will have a list of cubes that the target cube needs to enter.
         * - Handle the zoom-in effect:
         *   + Play zoom-in animation on the target cube.
         *   + Play zoom-in animation on the camera.
         *   + Let all the cubes in the accumulated cube list handle its own logic on the target cube (set isReverse, ...).
         *   + Teleport the target cube and the camera to the target position.
         *   
         * - Differences between Enter() and SubEnter():
         *   + SubEnter() function is used for recursion, it checks for a list of cubes that the target cube needs to enter
         *   + Enter() function calls SubEnter and also handle the zoom-in effect
         */

        Debug.Log("Cube: " + targetCube.name + " is trying to enter!");

        if (mainCube == null)
        {
            Debug.LogWarning("Cube: " + name + " has no main cube assigned to be enterable!");
            return false;
        }

        List<EnterableCube> cubesToEnter = new();

        if (SubEnter(targetCube, direction, cubesToEnter))
        {
            ProcessEntering(cubesToEnter, direction);
            return false; //
        }

        return false;
    }
    public bool SubEnter(CubeMovement targetCube, Vector2 direction, List<EnterableCube> cubesToEnter)
    {
        Debug.Log("Trying sub-enter on: " + gameObject.name);

        if (mainCube == null) return false;
        if (cubesToEnter == null) return false;
        if (cubesToEnter.Contains(this)) return true; // Avoid infinite recursion

        Vector2 targetPosition = (Vector2)mainCube.transform.position - direction.normalized * (mainCube.Size * 0.5f - 0.5f);
        var rayCastHits = Physics2D.RaycastAll(targetPosition - direction.normalized, direction, 1);

        foreach (var hit in rayCastHits)
        {
            if (hit.collider.gameObject == targetCube) continue; // Skip the cube itself

            // Check if the hit object is an obstacle that cannot be moved.
            if (((1 << hit.collider.gameObject.layer) & targetCube.ObstacleLayer) != 0)
            {
                return false;
            }

            if (hit.collider.GetComponent<Cube>() != null)
            {
                // Try to push the other cube.
                var cubeMovement = hit.collider.GetComponent<CubeMovement>();
                if (cubeMovement != null)
                {
                    if (cubeMovement.Move(direction, targetCube.EnterExitSpeed))
                    {
                        cubesToEnter.Add(this);
                        return true;
                    }
                }

                // Check if the hit object can be entered.
                var hitMiniCube = hit.collider.GetComponent<EnterableCube>();
                if (hitMiniCube != null)
                {
                    if (hitMiniCube.SubEnter(targetCube, direction, cubesToEnter))
                    {
                        cubesToEnter.Add(this);
                        return true;
                    }
                }

                return false;
            }
        }

        cubesToEnter.Add(this);
        return true;
    }

    public void ProcessEntering(List<EnterableCube> cubesToEnter, Vector2 direction)
    {
        if (cubesToEnter == null || cubesToEnter.Count == 0) return;

        Debug.Log("Final cube: " + cubesToEnter[0].name);

        // Distance between the center of the main cube to the center of the final cube
        float finalCubeCenterDistance = 0;
        // Distance between the center of the main cube to the target position
        float targetPositionDistance = 0;
        // Target cube size
        float targetCubeSize = 1;
        // Target camera orthographic size
        float targetCamOrthographicSize = 0;

        for (int i = 0; i < cubesToEnter.Count; i++)
        {
            targetCubeSize /= cubesToEnter[i].mainCube.Size;

            targetPositionDistance += (cubesToEnter[i].mainCube.Size - 1) * 0.5f;
            targetPositionDistance /= cubesToEnter[i].mainCube.Size;

            if (i == 0) continue;
            finalCubeCenterDistance += (cubesToEnter[i].mainCube.Size - 1) * 0.5f;
            finalCubeCenterDistance /= cubesToEnter[i].mainCube.Size;
        }

        Debug.Log($"Target position distance: {targetPositionDistance}, final cube center distance: {finalCubeCenterDistance}");
    }

    public void StartZoomIn(GameObject targetCube, float targetCubeSize, Vector2 targetCamPos, float targetCamSize, EnterableCube finalCube)
    {
        Debug.Log($"Target cube: {targetCube.name}, target cube size: {targetCubeSize}, target cam pos: {targetCamPos}, target cam size: {targetCamSize}, final cube: {finalCube.name}");
    }

    private void OnPreRenderCallback(ScriptableRenderContext context, Camera cam)
    {
        if (cam == backSurroundingCam || cam == frontSurroundingCam)
        {
            if (mainCube != null) mainCube.HideFrontSurroundings();
            if (mainCube != null) mainCube.HideBackSurroundings();

            if (cam == frontSurroundingCam && cubeRenderer != null) cubeRenderer.enabled = false;
        }
    }
    private void OnPostRenderCallback(ScriptableRenderContext context, Camera cam)
    {
        if (cam == backSurroundingCam || cam == frontSurroundingCam)
        {
            if (mainCube != null) mainCube.ShowFrontSurroundings();
            if (mainCube != null) mainCube.ShowBackSurroundings();

            if (cam == frontSurroundingCam && cubeRenderer != null) cubeRenderer.enabled = true;
        }
    }

    private void SetUpMaterials()
    {
        if (cubeRenderer != null && mainCube != null)
        {
            cubeRenderer.material.SetTexture("_MainTex", mainCube.MiniCubeCamRTex);
        }
    }
    private void SetUpCameras()
    {
        if (mainCube == null) return;
        if (frontSurroundingCam != null)
        {
            frontSurroundingCam.enabled = true;
            frontSurroundingCam.orthographic = true;
            frontSurroundingCam.orthographicSize = mainCube.SurroundingSizeMultiplier * 0.5f;
        }
        if (backSurroundingCam != null)
        {
            backSurroundingCam.enabled = true;
            backSurroundingCam.orthographic = true;
            backSurroundingCam.orthographicSize = mainCube.SurroundingSizeMultiplier * 0.5f;
        }

        RenderPipelineManager.beginCameraRendering += OnPreRenderCallback;
        RenderPipelineManager.endCameraRendering += OnPostRenderCallback;
    }
}
