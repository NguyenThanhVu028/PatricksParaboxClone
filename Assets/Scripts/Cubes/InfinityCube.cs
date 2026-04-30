using UnityEngine;

public class InfinityCube : CloneCube
{
    [Min(1)]
    [SerializeField] int level = 1;
    [SerializeField] CustomTexture infinityTexture;

    public ContainerCube MainContainerCube { get => mainContainerCube; set => mainContainerCube = value; }
    public int Level { get => level; set => level = value; }

    public override void Init()
    {
        if (hasInit) return;

        isPlayer = false;
        canBePlayer = false;
        isEnterable = false;
        isLeavable = false;

        base.Init();
    }

    public override void DrawSurfaceEffects(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        base.DrawSurfaceEffects(position, depth, exposure, scissorRect);

        // Draw the infinity texture
    }
}
