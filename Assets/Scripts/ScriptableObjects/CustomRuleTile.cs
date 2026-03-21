using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomRuleTile", menuName = "Scriptable Objects/CustomRuleTile")]
public class CustomRuleTile : ScriptableObject
{
    [SerializeField] Texture2D defaultTile;
    // Prototype codes
    [SerializeField] List<TilingRule> tilingRules = new()
    {
        new TilingRule(
            "Top Left",
            null,
            new int[,]
                {
                    {  0, -1, 0 },
                    { -1,  0, 1 },
                    {  0,  1, 0 }
                }
            ),
        new TilingRule(
            "Left",
            null,
            new int[,]
                {
                    {  0,  1, 0 },
                    { -1,  0, 0 },
                    {  0,  1, 0 }
                }
            ),
        new TilingRule(
            "Bottom Left",
            null,
            new int[,]
                {
                    {  0,  1, 0 },
                    { -1,  0, 1 },
                    {  0, -1, 0 }
                }
            ),
        new TilingRule(
            "Bottom",
            null,
            new int[,]
                {
                    {  0,  0, 0 },
                    {  1,  0, 1 },
                    {  0, -1, 0 }
                }
            ),
        new TilingRule(
            "Bottom Right",
            null,
            new int[,]
                {
                    {  0,  1,  0 },
                    {  1,  0, -1 },
                    {  0, -1,  0 }
                }
            ),
        new TilingRule(
            "Right",
            null,
            new int[,]
                {
                    { 0,  1,  0 },
                    { 0,  0, -1 },
                    { 0,  1,  0 }
                }
            ),
        new TilingRule(
            "Top Right",
            null,
            new int[,]
                {
                    { 0, -1,  0 },
                    { 1,  0, -1 },
                    { 0,  1,  0 }
                }
            ),
        new TilingRule(
            "Top",
            null,
            new int[,]
                {
                    { 0, -1, 0 },
                    { 1,  0, 1 },
                    { 0,  0, 0 }
                }
            ),
        new TilingRule(
            "Inner Top Left",
            null,
            new int[,]
                {
                    { -1, 1, 0 },
                    {  1, 0, 0 },
                    {  0, 0, 0 }
                }
            ),
        new TilingRule(
            "Inner Bottom Left",
            null,
            new int[,]
                {
                    {  0, 0, 0 },
                    {  1, 0, 0 },
                    { -1, 1, 0 }
                }
            ),
        new TilingRule(
            "Inner Bottom Right",
            null,
            new int[,]
                {
                    { 0, 0,  0 },
                    { 0, 0,  1 },
                    { 0, 1, -1 }
                }
            ),
        new TilingRule(
            "Inner Top Right",
            null,
            new int[,]
                {
                    { 0, 1, -1 },
                    { 0, 0,  1 },
                    { 0, 0,  0 }
                }
            )
    };
    
    public Texture2D GetTile(int[,] grid, int row, int column)
    {
        if (row < 0 || column < 0) return null;
        if (column >= grid.GetLength(1) || row >= grid.GetLength(0)) return null;

        List<TilingRule> suitableTilingRules = new(tilingRules);
        List<TilingRule> tilingRulesToScan;

        // Direction grid: a 3x3 array representing direction to check suitable
        for (int dirGridRow = 0; dirGridRow < 3; dirGridRow++)
        {
            for (int dirGridCol = 0; dirGridCol < 3; dirGridCol++)
            {
                if (dirGridRow == 1 && dirGridCol == 1) continue; //  Ignore the tile itself, only check its neighbors

                tilingRulesToScan = new(suitableTilingRules);
                suitableTilingRules.Clear();

                foreach (var tilingRule in tilingRulesToScan)
                {
                    if (tilingRule == null) continue;
                    if (tilingRule.CheckSuitable(grid, row, column, dirGridRow, dirGridCol))
                    {
                        suitableTilingRules.Add(tilingRule);
                    }
                }

                if (suitableTilingRules.Count == 0) return defaultTile;
            }
        }

        return suitableTilingRules[0].Texture;
    }

    [Serializable]
    public class TilingRule
    {
        [SerializeField] string ruleName;
        [SerializeField] Texture2D tileText;
        [SerializeField] int[,] rule = new int[3, 3];
        public Texture2D Texture { get => tileText; }

        public TilingRule(string ruleName, Texture2D tileText, int[,] rule)
        {
            this.ruleName = ruleName;
            this.tileText = tileText;
            this.rule = rule;
        }

        public bool CheckSuitable(int[,] grid, int row, int column, int dirGridRow, int dirGridCol)
        {
            // Grid: the grid to scan
            // x, y: Position to check
            // row, col: Index of a 3x3 grid, representing the direction to check

            if (dirGridRow < 0 || dirGridRow >= rule.GetLength(0)) { Debug.Log("In valid row value!"); return false; }
            if (dirGridCol < 0 || dirGridCol >= rule.GetLength(1)) { Debug.Log("In valid col value!"); return false; }

            int topLeftColumn = column - 1, topLeftRow = row - 1;

            // If the checking direction if out of bounds
            if (topLeftColumn + dirGridCol < 0 || topLeftColumn + dirGridCol >= grid.GetLength(1) ||
                topLeftRow + dirGridRow < 0 || topLeftRow + dirGridRow >= grid.GetLength(0))
            {
                if (rule[dirGridRow, dirGridCol] == 1 || rule[dirGridRow, dirGridCol] == -1) return false;
                return true;
            }

            // Compare value
            //Debug.Log($"Name: {ruleName}, row: {row}, column: {column}, direction row: {dirGridRow}, direction col {dirGridCol}, compare value: {grid[topLeftRow + dirGridRow, topLeftColumn + dirGridCol]}");
            switch(rule[dirGridRow, dirGridCol])
            {
                case 1:
                    if (grid[topLeftRow + dirGridRow, topLeftColumn + dirGridCol] == grid[row, column]) return true;
                    return false;
                case -1:
                    if (grid[topLeftRow + dirGridRow, topLeftColumn + dirGridCol] != grid[row, column]) return true;
                    return false;
                default:
                    return true;

            }
        }
    }
}


