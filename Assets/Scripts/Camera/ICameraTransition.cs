using System.Collections;
using UnityEngine;

public interface ICameraTransition
{
    public IEnumerator ExecuteCoroutine(MainCamera mainCamera);
}

public class ZoomingTransition : ICameraTransition
{
    private Vector2 prevRPosToNew;
    private Vector2 prevRSclToNew;
    private ContainerCube targetCube;
    private float targetTime;

    public ZoomingTransition(Vector2 prevRPosToNew, Vector2 prevRSclToNew, ContainerCube targetCube, float targetTime)
    {
        this.prevRPosToNew = prevRPosToNew;
        this.prevRSclToNew = prevRSclToNew;
        this.targetCube = targetCube;
        this.targetTime = targetTime;
    }

    public IEnumerator ExecuteCoroutine(MainCamera mainCamera)
    {
        if (targetTime <= 0) return null;

        if (mainCamera == null || mainCamera.RenderMode != MainCamera.MainCameraRenderMode.SingleCube) return null;

        if (targetCube == null || targetCube == mainCamera.TargetCube) return null;
        if (mainCamera.TargetCube == null)
        {
            mainCamera.SetNewTargetCube(targetCube);
            mainCamera.FocusOnTargetCube();
            return null;
        }

        // Zooming out
        if (prevRSclToNew.x <= 1 || prevRSclToNew.y <= 1) return ZoomOutCoroutine(mainCamera, prevRPosToNew, prevRSclToNew, targetCube, targetTime);
        // Zooming in
        else return ZoomInCoroutine(mainCamera, prevRPosToNew, prevRSclToNew, targetCube, targetTime);
    }

    private IEnumerator ZoomInCoroutine(MainCamera mainCamera, Vector2 prevRPosToNew, Vector2 prevRSclToNew, ContainerCube newTarget, float time)
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
        Rect newTargetRect = Relativity.CRectFromPRect(mainCamera.RenderPosition, newTargetRScl, newTargetRPos);

        float elapsedTime = -1;
        float oldOrthoSize = mainCamera.OrthographicSize;
        float targetOrthoSize = mainCamera.GetIdealOrthographicSize(newTargetRect, newTarget);
        Vector3 oldPos = mainCamera.transform.position;
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

            mainCamera.OrthographicSize = oldOrthoSize;
            mainCamera.transform.position = oldPos;

            if (elapsedTime < 0) elapsedTime = 0; // Make sure the zooming in process starts at elapsed time = 0, not deltaTime
            else elapsedTime += Time.deltaTime;

            mainCamera.OrthographicSize = Mathf.Lerp(oldOrthoSize, targetOrthoSize, elapsedTime / time);
            mainCamera.transform.position = Vector3.Lerp(oldPos, targetPos, elapsedTime / time);

            if (elapsedTime >= time)
            {
                // Last frame
                mainCamera.SetNewTargetCube(newTarget);
                mainCamera.FocusOnTargetCube();
                mainCamera.Render(-0.1f);
            }
            yield return null;
        }

        mainCamera.FocusOnTargetCube();
    }
    private IEnumerator ZoomOutCoroutine(MainCamera mainCamera, Vector2 prevRPosToNew, Vector2 prevRSclToNew, ContainerCube newTarget, float time)
    {
        /* Zoom out logic:
         * - Target cube is set to the new target
         * - Camera's ortho and position is set based on old target
         * - Calculate old ortho and old transform based on relative values and current Render Position
         * - Set ortho and transform the old value calculated above
         * - Lerp toward ideal ortho and Render Position's position
         * - Focus on the new target
         */

        mainCamera.SetNewTargetCube(newTarget);
        Rect oldTargetNewRect = Relativity.CRectFromPRect(mainCamera.RenderPosition, prevRSclToNew, prevRPosToNew);
        float cameraRelativeOrthoSize = mainCamera.OrthographicSize / mainCamera.RenderPosition.height;
        Vector2 cameraRPosToOldTarget = Relativity.CRPosFromCRealPos(mainCamera.RenderPosition, mainCamera.transform.position);

        float elapsedTime = -1;
        var oldOrthoSize = cameraRelativeOrthoSize * oldTargetNewRect.height;
        var targetOrthoSize = mainCamera.GetIdealOrthographicSize(mainCamera.RenderPosition, newTarget);
        Vector3 oldPos = Relativity.CRealPosFromCRPos(oldTargetNewRect, cameraRPosToOldTarget); ; oldPos.z = -10;
        Vector3 targetPos = mainCamera.RenderPosition.position; targetPos.z = -10;

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