using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Cube))]
public class CubeMovement : MonoBehaviour
{
    public enum GridDirections{ Up, Down, Left, Right, None }
    public enum LayerDirections { In, Out, None }

    [Header("Movement Settings")]
    [SerializeField] float defaultMoveTime = 0.1f; // Used when player simply moving from one point to another
    [SerializeField] float defaultEnterTime = 0.5f; // Used when player enters a cube, transforms, . . .
    [SerializeField] float coolDownTime = 0.075f;
    [SerializeField] protected Cube selfCube;

    [Header("SoundsSettings")]
    [SerializeField] string movementAudioID = "Movement";
    [SerializeField] string zoomInAudioID = "ZoomIn";
    [SerializeField] string zoomOutAudioID = "ZoomOut";
    [SerializeField] string noZoomAudioID = "NoZoom";

    protected Coroutine movingCoroutine = null;
    protected float coolDownTimer = 0f;
    protected UnityEvent onMoveStart = new();
    protected UnityEvent onMoveEnd = new();
    protected bool isExternal = false;
    protected float movingProgress = 0f;

    private bool debugMovement = false;
    private SaveAndLoadManager.GameData gameData;

    public bool Movable { get { return selfCube != null && selfCube.CubeType != Cube.CubeTypes.Static && selfCube.CubeType != Cube.CubeTypes.Empty; } }
    public bool IsMoving { get => movingCoroutine != null; }
    public bool IsCoolingDown { get => (coolDownTimer > 0); }
    public bool IsExternal { get => isExternal; }
    public float MoveTime
    {
        get
        {
            if (gameData != null) return gameData.MoveTime / 1000f;
            return defaultMoveTime;
        }
    }
    public float EnterTime
    {
        get
        {
            if (gameData != null) return gameData.EnterTime / 1000f;
            return defaultEnterTime;
        }
    }
    public Cube SelfCube { get => selfCube; }
    public UnityEvent OnMoveStart { get => onMoveStart; }
    public UnityEvent OnMoveEnd { get => onMoveEnd; }
    public float MovingProgress { get => movingProgress; }

    private void OnEnable()
    {
        selfCube = GetComponent<Cube>();
    }
    private void Start()
    {
        if (SaveAndLoadManager.GeneralGameData != null) gameData = SaveAndLoadManager.GeneralGameData;
    }
    private void Update()
    {
        if (selfCube.IsPlayer &&
            selfCube.Parent != null &&
            PlayerInputsManager.Instance != null &&
            !IsMoving && !IsCoolingDown)
        {
            GridDirections movementInput = PlayerInputsManager.Instance.GameplayInputs.GetLatestMovementInput();
            if (MainCamera.Instance != null && MainCamera.Instance.RenderMode == MainCamera.MainCameraRenderMode.SingleCube)
            {
                if (MainCamera.Instance.TargetCubeRect.size.x < 0) movementInput = CubeMovement.FlipMovementInput(movementInput, true);
            }
            if (movementInput != CubeMovement.GridDirections.None)
            {
                var targetTime = selfCube.Parent.RequestToMove(selfCube.RelativePosition, selfCube.RelativeScale, selfCube, movementInput);
                // Update camera trasition
                if (targetTime > 0)
                {
                    if (MainCamera.Instance != null)
                    {
                        MainCamera.Instance.PlayTransition(selfCube.PreviousParents, targetTime);
                    }
                }
                else if (selfCube.IsPossessing)
                {
                    Debug.Log(selfCube.name + " is possessing: " + selfCube.PreviousParents[selfCube.PreviousParents.Count - 1].Cube);
                    if (MainCamera.Instance != null)
                    {
                        MainCamera.Instance.PlayTransition(selfCube.PreviousParents, selfCube.PossessingTime);
                    }
                    selfCube.PreviousParents.Clear();
                    selfCube.PreviousParents.Add(new Cube.PreviousParentDetails(CubeMovement.LayerDirections.In, selfCube.Parent));
                }
                else if (MainCamera.Instance != null) MainCamera.Instance.ClearTransition();

                HistoryManager historyManager = HistoryManager.Instance;
                if (historyManager != null) historyManager.NormalArchiveHistoryRecord(); // Player will archive the record
                
                coolDownTimer = coolDownTime;
            }
        }
        
        if (coolDownTimer > 0) coolDownTimer -= Time.deltaTime;
        if (coolDownTimer < 0) coolDownTimer = 0;
    }


