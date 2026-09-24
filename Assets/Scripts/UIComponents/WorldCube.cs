using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldCube : ContainerCube
{
    [SerializeField] string worldName = "";
    [SerializeField] List<WorldCube> dependentWorldCubes = new();
    [SerializeField] int requiredMapCount = 2;
    [SerializeField] TextMeshPro requiredMapCountText;
    [SerializeField] GameObject requiredMapText;
    [SerializeField] SpriteRenderer lockIconDisplayer;
    [SerializeField] Sprite lockedIcon;
    [SerializeField] Sprite unlockedIcon;

    public string WorldName { get => worldName; }

    // This init is called after all the map selection cubes of this cube have been init
    public override void Init()
    {
        base.Init();

        int finishedCount = 0;
        foreach(var childCube in childGrid.Children)
        {
            if (childCube.Cube == null || !(childCube.Cube is MapSelectionCube)) continue;
            if ((childCube.Cube as MapSelectionCube).HasFinished) finishedCount++;
            if (finishedCount >= requiredMapCount)
            {
                if (lockIconDisplayer != null && unlockedIcon != null) lockIconDisplayer.sprite = unlockedIcon;
                foreach (var depedentWorld in dependentWorldCubes)
                {
                    if (depedentWorld == null) continue;
                    depedentWorld.OnRequiredWorldUnlocked(true);
                }
                break;
            }
        }

        if (finishedCount < requiredMapCount)
        {
            if (lockIconDisplayer != null && lockedIcon != null) lockIconDisplayer.sprite = lockedIcon;
            foreach (var depedentWorld in dependentWorldCubes)
            {
                if (depedentWorld == null) continue;
                depedentWorld.OnRequiredWorldUnlocked(false);
            }
        }

        if (requiredMapCountText != null) requiredMapCountText.text = finishedCount.ToString() + "/" + requiredMapCount.ToString();
    }

    public override void DrawCube(Rect position, int priority, float depth, float exposure, Rect? scissorRect)
    {
        base.DrawCube(position, priority, depth, exposure, scissorRect);

        if (requiredMapText != null && requiredMapCountText != null)
        {
            requiredMapText.gameObject.SetActive(true);

            Vector3 textPos = position.position + Vector2.down * (position.size.y * 0.5f + requiredMapCountText.rectTransform.sizeDelta.y * 0.5f);
            textPos.z = depth - 1f;
            requiredMapText.transform.position = textPos;
        }
    }

    public void OnRequiredWorldUnlocked(bool hasUnlocked)
    {
        isEnterable = hasUnlocked;
    }
}
