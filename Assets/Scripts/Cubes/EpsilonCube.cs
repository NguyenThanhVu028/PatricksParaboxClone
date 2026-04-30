using UnityEngine;

public class EpsilonCube : ContainerCube
{
    [SerializeField] ContainerCube mainContainerCube;
    [Min(1)]
    [SerializeField] int level = 1;
    [SerializeField] CustomTexture epsilonTexture;

    public ContainerCube MainContainerCube { get => mainContainerCube; set => mainContainerCube = value; }
    public int Level { get => level; set => level = value; }
}
