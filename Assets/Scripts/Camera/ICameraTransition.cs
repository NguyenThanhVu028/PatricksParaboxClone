using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICameraTransition
{
    public IEnumerator ExecuteCoroutine(MainCamera mainCamera);
}

public class ZoomingTransition : ICameraTransition
{

    private List<Cube.PreviousParentDetails> cubesToTraverse; 
    private ContainerCube targetCube;
    private Rect targetCubeRect;
    private float targetTime;

    public ZoomingTransition(List<Cube.PreviousParentDetails> cubesToTraverse, ContainerCube targetCube, float targetTime)
    {
        this.cubesToTraverse = cubesToTraverse;
        this.targetCube = targetCube;
        this.targetTime = targetTime;
    }

    public IEnumerator ExecuteCoroutine(MainCamera mainCamera)
    {
        if (mainCamera == null || mainCamera.RenderMode != MainCamera.MainCameraRenderMode.SingleCube) return null;

        if (targetCube == null || targetCube == mainCamera.TargetCube) return null;
        if (mainCamera.TargetCube == null || targetTime <= 0)
        {
            mainCamera.SetNewTargetCube(targetCube);
            mainCamera.FocusOnTargetCube();
            return null;
        }

        // Traverse the previous parents list to calculate new target cube rect
        if (cubesToTraverse == null || cubesToTraverse.Count == 0) return null;
        targetCubeRect = mainCamera.TargetCubeRect;
        ContainerCube currentCube = cubesToTraverse[0].Cube;
        for(int i =1; i < cubesToTraverse.Count; i++)
        {
            Debug.Log(cubesToTraverse[i]);
            if (cubesToTraverse[i].Cube == null) continue;
            if (cubesToTraverse[i].direction == Cube.PreviousParentDetails.Directions.Out)
            {
                Vector2 oldTargetRectPos = targetCubeRect.position;
                targetCubeRect = Relativity.PRectFromCRect(targetCubeRect, currentCube.RelativeScale, currentCube.RelativePosition);
                if (currentCube.IsHorizFlipped)
                {
                    targetCubeRect.size = new(-targetCubeRect.size.x, targetCubeRect.size.y);
                    targetCubeRect.position = new(oldTargetRectPos.x - (targetCubeRect.position.x - oldTargetRectPos.x), targetCubeRect.position.y);
                }
            }
            else
            {
                Vector2 oldTargetRectPos = targetCubeRect.position;
                targetCubeRect = Relativity.CRectFromPRect(targetCubeRect, cubesToTraverse[i].Cube.RelativeScale, cubesToTraverse[i].Cube.RelativePosition);
                if (cubesToTraverse[i].Cube.IsHorizFlipped)
                {
                    targetCubeRect.size = new(-targetCubeRect.size.x, targetCubeRect.size.y);
                }
            }
            currentCube = cubesToTraverse[i].Cube;
        }

        // Record history
        //if (HistoryManager.Instance != null)
        //{
        //    if (HistoryManager.Instance.GetCurrentRecord().CameraEvent == null) HistoryManager.Instance.GetCurrentRecord().CameraEvent = new(mainCamera.IsHorizFlipped);
        //    else HistoryManager.Instance.GetCurrentRecord().CameraEvent.IsCamHorizFlipped = mainCamera.IsHorizFlipped;
        //    if (targetCubeRect.size.x > 0 == targetCube.IsHorizFlipped)
        //    {
        //        HistoryManager.Instance.RecordCameraInfo(true);
        //    }
        //    else HistoryManager.Instance.RecordCameraInfo(false);
        //}

        // Zooming out
        if (Mathf.Abs(targetCubeRect.size.x) <= Mathf.Abs(mainCamera.TargetCubeRect.size.x) || Mathf.Abs(targetCubeRect.size.y) <= Mathf.Abs(mainCamera.TargetCubeRect.size.y)) return ZoomInCoroutine(mainCamera, targetCube, targetTime);
        // Zooming in
        else return ZoomOutCoroutine(mainCamera, targetCube, targetTime);
    }

