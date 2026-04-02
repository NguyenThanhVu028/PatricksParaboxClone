using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Cube))]
public class CubeMovement : MonoBehaviour
{
    //[SerializeField] bool movable = true;
    [SerializeField] float normalMoveTime = 0.1f; // Used when player simply moving form one point to another
    [SerializeField] float specialMoveTime = 0.5f; // Used when player enters a cube, transforms, . . .
    [SerializeField] float coolDownTime = 0.075f;

    [SerializeField] protected Cube selfCube;
    protected PlayerInputsManager playerMovementInputsManager;
    protected Coroutine movingCoroutine = null;
    //protected Vector2 targetRPos = Vector2.zero;
    //protected Vector2 previousRPos = Vector2.zero;
    //protected Vector2 targetRScl = Vector2.zero;
    //protected Vector2 previousRScl = Vector2.zero;
    protected float coolDownTimer = 0f;
    protected UnityEvent onMoveStart = new();
    protected UnityEvent onMoveEnd = new();

    public bool Movable { get { return selfCube != null && selfCube.CubeType != Cube.CubeTypes.Static && selfCube.CubeType != Cube.CubeTypes.Empty; } }
    public bool IsMoving { get => movingCoroutine != null; }
    public bool IsCoolingDown { get => (coolDownTimer > 0); }
    public float NormalMoveTime { get => normalMoveTime; }
    public float SpecialMoveTime { get => specialMoveTime; }
    public Cube SelfCube { get => selfCube; }
    public UnityEvent OnMoveStart { get => onMoveStart; }
    public UnityEvent OnMoveEnd { get => onMoveEnd; }

    private void OnEnable()
    {
        selfCube = GetComponent<Cube>();
        //selfCube.OnInit.AddListener(OnSelfCubeInit);
        playerMovementInputsManager = PlayerInputsManager.Instance;
    }
    private void Update()
    {
        if (selfCube.IsPlayer &&
            selfCube.Parent != null && 
            playerMovementInputsManager != null &&
            !IsMoving && !IsCoolingDown)
        {
            PlayerInputsManager.MovementInputs movementInput = playerMovementInputsManager.GetLatestMovementInput();
            if (movementInput != PlayerInputsManager.MovementInputs.None)
            {
                Vector2Int currentPosInGrid = Relativity.GridPosFromRPos(selfCube.Parent.Tiling, selfCube.Parent.Tiling, selfCube.RelativePosition);
                Vector2Int targetPosInGrid = currentPosInGrid + PlayerInputsManager.ConvertMovementInputToGridDirection(movementInput);
                if (selfCube.Parent.RequestToMove(selfCube.RelativePosition, selfCube.RelativeScale, selfCube, movementInput, targetPosInGrid.x, targetPosInGrid.y) == 0)
                {
                    // If fail to move
                    coolDownTimer = coolDownTime;
                }
            }
        }
        
        if (coolDownTimer > 0) coolDownTimer -= Time.deltaTime;
        if (coolDownTimer < 0) coolDownTimer = 0;
    }


    // Called by parent cube
    public float StartMoving(Vector2 startRPos, Vector2 startRScl, Vector2 endRPos, Vector2 endRScl, float targetTime)
    {
        if (IsMoving || (selfCube.IsPlayer && IsCoolingDown)) return 0;

        // Record history event
        Debug.Log($"Cube {selfCube.name} moves");
        if (HistoryManager.Instance != null)
        {
            HistoryManager.Instance.RecordNewEvent(selfCube, selfCube.PreviousParent, selfCube.RelativePosition, selfCube.RelativeScale);
            if (selfCube.IsPlayer) HistoryManager.Instance.ArchiveHistoryRecord(); // Player will archive the record
        }

        // If this cube is a player -> update camera
        UpdateCamera(selfCube, startRPos, startRScl, targetTime);

        movingCoroutine = StartCoroutine(MovingCoroutine(startRPos, startRScl, endRPos, endRScl, targetTime));

        return targetTime;
    }

    public void StopMoving()
    {
        if (movingCoroutine != null) StopCoroutine(movingCoroutine);
        movingCoroutine = null;

        if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.ContinueUsingMovementInputs();
        onMoveEnd.Invoke();
    }

    private void UpdateCamera(Cube player, Vector2 playerStartRPos, Vector2 playerStartRScl, float targetTime)
    {
        Debug.Log("Update cam");
        if (!player.IsPlayer) return;
        if (MainCamera.Instance != null)
        {
            Vector2 oldParentRPos = Relativity.PRPosToAChild(player.RelativePosition, player.RelativeScale);
            Vector2 oldParentRScl = Relativity.PRSclToAChild(player.RelativeScale);

            Vector2 newParentRPos = Relativity.PRPosToAChild(playerStartRPos, playerStartRScl);
            Vector2 newParentRScl = Relativity.PRSclToAChild(playerStartRScl);

            Vector2 oldParentRPosToNewParent = Relativity.SRPosFromSameParent(newParentRPos, newParentRScl, oldParentRPos);
            Vector2 oldParentRSclToNewParent = Relativity.SRSclFromSameParent(newParentRScl, oldParentRScl);

            player.RelativePosition = playerStartRPos;
            player.RelativeScale = playerStartRScl;

            MainCamera.Instance.ChangeTarget(oldParentRPosToNewParent, oldParentRSclToNewParent, player.Parent, targetTime);
        }
    }

    protected IEnumerator MovingCoroutine(Vector2 startRPos, Vector2 startRScl, Vector2 endRPos, Vector2 endRScl, float time)
    {
        float elapsedTime = -1;
        selfCube.RelativePosition = startRPos;
        selfCube.RelativeScale = startRScl;
        if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.StopUsingMovementInputs();
        onMoveStart.Invoke();
        while (elapsedTime < time)
        {
            if (elapsedTime < 0) elapsedTime = 0;
            else elapsedTime += Time.deltaTime;

            selfCube.RelativePosition = Vector2.Lerp(startRPos, endRPos, elapsedTime / time);
            selfCube.RelativeScale = Vector2.Lerp(startRScl, endRScl, elapsedTime / time);

            yield return null;
        }
        movingCoroutine = null;
        coolDownTimer = coolDownTime;
        selfCube.PreviousParent = selfCube.Parent; // Update current parent after moving
        if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.ContinueUsingMovementInputs();
        onMoveEnd.Invoke();
    }
}
