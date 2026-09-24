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

            // Stop all coroutines
            if (historyEvent.TargetCube.GetComponent<CubeMovement>() != null)
                historyEvent.TargetCube.GetComponent<CubeMovement>().StopMoving();
            historyEvent.TargetCube.StopPossessing();

            // Let target cube leave its current parent
            if (historyEvent.TargetCube.Parent != null)
            {
                Debug.Log($"{historyEvent.TargetCube.Parent} removes {historyEvent.TargetCube}");
                historyEvent.TargetCube.Parent.ChildGrid.RemoveChild(
                    (child) => child.Cube == historyEvent.TargetCube,
                    (ref ContainerCube.ChildCubeDetails child) => { child.Cube = null; child.MovingDirection = CubeMovement.GridDirections.None; }
                    );
            }

            // Let target cube return to its previous parent
            if (historyEvent.Properties.Parent == null)
            {
                historyEvent.TargetCube.Parent = null;
                continue;
            }

            Vector2Int targetCubePrevPosInParent = Relativity.GridPosFromRPos(historyEvent.Properties.Parent.Tiling.y, historyEvent.Properties.Parent.Tiling.x, historyEvent.Properties.RelativePosition);
            if (!historyEvent.Properties.Parent.ChildGrid.CheckValidGridPosition(targetCubePrevPosInParent.x, targetCubePrevPosInParent.y)) return;
            // Return target cube back to its previous parent cube
            historyEvent.Properties.Parent.ChildCubes[targetCubePrevPosInParent.x, targetCubePrevPosInParent.y].Cube = historyEvent.TargetCube;
            historyEvent.Properties.Parent.ChildCubes[targetCubePrevPosInParent.x, targetCubePrevPosInParent.y].MovingDirection = CubeMovement.GridDirections.None;
            historyEvent.TargetCube.ApplyNewProperties(historyEvent.Properties);
            //historyEvent.TargetCube.Parent = historyEvent.Properties.Parent;
            historyEvent.TargetCube.PreviousParents.Clear();
            historyEvent.TargetCube.PreviousParents.Add(new(CubeMovement.LayerDirections.In, historyEvent.Properties.Parent));
            // Return target cube's relative position, relative scale and horizontal flip status back to previous values
            //historyEvent.TargetCube.RelativePosition = historyEvent.Properties.RelativePosition;
            //historyEvent.TargetCube.RelativeScale = historyEvent.Properties.RelativeScale;
            //historyEvent.TargetCube.IsHorizFlipped = historyEvent.IsHorizFlipped;
            //historyEvent.TargetCube.IsPlayer = historyEvent.IsPlayer;
            //if (historyEvent.TargetCube is ContainerCube containerCube)
            //{
            //    containerCube.IsEnterable = historyEvent.IsEnterable;
            //    containerCube.IsLeavable = historyEvent.IsLeavable;
            //}
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
