using System;
using UnityEngine;

[Serializable]
public class CustomGrid <T> where T: new()
{
    [Min(1)]
    [SerializeField] Vector2Int tiling = new(9, 9);

    public Vector2Int Tiling { get => tiling; set => tiling = value; }

    [Min(1)]
    private T[,] children;

    public T[,] Children { get => children; }

    public void Init()
    {
        children = new T[tiling.x, tiling.y];
        for(int row = 0; row < tiling.x; row++)
        {
            for (int column = 0; column < tiling.y; column++)
            {
                children[row, column] = new();
            }
        }
    }
    public Vector2Int FindChild(Func<T, bool> condition)
    {
        for (int row = 0; row < children.GetLength(0); row++)
        {
            for (int column = 0; column < children.GetLength(1); column++)
            {
                if (condition(children[row, column])) return new(row, column);
            }
        }
        return new(-1, -1);
    }
    public delegate void RemovalAction(ref T child);
    public void RemoveChild(Func<T, bool> condition, RemovalAction removalAction)
    {
        for (int row = 0; row < children.GetLength(0); row++)
        {
            for (int column = 0; column < children.GetLength(1); column++)
            {
                if (condition(children[row, column]))
                {
                    removalAction(ref children[row, column]);
                }
            }
        }
    }
    public void RemoveChild(int row, int column, RemovalAction removalAction)
    {
        if (!CheckValidGridPosition(row, column)) return;
        removalAction(ref children[row, column]);
    }

    // This function is used when a child outside of the grid wants to enter from a specific direction
    public Vector2Int GetEnterPosition(CubeMovement.MovementDirections direction, Vector2 rPos)
    {
        int column = 0, row = 0;
        switch (direction)
        {
            case CubeMovement.MovementDirections.Up:
                column = Mathf.RoundToInt((tiling.y - 1) * (rPos.x + 1) * 0.5f);
                row = tiling.x - 1;
                break;
            case CubeMovement.MovementDirections.Down:
                column = Mathf.RoundToInt((tiling.y - 1) * (rPos.x + 1) * 0.5f);
                row = 0;
                break;
            case CubeMovement.MovementDirections.Right:
                row = Mathf.RoundToInt((tiling.x - 1) * (1 - rPos.y) * 0.5f);
                column = 0;
                break;
            case CubeMovement.MovementDirections.Left:
                row = Mathf.RoundToInt((tiling.x - 1) * (1 - rPos.y) * 0.5f);
                column = tiling.y - 1;
                break;
        }

        if (column < 0 || column > tiling.y - 1) column = Mathf.RoundToInt((tiling.y - 1) * 0.5f);
        if (row < 0 || row > tiling.x - 1) row = Mathf.RoundToInt((tiling.x - 1) * 0.5f);
        return new Vector2Int(row, column);
    }

    public bool CheckValidGridPosition(int row, int column)
    {
        if (row < 0 || column < 0 || row >= tiling.x || column >= tiling.y) return false;
        return true;
    }
}
