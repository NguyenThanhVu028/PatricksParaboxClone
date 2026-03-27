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
     * - CRect: Child rectangle
     * - PRect: Parent rectangle
     */

    public static Vector2 RPosFromGridTile(int gridWidth, int gridHeight, int row, int col)
    {
        Vector2 tileSize = new Vector2(2.0f / gridWidth, 2.0f / gridHeight);
        Vector2 topLeftRelativePos = new Vector2(-1.0f + tileSize.x * 0.5f, 1.0f - tileSize.y * 0.5f);
        return new Vector2(topLeftRelativePos.x + col * tileSize.x, topLeftRelativePos.y - row * tileSize.y);
    }

    public static Rect CRectFromGridTile(int gridWidth, int gridHeight, int row, int col)
    {
        Rect resRect = new();
        resRect.position = RPosFromGridTile(gridWidth, gridHeight, row, col);
        resRect.size = new Vector2(1.0f / gridWidth, 1.0f / gridHeight);
        return resRect;
    }

    public static Vector2Int GridPosFromRPos(int gridWidth, int gridHeight, Vector2 rPos)
    {
        Vector2 center = new Vector2((float)gridWidth * 0.5f, (float)gridHeight * 0.5f);
        Vector2Int resPos = new();
        resPos.x = Mathf.RoundToInt(center.x + rPos.x * gridWidth * 0.5f);
        resPos.y = Mathf.RoundToInt(center.y + rPos.y * gridHeight * 0.5f);
        return resPos;
    }

    public static Rect CRectFromPRect(Rect pRect, Vector2 rScl, Vector2 rPos)
    {
        Rect cRect = new Rect();
        cRect.width = pRect.width * rScl.x; cRect.height = pRect.height * rScl.y;
        cRect.x = pRect.x + pRect.width * 0.5f * rPos.x;
        cRect.y = pRect.y + pRect.height * 0.5f * rPos.y;
        return cRect;
    }

    public static Rect PRectFromCRect(Rect cRect, Vector2 rScl, Vector2 rPos)
    {
        Rect pRect = new Rect();
        pRect.width = cRect.width / rScl.x; pRect.height = cRect.height / rScl.y;
        pRect.x = cRect.x - pRect.width * 0.5f * rPos.x;
        pRect.y = cRect.y - pRect.height * 0.5f * rPos.y;
        return pRect;
    }

    public static Vector2 CRPosFromCRealPos(Rect pRect, Vector2 cRealPos)
    {
        Vector2 cRPos = new();
        cRPos.x = (cRealPos.x - pRect.x) / ((float)pRect.width * 0.5f);
        cRPos.y = (cRealPos.y - pRect.y) / ((float)pRect.height * 0.5f);
        return cRPos;
    }
    public static Vector2 CRealPosFromCRPos(Rect pRect, Vector2 cRPos)
    {
        Vector2 cRealPos = new();
        cRealPos.x = pRect.x + cRPos.x * pRect.width;
        cRealPos.y = pRect.y + cRPos.y * pRect.height;
        return cRealPos;
    }
    public static Vector2 PRPosFromCRPosAndCRScl(Vector2 cRPos, Vector2 cRScl)
    {
        Vector2 pRPos = new();
        pRPos.x = -cRPos.x / cRScl.x;
        pRPos.y = -cRPos.y / cRScl.y;
        return pRPos;
    }
    //public static Vector2 CRPosFromPRPosAndPRScl(Vector2 pRPos, Vector2 pRScl)
    //{
    //    Vector2 cRPos = new();
    //    cRPos.x = -pRPos.x / pRScl.x;
    //    cRPos.y = -pRPos.y / pRScl.y;
    //    return cRPos;
    //}
}
