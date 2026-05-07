using UnityEngine;
using UnityEngine.Events;

public class TriggerButton : Cube
{
    [SerializeField] protected UnityEvent onButtonActivated = new();
    [SerializeField] protected UnityEvent onButtonDeactivated = new();

    public UnityEvent OnButtonActivated { get => onButtonActivated; }
    public UnityEvent OnButtonDeactivated { get => onButtonDeactivated; }

    protected Vector2Int positionInParent = Vector2Int.zero;
    protected bool isActivated = false;

    public override void Init()
    {
        if (parent != null)
        {
            positionInParent = Relativity.GridPosFromRPos(parent.Tiling.y, parent.Tiling.x, relativePosition);
        }

        base.Init();
    }

    private void Update()
    {
        if (parent != null && parent.ChildGrid != null && parent.ChildGrid.CheckValidGridPosition(positionInParent.x, positionInParent.y))
        {
            var targetCube = parent.ChildCubes[positionInParent.x, positionInParent.y];

            if (targetCube.Cube != null)
            {
                if (!isActivated)
                {
                    OnActivated();
                }
            }
            else
            {
                if (isActivated)
                {
                    OnDeactivated();
                }
            }
        }
    }

    protected virtual void OnActivated()
    {
        Debug.Log("On activated");
        isActivated = true;
        onButtonActivated.Invoke();
    }

    protected virtual void OnDeactivated()
    {
        Debug.Log("On deactivated");
        isActivated = false;
        onButtonDeactivated.Invoke();
    }

    public override void DrawCube(Rect position, int priority, float depth, float exposure, Rect? scissorRect)
    {
        if (defaultTexture != null && defaultTexture.GetTexture() != null)
        {
            Color cubeColor = Color.white;
            if (colorPalette != null) cubeColor = colorPalette.GetColor(this.cubeColor);
            CustomTextureRenderer2D.RenderMesh(cubeMesh, normalMat, defaultTexture.GetTexture(), cubeColor, exposure, position.position, position.size, depth, scissorRect, priority);
        }
    }
}
