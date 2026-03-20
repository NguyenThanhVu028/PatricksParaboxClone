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
    };
    
    public Texture2D GetTile(int[,] grid, Vector2 tilePos)
    {
        return defaultTile;
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
    }
}


