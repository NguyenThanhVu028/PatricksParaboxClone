using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Cube : MonoBehaviour
{
    // Normal: cube that can be pushed -> Try both pushing, entering and possessing
    // Static: cube that can't be pushed -> Try entering and possessing
    // Empty: cube that other cubes can go through -> Ignore
    public enum CubeTypes { Normal, Static, Empty }

    [Header("General Info")]
    //[HideInInspector]
    [SerializeField] protected bool isPlayer = false;
    //[HideInInspector]
    [SerializeField] protected bool canBePlayer = false;

    [SerializeField] protected CubeTypes cubeType;
    [SerializeField] protected ColorPalette colorPalette;
    [SerializeField] protected ColorPalette.ColorEnum cubeColor;
    [SerializeField] protected bool needInstantiating = true;
    [Header("Rendering")]
    [SerializeField] protected int minPixelToRender = 2; // Don't render if the render rectangle size in pixel is smaller than this value
    [SerializeField] protected Material cubeMat;
    [SerializeField] protected Mesh cubeMesh;
    // Protorype
    [SerializeField] protected Texture playerFaceTexture;
    [SerializeField] protected Texture possessableFaceTexture;
    [Header("Cube stats")]
    [SerializeField] protected EnterableCube parent;
    [SerializeField] protected Vector2 relativeScale = new(1, 1);
    [SerializeField] protected Vector2 relativePosition = new(0, 0);
    [Header("Other cube settings")]
    [SerializeField] protected float possessingTime = 0.5f;

    protected UnityEvent onParentChanged = new();

    public bool IsPlayer { get => isPlayer; set => isPlayer = value; }
    public bool CanBePlayer { get => canBePlayer; set => canBePlayer = value; }
    public CubeTypes CubeType { get => cubeType; }
    public ColorPalette.ColorEnum CubeColor { get => cubeColor; set => cubeColor = value; }
    public bool NeedInstantiating { get => needInstantiating; }
    public EnterableCube Parent { get => parent; set { parent = value; onParentChanged.Invoke(); } }
    public Vector2 RelativeScale { get => relativeScale; set => relativeScale = value; }
    public Vector2 RelativePosition { get => relativePosition; set => relativePosition = value; }

    public virtual void Init()
    {
        if (isPlayer && MainCamera.Instance != null)
        {
            MainCamera.Instance.SetNewTargetCube(parent);
        }
    }

    public virtual void Draw(Rect position, float depth = 0)
    {
        //if (!CustomTextureRenderer2D.CheckVisibility(position.position, position.size)) return;

        Vector2 rectSizeInPixel = CustomTextureRenderer2D.ConvertScaleToPixel(position.size);
        if (rectSizeInPixel.x < minPixelToRender || rectSizeInPixel.y < minPixelToRender) return; // Don't draw if the requested rectangle is too small (To avoid infinite rendering)

        DrawCube(position, depth);
        DrawPlayerFace(position, depth);
        DrawSurfaceEffects(position, depth);
    }
    protected virtual void DrawCube(Rect position, float depth)
    {

    }
    protected virtual void DrawPlayerFace(Rect position, float depth)
    {
        // Get player face texture
        var cubeColor = Color.white;
        if (colorPalette != null) cubeColor = colorPalette.GetColor(this.cubeColor);
        if (IsPlayer && playerFaceTexture != null)
        {
            CustomTextureRenderer2D.RenderMesh(cubeMesh, cubeMat, playerFaceTexture, cubeColor, position.position, position.size, depth - 0.2f);
        }
        else if (!IsPlayer && canBePlayer && possessableFaceTexture != null)
        {
            CustomTextureRenderer2D.RenderMesh(cubeMesh, cubeMat, possessableFaceTexture, cubeColor, position.position, position.size, depth - 0.2f);
        }
    }
    protected virtual void DrawSurfaceEffects(Rect position, float depth)
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
