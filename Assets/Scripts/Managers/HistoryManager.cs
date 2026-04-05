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
    [SerializeField] HistoryRecord currentHistoryRecord = new();

    [SerializeField] int currentIndex = -1;

    public List<HistoryRecord> HistoryRecords { get => historyRecords; }
    public HistoryRecord CurrentHistoryRecord { set => currentHistoryRecord = value; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;

        currentIndex = -1;
    }

    // Called by every cube that moves
    public void RecordNewEvent(Cube targetCube, ContainerCube previousParent, Vector2 previousRPos, Vector2 previousRScl)
    {
        currentHistoryRecord.AddHistoryEvent(new HistoryEvent(targetCube, previousParent, previousRPos, previousRScl));
    }

    // Player cube will archive the record after other cubes have finished recording its event
    public void ArchiveHistoryRecord()
    {
        if (maxHistoryRecordCount == 0)
        {
            if (historyRecords.Count != 0) historyRecords.Clear();
            currentIndex = -1;
            currentHistoryRecord = new();
            return;
        }

        if (currentIndex < 0) currentIndex = 0;
        else currentIndex++;

        if (currentIndex >= historyRecords.Count)
        {
            if (historyRecords.Count >= maxHistoryRecordCount && historyRecords.Count != 0)
            {
                historyRecords.RemoveAt(0);
            }
            historyRecords.Add(currentHistoryRecord);
            currentIndex = historyRecords.Count - 1;
        }
        else
        {
            historyRecords[currentIndex] = currentHistoryRecord;
            // Remove records from previous save
            if (currentIndex < historyRecords.Count - 1)
            {
                historyRecords.RemoveRange(currentIndex + 1, historyRecords.Count - currentIndex - 1);
            }
        }

        currentHistoryRecord = new();
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
    [SerializeField] List<HistoryEvent> events = new();

    public List<HistoryEvent> Events { get => events; }

    public void AddHistoryEvent(HistoryEvent newDetails)
    {
        events.Add(newDetails);
    }
}

[Serializable]
public class HistoryEvent
{
    [SerializeField] Cube targetCube;
    [SerializeField] ContainerCube previousParent;
    [SerializeField] Vector2 previousRPos = new();
    [SerializeField] Vector2 previousRScl = new();

    public Cube TargetCube { get => targetCube; }
    public ContainerCube PreviousParent { get => previousParent; set => previousParent = value; }
    public Vector2 PreviousRPos { get => previousRPos; set => previousRPos = value; }
    public Vector2 PreviousRScl { get => previousRScl; set => previousRScl = value; }

    public HistoryEvent(Cube targetCube, ContainerCube previousParent, Vector2 previousRPos, Vector2 previousRScl)
    {
        this.targetCube = targetCube;
        this.previousParent = previousParent;
        this.previousRPos = previousRPos;
        this.previousRScl = previousRScl;
    }
}
