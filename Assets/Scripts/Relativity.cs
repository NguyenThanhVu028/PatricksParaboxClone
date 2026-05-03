using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public static class Relativity
{
    /*
     * Conventions of relative scale:
     * - Relative scale is a pair of values (relative width, relative height)
     * - Scale of the parent object is always (1, 1)
     * - Relative scale of the child object indicates how smaller/bigger it is compared to its parent
     * -> Child object's real width(height) = parent's real width(height) * relative width(height)
     * 
     * Conventions of relative position:
     * - Relative position is a pair of values (relative X, relative Y)
     * - The center of the parent object has relative position: (0, 0)
     * - Top left corner of the parent object has relative position: (-1, 1)
     * -> Child object's real x(y) = parent's real x(y) + parent's width(height) * 0.5f * relative X(Y)
     *
     * Abbreviations:
     * - RPos: Relative position
     * - RealPos: Real position
     * - RScl: Relative scale
     * - RealScl: Real scale
     * - C: Child
     * - P: Parent
     * - S: Sibling
     */

    // Get relative position based on the grid position of the tile in the parent cube
    public static Vector2 RPosFromGridTile(int gridWidth, int gridHeight, int row, int col)
    {
        Vector2 tileSize = new Vector2(2.0f / gridWidth, 2.0f / gridHeight);
        Vector2 topLeftRelativePos = new Vector2(-1.0f + tileSize.x * 0.5f, 1.0f - tileSize.y * 0.5f);
        return new Vector2(topLeftRelativePos.x + col * tileSize.x, topLeftRelativePos.y - row * tileSize.y);
    }
    // Get the rectangle of the child cube based on the grid position of the tile in the parent cube
    public static Rect CRectFromGridTile(int gridWidth, int gridHeight, int row, int col)
    {
        Rect resRect = new();
        resRect.position = RPosFromGridTile(gridWidth, gridHeight, row, col);
        resRect.size = new Vector2(1.0f / gridWidth, 1.0f / gridHeight);
        return resRect;
    }
    // Get grid position of the tile in the parent cube based on the relative position of the child cube
    public static Vector2Int GridPosFromRPos(int gridWidth, int gridHeight, Vector2 rPos)
    {
        Vector2 tileSize = new Vector2(2.0f / gridWidth, 2.0f / gridHeight);
        Vector2Int resPos = new();
        resPos.x = Mathf.FloorToInt((1.0f - rPos.y) / tileSize.y);
        resPos.y = Mathf.FloorToInt((rPos.x + 1.0f) / tileSize.x);
        return resPos;
    }
    // Get child's rectangle based on parent's rectangle and child's relative values
    public static Rect CRectFromPRect(Rect pRect, Vector2 rScl, Vector2 rPos)
    {
        Rect cRect = new Rect();
        cRect.width = pRect.width * rScl.x; cRect.height = pRect.height * rScl.y;
        cRect.x = pRect.x + pRect.width * 0.5f * rPos.x;
        cRect.y = pRect.y + pRect.height * 0.5f * rPos.y;
        return cRect;
    }
    // Get parent's rectangle based on child's rectangle and child's relative values
    public static Rect PRectFromCRect(Rect cRect, Vector2 rScl, Vector2 rPos)
    {
        Rect pRect = new Rect();
        pRect.width = cRect.width / rScl.x; pRect.height = cRect.height / rScl.y;
        pRect.x = cRect.x - pRect.width * 0.5f * rPos.x;
        pRect.y = cRect.y - pRect.height * 0.5f * rPos.y;
        return pRect;
    }
    // Get relative position to a parent of the child cube based on the real position of the child cube
    public static Vector2 CRPosFromCRealPos(Rect pRect, Vector2 cRealPos)
    {
        Vector2 cRPos = new();
        cRPos.x = (cRealPos.x - pRect.x) / ((float)pRect.width * 0.5f);
        cRPos.y = (cRealPos.y - pRect.y) / ((float)pRect.height * 0.5f);
        return cRPos;
    }
    // Get real position of the child cube based on the relative position to its parent
    public static Vector2 CRealPosFromCRPos(Rect pRect, Vector2 cRPos)
    {
        Vector2 cRealPos = new();
        cRealPos.x = pRect.x + cRPos.x * pRect.width;
        cRealPos.y = pRect.y + cRPos.y * pRect.height;
        return cRealPos;
    }
    // Get relative position to a parent to its child cube
    public static Vector2 PRPosToAChild(Vector2 cRPos, Vector2 cRScl)
    {
        Vector2 pRPos = new();
        pRPos.x = -cRPos.x / cRScl.x;
        pRPos.y = -cRPos.y / cRScl.y;
        return pRPos;
    }
    // Get relative position of sub child to main child based on their relative positions to the same parent
    public static Vector2 PRSclToAChild(Vector2 cRScl)
    {
        Vector2 pRScl = new();
        pRScl.x = 1.0f / cRScl.x;
        pRScl.y = 1.0f / cRScl.y;
        return pRScl;
    }
    public static Vector2 SRPosFromSameParent(Vector2 mainCRPos, Vector2 mainRScl, Vector2 subRPos)
    {
        Vector2 sRPos = subRPos - mainCRPos;
        sRPos.x /= mainRScl.x; sRPos.y /= mainRScl.y;
        return sRPos;
    }
    // Get relative scale of sub child to main child based on their relative scales to the same parent
    public static Vector2 SRSclFromSameParent(Vector2 mainRScl, Vector2 subRScl)
    {
        Vector2 sRScl = new();
        sRScl.x = subRScl.x / mainRScl.x;
        sRScl.y = subRScl.y / mainRScl.y;
        return sRScl;
    }

    public static List<CubeMovement.GridDirections> CheckEdgeOfGrid(int gridWidth, int gridHeight, Vector2 rPos)
    {
        List<CubeMovement.GridDirections> edges = new();
        Vector2Int gridPos = GridPosFromRPos(gridWidth, gridHeight, rPos);
        if (gridPos.x == 0) edges.Add(CubeMovement.GridDirections.Up);
        if (gridPos.x == gridHeight - 1) edges.Add(CubeMovement.GridDirections.Down);
        if (gridPos.y == 0) edges.Add(CubeMovement.GridDirections.Left);
        if (gridPos.y == gridWidth - 1) edges.Add(CubeMovement.GridDirections.Right);

        if (edges.Count == 0) edges.Add(CubeMovement.GridDirections.None);
        return edges;
    }
}
