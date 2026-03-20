using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    //[SerializeField] bool allowMovement = true;
    //[SerializeField] float cubeSize = 1f;
    //[SerializeField] float defaultSpeed = 12f;
    //[SerializeField] float enterExitSpeed = 5f;
    //[SerializeField] float moveUnit = 1f;
    //[SerializeField] float minDistance = 0.01f;
    //[SerializeField] float coolDownTime = 0.075f;
    //[SerializeField] LayerMask obstacleLayer;

    //protected float speed = 0f;
    //protected bool isMoving = false;
    //protected Vector2 targetPosition = Vector2.zero;
    //protected Vector2 previousPosition = Vector2.zero;
    //protected float coolDownTimer = 0f;

    //public bool IsMoving { get => isMoving; }
    //public bool IsCoolingDown { get => coolDownTimer > 0; }
    //public float EnterExitSpeed { get => enterExitSpeed; }
    //public LayerMask ObstacleLayer { get => obstacleLayer; }

    //protected void Update()
    //{
    //    if (isMoving && allowMovement)
    //    {
    //        Vector2 moveDirection = targetPosition - (Vector2)transform.position;
    //        transform.Translate(moveDirection.normalized * speed * Time.deltaTime);

    //        if (CheckTargetReached())
    //        {
    //            transform.position = targetPosition;
    //            isMoving = false;
    //            coolDownTimer = coolDownTime; // Start cooldown after the cube has reached the target
    //            return;
    //        }

    //        previousPosition = transform.position;
    //    }

    //    if (coolDownTimer > 0)
    //    {
    //        coolDownTimer -= Time.deltaTime;
    //        if (coolDownTimer < 0) coolDownTimer = 0;
    //    }
    //}

    //private bool CheckTargetReached()
    //{
    //    // Check if the cube has reached the minimun distance from the target or has passed it.
    //    return Vector2.Distance(transform.position, targetPosition) < minDistance ||
    //        ((previousPosition.x - targetPosition.x) * (transform.position.x - targetPosition.x) < 0) ||
    //        ((previousPosition.y - targetPosition.y) * (transform.position.y - targetPosition.y) < 0);
    //}

    //private RaycastHit2D[] CheckHit(Vector2 direction)
    //{
    //    return Physics2D.RaycastAll((Vector2)transform.position, direction.normalized, moveUnit/*, obstacleLayer*/);
    //}

    //public bool Move(Vector2 direction, float speed = -1)
    //{
    //    if (!allowMovement) return false;
    //    if (direction == Vector2.zero) return false;
    //    if (isMoving || coolDownTimer > 0) return false;

    //    //Debug.Log("Cube: " + name + " is trying to move in direction: " + direction);

    //    if (speed <= 0) this.speed = defaultSpeed;
    //    else this.speed = speed;

    //    coolDownTimer = coolDownTime;

    //    /* 
    //     * Check hit:
    //     * - If there is an obstacle, the cube cannot move.
    //     * - If there is another cube, try to push it. If failed, try to enter it. If both failed, the cube cannot move.
    //     * - Move normally if there is nothing or only ignorable objects are detected in the way.
    //     */
    //    var rayCastHits = CheckHit(direction);
    //    foreach(var hit in rayCastHits)
    //    {
    //        if (hit.collider.gameObject == gameObject) continue; // Skip the cube itself

    //        // Check if the hit object is an obstacle that cannot be moved.
    //        if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
    //        {
    //            return false;
    //        }

    //        // Check if the hit object is another cube
    //        if (hit.collider.GetComponent<Cube>() != null)
    //        {
    //            // Try to push the other cube.
    //            var cubeMovement = hit.collider.GetComponent<CubeMovement>();
    //            if (cubeMovement != null)
    //            {
    //                if (cubeMovement.IsMoving || cubeMovement.IsCoolingDown) return false;
    //                if (cubeMovement.Move(direction, this.speed))
    //                {
    //                    NormalMove(direction); 
    //                    return true;            
    //                }
    //            }

    //            // Try to enter the other cube.
    //            var otherMiniCube = hit.collider.GetComponent<EnterableCube>();
    //            if (otherMiniCube != null)
    //            {
    //                if (otherMiniCube.Enter(this, direction))
    //                {
    //                    return true;
    //                }
    //            }

    //            return false;
    //        }
    //    }

    //    NormalMove(direction);
    //    return true;
    //}

    //private void NormalMove(Vector2 direction)
    //{
    //    previousPosition = transform.position;
    //    targetPosition = (Vector2)transform.position + direction.normalized * moveUnit;
    //    isMoving = true;
    //}
}
