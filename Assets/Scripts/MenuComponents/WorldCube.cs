using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldCube : ContainerCube
{
    [SerializeField] string worldName = "";
    [SerializeField] List<MapSelectionCube> mapSelectionCubesList = new();
    [SerializeField] List<WorldCube> dependentWorldCubes = new();
    [SerializeField] int requiredMapCount = 2;
    [SerializeField] TextMeshPro requiredMapCountText;

    private bool hasUnlocked = false;

    public string WorldName { get => worldName; }

    // This init is called after all the map select cubes of this cube have been init
    public override void Init()
    {
        base.Init();

        int finishedCount = 0;
        foreach(var mapSelectionCube in mapSelectionCubesList)
        {
            if (mapSelectionCube == null) continue;
            if (mapSelectionCube.HasFinished) finishedCount++;
            if (finishedCount >= requiredMapCount)
            {
                foreach(var depedentWorld in dependentWorldCubes)
                {
                    if (depedentWorld == null) continue;
                    depedentWorld.OnRequiredWorldUnlocked(true);
                }
                break;
            }
        }

        if (finishedCount < requiredMapCount)
        {
            foreach (var depedentWorld in dependentWorldCubes)
            {
                if (depedentWorld == null) continue;
                depedentWorld.OnRequiredWorldUnlocked(false);
            }
        }

        if (requiredMapCountText != null) requiredMapCountText.text = finishedCount.ToString() + "/" + requiredMapCount.ToString();
    }

    protected override void DrawSurfaceEffects(Rect position, float depth)
    {
        base.DrawSurfaceEffects(position, depth);

        if (requiredMapCountText != null)
        {
            requiredMapCountText.gameObject.SetActive(true);

            Vector3 textPos = position.position + Vector2.down * (position.size.y * 0.5f + requiredMapCountText.rectTransform.sizeDelta.y * 0.5f);
            textPos.z = -1f;
            requiredMapCountText.rectTransform.position = textPos;
        }
    }

    public void OnRequiredWorldUnlocked(bool hasUnlocked)
    {
        if (this.hasUnlocked)
        {
            isEnterable = true;
            return;
        }
        isEnterable = hasUnlocked;
    }
}
