using UnityEngine;

public class Cube : MonoBehaviour
{
    [Header("General Info")]
    [SerializeField] protected bool isPlayer = false;
    [SerializeField] protected bool isBlockage = true;
    [SerializeField] protected ColorPalette colorPalette;
    [SerializeField] protected ColorPalette.ColorEnum color;
    [Header("Rendering")]
    [SerializeField] protected Material tileMat;
    [SerializeField] protected Mesh tileMesh;
    [Header("Cube stats")]
    [SerializeField] protected Vector2 relativeSize = new(1, 1);
    [SerializeField] protected Vector2 relativePosition = new(0, 0);
    [SerializeField] protected bool isMoving = false;

    public Vector2 RelativeSize { get => relativeSize; set => relativeSize = value; }
    public Vector2 RelativePosition { get => relativePosition; set => relativePosition = value; }
    public bool IsMoving { get => isMoving; }
    public bool IsBlockage { get => isBlockage; }

    public virtual void Draw(Rect position)
    {

    }
}
