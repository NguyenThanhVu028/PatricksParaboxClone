using System;
using System.Collections.Generic;
using UnityEngine;

public class HistoryManager : MonoBehaviour
{
    private static HistoryManager instance;

    public static HistoryManager Instance { get => instance; }

    [Min(0)]
    [SerializeField] int maxHistoryRecordCount = 50;

    [SerializeField] List<HistoryRecord> historyRecords = new();
    private HistoryRecord newHistoryRecord = new();

    [SerializeField] int currentIndex = -1;

    public List<HistoryRecord> HistoryRecords { get => historyRecords; }
    public HistoryRecord NewHistoryRecord { set => newHistoryRecord = value; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;

        currentIndex = -1;
    }

    // Called by every cube that moves
    public void RecordNewEvent(Cube targetCube, ContainerCube previousParent, Vector2 previousRPos, Vector2 previousRScl, bool isHorizFlipped)
    {
        newHistoryRecord.AddHistoryEvent(new HistoryEvent(targetCube, previousParent, previousRPos, previousRScl, isHorizFlipped));
    }
    public void RecordCameraInfo(bool isHorizFlipped)
    {
        newHistoryRecord.AddCameraEvent(new(isHorizFlipped));
    }

    // Player cube will archive the record after other cubes have finished recording its event
    public void ForceArchiveRecord()
    {
        ArchiveHistoryRecord();
    }
    public void NormalArchiveHistoryRecord()
    {
        if (newHistoryRecord.Events.Count == 0) return;
        ArchiveHistoryRecord();
    }

    private void ArchiveHistoryRecord()
    {
        if (maxHistoryRecordCount == 0)
        {
            if (historyRecords.Count != 0) historyRecords.Clear();
            currentIndex = -1;
            newHistoryRecord = new();
            return;
        }

        if (MainCamera.Instance != null)
        {
            if (newHistoryRecord.CameraEvent == null) newHistoryRecord.CameraEvent = new(MainCamera.Instance.IsHorizFlipped);
        }

        if (currentIndex < 0) currentIndex = 0;
        else currentIndex++;

        if (currentIndex >= historyRecords.Count)
        {
            if (historyRecords.Count >= maxHistoryRecordCount && historyRecords.Count != 0)
            {
                historyRecords.RemoveAt(0);
            }
            historyRecords.Add(newHistoryRecord);
            currentIndex = historyRecords.Count - 1;
        }
        else
        {
            historyRecords[currentIndex] = newHistoryRecord;
            // Remove records from previous save
            if (currentIndex < historyRecords.Count - 1)
            {
                historyRecords.RemoveRange(currentIndex + 1, historyRecords.Count - currentIndex - 1);
            }
        }

        newHistoryRecord = new();
    }

    public HistoryRecord GetRecordAt(int index)
    {
        if (index >= historyRecords.Count || index < 0) return null;
        return historyRecords[index];
    }

    public HistoryRecord ReturnToPreviousRecord()
    {
        currentIndex--;

        if (currentIndex < 0)
        {
            currentIndex = 0;
            return null;
        }

        if (currentIndex >= historyRecords.Count)
        {
            currentIndex = historyRecords.Count - 1;
            return null;
        }
        return historyRecords[currentIndex];
    }

    public HistoryRecord GetCurrentRecord()
    {
        if (currentIndex < 0) return null;
        if (currentIndex >= historyRecords.Count) return null;
        return historyRecords[currentIndex];
    }

    public HistoryRecord SkipToNextRecord()
    {
        currentIndex++;

        if (currentIndex >= historyRecords.Count)
        {
            currentIndex = historyRecords.Count - 1;
            return null;
        }

        if (currentIndex < 0)
        {
            currentIndex = 0;
            return null;
        }

        return historyRecords[currentIndex];
    }
}
[Serializable]
public class HistoryRecord
{
    // Cubes
    [SerializeField] List<HistoryEvent> events = new();
    // Camera
    [SerializeField] CameraEvent cameraEvent = null;

    public List<HistoryEvent> Events { get => events; }
    public CameraEvent CameraEvent { get => cameraEvent; set => cameraEvent = value; }

    public void AddHistoryEvent(HistoryEvent newDetails)
    {
        events.Add(newDetails);
    }
    public void AddCameraEvent(CameraEvent cameraEvent)
    {
        this.cameraEvent = cameraEvent;
    }
}

[Serializable]
public class HistoryEvent
{
    [SerializeField] Cube targetCube;
    [SerializeField] ContainerCube previousParent;
    [SerializeField] Vector2 previousRPos = new();
    [SerializeField] Vector2 previousRScl = new();
    [SerializeField] bool isHorizFlipped = false;

    public Cube TargetCube { get => targetCube; }
    public ContainerCube PreviousParent { get => previousParent; set => previousParent = value; }
    public Vector2 PreviousRPos { get => previousRPos; set => previousRPos = value; }
    public Vector2 PreviousRScl { get => previousRScl; set => previousRScl = value; }
    public bool IsHorizFlipped { get => isHorizFlipped; set => isHorizFlipped = value; }

    public HistoryEvent(Cube targetCube, ContainerCube previousParent, Vector2 previousRPos, Vector2 previousRScl, bool isHorizFlipped)
    {
        this.targetCube = targetCube;
        this.previousParent = previousParent;
        this.previousRPos = previousRPos;
        this.previousRScl = previousRScl;
        this.isHorizFlipped = isHorizFlipped;
    }
}
[Serializable]
public class CameraEvent
{
    [SerializeField] bool isCamHorizFlipped = false;
    public bool IsCamHorizFlipped { get => isCamHorizFlipped; set => isCamHorizFlipped = value; }
    public CameraEvent(bool isCamHorizFlipped)
    {
        this.isCamHorizFlipped = isCamHorizFlipped;
    }
}