    // Called by parent cube
    public float StartMoving(Vector2 startRPos, Vector2 startRScl, Vector2 endRPos, Vector2 endRScl, float targetTime, ContainerCube targetParent, bool isExternal = false)
    {
        if (IsMoving || (selfCube.IsPlayer && IsCoolingDown)) return 0;

        // Update self cube status and history
        HistoryManager historyManager = HistoryManager.Instance;
        if (historyManager != null)
        {
            // Modify current record to save its previous details
            var currentRecord = historyManager.GetCurrentRecord();
            var currentProperties = selfCube.GetCubeProperties();
            if (currentRecord != null)
            {
                currentRecord.AddHistoryEvent(selfCube,
                    //(selfCube.PreviousParents.Count > 0) ? selfCube.PreviousParents[0].Cube : null, 
                    currentProperties
                    );
            }
        }

        //for (int i = 1; i < selfCube.PreviousParents.Count; i++)
        //{
        //    if (selfCube.PreviousParents[i].Cube == null) continue;
        //    if (selfCube.PreviousParents[i].Direction == CubeMovement.LayerDirections.In)
        //    {
        //        selfCube.PreviousParents[i].Cube.ModifyChildCubeEnter(selfCube);
        //    }
        //    else if (selfCube.PreviousParents[i].Direction == CubeMovement.LayerDirections.Out)
        //    {
        //        selfCube.PreviousParents[i].Cube.ModifyChildCubeInnerEnter(selfCube);
        //        selfCube.PreviousParents[i - 1].Cube.ModifyChildCubeExit(selfCube);
        //    }
        //}
        if (selfCube.PreviousParents.Count > 0) selfCube.Parent = selfCube.PreviousParents[selfCube.PreviousParents.Count - 1].Cube;

        //if (historyManager != null)
        //{
        //    // Add new record for its new details
        //    historyManager.RecordNewEvent(selfCube, 
        //        selfCube.Parent, 
        //        endRPos, 
        //        endRScl, 
        //        selfCube.IsHorizFlipped, 
        //        selfCube.IsPlayer,
        //        (selfCube is ContainerCube containerCube) ? containerCube.IsEnterable : true,
        //        (selfCube is ContainerCube containerCube2) ? containerCube2.IsLeavable : true
        //        );
        //}

        Vector2 cubeOldRPos = selfCube.RelativePosition;
        Vector2 cubeOldRScl = selfCube.RelativeScale;

        this.isExternal = isExternal;

        // Play sounds
        if (!isExternal)
        {
            if (selfCube.IsPlayer) SoundsManager.Instance.PlayUniqueSFX(movementAudioID, -1, Random.Range(0.75f, 1.0f));
        }
        else
        {
            Vector2 oldParentRPos = Relativity.PRPosToAChild(cubeOldRPos, cubeOldRScl);
            Vector2 oldParentRScl = Relativity.PRSclToAChild(cubeOldRScl);

            Vector2 newParentRPos = Relativity.PRPosToAChild(startRPos, startRScl);
            Vector2 newParentRScl = Relativity.PRSclToAChild(startRScl);

            Vector2 oldParentRSclToNewParent = Relativity.SRSclFromSameParent(newParentRScl, oldParentRScl);

            if (oldParentRSclToNewParent.x < 1 && oldParentRSclToNewParent.y < 1)
            {
                SoundsManager.Instance.PlayUniqueSFX(zoomOutAudioID);
            }
            else if (oldParentRSclToNewParent.x > 1 && oldParentRSclToNewParent.y > 1)
            {
                SoundsManager.Instance.PlayUniqueSFX(zoomInAudioID, -1);
            }
            else SoundsManager.Instance.PlayUniqueSFX(noZoomAudioID, -1);
        }

        movingCoroutine = StartCoroutine(MovingCoroutine(startRPos, startRScl, endRPos, endRScl, targetTime));

        if (debugMovement) Debug.Log($"Cube {selfCube.name} moves {IsMoving}");

        return targetTime;
    }

    public void StopMoving()
    {
        if (movingCoroutine != null) StopCoroutine(movingCoroutine);
        movingCoroutine = null;

        isExternal = false;
        selfCube.PreviousParents.Clear();
        selfCube.PreviousParents.Add(new(LayerDirections.In, selfCube.Parent)); // Update current parent after moving
        if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.GameplayInputs.ContinueUsingMovementInputs();
        onMoveEnd.Invoke();
    }

