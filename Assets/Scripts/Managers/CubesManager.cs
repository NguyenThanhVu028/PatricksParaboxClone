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

            // Let target cube leave its current parent
            if (historyEvent.TargetCube.Parent != null)
            {
                Vector2Int targetCubeCurrentPosInParent = Relativity.GridPosFromRPos(historyEvent.TargetCube.Parent.Tiling, historyEvent.TargetCube.Parent.Tiling, historyEvent.TargetCube.RelativePosition);
                if (historyEvent.TargetCube.Parent.CheckValidGridPosition(targetCubeCurrentPosInParent.x, targetCubeCurrentPosInParent.y))
                {
                    if (historyEvent.TargetCube.Parent.CubesGrid[targetCubeCurrentPosInParent.x, targetCubeCurrentPosInParent.y] == historyEvent.TargetCube)
                    {
                        historyEvent.TargetCube.Parent.CubesGrid[targetCubeCurrentPosInParent.x, targetCubeCurrentPosInParent.y] = null;
                    }
                }
            }

            // Let target cube return to its previous parent
            if (historyEvent.PreviousParent == null)
            {
                historyEvent.TargetCube.Parent = null;
                continue;
            }
            Vector2Int targetCubePrevPosInParent = Relativity.GridPosFromRPos(historyEvent.PreviousParent.Tiling, historyEvent.PreviousParent.Tiling, historyEvent.PreviousRPos);
            if (!historyEvent.PreviousParent.CheckValidGridPosition(targetCubePrevPosInParent.x, targetCubePrevPosInParent.y)) return;
            historyEvent.PreviousParent.CubesGrid[targetCubePrevPosInParent.x, targetCubePrevPosInParent.y] = historyEvent.TargetCube;
            historyEvent.TargetCube.Parent = historyEvent.PreviousParent;
            historyEvent.TargetCube.RelativePosition = historyEvent.PreviousRPos;
            historyEvent.TargetCube.RelativeScale = historyEvent.PreviousRScl;

        }
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
