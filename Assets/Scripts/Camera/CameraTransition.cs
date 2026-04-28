using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraTransition
{
    protected int priority; // The higher the priority, the earlier the transition
    protected float targetTime;
    protected List<Cube.PreviousParentDetails> cubesToTraverse = null;
    public abstract IEnumerator ExecuteCoroutine(MainCamera mainCamera);
    public bool IsHigherPriorityThan(CameraTransition other)
    {
        if (other == null) return true;
        return priority > other.priority;
    }
    public void SetTargetTime(float time)
    {
        targetTime = time;
    }
    public void SetCubesToTraverse(List<Cube.PreviousParentDetails> cubes)
    {
        cubesToTraverse = cubes;
    }
}

public class ZoomingTransition : CameraTransition
{
    private Rect targetCubeRect;

    public ZoomingTransition()
    {
        priority = 0;
    }

    public override IEnumerator ExecuteCoroutine(MainCamera mainCamera)
    {
        if (mainCamera == null || mainCamera.RenderMode != MainCamera.MainCameraRenderMode.SingleCube) return null;

        if (cubesToTraverse == null || cubesToTraverse.Count <= 1 || cubesToTraverse[cubesToTraverse.Count - 1].Cube == null || cubesToTraverse[cubesToTraverse.Count - 1].Cube == cubesToTraverse[0].Cube) return null;

        // Traverse the previous parents list to calculate new target cube rect
        targetCubeRect = mainCamera.TargetCubeRect;
        Cube.PreviousParentDetails currentCube = cubesToTraverse[0];
        for(int i =1; i < cubesToTraverse.Count; i++)
        {
            if (cubesToTraverse[i].Cube == null) continue;
            if (cubesToTraverse[i].Direction == Cube.PreviousParentDetails.Directions.Out)
            {
                Vector2 oldTargetRectPos = targetCubeRect.position;
                targetCubeRect = Relativity.PRectFromCRect(targetCubeRect, currentCube.RelativeScale, currentCube.RelativePosition);
                if (currentCube.IsHorizFlipped)
                {
                    targetCubeRect.width = -targetCubeRect.width;
                    targetCubeRect.x = oldTargetRectPos.x - (targetCubeRect.position.x - oldTargetRectPos.x);
                }
            }
            else
            {
                targetCubeRect = Relativity.CRectFromPRect(targetCubeRect, cubesToTraverse[i].RelativeScale, cubesToTraverse[i].RelativePosition);
                if (cubesToTraverse[i].IsHorizFlipped)
                {
                    targetCubeRect.width = -targetCubeRect.width;
                }
            }
            currentCube = cubesToTraverse[i];
        }

        // Save camera's info
        HistoryManager historyManager = HistoryManager.Instance;
        if (historyManager != null)
        {
            if (targetCubeRect.size.x > 0 == cubesToTraverse[cubesToTraverse.Count - 1].IsHorizFlipped) historyManager.RecordCameraInfo(true);
            else historyManager.RecordCameraInfo(false);
        }

        if (mainCamera.TargetCube == null || targetTime <= 0)
        {
            if (targetCubeRect.size.x > 0 == cubesToTraverse[cubesToTraverse.Count - 1].IsHorizFlipped) mainCamera.IsHorizFlipped = true;
            else mainCamera.IsHorizFlipped = false;
            mainCamera.SetNewTargetCube(cubesToTraverse[cubesToTraverse.Count - 1].Cube);
            mainCamera.FocusOnTargetCube();
            return null;
        }

        // Zooming out
        if (Mathf.Abs(targetCubeRect.size.x) <= Mathf.Abs(mainCamera.TargetCubeRect.size.x) || Mathf.Abs(targetCubeRect.size.y) <= Mathf.Abs(mainCamera.TargetCubeRect.size.y)) return ZoomInCoroutine(mainCamera, cubesToTraverse[cubesToTraverse.Count - 1].Cube, targetTime);
        // Zooming in
        else return ZoomOutCoroutine(mainCamera, cubesToTraverse[cubesToTraverse.Count - 1].Cube, targetTime);
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
        if ((targetCubeRect.size.x > 0) == newTarget.IsHorizFlipped)
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
        if ((targetCubeRect.size.x > 0) == newTarget.IsHorizFlipped)
        {
            mainCamera.IsHorizFlipped = true;
        }
        else mainCamera.IsHorizFlipped = false;

        // [old] --> new
        // old --> [new]
        Rect oldTargetNewRect = new();
        Vector2 oldDistanceFromNewTarget = oldTargetRect.position - targetCubeRect.position;
        oldTargetNewRect.width = (oldTargetRect.size.x / targetCubeRect.size.x) * mainCamera.TargetCubeRect.size.x;
        oldTargetNewRect.height = (oldTargetRect.size.y / targetCubeRect.size.y) * mainCamera.TargetCubeRect.size.y;
        oldTargetNewRect.x = mainCamera.TargetCubeRect.position.x + (oldDistanceFromNewTarget.x / targetCubeRect.size.x) * mainCamera.TargetCubeRect.size.x;
        oldTargetNewRect.y = mainCamera.TargetCubeRect.position.y + (oldDistanceFromNewTarget.y / targetCubeRect.size.y) * mainCamera.TargetCubeRect.size.y;

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

public class FadeTransition : CameraTransition
{
    public FadeTransition()
    {
        priority = 1;
    }

    public override IEnumerator ExecuteCoroutine(MainCamera mainCamera)
    {
        if (targetTime <= 0) yield break;

        if (mainCamera == null || mainCamera.RenderMode != MainCamera.MainCameraRenderMode.SingleCube) yield break;
        if (cubesToTraverse == null || cubesToTraverse.Count <= 1 || cubesToTraverse[cubesToTraverse.Count - 1].Cube == null || cubesToTraverse[cubesToTraverse.Count - 1].Cube == cubesToTraverse[0].Cube) yield break;

        // Traverse the previous parents list to check for isHorizFlipped status
        bool isNegative = mainCamera.TargetCubeRect.size.x < 0;
        Cube.PreviousParentDetails currentCube = cubesToTraverse[0];
        for (int i = 1; i < cubesToTraverse.Count; i++)
        {
            if (cubesToTraverse[i].Cube == null) continue;
            if (cubesToTraverse[i].Direction == Cube.PreviousParentDetails.Directions.Out)
            {
                if (currentCube.IsHorizFlipped)
                {
                    isNegative = !isNegative;
                }
            }
            else
            {
                if (cubesToTraverse[i].IsHorizFlipped)
                {
                    isNegative = !isNegative;
                }
            }
            currentCube = cubesToTraverse[i];
        }

        // Save camera's info
        HistoryManager historyManager = HistoryManager.Instance;
        if (historyManager != null)
        {
            if (!isNegative == cubesToTraverse[cubesToTraverse.Count - 1].IsHorizFlipped) historyManager.RecordCameraInfo(true);
            else historyManager.RecordCameraInfo(false);
        }

        float elapsedTime = -1;
        float defaultExposure = mainCamera.Exposure;
        bool updatedCamera = false;
        ContainerCube targetCube = cubesToTraverse[cubesToTraverse.Count - 1].Cube;
        while (elapsedTime < targetTime)
        {
            if (elapsedTime < 0) elapsedTime = 0;
            else elapsedTime += Time.deltaTime;

            if (elapsedTime > targetTime)
            {
                mainCamera.Exposure = defaultExposure;
                yield break;
            }

            if (elapsedTime <= targetTime * 0.5f)
            {
                // Fade out
                mainCamera.Exposure = Mathf.Lerp(defaultExposure, -1f, elapsedTime / (targetTime * 0.5f));
            }
            else
            {
                if (!updatedCamera)
                {
                    updatedCamera = true;
                    if (!isNegative == targetCube.IsHorizFlipped) mainCamera.IsHorizFlipped = true;
                    else mainCamera.IsHorizFlipped = false;
                    mainCamera.SetNewTargetCube(targetCube);
                    mainCamera.FocusOnTargetCube();
                }
                // Fade in
                mainCamera.Exposure = Mathf.Lerp(-1.0f, defaultExposure, (elapsedTime - (targetTime * 0.5f))/ (targetTime * 0.5f));
            }
            yield return null;
        }
    }
}