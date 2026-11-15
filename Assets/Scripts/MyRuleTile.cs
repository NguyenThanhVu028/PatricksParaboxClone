using System.Collections.Generic;
using UnityEngine;

public class MyRuleTile<T>
{
    private List<TilingRule> _rules = new()
    {
        new TilingRule()
        {
            RuleName = "Top Left",
            Rule = new List<int>[,]
            {
                {new(){0}, new(){-1}, new(){0}},
                {new(){-1}, new(){0}, new(){1}},
                {new(){0}, new(){1}, new(){0}}
            }
        },
        new TilingRule()
        {
            RuleName = "Left",
            Rule = new List<int>[,]
            {
                {new(){0}, new(){1, -2}, new(){0}},
                {new(){-1}, new(){0}, new(){1, -2}},
                {new(){0}, new(){1, -2}, new(){0}}
            }
        },
        new TilingRule()
        {
            RuleName = "Bottom Left",
            Rule = new List<int>[,]
            {
                {new(){0}, new(){1}, new(){0}},
                {new(){-1}, new(){0}, new(){1}},
                {new(){0}, new(){-1}, new(){0}}
            }
        },
        new TilingRule()
        {
            RuleName = "Bottom",
            Rule = new List<int>[,]
            {
                {new(){0}, new(){1, -2}, new(){0}},
                {new(){1, -2}, new(){0}, new(){1, -2}},
                {new(){0}, new(){-1}, new(){0}}
            }
        },
        new TilingRule()
        {
            RuleName = "Bottom Right",
            Rule = new List<int>[,]
            {
                {new(){0}, new(){1}, new(){0}},
                {new(){1}, new(){0}, new(){-1}},
                {new(){0}, new(){-1}, new(){0}}
            }
        },
        new TilingRule()
        {
            RuleName = "Right",
            Rule = new List<int>[,]
            {
                {new(){0}, new(){1, -2}, new(){0}},
                {new(){1, -2}, new(){0}, new(){-1}},
                {new(){0}, new(){1, -2}, new(){0}}
            }
        },
        new TilingRule()
        {
            RuleName = "Top Right",
            Rule = new List<int>[,]
            {
                {new(){0}, new(){-1}, new(){0}},
                {new(){1}, new(){0}, new(){-1}},
                {new(){0}, new(){1}, new(){0}}
            }
        },
        new TilingRule()
        {
            RuleName = "Top",
            Rule = new List<int>[,]
            {
                {new(){0}, new(){-1}, new(){0}},
                {new(){1, -2}, new(){0}, new(){1, -2}},
                {new(){0}, new(){1, -2}, new(){0}}
            }
        },
        // new TilingRule()
        // {
        //     RuleName = "Center",
        //     Rule = new List<int>[,]
        //     {
        //         {new(){0}, new(){1, -2}, new(){0}},
        //         {new(){1, -2}, new(){0}, new(){1, -2}},
        //         {new(){0}, new(){1, -2}, new(){0}}
        //     }
        // },
        new TilingRule()
        {
            RuleName = "Inner Top Left",
            Rule = new List<int>[,]
            {
                {new(){-1}, new(){1}, new(){0}},
                {new(){1}, new(){0}, new(){1, -2}},
                {new(){0}, new(){1, -2}, new(){0}}
            }
        },
        new TilingRule()
        {
            RuleName = "Inner Bottom Left",
            Rule = new List<int>[,]
            {
                {new(){0}, new(){1, -2}, new(){0}},
                {new(){1}, new(){0}, new(){1, -2}},
                {new(){-1}, new(){1}, new(){0}}
            }
        },
        new TilingRule()
        {
            RuleName = "Inner Bottom Right",
            Rule = new List<int>[,]
            {
                {new(){0}, new(){1, -2}, new(){0}},
                {new(){1, -2}, new(){0}, new(){1}},
                {new(){0}, new(){1}, new(){-1}}
            }
        },
        new TilingRule()
        {
            RuleName = "Inner Top Right",
            Rule = new List<int>[,]
            {
                {new(){0}, new(){1}, new(){-1}},
                {new(){1, -2}, new(){0}, new(){1}},
                {new(){0}, new(){1, -2}, new(){0}}
            }
        }
    };
    private Texture2D _defaultTexture;
    private Material _material;

    public MyRuleTile(Material mat)
    {
        _material = mat;
    }

    public void DrawOnRenderTexture(object[,] grid, float tileSubdivision, Vector2 tileTextureSize, ref RenderTexture renderTexture, int renderDepth = 16)
    {
        renderTexture = new RenderTexture(Mathf.RoundToInt(grid.GetLength(1) * tileSubdivision * tileTextureSize.x), Mathf.RoundToInt(grid.GetLength(0) * tileSubdivision * tileTextureSize.y), renderDepth)
        {
            filterMode = FilterMode.Point
        };

        RenderTexture.active = renderTexture;
        GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);

        int tileGridRowCount = Mathf.RoundToInt(grid.GetLength(0) * tileSubdivision);
        int tileGridColumnCount = Mathf.RoundToInt(grid.GetLength(1) * tileSubdivision);

