using UnityEngine;

public class MainCube : MonoBehaviour
{
    //[SerializeField] float size = 9;
    //[Header("Cameras")]
    //[SerializeField] Camera miniCubeCam;
    //[SerializeField] MainCamera mainCamera;
    //[Header("Surrounding Renderers")]
    //[SerializeField] Renderer frontSurroundings;
    //[SerializeField] Renderer backSurroundings;
    //[SerializeField] float surroundingSizeMultiplier = 5f;
    //[Header("Renderings")]
    //[SerializeField] int miniCubeCamRTexRes = 1080;
    //[SerializeField] int miniCubeCamRTexDepth = 32;
    //[SerializeField] int frontSurroundingsRenderQueue = 3001;
    //[SerializeField] int backSurroundingsRenderQueue = 3000;

    //private EnterableCube miniCube;

    //public float Size { get => size; }
    //public float SurroundingSizeMultiplier { get => surroundingSizeMultiplier; }
    //public Renderer FrontSurroundings { get => frontSurroundings; }
    //public Renderer BackSurroundings { get => backSurroundings; }
    //public EnterableCube MiniCube { get => miniCube; }
    //public RenderTexture MiniCubeCamRTex
    //{
    //    get
    //    {
    //        if (miniCubeCam == null) return null;
    //        if (miniCubeCam.targetTexture == null)
    //        {
    //            RenderTexture renderTexture = new RenderTexture(miniCubeCamRTexRes, miniCubeCamRTexRes, miniCubeCamRTexDepth);
    //            renderTexture.filterMode = FilterMode.Point;

    //            miniCubeCam.targetTexture = renderTexture;
    //        }
    //        return miniCubeCam.targetTexture;
    //    }
    //}

    //public void Init()
    //{
    //    SetUpMaterials();
    //    SetUpCameras();
    //    //SetUpSurroundings();
    //    SetUpColliders();
    //}
    //private void OnValidate()
    //{
    //    if (miniCubeCam == null) Debug.LogWarning("Main cube: " + name + " is missing a mini cube camera reference");
    //    if (size % 2 == 0) Debug.LogWarning("Main cube: " + name + " size should be an odd number to avoid glitches");
    //}

    //private void Update()
    //{
    //    UpdateSurroundings();
    //}

    //public void AssignMiniCube(EnterableCube miniCube)
    //{
    //    this.miniCube = miniCube;
    //}
    //public void SetFrontSurroundingsVisibility(bool isVisible)
    //{
    //    if (frontSurroundings != null) frontSurroundings.enabled = isVisible;
    //}
    //public void SetBackSurroundingsVisibility(bool isVisible)
    //{
    //    if (backSurroundings != null) backSurroundings.enabled = isVisible;
    //}

    //private void SetUpMaterials()
    //{
    //    if (frontSurroundings != null)
    //    {
    //        frontSurroundings.material.SetTexture("_MainTex", miniCube.FrontSurroundingCamRTex);
    //        frontSurroundings.material.renderQueue = frontSurroundingsRenderQueue;
    //    }
    //    if (backSurroundings != null)
    //    {
    //        backSurroundings.material.SetTexture("_MainTex", miniCube.BackSurroundingCamRTex);
    //        backSurroundings.material.renderQueue = backSurroundingsRenderQueue;
    //    }
    //}
    //private void SetUpCameras()
    //{
    //    if (miniCubeCam != null)
    //    {
    //        miniCubeCam.enabled = true;
    //        miniCubeCam.orthographic = true;
    //        miniCubeCam.orthographicSize = size * 0.5f;
    //    }
    //}
    //private void SetUpSurroundings()
    //{
    //    if (frontSurroundings != null) frontSurroundings.transform.localScale = Vector3.one * size * surroundingSizeMultiplier;
    //    if (backSurroundings != null) backSurroundings.transform.localScale = Vector3.one * size * surroundingSizeMultiplier;
    //}
    //private void SetUpColliders()
    //{
    //    BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
    //    if (boxCollider2D != null)
    //    {
    //        boxCollider2D.offset = Vector2.zero;
    //        boxCollider2D.size = Vector2.one * size;
    //    }
    //}
    //private void UpdateSurroundings()
    //{
    //    if (mainCamera == null) return;
    //    if (mainCamera.TargetMainCube == this) return; // Main camera will update the surroundings of its target main cube
    //    if (frontSurroundings != null)
    //    {

    //    }
    //    if (backSurroundings != null)
    //    {

    //    }
    //}
}
