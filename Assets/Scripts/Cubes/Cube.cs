using System.Collections;
using UnityEngine;

public class Cube : MonoBehaviour
{
    // Normal: cube that can be pushed -> Try both pushing, entering and possessing
    // Static: cube that can't be pushed -> Try entering and possessing
    // Empty: cube that other cubes can go through -> Ignore
    public enum CubeTypes { Normal, Static, Empty }

    [Header("General Info")]
    [HideInInspector]
    [SerializeField] protected bool isPlayer = false;
    [HideInInspector]
    [SerializeField] protected bool canBePlayer = false;

    [SerializeField] protected CubeTypes cubeType;
    [SerializeField] protected ColorPalette colorPalette;
    [SerializeField] protected ColorPalette.ColorEnum cubeColor;
    [SerializeField] protected bool needInstantiating = true;
    [Header("Rendering")]
    [SerializeField] protected int minPixelToRender = 2; // Don't render if the render rectangle size in pixel is smaller than this value
    [SerializeField] protected Material cubeMat;
    [SerializeField] protected Mesh cubeMesh;
    [Header("Cube stats")]
    [SerializeField] protected EnterableCube parent;
    [SerializeField] protected Vector2 relativeScale = new(1, 1);
    [SerializeField] protected Vector2 relativePosition = new(0, 0);
    [Header("Other cube settings")]
    [SerializeField] protected float possessingTime = 1.0f;

    public bool IsPlayer { get => isPlayer; set => isPlayer = value; }
    public bool CanBePlayer { get => canBePlayer; set => canBePlayer = value; }
    public CubeTypes CubeType { get => cubeType; }
    public ColorPalette.ColorEnum CubeColor { get => cubeColor; set => cubeColor = value; }
    public bool NeedInstantiating { get => needInstantiating; }
    public EnterableCube Parent { get => parent; set => parent = value; }
    public Vector2 RelativeScale { get => relativeScale; set => relativeScale = value; }
    public Vector2 RelativePosition { get => relativePosition; set => relativePosition = value; }

    private void Start()
    {
        if (isPlayer && MainCamera.Instance != null)
        {
            MainCamera.Instance.SetNewTargetCube(parent);
        }
    }

    public virtual void Draw(Rect position)
    {
        //if (!CustomTextureRenderer2D.CheckVisibility(position.position, position.size)) return;

        Vector2 rectSizeInPixel = CustomTextureRenderer2D.ConvertScaleToPixel(position.size);
        if (rectSizeInPixel.x < minPixelToRender || rectSizeInPixel.y < minPixelToRender) return; // Don't draw if the requested rectangle is too small (To avoid infinite rendering)

        DrawCube(position);
        DrawPlayerFace(position);
        DrawSurfaceEffects(position);
    }
    protected virtual void DrawCube(Rect position)
    {

    }
    protected virtual void DrawPlayerFace(Rect position)
    {
        // Get player face texture

    }
    protected virtual void DrawSurfaceEffects(Rect position)
    {

    }

    public bool StartPossessing(Cube targetCube)
    {
        if (!isPlayer) return false;
        if (targetCube == null) return false;
        if (targetCube.IsPlayer) return false;
        if (!targetCube.CanBePlayer) return false;
        StartCoroutine(PossessingCoroutine(targetCube));
        return true;
    }

    private IEnumerator PossessingCoroutine(Cube targetCube)
    {
        IsPlayer = false;
        if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.StopUsingMovementInputs();
        // Play animation or effect for possessing here
        yield return new WaitForSeconds(possessingTime);
        // Stop the animation or effect here
        targetCube.IsPlayer = true;
        if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.ContinueUsingMovementInputs();
    }
}
