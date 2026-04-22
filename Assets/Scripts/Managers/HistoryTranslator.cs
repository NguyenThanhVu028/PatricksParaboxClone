using UnityEngine;
using static Cube;

public class HistoryTranslator : MonoBehaviour
{
    void Start()
    {
        PlayerInputsManager playerInputsManager = PlayerInputsManager.Instance;
        if (playerInputsManager != null)
        {
            playerInputsManager.GameplayInputs.OnUndoEvent.AddListener(OnUndo);
            playerInputsManager.GameplayInputs.OnRedoEvent.AddListener(OnRedo);
        }

        // Save the initial empty history record
        HistoryManager historyManager = HistoryManager.Instance;
        if (historyManager != null)
            historyManager.ForceArchiveRecord();
    }

    public void OnUndo()
    {
        HistoryManager historyManager = HistoryManager.Instance;
        if (historyManager == null) return;
        HistoryRecord historyRecord = historyManager.ReturnToPreviousRecord();

        TranslateHistoryRecord(historyRecord);
    }

    public void OnRedo()
    {
        HistoryManager historyManager = HistoryManager.Instance;
        if (historyManager == null) return;
        HistoryRecord historyRecord = historyManager.SkipToNextRecord();

        TranslateHistoryRecord(historyRecord);
    }

    private void TranslateHistoryRecord(HistoryRecord historyRecord)
    {
        if (historyRecord == null) return;

        foreach (var historyEvent in historyRecord.Events)
        {
            if (historyEvent == null || historyEvent.TargetCube == null) continue;

            // Stop moving coroutine
            if (historyEvent.TargetCube.GetComponent<CubeMovement>() != null)
                historyEvent.TargetCube.GetComponent<CubeMovement>().StopMoving();

            // Let target cube leave its current parent
            if (historyEvent.TargetCube.Parent != null)
            {
                Debug.Log($"{historyEvent.TargetCube.Parent} removes {historyEvent.TargetCube}");
                historyEvent.TargetCube.Parent.ChildGrid.RemoveChild(historyEvent.TargetCube);
            }

            // Let target cube return to its previous parent
            if (historyEvent.PreviousParent == null)
            {
                historyEvent.TargetCube.Parent = null;
                continue;
            }

            Vector2Int targetCubePrevPosInParent = Relativity.GridPosFromRPos(historyEvent.PreviousParent.Tiling.y, historyEvent.PreviousParent.Tiling.x, historyEvent.PreviousRPos);
            if (!historyEvent.PreviousParent.ChildGrid.CheckValidGridPosition(targetCubePrevPosInParent.x, targetCubePrevPosInParent.y)) return;
            historyEvent.PreviousParent.ChildCubes[targetCubePrevPosInParent.x, targetCubePrevPosInParent.y] = historyEvent.TargetCube;
            historyEvent.TargetCube.Parent = historyEvent.PreviousParent;
            historyEvent.TargetCube.PreviousParents.Clear();
            historyEvent.TargetCube.PreviousParents.Add(new(PreviousParentDetails.Directions.In, historyEvent.PreviousParent));
            historyEvent.TargetCube.RelativePosition = historyEvent.PreviousRPos;
            historyEvent.TargetCube.RelativeScale = historyEvent.PreviousRScl;
            ResetCameraTarget(historyEvent);
        }
        ResetCameraStatus(historyRecord);
    }

    private void ResetCameraTarget(HistoryEvent historyEvent)
    {
        if (!historyEvent.TargetCube.IsPlayer || MainCamera.Instance == null) return;
        MainCamera.Instance.StopTransition();
        MainCamera.Instance.SetNewTargetCube(historyEvent.TargetCube.Parent);
        MainCamera.Instance.FocusOnTargetCube();
    }
    private void ResetCameraStatus(HistoryRecord historyRecord)
    {
        if (MainCamera.Instance == null || historyRecord.CameraEvent == null) return;
        MainCamera.Instance.IsHorizFlipped = historyRecord.CameraEvent.IsCamHorizFlipped;
    }
}
