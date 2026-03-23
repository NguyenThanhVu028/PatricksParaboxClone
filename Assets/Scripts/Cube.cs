using UnityEngine;

public class Cube : MonoBehaviour
{
    [Header("General Info")]
    [SerializeField] protected bool isPlayer = false;
    [SerializeField] protected bool isBlockage = true; // If false, a cube can move into it right away without trying to push or enter it
    [SerializeField] protected ColorPalette colorPalette;
    [SerializeField] protected ColorPalette.ColorEnum color;
    [Header("Rendering")]
    [SerializeField] protected int minPixelToRender = 2; // Don't render if the rectangle size is smaller than this value
    [SerializeField] protected Material cubeMat;
    [SerializeField] protected Mesh cubeMesh;
    [Header("Cube stats")]
    [SerializeField] protected Cube parent;
    [SerializeField] protected Vector2 relativeSize = new(1, 1);
    [SerializeField] protected Vector2 relativePosition = new(0, 0);

    public Vector2 RelativeSize { get => relativeSize; set => relativeSize = value; }
    public Vector2 RelativePosition { get => relativePosition; set => relativePosition = value; }
    public bool IsBlockage { get => isBlockage; }
    public Cube Parent { get => parent; set => parent = value; }

    public virtual void Draw(Rect position)
    {

    }
}
