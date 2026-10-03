using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LevelDetails
{
    private float _time;
    private List<CubeDetails> _cubeDetailsList = new();

    public float Time
    {
        get => _time;
        set => _time = value;
    }
    public List<CubeDetails> CubeDetailsList
    {
        get => _cubeDetailsList;
        set => _cubeDetailsList = value;
    }

    public void LogProperties()
    {
        Debug.Log($"[LevelDetails] Time: {Time}");

        if (CubeDetailsList == null)
        {
            Debug.Log("[LevelDetails] CubeDetailsList: null");
            return;
        }

        Debug.Log($"[LevelDetails] CubeDetailsList count: {CubeDetailsList.Count}");

        for (int i = 0; i < CubeDetailsList.Count; i++)
        {
            Debug.Log($"[LevelDetails] CubeDetailsList[{i}]:");

            if (CubeDetailsList[i] == null)
            {
                Debug.Log("null");
                continue;
            }

            CubeDetailsList[i].LogProperties();
        }
    }
}

public class CubeDetails
{
    private string _cubeID;
    private CubeTypes _cubeType;
    private string _colorID;
    private bool _isPlayer;
    private bool _isFlipped;
    private bool _isPossessable;
    private bool _isSecondaryPlayer;
    private string _mainCubeID;
    private List<List<string>> _cubeGrid;

    public string CubeID
    {
        get => _cubeID;
        set => _cubeID = value;
    }
    public CubeTypes CubeType
    {
        get => _cubeType;
        set => _cubeType = value;
    }
    public string ColorID
    {
        get => _colorID;
        set => _colorID = value;
    }
    public bool IsPlayer
    {
        get => _isPlayer;
        set => _isPlayer = value;
    }
    public bool IsFlipped
    {
        get => _isFlipped;
        set => _isFlipped = value;
    }
    public bool IsPossessable
    {
        get => _isPossessable;
        set => _isPossessable = value;
    }
    public bool IsSecondaryPlayer
    {
        get => _isSecondaryPlayer;
        set => _isSecondaryPlayer = value;
    }
    public string MainCubeID
    {
        get => _mainCubeID;
        set => _mainCubeID = value;
    }
    public List<List<string>> CubeGrid
    {
        get => _cubeGrid;
        set => _cubeGrid = value;
    }

    public void LogProperties()
    {
        StringBuilder log = new StringBuilder()
            .AppendLine("[CubeDetails]")
            .AppendLine($"CubeID: {CubeID}")
            .AppendLine($"CubeType: {CubeType}")
            .AppendLine($"ColorID: {ColorID}")
            .AppendLine($"IsPlayer: {IsPlayer}")
            .AppendLine($"IsFlipped: {IsFlipped}")
            .AppendLine($"IsPossessable: {IsPossessable}")
            .AppendLine($"IsSecondaryPlayer: {IsSecondaryPlayer}")
            .AppendLine($"MainCubeID: {MainCubeID}");

        if (CubeGrid == null)
        {
            log.Append("CubeGrid: null");
        }
        else if (CubeGrid.Count == 0)
        {
            log.Append("CubeGrid: []");
        }
        else
        {
            log.AppendLine("CubeGrid:");

            for (int rowIndex = 0; rowIndex < CubeGrid.Count; rowIndex++)
            {
                List<string> row = CubeGrid[rowIndex];
                log.Append($"  [{rowIndex}]: ");

                if (row == null)
                {
                    log.Append("null");
                }
                else
                {
                    log.Append('[').Append(string.Join(", ", row)).Append(']');
                }

                if (rowIndex < CubeGrid.Count - 1)
                {
                    log.AppendLine();
                }
            }
        }

        Debug.Log(log.ToString());
    }
}
