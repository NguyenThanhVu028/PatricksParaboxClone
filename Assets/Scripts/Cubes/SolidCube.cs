using UnityEngine;

public class SolidCube : BaseCube
{
    public SolidCube(bool isActive = true) : base(isActive)
    {
        Renderer = new SolidCubeRenderer();
    }
}
