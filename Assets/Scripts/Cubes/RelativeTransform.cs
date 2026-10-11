using UnityEngine;

public class RelativeTransform : MyComponent
{
    private Vector2 _relativePosition = new();
    private Vector2 _relativeScale = new();

    public Vector2 RelativePosition
    {
        get => _relativePosition;
        set => _relativePosition = value;
    }

    public Vector2 RelativeScale
    {
        get => _relativeScale;
        set => _relativeScale = value;
    }
}
