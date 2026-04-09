using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelectionCube : ContainerCube
{
    [SerializeField] MapSelectionCube requiredMap;
    [SerializeField] TriggerButton triggerButtonPrefab;
    [Min(1)]
    [SerializeField] int mapIndex = 1;
    [SerializeField] string mapName;
    [SerializeField] float openMapDelayTime = 0.5f;
    [SerializeField] TextMeshPro mapIndexText;
    [SerializeField] float mapIndexTextPadding = 0.75f;

    private bool hasInit = false;
    private bool hasFinished = false;
    private Coroutine openMapCoroutine;

    public bool HasFinished { get => hasFinished; set => hasFinished = value; }

    public override void Init()
    {
        if (hasInit) return;

        hasInit = true;

        isPlayer = false;
        childGrid.Tiling = Vector2Int.one;
        childGrid.Init();

        if (triggerButtonPrefab != null)
        {
            var triggerButton = Instantiate(triggerButtonPrefab);

            triggerButton.RelativeScale = new Vector2(1.0f / childGrid.Tiling.y, 1.0f / childGrid.Tiling.x);
            triggerButton.RelativePosition = Relativity.RPosFromGridTile(childGrid.Tiling.y, childGrid.Tiling.x, 0, 0);
            triggerButton.Parent = this;
            emptyCubes.Add(triggerButton);
            ModifyChildCube(triggerButton);
            triggerButton.Init();

            triggerButton.OnButtonActivated.AddListener(OpenMap);
        }

        previousParent = parent;

        if (SaveAndLoadManager.Instance != null)
        {
            SaveAndLoadManager.GameData gameData = SaveAndLoadManager.Instance.GeneralGameData;
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

        if (!hasFinished && surfaceEffectsAnimation == null)
        {
            SetSurfaceEffects("RegularShiny");
        }
        else if (hasFinished) surfaceEffectsAnimation = null;

        onInit.Invoke();
    }

    protected override void DrawSurfaceEffects(Rect position, float depth)
    {
        base.DrawSurfaceEffects(position, depth);
        if (mapIndexText != null)
        {
            mapIndexText.gameObject.SetActive(true);
            mapIndexText.rectTransform.sizeDelta = position.size * mapIndexTextPadding;
            mapIndexText.text = mapIndex.ToString();
            mapIndexText.transform.position = new(position.x, position.y, -1f);
        }
    }

    public void OpenMap()
    {
        if (openMapCoroutine != null) return;
        openMapCoroutine = StartCoroutine(OpenMapCoroutine());
    }

    public void CheckRequiredMap()
    {
        if (requiredMap == null || hasFinished)
        {
            isEnterable = true;
            return;
        }
        if (!requiredMap.hasFinished) isEnterable = false;
    }

    private IEnumerator OpenMapCoroutine()
    {
        yield return new WaitForSeconds(openMapDelayTime);
        SceneManager.LoadSceneAsync(mapName);
        openMapCoroutine = null;
    }
}
