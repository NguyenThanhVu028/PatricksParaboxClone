using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Cube))]
public class CubeMovement : MonoBehaviour
{
    [SerializeField] bool movable = true;
    [SerializeField] float normalMoveTime = 0.5f; // Used when player simply moving form one point to another
    [SerializeField] float specialMoveTime = 1.0f; // Used when player enters a cube, transforms, . . .
    [SerializeField] float coolDownTime = 0.075f;

    protected Cube targetCube;
    protected PlayerMovementInputsManager playerMovementInputsManager;
    protected Coroutine movingCoroutine = null;
    protected Vector2 targetRPos = Vector2.zero;
    protected Vector2 previousRPos = Vector2.zero;
    protected Vector2 targetRScl = Vector2.zero;
    protected Vector2 previousRScl = Vector2.zero;
    protected float coolDownTimer = 0f;

    public bool IsMoving { get => (movingCoroutine != null); }
    public bool IsCoolingDown { get => (coolDownTimer > 0); }
    public Cube TargetCube { get => targetCube; }
    private void Start()
    {
        targetCube = GetComponent<Cube>();
        playerMovementInputsManager = PlayerMovementInputsManager.Instance;
    }
    private void Update()
    {
        if (targetCube != null && targetCube.IsPlayer && playerMovementInputsManager != null)
        {
            Vector2 movementInput = playerMovementInputsManager.GetLatestMovementInput();
            Push(movementInput); // Push itself
        }
    }

    // Called by other cubes or itself, it will return moving time of this object
    public float Push(Vector2 direction)
    {
        if (targetCube == null || targetCube.Parent == null) return 0;
        return targetCube.Parent.RequestToMove(this, direction);
    }

    // Called by parent cube
    public void StartMoving(Rect targetPosition, bool normalMove)
    {
        if (IsMoving || IsCoolingDown) return;
        float targetTime = (normalMove) ? normalMoveTime : specialMoveTime;
        movingCoroutine = StartCoroutine(MovingCoroutine(targetPosition, targetTime));
    }

    protected IEnumerator MovingCoroutine(Rect targetPosition, float time)
    {
        float elapsedTime = 0;
        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        movingCoroutine = null;
        coolDownTimer = coolDownTime;
    }
}
