using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelectionCube : ContainerCube
{
    [SerializeField] List<MapSelectionCube> dependentMapSelectionCubes = new();
    [SerializeField] TriggerButton triggerButtonPrefab;
    [Min(1)]
    [SerializeField] int mapIndex = 1;
    [SerializeField] string mapName;
    [SerializeField] float defaultDelayTime = 0.5f;
    [SerializeField] TextMeshPro mapIndexText;
    [SerializeField] float mapIndexTextPadding = 0.75f;
    [SerializeField] string notFinishedEffect = "RegularShiny";

    private SaveAndLoadManager.GameData gameData;
    private bool hasFinished = false;
    private Coroutine openMapCoroutine;
    private float OpenMapDelayTime
    {
        get
        {
            if (gameData != null) return gameData.EnterTime / 1000f;
            return defaultDelayTime;
        }
    }

    public bool HasFinished { get => hasFinished; set => hasFinished = value; }

    public override void Init()
    {
        isPlayer = false;
        childGrid.Tiling = Vector2Int.one;
        childGrid.Init();

        // Add triger button at the center to detect when player enters
        if (triggerButtonPrefab != null)
        {
            var triggerButton = Instantiate(triggerButtonPrefab);

            triggerButton.RelativeScale = new Vector2(1.0f / childGrid.Tiling.y, 1.0f / childGrid.Tiling.x);
            triggerButton.RelativePosition = Relativity.RPosFromGridTile(childGrid.Tiling.y, childGrid.Tiling.x, 0, 0);
            triggerButton.Parent = this;
            emptyCubes.Add(triggerButton);
            //ModifyChildCube(triggerButton);
            triggerButton.Init();

            triggerButton.OnButtonActivated.AddListener(OpenMap);
        }

        // Check hasFinished status
        if (SaveAndLoadManager.GeneralGameData != null)
        {
            gameData = SaveAndLoadManager.GeneralGameData;
            if (gameData != null)
            {
                foreach(var mapID in gameData.FinishedMapIDs)
                {
                    if (mapID == mapName)
                    {
                        hasFinished = true;
                        break;
                    }
                }
            }
        }

        // Init effects and announce all dependent cubes
        if (!hasFinished)
        {
            if (surfaceEffectsAnimation == null)
                SetSurfaceEffects(notFinishedEffect);
            foreach(var dependentMap in dependentMapSelectionCubes)
            {
                if (dependentMap == null) continue;
                dependentMap.OnRequiredMapFinished(false);
            }
        }
        else if (hasFinished)
        {
            surfaceEffectsAnimation = null;
            foreach (var dependentMap in dependentMapSelectionCubes)
            {
                if (dependentMap == null) continue;
                dependentMap.OnRequiredMapFinished(true);
            }
        }

        onInit.Invoke();
    }

    public override void DrawSurfaceEffects(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        base.DrawSurfaceEffects(position, depth, exposure, scissorRect);
        if (mapIndexText != null)
        {
            mapIndexText.gameObject.SetActive(true);
            Vector2 textSize = new(Mathf.Abs(position.size.x), Mathf.Abs(position.size.y));
            mapIndexText.rectTransform.sizeDelta = textSize * mapIndexTextPadding;
            mapIndexText.text = mapIndex.ToString();
            mapIndexText.transform.position = new(position.x, position.y, depth);
        }
    }

    public void OnRequiredMapFinished(bool hasFinished)
    {
        if (this.hasFinished)
        {
            isEnterable = true;
            return;
        }
        isEnterable = hasFinished;
    }

    public void OpenMap()
    {
        if (openMapCoroutine != null) return;
        openMapCoroutine = StartCoroutine(OpenMapCoroutine());
    }


    private IEnumerator OpenMapCoroutine()
    {
        yield return new WaitForSeconds(OpenMapDelayTime);
        SceneManager.LoadSceneAsync(mapName);
        openMapCoroutine = null;
    }
}
