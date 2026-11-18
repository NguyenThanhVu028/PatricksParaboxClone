using UnityEngine;

public abstract class BaseRenderer : MyComponent
{
    public abstract void Render(float minSortingOrder, float maxSortingOrder, Rect rect);
}
