using UnityEngine;

[RequireComponent(typeof(Cube))]
public class CubeMovement : MonoBehaviour
{
    [SerializeField] bool movable = true;
    //[SerializeField] float cubeSize = 1f;
    //[SerializeField] float defaultSpeed = 12f;
    //[SerializeField] float enterExitSpeed = 5f;
    //[SerializeField] float moveUnit = 1f;
    [SerializeField] float normalMoveTime = 0.5f; // Used when player simply moving form one point to another
    [SerializeField] float specialMoveTime = 1.0f; // Used when player enters a cube, transforms, . . .
    //[SerializeField] float minDistance = 0.01f;
    [SerializeField] float coolDownTime = 0.075f;

    //protected float speed = 0f;
    protected Coroutine movingCoroutine = null;
    protected Vector2 targetRPos = Vector2.zero;
    protected Vector2 previousRPos = Vector2.zero;
    protected Vector2 targetRScl = Vector2.zero;
    protected Vector2 previousRScl = Vector2.zero;
    protected float coolDownTimer = 0f;

    public bool IsMoving
    {
        get 
        {
            return (movingCoroutine != null);
        }
    }
    public bool IsCoolingDown { get => coolDownTimer > 0; }

}
