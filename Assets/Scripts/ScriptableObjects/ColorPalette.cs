using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Scriptable Objects/ColorPalette")]
public class ColorPalette : ScriptableObject
{
    [SerializeField] Color defaultColor = Color.white;
    [SerializeField] List<ColorInfo> colors = new();

    public Color GetColor(ColorEnum colorEnum)
    {
        foreach(var colorInfo in colors)
        {
            if (colorInfo.ColorEnum == colorEnum) return colorInfo.Color;
        }
        return defaultColor;
    }

    public enum ColorEnum 
    { 
        Red, 
        Green, 
        Blue, 
        Yellow, 
        Cyan, 
        Purple, 
        Pink, 
        Silver, 
        Player, 
        SilverBlue, 
        Orange,
        Black
    }

    [Serializable]
    public class ColorInfo
    {
        [SerializeField] ColorEnum colorEnum;
        [SerializeField] Color color;

        public ColorEnum ColorEnum { get => colorEnum; }
        public Color Color { get => color; }
    }
}
