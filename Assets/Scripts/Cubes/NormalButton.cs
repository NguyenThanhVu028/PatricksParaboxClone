using UnityEngine;
using UnityEngine.Events;

public class NormalButton : Cube
{
    [SerializeField] Texture2D buttonTexture;
    [SerializeField] UnityEvent onButtonActivated = new();
    [SerializeField] UnityEvent onButtonDeactivated = new();

    private Vector2Int positionInParent = Vector2Int.zero;
    private bool isActivated = false;
    private bool hasAssignedToGameManager = false;

    private void Update()
    {
        if (parent != null && parent.CubesGrid != null && parent.CheckValidGridPosition(positionInParent.x, positionInParent.y))
        {
            var targetCube = parent.CubesGrid[positionInParent.x, positionInParent.y];
            if (targetCube != null)
            {
                if (!targetCube.IsPlayer && !(targetCube is WallCube) && !isActivated)
                {
                    isActivated = true;
                    GameManager.Instance.AnnounceNormalButtonActivated();
                    onButtonActivated.Invoke();
                }
                if ((targetCube.IsPlayer || targetCube is WallCube) && isActivated)
                {
                    isActivated = false;
                    GameManager.Instance.AnnounceNormalButtonDeactivated();
                    onButtonDeactivated.Invoke();
                }
            }
            else
            {
                if (isActivated)
                {
                    isActivated = false;
                    GameManager.Instance.AnnounceNormalButtonDeactivated();
                    onButtonDeactivated.Invoke();
                }
            }
        }
    }

    public override void Init()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AssignNormalButton();
            hasAssignedToGameManager = true;
        }

        if (parent != null)
        {
            positionInParent = Relativity.GridPosFromRPos(parent.Tiling, parent.Tiling, relativePosition);
        }

        base.Init();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && hasAssignedToGameManager)
        {
            GameManager.Instance.UnAssignNormalButton();
            hasAssignedToGameManager = false;
        }
    }

    protected override void DrawCube(Rect position, float depth)
    {
        Color color = new Color(1, 1, 1, 0.5f);
        CustomTextureRenderer2D.RenderMesh(cubeMesh, cubeMat, buttonTexture, color, position.position, position.size, depth);
    }
}