    protected IEnumerator MovingCoroutine(Vector2 startRPos, Vector2 startRScl, Vector2 endRPos, Vector2 endRScl, float time)
    {
        Cube.CubeProperties applyProperties = selfCube.GetCubeProperties();
        Cube.CubeProperties currentProperties = selfCube.GetCubeProperties();
        if (time > 0)
        {
            float elapsedTime = -1;
            var previousRPos = selfCube.RelativePosition;
            var previousRScl = selfCube.RelativeScale;

            selfCube.RelativePosition = startRPos;
            selfCube.RelativeScale = startRScl;
            if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.GameplayInputs.StopUsingMovementInputs();

            onMoveStart.Invoke();
            movingProgress = 0;

            var previousEndRPos = endRPos;
            var previousEndRScl = endRScl;

            // Modify based on external parent
            ContainerCube externalParent = null;

            if (isExternal)
            {
                externalParent = selfCube.PreviousParents[0].Cube;
                //applyProperties.CopyProperties(currentProperties);
                Cube.PreviousParentDetails currentCube = selfCube.PreviousParents[0];
                for (int i = 0; i < selfCube.PreviousParents.Count; i++)
                {
                    if (selfCube.PreviousParents[i].Cube == null) continue;

                    // Find external parent
                    if (selfCube.PreviousParents[i].Direction == LayerDirections.In)
                    {
                        if (externalParent == null && i > 0)
                        {
                            externalParent = selfCube.PreviousParents[i - 1].Cube;
                            if (applyProperties == null) applyProperties = selfCube.GetEmptyProperties();
                            applyProperties.CopyProperties(currentProperties);
                        }
                        if (i > 0)
                        {
                            selfCube.PreviousParents[i].Cube.ModifyChildCubeEnter(currentProperties);
                        }
                    }
                    else
                    {
                        if (selfCube.PreviousParents[i].Direction == LayerDirections.Out && i > 0)
                        {
                            selfCube.PreviousParents[i - 1].Cube.ModifyChildCubeExit(currentProperties);
                            selfCube.PreviousParents[i].Cube.ModifyChildCubeInnerEnter(currentProperties);
                        }
                        externalParent = null;
                        //applyProperties = null;
                    }
                }
            }
            //Debug.Log("External parent to apply to " + selfCube.name + " is " + externalParent);
            if (externalParent != null)
            {
                //Debug.Log("Apply property: " + applyProperties.IsHorizFlipped + " to " + selfCube.name);
                selfCube.ApplyNewProperties(applyProperties);
            }
            else
            {
                selfCube.ApplyNewProperties(currentProperties);
            }

            var historyManager = HistoryManager.Instance;
            if (historyManager != null)
            {
                // Add new record for its new details
                var targetProperties = selfCube.GetEmptyProperties();
                targetProperties.CopyProperties(currentProperties);
                targetProperties.RelativePosition = endRPos;
                targetProperties.RelativeScale = endRScl;
                historyManager.RecordNewEvent(selfCube,
                    targetProperties
                    );
            }

            while (elapsedTime < time)
            {
                if (elapsedTime < 0) elapsedTime = 0;
                else elapsedTime += Time.deltaTime;

                if (elapsedTime > time) elapsedTime = time;

                // Select a parent to render it as external cube
                if (isExternal)
                {
                    externalParent = selfCube.PreviousParents[0].Cube;
                    Rect initParentRect = new(0, 0, 1, 1);
                    Rect externalParentRect = initParentRect;
                    Rect finalParentRect = initParentRect;
                    Cube.PreviousParentDetails currentCube = selfCube.PreviousParents[0];
                    for (int i = 0; i < selfCube.PreviousParents.Count; i++)
                    {
                        if (selfCube.PreviousParents[i].Cube == null) continue;

                        // Remove external cubes from all parent -> reassign to the correct parent later
                        if (selfCube.PreviousParents[i].Cube.ExternalCubes.Contains(selfCube))
                        {
                            selfCube.PreviousParents[i].Cube.ExternalCubes.Remove(selfCube);
                        }

                        // Conditions to hide or unhide itself when it is either the enter / exit cube from a parent
                        if (selfCube.PreviousParents[i].Cube.AlterEnterCube == selfCube || selfCube.PreviousParents[i].Cube.ExitCube == selfCube)
                        {
                            if (i > 0 && selfCube.PreviousParents[i - 1].Direction == LayerDirections.None)
                            {
                                selfCube.PreviousParents[i].Cube.HideAlterEnterCube = false;
                                selfCube.PreviousParents[i].Cube.HideExitCube = false;
                            }
                            else
                            {
                                selfCube.PreviousParents[i].Cube.HideAlterEnterCube = true;
                                selfCube.PreviousParents[i].Cube.HideExitCube = true;
                            }
                        }

                        // Find external parent
                        if (selfCube.PreviousParents[i].Direction == LayerDirections.In)
                        {
                            if (externalParent == null && i > 0)
                            {
                                externalParent = selfCube.PreviousParents[i - 1].Cube;
                                externalParentRect = finalParentRect;
                            }
                        }
                        else
                        {
                            externalParent = null;
                        }

                        // Calculate final parent rect
                        if (i > 0)
                        {
                            if (selfCube.PreviousParents[i].Direction == CubeMovement.LayerDirections.Out)
                            {
                                Vector2 oldTargetRectPos = finalParentRect.position;
                                var rScl = (currentCube.UseDefaultValues) ? currentCube.Cube.RelativeScale : currentCube.RelativeScale;
                                var rPos = (currentCube.UseDefaultValues) ? currentCube.Cube.RelativePosition : currentCube.RelativePosition;
                                var isHorizFlipped = (currentCube.UseDefaultValues) ? currentCube.Cube.IsHorizFlipped : currentCube.IsHorizFlipped;
                                finalParentRect = Relativity.PRectFromCRect(finalParentRect, rScl, rPos);
                                if (isHorizFlipped)
                                {
                                    finalParentRect.width = -finalParentRect.width;
                                    finalParentRect.x = oldTargetRectPos.x - (finalParentRect.position.x - oldTargetRectPos.x);
                                }
                            }
                            else
                            {
                                var rScl = (selfCube.PreviousParents[i].UseDefaultValues) ? selfCube.PreviousParents[i].Cube.RelativeScale : selfCube.PreviousParents[i].RelativeScale;
                                var rPos = (selfCube.PreviousParents[i].UseDefaultValues) ? selfCube.PreviousParents[i].Cube.RelativePosition : selfCube.PreviousParents[i].RelativePosition;
                                var isHorizFlipped = (selfCube.PreviousParents[i].UseDefaultValues) ? selfCube.PreviousParents[i].Cube.IsHorizFlipped : selfCube.PreviousParents[i].IsHorizFlipped;
                                //Debug.Log("RScl: " + rScl);
                                finalParentRect = Relativity.CRectFromPRect(finalParentRect, rScl, rPos);
                                if (isHorizFlipped)
                                {
                                    finalParentRect.width = -finalParentRect.width;
                                }
                            }
                            //Debug.Log($"Cube: {selfCube.PreviousParents[i].Cube} rect: {finalParentRect}");
                            currentCube = selfCube.PreviousParents[i];
                        }
                    }


                    if (externalParent != null)
                    {
                        // Calculate correct rPos, rScl, . . .
                        var realStartPos = Relativity.CRealPosFromCRPos(initParentRect, previousRPos);
                        startRPos = Relativity.CRPosFromCRealPos(externalParentRect, realStartPos);
                        var realEndPos = Relativity.CRealPosFromCRPos(finalParentRect, previousEndRPos);
                        endRPos = Relativity.CRPosFromCRealPos(externalParentRect, realEndPos);
                        //Debug.Log("Cube: " + selfCube.name);
                        //Debug.Log($"exter parent: {externalParent.name}, exter rect: {externalParentRect}, final rect: {finalParentRect}");
                        //Debug.Log($"real start: {realStartPos}, startPos: {startRPos}, real end: {realEndPos}, end pos: {endRPos}");

                        startRScl = new(Mathf.Abs(previousRScl.x * (initParentRect.width / externalParentRect.width)), Mathf.Abs(previousRScl.y * (initParentRect.width / externalParentRect.height)));
                        endRScl = new(Mathf.Abs(previousEndRScl.x * (finalParentRect.width / externalParentRect.width)), Mathf.Abs(previousEndRScl.y * (finalParentRect.height / externalParentRect.height)));

                        //Debug.Log("External cube: " + externalParent.name + " with " + externalParent.ExternalCubes.Count);
                        if (!externalParent.ExternalCubes.Contains(selfCube)) externalParent.ExternalCubes.Add(selfCube);
                    }
                    else
                    {
                        //Debug.Log("Final parent rect: " + finalParentRect + " of " + selfCube.Parent.name + " with " + selfCube.Parent.ExternalCubes.Count);
                        if (!selfCube.Parent.ExternalCubes.Contains(selfCube)) selfCube.Parent.ExternalCubes.Add(selfCube);
                    }
                }

                movingProgress = elapsedTime / time;

                selfCube.RelativePosition = Vector2.Lerp(startRPos, endRPos, elapsedTime / time);
                selfCube.RelativeScale = Vector2.Lerp(startRScl, endRScl, elapsedTime / time);

                yield return null;
            }
        }

        movingCoroutine = null;
        isExternal = false;
        coolDownTimer = coolDownTime;
        selfCube.PreviousParents.Clear();
        selfCube.PreviousParents.Add(new(LayerDirections.In, selfCube.Parent)); // Update current parent after moving
        var selfCubePosition = selfCube.Parent.ChildGrid.FindChild((cubeDetail) => cubeDetail.Cube == selfCube);
        selfCube.ApplyNewProperties(currentProperties);
        selfCube.RelativePosition = Relativity.RPosFromGridTile(selfCube.Parent.Tiling.y, selfCube.Parent.Tiling.x, selfCubePosition.x, selfCubePosition.y);
        selfCube.RelativeScale = new(1.0f / selfCube.Parent.Tiling.y, 1.0f / selfCube.Parent.Tiling.x);
        if (selfCube.IsPlayer && MainCamera.Instance != null)
        {
            MainCamera.Instance.SetNewTargetCube(selfCube.Parent);
            MainCamera.Instance.StopTransition();
            MainCamera.Instance.FocusOnTargetCube();
        }
        if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.GameplayInputs.ContinueUsingMovementInputs();
        onMoveEnd.Invoke();
    }

