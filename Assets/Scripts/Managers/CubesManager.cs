using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CubesManager : MonoBehaviour
{
    private static CubesManager instance = null;

    [SerializeField] List<CubeDetails> allCubes = new();

    public static CubesManager Instance { get => instance; }

    public List<CubeDetails> AllCubes { get => allCubes; }

    private void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
        }
        instance = this;
    }

    private void Start()
    {
        if (PlayerInputsManager.Instance != null)
        {
            PlayerInputsManager.Instance.OnUndoEvent.AddListener(OnUndo);
        }
    }

    public Cube GetCube(int id)
    {
        foreach(var cube in allCubes)
        {
            if (cube == null) continue;
            if (cube.ID == id)
            {
                if (cube.Cube == null) return null;
                if (cube.Cube.NeedInstantiating)
                    return Instantiate(cube.Cube);
                else return cube.Cube;
            }
        }
        return null;
    }

    public void OnUndo()
    {
        if (HistoryManager.Instance == null) return;
        HistoryRecord lastestRecord = HistoryManager.Instance.GetLatestRecord();
        if (lastestRecord == null) return;
        Debug.Log("Begin undo");

        foreach(var historyEvent in lastestRecord.Events)
        {
            if (historyEvent == null || historyEvent.TargetCube == null) continue;

            // Stop moving coroutine
            if (historyEvent.TargetCube.GetComponent<CubeMovement>() != null)
                historyEvent.TargetCube.GetComponent<CubeMovement>().StopMoving();

            // Let target cube leave its current parent
            if (historyEvent.TargetCube.Parent != null)
            {
                Debug.Log($"{historyEvent.TargetCube.Parent} removes {historyEvent.TargetCube}");
                historyEvent.TargetCube.Parent.RemoveChild(historyEvent.TargetCube);
            }

            // Let target cube return to its previous parent
            if (historyEvent.PreviousParent == null)
            {
                historyEvent.TargetCube.Parent = null;
                continue;
            }

            Vector2Int targetCubePrevPosInParent = Relativity.GridPosFromRPos(historyEvent.PreviousParent.Tiling, historyEvent.PreviousParent.Tiling, historyEvent.PreviousRPos);
            if (!historyEvent.PreviousParent.CheckValidGridPosition(targetCubePrevPosInParent.x, targetCubePrevPosInParent.y)) return;
            Debug.Log($"Reset {historyEvent.TargetCube} parent to {historyEvent.PreviousParent}");
            historyEvent.PreviousParent.CubesGrid[targetCubePrevPosInParent.x, targetCubePrevPosInParent.y] = historyEvent.TargetCube;
            historyEvent.TargetCube.Parent = historyEvent.PreviousParent;
            historyEvent.TargetCube.PreviousParent = historyEvent.PreviousParent;
            historyEvent.TargetCube.RelativePosition = historyEvent.PreviousRPos;
            historyEvent.TargetCube.RelativeScale = historyEvent.PreviousRScl;
            ResetCamera(historyEvent.TargetCube);
        }
    }

    private void ResetCamera(Cube targetCube)
    {
        if (!targetCube.IsPlayer || MainCamera.Instance == null) return;
        MainCamera.Instance.StopZooming();
        MainCamera.Instance.SetNewTargetCube(targetCube.Parent);
        MainCamera.Instance.FocusOnTargetCube();
    }
}

[Serializable]
public class CubeDetails
{
    [Min(1)]
    [SerializeField] int id = 1;
    [SerializeField] Cube cube;

    public int ID { get => id; }
    public Cube Cube { get => cube; }
}
