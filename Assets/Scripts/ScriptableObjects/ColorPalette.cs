using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Scriptable Objects/ColorPalette")]
public class ColorPalette : ScriptableObject
{
    [SerializeField] Color defaultColor = Color.white;
    [SerializeField] List<ColorInfo> colors = new();

    public Color GetColor(ColorType colorEnum)
    {
        foreach (var colorInfo in colors)
        {
            if (colorInfo.ColorType == colorEnum) return colorInfo.Color;
        }
        return defaultColor;
    }

    [Serializable]
    public struct ColorInfo
    {
        public ColorType ColorType;
        public Color Color;
    }
}