        Vector2Int TileGridPosToGridPos(Vector2Int position)
        {
            return new()
            {
                x = Mathf.FloorToInt(position.x / tileSubdivision),
                y = Mathf.FloorToInt(position.y / tileSubdivision)
            };
        }

        object[,] tileGrid = new object[tileGridRowCount, tileGridColumnCount];
        for (int row = 0; row < tileGridRowCount; row++)
        {
            for (int col = 0; col < tileGridColumnCount; col++)
            {
                var gridPos = TileGridPosToGridPos(new(row, col));
                tileGrid[row, col] = grid[gridPos.x, gridPos.y];
            }
        }

        for (int row = 0; row < tileGridRowCount; row++)
        {
            for (int col = 0; col < tileGridColumnCount; col++)
            {
                if (tileGrid[row, col] == null ||
                    tileGrid[row, col].GetType() != typeof(T))
                {
                    continue;
                }

                bool validRuleFound = false;
                Rect rectToDraw = new()
                {
                    x = col * tileTextureSize.x,
                    y = row * tileTextureSize.y,
                    size = tileTextureSize
                };

                foreach (var rule in _rules)
                {
                    if (rule.CheckTileAt(tileGrid, row, col))
                    {
                        validRuleFound = true;
                        Graphics.DrawTexture(rectToDraw, rule.Output, _material);
                    }

                    if (validRuleFound)
                    {
                        break;
                    }
                }
                if (!validRuleFound)
                {
                    Graphics.DrawTexture(rectToDraw, _defaultTexture, _material);
                }
            }
        }
        RenderTexture.active = null;
    }

    public void SetOutput(string ruleName, Texture2D texture)
    {
        var targetRule = _rules.Find((rule) => rule.RuleName == ruleName);
        if (targetRule != null)
        {
            targetRule.Output = texture;
        }
    }

    public void AddNewRule(TilingRule rule)
    {
        if (rule != null)
        {
            _rules.Add(rule);
        }
    }

    public void SetDefaultTexture(Texture2D defaultTexture)
    {
        _defaultTexture = defaultTexture;
    }

    public class TilingRule
    {
        // Conventions:
        // 1: Same
        // 0: Dont care
        // -1: Different
        // -2: Null

        public string RuleName;
        public Texture2D Output;
        public List<int>[,] Rule;

        // Grid * Subdivision = TileGrid
        public bool CheckTileAt(object[,] tileGrid, int inputTileGridRow, int inputTileGridCol)
        {
            if (Rule == null)
            {
                return false;
            }

            //Debug.Log($"Checking rule: {RuleName} at {inputTileGridRow}, {inputTileGridCol}");

            int tileGridRowCount = tileGrid.GetLength(0);
            int tileGridColumnCount = tileGrid.GetLength(1);

            int ruleRowCount = Rule.GetLength(0);
            int ruleColumnCount = Rule.GetLength(1);

            for (int ruleRow = 0; ruleRow < ruleRowCount; ruleRow++)
            {
                for (int ruleCol = 0; ruleCol < ruleColumnCount; ruleCol++)
                {
                    if (Rule[ruleRow, ruleCol].Contains(0))
                    {
                        //Debug.Log($"Rule {RuleName} ignore tile at: {ruleRow}, {ruleCol}");
                        continue;
                    }

                    int tileGridRow = inputTileGridRow - Mathf.FloorToInt(ruleRowCount / 2.0f) + ruleRow;
                    int tileGridCol = inputTileGridCol - Mathf.FloorToInt(ruleColumnCount / 2.0f) + ruleCol;

                    if (tileGridRow == inputTileGridRow && tileGridCol == inputTileGridCol)
                    {
                        continue;
                    }

                    // Invalid range -> Treat as null
                    if (tileGridRow < 0 ||
                        tileGridCol < 0 ||
                        tileGridRow >= tileGridRowCount ||
                        tileGridCol >= tileGridColumnCount ||
                        tileGrid[tileGridRow, tileGridCol] == null)
                    {
                        //Debug.Log($"Rule {RuleName} found null tile at: {ruleRow}, {ruleCol}");
                        if (!Rule[ruleRow, ruleCol].Contains(-2))
                        {
                            //Debug.Log($"Rule {RuleName} don't accept null tile at: {ruleRow}, {ruleCol}");
                            return false;
                        }
                        continue;
                    }

                    if (tileGrid[tileGridRow, tileGridCol].GetType() == tileGrid[inputTileGridRow, inputTileGridCol].GetType())
                    {
                        if (!Rule[ruleRow, ruleCol].Contains(1))
                        {
                            //Debug.Log($"Rule {RuleName} wants same tile at: {ruleRow}, {ruleCol}");
                            return false;
                        }
                    }
                    else
                    {
                        if (!Rule[ruleRow, ruleCol].Contains(-1))
                        {
                            //Debug.Log($"Rule {RuleName} wants different tile at: {ruleRow}, {ruleCol}");
                            return false;
                        }
                    }
                }
            }

            //Debug.Log($"Rule {RuleName} Accept tile!");
            return true;
        }
    }
}