    // Helper functions
    public static Vector2Int ConvertGridDirectionToGridOffset(GridDirections input)
    {
        switch (input)
        {
            case GridDirections.Up:
                return new Vector2Int(-1, 0);
            case GridDirections.Down:
                return new Vector2Int(1, 0);
            case GridDirections.Left:
                return new Vector2Int(0, -1);
            case GridDirections.Right:
                return new Vector2Int(0, 1);
            default:
                return Vector2Int.zero;
        }
    }

    public static Vector2 ConvertGridDirectionToPositionOffset(GridDirections direction)
    {
        switch (direction)
        {
            case GridDirections.Up:
                return Vector2.up;
            case GridDirections.Down:
                return Vector2.down;
            case GridDirections.Left:
                return Vector2.left;
            case GridDirections.Right:
                return Vector2.right;
            default:
                return Vector2.zero;
        }
    }

    public static GridDirections FlipMovementInput(GridDirections input, bool horizontal)
    {
        switch (input)
        {
            case GridDirections.Up:
                return (horizontal) ? GridDirections.Up : GridDirections.Down;
            case GridDirections.Down:
                return (horizontal) ? GridDirections.Down : GridDirections.Up;
            case GridDirections.Left:
                return (horizontal) ? GridDirections.Right : GridDirections.Left;
            case GridDirections.Right:
                return (horizontal) ? GridDirections.Left : GridDirections.Right;
            default:
                return GridDirections.None;
        }
    }
    public static GridDirections ReverseMovementInput(GridDirections input)
    {
        switch (input)
        {
            case GridDirections.Up:
                return GridDirections.Down;
            case GridDirections.Down:
                return GridDirections.Up;
            case GridDirections.Left:
                return GridDirections.Right;
            case GridDirections.Right:
                return GridDirections.Left;
            default:
                return GridDirections.None;
        }
    }
    public static bool CheckOppositeMovementInputs(GridDirections input1, GridDirections input2)
    {
        if (input1 == GridDirections.None || input2 == GridDirections.None) return false;
        return (input1 == GridDirections.Up && input2 == GridDirections.Down) ||
               (input1 == GridDirections.Down && input2 == GridDirections.Up) ||
               (input1 == GridDirections.Left && input2 == GridDirections.Right) ||
               (input1 == GridDirections.Right && input2 == GridDirections.Left);
    }
}
