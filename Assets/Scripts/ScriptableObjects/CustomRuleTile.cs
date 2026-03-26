using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomRuleTile", menuName = "Scriptable Objects/CustomRuleTile")]
public class CustomRuleTile : ScriptableObject
{
    [SerializeField] Texture2D defaultTexture;
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
    
    public Texture2D GetTexture(int[,] grid, int row, int column)
    {
        /*
         * - This function is used to get a suitable texture at any given row and column of a grid
         * - It only check neighbours around the given position (in a 3x3 area)
         * 
         * Get tile logic:
         * - Create 2 lists to store suitable rules and rules to scan
         * - With the given row and column position, loop through all of its neighbors:
         *      + Loop through all the current suitable rules, check if each rule is still suitable for the current neighbor
         *      + If there are no suitable rules -> return default texture
         *      + Else return texture of the first suitable rule
         */

        if (row < 0 || column < 0) return null;
        if (column >= grid.GetLength(1) || row >= grid.GetLength(0)) return null;

        List<TilingRule> suitableTilingRules = new(tilingRules);
        List<TilingRule> tilingRulesToScan;

        // Direction grid: a 3x3 array representing direction to check for suitable rule
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

                if (suitableTilingRules.Count == 0) return defaultTexture;
            }
        }

        return suitableTilingRules[0].Texture;
    }

    [Serializable]
    public class TilingRule
    {
        public enum TreatNullValue { Same, Different, Invalid, Ignore}

        [SerializeField] string ruleName;
        [SerializeField] Texture2D tileText;
        //[SerializeField] bool acceptNullValue = false; // If true, treat null value as -1
        [SerializeField] TreatNullValue treatNullValueAs = TreatNullValue.Invalid;
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
            /* grid: the grid to scan
             * row, colunm: Position of target
             * dirGridRow, dirGridCol: Index of a 3x3 grid, representing the direction of the target neighbor
             * 
             * Check suitable logic:
             * - If the target neighbor is out of bounds, then check what the rule treats null value as (same, different, invalid or ignore)
             * - If the rule value in the given direction is equal to 1, then the target value and the target neighbor value should be the same.
             * - If the rule value in the given direction is equal to -1, then the target value and the target neighbor value should be different.
             * - If the rule value in the given direction is equal to 0, then the target neighbor is always suitable no mattter its value.
             */

            if (dirGridRow < 0 || dirGridRow >= rule.GetLength(0)) { Debug.Log("In valid row value for direction grid!"); return false; }
            if (dirGridCol < 0 || dirGridCol >= rule.GetLength(1)) { Debug.Log("In valid col value for direction grid!"); return false; }

            int topLeftColumn = column - 1, topLeftRow = row - 1; // In the 3x3 grid

            // If the checking direction if out of bounds -> null value
            if (topLeftColumn + dirGridCol < 0 || topLeftColumn + dirGridCol >= grid.GetLength(1) ||
                topLeftRow + dirGridRow < 0 || topLeftRow + dirGridRow >= grid.GetLength(0))
            {
                switch (treatNullValueAs)
                {
                    case TreatNullValue.Ignore:
                        return true;
                    case TreatNullValue.Invalid:
                        if (rule[dirGridRow, dirGridCol] == 1 || rule[dirGridRow, dirGridCol] == -1) return false;
                        return true;
                    case TreatNullValue.Same:
                        if (rule[dirGridRow, dirGridCol] != -1) return true;
                        return false;
                    case TreatNullValue.Different:
                        if (rule[dirGridRow, dirGridCol] != 1) return true;
                        return false;
                }
                return true;
            }

            // Compare value
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


