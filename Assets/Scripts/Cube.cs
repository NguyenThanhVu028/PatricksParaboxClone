using UnityEngine;

public class Cube : MonoBehaviour
{
    [Header("General Info")]
    [SerializeField] bool isPlayer = false;
    [SerializeField] bool isBlockage = true;
    [SerializeField] ColorPalette colorPalette;
    [SerializeField] ColorPalette.ColorEnum color;

    [SerializeField] Vector2 relativeSize = new(1, 1);
    [SerializeField] Vector2 relativePosition = new(0, 0);

    [SerializeField] private bool isMoving = false;

    public Vector2 RelativeSize { get => relativeSize; set => relativeSize = value; }
    public Vector2 RelativePosition { get => relativePosition; set => relativePosition = value; }
    public bool IsMoving { get => isMoving; }
    public bool IsBlockage { get => isBlockage; }

    public virtual void Draw(Rect position)
    {

    }
}
