using System.Collections.Generic;
using UnityEngine;

public class WorldCube : ContainerCube
{
    [SerializeField] List<MapSelectionCube> mapsList = new();

    private bool hasUnlocked = false;

    public override void Init()
    {
        base.Init();


    }
}
