using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField] bool isBlockage = true;
    [SerializeField] Vector2 relativeSize = new(1, 1);
    [SerializeField] Vector2 relativePosition = new(0, 0);

    [SerializeField] private bool isMoving = false;

    public Vector2 RelativeSize { get => relativeSize; set => relativeSize = value; }
    public Vector2 RelativePosition { get => relativePosition; set => relativePosition = value; }
    public bool IsMoving { get => isMoving; }
    public bool IsBlockage { get => isBlockage; }

    private void Start() 
    {
        
    }

    public virtual void Draw(Rect position)
    {

    }
}
