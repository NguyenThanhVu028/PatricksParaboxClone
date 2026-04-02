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

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }

    // Called by every cube that moves
    public void RecordNewEvent(Cube targetCube, EnterableCube previousParent, Vector2 previousRPos, Vector2 previousRScl)
    {
        currentHistoryRecord.AddHistoryEvent(new HistoryEvent(targetCube, previousParent, previousRPos, previousRScl));
    }

    // Player cube will archive the record after other cubes have finished recording its event
    public void ArchiveHistoryRecord()
    {
        if (historyRecords.Count > maxHistoryRecordCount && historyRecords.Count != 0) historyRecords.RemoveAt(0);
        historyRecords.Add(currentHistoryRecord);
        currentHistoryRecord = new();
    }

    public HistoryRecord GetLatestRecord()
    {
        if (historyRecords.Count == 0) return null;
        HistoryRecord lastestRecord = historyRecords[historyRecords.Count - 1];
        historyRecords.RemoveAt(historyRecords.Count - 1);
        return lastestRecord;
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
    [SerializeField] EnterableCube previousParent;
    [SerializeField] Vector2 previousRPos = new();
    [SerializeField] Vector2 previousRScl = new();

    public Cube TargetCube { get => targetCube; }
    public EnterableCube PreviousParent { get => previousParent; }
    public Vector2 PreviousRPos { get => previousRPos; }
    public Vector2 PreviousRScl { get => previousRScl; }

    public HistoryEvent(Cube targetCube, EnterableCube previousParent, Vector2 previousRPos, Vector2 previousRScl)
    {
        this.targetCube = targetCube;
        this.previousParent = previousParent;
        this.previousRPos = previousRPos;
        this.previousRScl = previousRScl;
    }
}
