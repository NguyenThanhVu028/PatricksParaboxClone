using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Cube;

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
        PlayerInputsManager playerInputsManager = PlayerInputsManager.Instance;
        if (playerInputsManager != null)
        {
            playerInputsManager.GameplayInputs.OnResetEvent.AddListener(OnReset);
            playerInputsManager.GameplayInputs.OnUndoEvent.AddListener(OnUndo);
            playerInputsManager.GameplayInputs.OnRedoEvent.AddListener(OnRedo);
        }

        // Save the initial empty history record
        HistoryManager historyManager = HistoryManager.Instance;
        if (historyManager != null)
            historyManager.ForceArchiveRecord();
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

    public void OnReset()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        //HistoryManager historyManager = HistoryManager.Instance;
        //if (historyManager == null) return;
        //var previousRecords = historyManager.HistoryRecords;

        //for(int i = previousRecords.Count - 1; i >= 0; i--)
        //{
        //    TranslateHistoryRecord(previousRecords[i]);
        //}

        //HistoryRecord historyRecord = historyManager.GetRecordAt(0);
        //if (historyRecord != null)
        //{
        //    historyManager.CurrentHistoryRecord = historyRecord;
        //    historyManager.ArchiveHistoryRecord();
        //}

        //TranslateHistoryRecord(historyRecord);
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
            Debug.Log($"Reset {historyEvent.TargetCube} parent to {historyEvent.PreviousParent}");
            historyEvent.PreviousParent.ChildCubes[targetCubePrevPosInParent.x, targetCubePrevPosInParent.y] = historyEvent.TargetCube;
            historyEvent.TargetCube.Parent = historyEvent.PreviousParent;
            historyEvent.TargetCube.PreviousParents.Clear();
            historyEvent.TargetCube.PreviousParents.Add(new(PreviousParentDetails.Directions.In, historyEvent.PreviousParent));
            historyEvent.TargetCube.RelativePosition = historyEvent.PreviousRPos;
            historyEvent.TargetCube.RelativeScale = historyEvent.PreviousRScl;
            ResetCamera(historyEvent.TargetCube);
        }
    }

    private void ResetCamera(Cube targetCube)
    {
        if (!targetCube.IsPlayer || MainCamera.Instance == null) return;
        MainCamera.Instance.StopTransition();
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
