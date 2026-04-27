using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static Cube;

[RequireComponent(typeof(Cube))]
public class CubeMovement : MonoBehaviour
{
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
            PlayerInputsManager.MovementInputs movementInput = PlayerInputsManager.Instance.GameplayInputs.GetLatestMovementInput();
            if (MainCamera.Instance != null && MainCamera.Instance.RenderMode == MainCamera.MainCameraRenderMode.SingleCube)
            {
                if (MainCamera.Instance.TargetCubeRect.size.x < 0) movementInput = PlayerInputsManager.FlipMovementInput(movementInput, true);
            }
            if (movementInput != PlayerInputsManager.MovementInputs.None)
            {
                var targetTime = selfCube.Parent.RequestToMove(selfCube.RelativePosition, selfCube.RelativeScale, selfCube, movementInput);
                if (targetTime > 0)
                {
                    // Update camera trasition
                    if (selfCube.IsPlayer && MainCamera.Instance != null)
                    {
                        ZoomingTransition zoomingTransition = new(
                            selfCube.PreviousParents,
                            targetTime
                            );

                        MainCamera.Instance.PlayTransition(zoomingTransition);
                    }
                }
                if (selfCube.IsPlayer)
                {
                    HistoryManager historyManager = HistoryManager.Instance;
                    if (historyManager != null) historyManager.NormalArchiveHistoryRecord(); // Player will archive the record
                }
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
            if (currentRecord != null)
            {
                currentRecord.AddHistoryEvent(selfCube, (selfCube.PreviousParents.Count > 0) ? selfCube.PreviousParents[0].Cube : null, selfCube.RelativePosition, selfCube.RelativeScale, selfCube.IsHorizFlipped, selfCube.IsPlayer);
            }
        }

        for (int i = 1; i < selfCube.PreviousParents.Count; i++)
        {
            if (selfCube.PreviousParents[i].Direction == PreviousParentDetails.Directions.In)
            {
                selfCube.PreviousParents[i].Cube.ModifyChildCubeEnter(selfCube);
            }
            else if (selfCube.PreviousParents[i].Direction == PreviousParentDetails.Directions.Out && i >= 1)
            {
                selfCube.PreviousParents[i - 1].Cube.ModifyChildCubeExit(selfCube);
            }
        }
        if (selfCube.PreviousParents.Count > 0) selfCube.Parent = selfCube.PreviousParents[selfCube.PreviousParents.Count - 1].Cube;
        
        if (historyManager != null)
        {
            // Add new record for its new details
            historyManager.RecordNewEvent(selfCube, selfCube.Parent, endRPos, endRScl, selfCube.IsHorizFlipped, selfCube.IsPlayer);
        }

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
        selfCube.PreviousParents.Add(new(PreviousParentDetails.Directions.Out, selfCube.Parent)); // Update current parent after moving
        if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.GameplayInputs.ContinueUsingMovementInputs();
        onMoveEnd.Invoke();
    }

    protected IEnumerator MovingCoroutine(Vector2 startRPos, Vector2 startRScl, Vector2 endRPos, Vector2 endRScl, float time)
    {
        if (time > 0)
        {
            float elapsedTime = -1;
            selfCube.RelativePosition = startRPos;
            selfCube.RelativeScale = startRScl;
            if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.GameplayInputs.StopUsingMovementInputs();

            onMoveStart.Invoke();
            while (elapsedTime < time)
            {
                if (elapsedTime < 0) elapsedTime = 0;
                else elapsedTime += Time.deltaTime;

                selfCube.RelativePosition = Vector2.Lerp(startRPos, endRPos, elapsedTime / time);
                selfCube.RelativeScale = Vector2.Lerp(startRScl, endRScl, elapsedTime / time);

                yield return null;
            }
        }

        movingCoroutine = null;
        isExternal = false;
        coolDownTimer = coolDownTime;
        selfCube.PreviousParents.Clear();
        selfCube.PreviousParents.Add(new(PreviousParentDetails.Directions.Out, selfCube.Parent)); // Update current parent after moving
        selfCube.RelativePosition = endRPos;
        selfCube.RelativeScale = endRScl;
        if (selfCube.IsPlayer && MainCamera.Instance != null)
        {
            MainCamera.Instance.SetNewTargetCube(selfCube.Parent);
            MainCamera.Instance.StopTransition();
            MainCamera.Instance.FocusOnTargetCube();
        }
        if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.GameplayInputs.ContinueUsingMovementInputs();
        onMoveEnd.Invoke();
    }
}