    private IEnumerator ZoomInCoroutine(MainCamera mainCamera, ContainerCube newTarget, float time)
    {
        /* Zoom in logic:
         * - Target cube is still the old target at first
         * - Keep old ortho and transform
         * - Calculate new target Rect based on old target Rect
         * - Calculate new ortho and new position based on new target Rect
         * - Start lerping toward new ortho and new position in "time"
         * - Set new target and focus on new target
         */

        float elapsedTime = -1;
        float oldOrthoSize = mainCamera.OrthographicSize;
        float targetOrthoSize = mainCamera.GetIdealOrthographicSize(targetCubeRect, newTarget);
        Vector3 oldPos = mainCamera.transform.position;
        Vector3 targetPos = new Vector3(targetCubeRect.x, targetCubeRect.y, -10);
        while (elapsedTime < time)
        {

            mainCamera.OrthographicSize = oldOrthoSize;
            mainCamera.transform.position = oldPos;

            if (elapsedTime < 0) elapsedTime = 0; // Make sure the zooming in process starts at elapsed time = 0, not deltaTime
            else elapsedTime += Time.deltaTime;

            mainCamera.OrthographicSize = Mathf.Lerp(oldOrthoSize, targetOrthoSize, elapsedTime / time);
            mainCamera.transform.position = Vector3.Lerp(oldPos, targetPos, elapsedTime / time);

            if (elapsedTime >= time)
            {
                // Last frame
                break;
            }
            yield return null;
        }

        // Check if the camera should be horizontally flipped to display cube direction currectly
        if ((targetCubeRect.size.x > 0) == targetCube.IsHorizFlipped)
        {
            mainCamera.IsHorizFlipped = true;
        }
        else mainCamera.IsHorizFlipped = false;
        mainCamera.SetNewTargetCube(newTarget);
        mainCamera.FocusOnTargetCube();
        mainCamera.Render(-0.1f);
    }
    private IEnumerator ZoomOutCoroutine(MainCamera mainCamera, ContainerCube newTarget, float time)
    {
        /* Zoom out logic:
         * - Target cube is set to the new target
         * - Camera's ortho and position is set based on old target
         * - Calculate old ortho and old transform based on relative values and current Render Position
         * - Set ortho and transform the old value calculated above
         * - Lerp toward ideal ortho and Render Position's position
         * - Focus on the new target
         */
        Rect oldTargetRect = mainCamera.TargetCubeRect;
        mainCamera.SetNewTargetCube(newTarget);
        // Check if the camera should be horizontally flipped to display cube direction currectly
        if ((targetCubeRect.size.x > 0) == targetCube.IsHorizFlipped)
        {
            mainCamera.IsHorizFlipped = true;
        }
        else mainCamera.IsHorizFlipped = false;

        // [old] --> new
        // old --> [new]
        Rect oldTargetNewRect = new();
        Vector2 oldDistanceFromNewTarget = oldTargetRect.position - targetCubeRect.position;
        oldTargetNewRect.size = new((oldTargetRect.size.x / targetCubeRect.size.x) * mainCamera.TargetCubeRect.size.x, (oldTargetRect.size.y / targetCubeRect.size.y) * mainCamera.TargetCubeRect.size.y);
        oldTargetNewRect.position = new(mainCamera.TargetCubeRect.position.x + (oldDistanceFromNewTarget.x / targetCubeRect.size.x) * mainCamera.TargetCubeRect.size.x, mainCamera.TargetCubeRect.position.y + (oldDistanceFromNewTarget.y / targetCubeRect.size.y) * mainCamera.TargetCubeRect.size.y);

        float cameraRelativeOrthoSize = mainCamera.OrthographicSize / mainCamera.TargetCubeRect.height;
        Vector2 cameraRPosToOldTarget = Relativity.CRPosFromCRealPos(mainCamera.TargetCubeRect, mainCamera.transform.position);

        float elapsedTime = -1;
        var oldOrthoSize = Mathf.Abs(cameraRelativeOrthoSize * oldTargetNewRect.height);
        var targetOrthoSize = mainCamera.GetIdealOrthographicSize(mainCamera.TargetCubeRect, newTarget);
        Vector3 oldPos = Relativity.CRealPosFromCRPos(oldTargetNewRect, cameraRPosToOldTarget); ; oldPos.z = -10;
        Vector3 targetPos = mainCamera.TargetCubeRect.position; targetPos.z = -10;

        mainCamera.OrthographicSize = oldOrthoSize;
        mainCamera.transform.position = oldPos;

        mainCamera.Render(-0.1f);

        while (elapsedTime < time)
        {
            if (elapsedTime < 0) elapsedTime = 0;
            else elapsedTime += Time.deltaTime;

            mainCamera.OrthographicSize = Mathf.Lerp(oldOrthoSize, targetOrthoSize, elapsedTime / time);
            mainCamera.transform.position = Vector3.Lerp(oldPos, targetPos, elapsedTime / time);
            yield return null;
        }
        mainCamera.FocusOnTargetCube();
    }
}

public class FadeTransition : ICameraTransition
{
    private ContainerCube targetCube;
    private float targetTime;

    public FadeTransition(ContainerCube targetCube, float targetTime)
    {
        this.targetCube = targetCube;
        this.targetTime = targetTime;
    }

    public IEnumerator ExecuteCoroutine(MainCamera mainCamera)
    {
        if (targetTime <= 0) yield break;

        if (mainCamera == null || mainCamera.RenderMode != MainCamera.MainCameraRenderMode.SingleCube) yield break;
        if (targetCube == null || targetCube == mainCamera.TargetCube) yield break;

        float elapsedTime = -1;
        float defaultExposure = mainCamera.Exposure;
        while (elapsedTime < targetTime)
        {
            if (elapsedTime < 0) elapsedTime = 0;
            else elapsedTime += Time.deltaTime;

            if (elapsedTime <= targetTime * 0.5f)
            {
                // Fade out
                mainCamera.Exposure = Mathf.Lerp(defaultExposure, -1f, elapsedTime / (targetTime * 0.5f));
            }
            else
            {
                if (mainCamera.TargetCube != targetCube)
                {
                    mainCamera.SetNewTargetCube(targetCube);
                    mainCamera.FocusOnTargetCube();
                }
                // Fade in
                mainCamera.Exposure = Mathf.Lerp(-1.0f, defaultExposure, (elapsedTime - (targetTime * 0.5f))/ (targetTime * 0.5f));
            }
            yield return null;
        }
        mainCamera.Exposure = defaultExposure;
    }
}