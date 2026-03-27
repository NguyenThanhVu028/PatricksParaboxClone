using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Cube))]
public class CubeMovement : MonoBehaviour
{
    [SerializeField] bool movable = true;
    [SerializeField] float normalMoveTime = 0.1f; // Used when player simply moving form one point to another
    [SerializeField] float specialMoveTime = 0.5f; // Used when player enters a cube, transforms, . . .
    [SerializeField] float coolDownTime = 0.075f;

    protected Cube selfCube;
    protected PlayerMovementInputsManager playerMovementInputsManager;
    protected Coroutine movingCoroutine = null;
    protected Vector2 targetRPos = Vector2.zero;
    protected Vector2 previousRPos = Vector2.zero;
    protected Vector2 targetRScl = Vector2.zero;
    protected Vector2 previousRScl = Vector2.zero;
    protected float coolDownTimer = 0f;
    //protected bool isTryingToMove = false;

    public bool IsMoving { get => (movingCoroutine != null); }
    //public bool IsTryingToMove { get => isTryingToMove; set => isTryingToMove = value; }
    public bool IsCoolingDown { get => (coolDownTimer > 0); }
    public float NormalMoveTime { get => normalMoveTime; }
    public float SpecialMoveTime { get => specialMoveTime; }
    public Cube SelfCube { get => selfCube; }
    private void Start()
    {
        selfCube = GetComponent<Cube>();
        playerMovementInputsManager = PlayerMovementInputsManager.Instance;
    }
    private void Update()
    {
        if (selfCube.IsPlayer &&
            selfCube.Parent != null && 
            playerMovementInputsManager != null &&
            !IsMoving && !IsCoolingDown)
        {
            Vector2 movementInput = playerMovementInputsManager.GetLatestMovementInput();
            if (movementInput != Vector2.zero)
            {
                Vector2Int currentPosInGrid = Relativity.GridPosFromRPos(selfCube.Parent.Tiling, selfCube.Parent.Tiling, selfCube.RelativePosition);
                Debug.Log("Current pos in grid: " + currentPosInGrid);
                Vector2Int targetPosInGrid = new Vector2Int(currentPosInGrid.x - Mathf.RoundToInt(movementInput.normalized.y), currentPosInGrid.y + Mathf.RoundToInt(movementInput.normalized.x));
                Debug.Log("Target pos in grid: " + targetPosInGrid);
                selfCube.Parent.CubesGrid[currentPosInGrid.x, currentPosInGrid.y] = null;
                if (selfCube.Parent.RequestToMove(selfCube.RelativePosition, selfCube.RelativeScale, selfCube, targetPosInGrid.x, targetPosInGrid.y) == 0)
                {
                    // If fail to move
                    selfCube.Parent.CubesGrid[currentPosInGrid.x, currentPosInGrid.y] = selfCube;
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
        if (IsMoving || IsCoolingDown) return 0;
        movingCoroutine = StartCoroutine(MovingCoroutine(startRPos, startRScl, endRPos, endRScl, targetTime));
        return targetTime;
    }

    protected IEnumerator MovingCoroutine(Vector2 startRPos, Vector2 startRScl, Vector2 endRPos, Vector2 endRScl, float time)
    {
        float elapsedTime = -1;
        selfCube.RelativePosition = startRPos;
        selfCube.RelativeScale = startRScl;
        while(elapsedTime < time)
        {
            if (elapsedTime < 0) elapsedTime = 0;
            else elapsedTime += Time.deltaTime;

            selfCube.RelativePosition = Vector2.Lerp(startRPos, endRPos, elapsedTime / time);
            selfCube.RelativeScale = Vector2.Lerp(startRScl, endRScl, elapsedTime / time);

            yield return null;
        }
        movingCoroutine = null;
        coolDownTimer = coolDownTime;
    }
}
