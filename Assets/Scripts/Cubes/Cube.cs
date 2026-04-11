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
    [SerializeField] protected bool isPlayer = false;
    [SerializeField] protected bool canBePlayer = false;
    [SerializeField] protected CubeTypes cubeType;
    [SerializeField] protected ColorPalette colorPalette;
    [SerializeField] protected ColorPalette.ColorEnum cubeColor;
    [SerializeField] protected bool needInstantiating = true;
    [Header("Rendering")]
    [SerializeField] protected CustomTexture defaultTexture;
    [SerializeField] protected int minPixelToRender = 2; // Don't render if the render rectangle size in pixel is smaller than this value
    [SerializeField] protected Material normalMat;
    [SerializeField] protected Material outlineMat;
    [SerializeField] protected Mesh cubeMesh;
    [SerializeField] protected CustomTexture possessableFaceTexture;
    [Header("Cube stats")]
    [SerializeField] protected ContainerCube parent;
    [SerializeField] protected ContainerCube previousParent; // Record self cube's previous parent to record history
    [SerializeField] protected Vector2 relativeScale = new(1, 1);
    [SerializeField] protected Vector2 relativePosition = new(0, 0);
    [Header("Other cube settings")]
    [SerializeField] protected float possessingTime = 0.5f;
    [SerializeField] protected UnityEvent onInit = new();

    protected UnityEvent onParentChanged = new();
    protected CustomTexture faceTexture;
    protected CustomTexture surfaceEffectsAnimation;
    protected MaterialPropertyBlock materialPropertyBlock;

    public bool IsPlayer { get => isPlayer; set => isPlayer = value; }
    public bool CanBePlayer { get => canBePlayer; set => canBePlayer = value; }
    public CubeTypes CubeType { get => cubeType; }
    public ColorPalette ColorPalette { get => colorPalette; }
    public ColorPalette.ColorEnum CubeColor { get => cubeColor; set => cubeColor = value; }
    public Color RealCubeColor
    {
        get
        {
            if (colorPalette == null) return Color.white;
            return colorPalette.GetColor(cubeColor);
        }
    }
    public bool NeedInstantiating { get => needInstantiating; }
    public Material NormalMat { get => normalMat; }
    public ContainerCube Parent { get => parent; set { parent = value; onParentChanged.Invoke(); } }
    public ContainerCube PreviousParent { get => previousParent; set => previousParent = value; }
    public Vector2 RelativeScale { get => relativeScale; set => relativeScale = value; }
    public Vector2 RelativePosition { get => relativePosition; set => relativePosition = value; }
    public UnityEvent OnInit { get => onInit; }

    public virtual void Init()
    {
        if (isPlayer && MainCamera.Instance != null)
        {
            MainCamera.Instance.SetNewTargetCube(parent);
        }

        previousParent = parent;

        AnimationsManager animationsManager = AnimationsManager.Instance;
        if (animationsManager != null && SaveAndLoadManager.Instance != null)
        {
            var gameData = SaveAndLoadManager.Instance.GeneralGameData;
            faceTexture = animationsManager.GetPlayerFaceTextureAnimation(gameData.LastUsedFaceAniID);
        }

        onInit.Invoke();
    }

    public virtual void Draw(Rect position, float depth = 0)
    {
        Vector2 rectSizeInPixel = CustomTextureRenderer2D.ConvertScaleToPixel(position.size);
        if (rectSizeInPixel.x < minPixelToRender || rectSizeInPixel.y < minPixelToRender) return; // Don't draw if the requested rectangle is too small (To avoid infinite rendering)

        DrawCube(position, depth);
        DrawPlayerFace(position, depth - 0.05f);
        DrawSurfaceEffects(position, depth - 0.075f);
    }
    protected virtual void DrawCube(Rect position, float depth)
    {
        if (defaultTexture != null && defaultTexture.GetTexture() != null)
        {
            CustomTextureRenderer2D.RenderMesh(cubeMesh, outlineMat, defaultTexture.GetTexture(), RealCubeColor, position.position, position.size, depth);
        }
    }
    protected virtual void DrawPlayerFace(Rect position, float depth)
    {
        // Get player face texture
        if (IsPlayer && faceTexture != null)
        {
            CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, faceTexture.GetTexture(), RealCubeColor, position.position, position.size, depth);
        }
        else if (!IsPlayer && canBePlayer && possessableFaceTexture != null)
        {
            CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, possessableFaceTexture.GetTexture(), RealCubeColor, position.position, position.size, depth);
        }
    }
    protected virtual void DrawSurfaceEffects(Rect position, float depth)
    {
        var cubeColor = Color.white;
        //if (colorPalette != null) cubeColor = colorPalette.GetColor(this.cubeColor);

        if (surfaceEffectsAnimation != null)
        {
            CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, surfaceEffectsAnimation.GetTexture(), cubeColor, position.position, position.size, depth);
        }
    }

    public void SetSurfaceEffects(string aniID)
    {
        AnimationsManager animationsManager = AnimationsManager.Instance;
        if (animationsManager != null)
            surfaceEffectsAnimation = animationsManager.GetNormalTextureAnimation(aniID);
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
        if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.GameplayInputs.StopUsingMovementInputs();
        // Play animation or effect for possessing here
        yield return new WaitForSeconds(possessingTime);
        // Stop the animation or effect here
        targetCube.IsPlayer = true;
        if (PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.GameplayInputs.ContinueUsingMovementInputs();
    }
}
