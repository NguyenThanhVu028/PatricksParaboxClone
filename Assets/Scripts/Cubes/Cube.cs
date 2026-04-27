using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Cube : MonoBehaviour
{
    // Normal: cube that can be pushed -> Try both pushing, entering and possessing
    // Static: cube that can't be pushed -> Try entering and possessing
    // Empty: cube that other cubes can go through -> Ignore
    public enum CubeTypes { Normal, Static, Empty }
    public const float playerFaceDepthOffset = -0.11f;
    public const float surfaceEffectsDepthOffset = -0.12f;

    [Header("General Info")]
    [SerializeField] protected bool isPlayer = false;
    [SerializeField] protected bool canBePlayer = false;
    [SerializeField] protected CubeTypes cubeType;
    [SerializeField] protected ColorPalette colorPalette;
    [SerializeField] protected ColorPalette.ColorEnum cubeColor;
    [SerializeField] protected bool needInstantiating = true;
    [Header("Rendering")]
    [SerializeField] protected CustomTexture defaultTexture;
    [SerializeField] protected bool isHorizFlipped = false;
    [SerializeField] protected int minPixelToRender = 2; // Don't render if the render rectangle size in pixel is smaller than this value
    [SerializeField] protected Material normalMat;
    [SerializeField] protected Material outlineMat;
    [SerializeField] protected Mesh cubeMesh;
    [SerializeField] protected CustomTexture possessableFaceTexture;
    [SerializeField] protected bool enableOcclusionCulling = true;
    [SerializeField] protected string mirrorEffectID = "Mirror";
    [Header("Cube stats")]
    [SerializeField] protected ContainerCube parent;
    [SerializeField] protected List<PreviousParentDetails> previousParents = new(); // Record self cube's previous parent to record history
    [SerializeField] protected Vector2 relativeScale = new(1, 1);
    [SerializeField] protected Vector2 relativePosition = new(0, 0);
    [Header("Other cube settings")]
    [SerializeField] protected float possessingTime = 0.5f;
    [SerializeField] protected UnityEvent onInit = new();
    [SerializeField] protected UnityEvent<Rect, float, float, Rect?> onFinishedDrawing = new();

    protected Action<ContainerCube, ContainerCube> onParentChanged;
    protected CustomTexture faceTexture;
    protected CustomTexture surfaceEffectsAnimation;
    protected MaterialPropertyBlock materialPropertyBlock;
    protected bool hasInit = false;

    // General Info
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
    
    // Rendering
    public bool IsHorizFlipped { get => isHorizFlipped; set => isHorizFlipped = value; }
    public Material NormalMat { get => normalMat; }
    public bool EnableOcclusionCulling { get => enableOcclusionCulling; set => enableOcclusionCulling = value; }

    // Cube Stats
    public ContainerCube Parent { get => parent; set { onParentChanged?.Invoke(parent, value); parent = value; } }
    public List<PreviousParentDetails> PreviousParents { get => previousParents; set => previousParents = value; }
    public Vector2 RelativeScale { get => relativeScale; set => relativeScale = value; }
    public Vector2 RelativePosition { get => relativePosition; set => relativePosition = value; }

    // Other Settings
    public UnityEvent OnInit { get => onInit; }
    public UnityEvent<Rect, float, float, Rect?> OnFinishedDrawing { get => onFinishedDrawing; }

    public virtual void Init()
    {
        if (isPlayer && MainCamera.Instance != null)
        {
            MainCamera.Instance.SetNewTargetCube(parent);
            MainCamera.Instance.FocusOnTargetCube();
        }

        previousParents.Clear();
        previousParents.Add(new(PreviousParentDetails.Directions.Out, parent));

        AnimationsManager animationsManager = AnimationsManager.Instance;
        if (animationsManager != null && SaveAndLoadManager.GeneralGameData != null)
        {
            var gameData = SaveAndLoadManager.GeneralGameData;
            faceTexture = animationsManager.GetPlayerFaceTextureAnimation(gameData.LastUsedFaceAniID);
        }

        onInit.Invoke();
    }

    public virtual void Draw(Rect position, float depth = 0, float exposure = 0, Rect? scissorRect = null)
    {
        if (!CustomTextureRenderer2D.CheckVisibility(position.position, position.size) && enableOcclusionCulling) return;

        Vector2 rectSizeInPixel = CustomTextureRenderer2D.ConvertScaleToPixel(position.size);
        if (rectSizeInPixel.x < minPixelToRender || rectSizeInPixel.y < minPixelToRender) return; // Don't draw if the requested rectangle is too small (To avoid infinite rendering)

        if (isHorizFlipped) position.width = - position.width;
        DrawCube(position, depth, exposure, scissorRect);
        DrawPlayerFace(position, depth + playerFaceDepthOffset, exposure, scissorRect);
        DrawSurfaceEffects(position, depth + surfaceEffectsDepthOffset, exposure, scissorRect);

        onFinishedDrawing.Invoke(position, depth, exposure, scissorRect);
    }
    public virtual void DrawCube(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        if (defaultTexture != null && defaultTexture.GetTexture() != null)
        {
            CustomTextureRenderer2D.RenderMesh(cubeMesh, outlineMat, defaultTexture.GetTexture(), RealCubeColor, exposure, position.position, position.size, depth, scissorRect);
        }
    }
    public virtual void DrawPlayerFace(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        // Get player face texture
        if (IsPlayer && faceTexture != null)
        {
            CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, faceTexture.GetTexture(), RealCubeColor, exposure, position.position, position.size, depth, scissorRect);
        }
        else if (!IsPlayer && canBePlayer && possessableFaceTexture != null)
        {
            CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, possessableFaceTexture.GetTexture(), RealCubeColor, exposure, position.position, position.size, depth, scissorRect);
        }
    }
    public virtual void DrawSurfaceEffects(Rect position, float depth, float exposure, Rect? scissorRect)
    {
        var cubeColor = Color.white;
        //if (colorPalette != null) cubeColor = colorPalette.GetColor(this.cubeColor);

        if (surfaceEffectsAnimation != null)
        {
            CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, surfaceEffectsAnimation.GetTexture(), cubeColor, exposure, position.position, position.size, depth, scissorRect);
        }
        if (isHorizFlipped)
        {
            if (AnimationsManager.Instance == null) return;
            var mirrorEffect = AnimationsManager.Instance.GetNormalTextureAnimation("Mirror");
            if (mirrorEffect != null) CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, mirrorEffect.GetTexture(), cubeColor, exposure, position.position, position.size, depth, scissorRect);
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
        // Update self cube status and history
        HistoryManager historyManager = HistoryManager.Instance;
        if (historyManager != null)
        {
            // Modify current record to save its previous details
            var currentRecord = historyManager.GetCurrentRecord();
            if (currentRecord != null)
            {
                currentRecord.AddHistoryEvent(this, (previousParents.Count > 0) ? previousParents[0].Cube : null, relativePosition, relativeScale, isHorizFlipped, true);
                currentRecord.AddHistoryEvent(targetCube, (targetCube.PreviousParents.Count > 0) ? targetCube.PreviousParents[0].Cube : null, targetCube.RelativePosition, targetCube.RelativeScale, targetCube.IsHorizFlipped, false);
            }
            historyManager.RecordNewEvent(this, (previousParents.Count > 0) ? previousParents[0].Cube : null, relativePosition, relativeScale, isHorizFlipped, false);
            historyManager.RecordNewEvent(targetCube, (targetCube.PreviousParents.Count > 0) ? targetCube.PreviousParents[0].Cube : null, targetCube.RelativePosition, targetCube.RelativeScale, targetCube.IsHorizFlipped, true);
        }
        if (MainCamera.Instance != null)
        {
            ZoomingTransition zoomingTransition = new(previousParents, possessingTime);
            MainCamera.Instance.PlayTransition(zoomingTransition);
        }
        if (historyManager != null) historyManager.NormalArchiveHistoryRecord();
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

    public void RemovePreviousParent(ContainerCube cube)
    {
        for(int i = previousParents.Count - 1; i >= 0; i--)
        {
            if (previousParents[i].Cube == cube)
            {
                previousParents.RemoveAt(i);
                return;
            }
        }
    }
    [Serializable]
    public struct PreviousParentDetails
    {
        public enum Directions { In, Out }
        private Directions direction;
        private ContainerCube cube;
        private Vector2 relativePosition;
        private Vector2 relativeScale;
        private bool isHorizFlipped;

        public Directions Direction { get => direction; set => direction = value; }
        public ContainerCube Cube { get => cube; set => cube = value; }
        public Vector2 RelativePosition
        {
            get
            {
                if (relativePosition == null) return cube.RelativePosition;
                return relativePosition;
            }
            set => relativePosition = value;
        }
        public Vector2 RelativeScale
        {
            get
            {
                if (relativeScale == null) return cube.RelativeScale;
                return relativeScale;
            }
            set => relativeScale = value;
        }
        public bool IsHorizFlipped
        {
            get => isHorizFlipped;
            set => isHorizFlipped = value;
        }
        public PreviousParentDetails(Directions direction, ContainerCube cube)
        {
            this.cube = cube;
            this.direction = direction;
            this.relativePosition = (cube != null) ? cube.RelativePosition : Vector2.zero;
            this.relativeScale = (cube != null) ? cube.RelativeScale : Vector2.zero;
            this.isHorizFlipped = (cube != null) ? cube.IsHorizFlipped : false;
        }

        public PreviousParentDetails(Directions direction, ContainerCube cube, Vector2 relativePosition, Vector2 relativeScale, bool isHorizFlipped) 
        { 
            this.cube = cube;
            this.direction = direction;
            this.relativePosition = relativePosition;
            this.relativeScale = relativeScale;
            this.isHorizFlipped = isHorizFlipped;
        }
    }
}
