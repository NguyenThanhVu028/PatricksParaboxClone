using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour
{
    //[SerializeField] EnterableCube targetCube;
    //private Camera cam;
    //public Camera Camera
    //{
    //    get
    //    {
    //        if (cam == null) cam = GetComponent<Camera>();
    //        return cam;
    //    }
    //}

    //public EnterableCube TargetCube { get => targetCube; }
    //public MainCube TargetMainCube { get => targetCube.MainCube; }
    //public Vector2 WorldSpaceSize
    //{
    //    get
    //    {
    //        return new Vector2(Camera.orthographicSize * 2.0f * ((float)Screen.width / Screen.height), Camera.orthographicSize * 2.0f);
    //    }
    //}

    //private void Start()
    //{
    //    cam = GetComponent<Camera>();
    //    GetVirtualRenderingRect();
    //}

    //private void Update()
    //{
    //    if (targetCube == null || targetCube.MainCube == null) return;
    //    UpdateTargetCubeSurroundings();
    //}

    //public Rect GetVirtualRenderingRect()
    //{
    //    Rect rect = new Rect(0, 0, 0, 0);
    //    if (targetCube == null) return rect;
    //    if (targetCube.MainCube == null) return rect;

    //    float ratio = 1.0f / targetCube.MainCube.Size;
    //    //Debug.Log("Camera size: " + mainCamSize);
    //    Vector2 mainCamBottomLeftLeftPoint = (Vector2)transform.position + new Vector2(-WorldSpaceSize.x * 0.5f, -WorldSpaceSize.y * 0.5f);
    //    //Debug.Log("Main cam bottom left: " + mainCamBottomLeftLeftPoint);
    //    rect.position = (Vector2)targetCube.transform.position + (mainCamBottomLeftLeftPoint - (Vector2)targetCube.MainCube.transform.position) * ratio;
    //    rect.size = WorldSpaceSize * ratio;
    //    //Debug.Log("Virtual rendering rect: " + rect);

    //    return rect;
    //}

    //[ContextMenu("Focus on cube")]
    //public void FocusOnCube()
    //{
    //    if (targetCube == null) return;
    //    if (targetCube.MainCube == null) return;
    //    transform.position = new Vector3(targetCube.MainCube.transform.position.x, targetCube.MainCube.transform.position.y, transform.position.z);
    //    Camera.orthographicSize = targetCube.MainCube.Size * 0.5f + 1.0f;
    //}

    //private void UpdateTargetCubeSurroundings()
    //{
    //    if (targetCube.MainCube.FrontSurroundings != null)
    //    {
    //        targetCube.MainCube.FrontSurroundings.gameObject.transform.localScale = WorldSpaceSize;
    //        targetCube.MainCube.FrontSurroundings.gameObject.transform.position = new Vector3(
    //            transform.position.x,
    //            transform.position.y,
    //            targetCube.MainCube.FrontSurroundings.gameObject.transform.position.z
    //            );
    //    }
    //    if (targetCube.MainCube.BackSurroundings != null)
    //    {
    //        targetCube.MainCube.BackSurroundings.gameObject.transform.localScale = WorldSpaceSize;
    //        targetCube.MainCube.BackSurroundings.gameObject.transform.position = new Vector3(
    //            transform.position.x,
    //            transform.position.y,
    //            targetCube.MainCube.BackSurroundings.gameObject.transform.position.z
    //            );
    //    }
    //}
}
