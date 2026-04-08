using UnityEngine;
using UnityEngine.Events;

public class NormalButton : TriggerButton
{
    private bool hasAssignedToGameManager = false;

    private void Update()
    {
        if (parent != null && parent.ChildGrid != null && parent.ChildGrid.CheckValidGridPosition(positionInParent.x, positionInParent.y))
        {
            var targetCube = parent.ChildCubes[positionInParent.x, positionInParent.y];
            if (targetCube != null)
            {
                if (!targetCube.IsPlayer && !(targetCube is WallCube) && !isActivated)
                {
                    OnActivated();
                }
                if ((targetCube.IsPlayer || targetCube is WallCube) && isActivated)
                {
                    OnDeactivated();
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

    protected override void OnActivated()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AnnounceNormalButtonActivated();
        base.OnActivated();
    }

    protected override void OnDeactivated()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AnnounceNormalButtonDeactivated();
        base.OnDeactivated();
    }

    public override void Init()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AssignNormalButton();
            hasAssignedToGameManager = true;
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
}
