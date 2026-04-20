using UnityEngine;
using UnityEngine.Events;

public class PlayerButton : TriggerButton
{
    [SerializeField] string buttonActivatedAudioID = "Button Activated";
    [SerializeField] string buttonDeactivatedAudioID = "Button Deactivated";
    private bool hasAssignedToGameManager = false;

    private void Update()
    {
        if (parent != null && parent.ChildGrid != null && parent.ChildGrid.CheckValidGridPosition(positionInParent.x, positionInParent.y))
        {
            var targetCube = parent.ChildCubes[positionInParent.x, positionInParent.y];
            if (targetCube != null)
            {
                if (targetCube.IsPlayer && !isActivated)
                {
                    OnActivated();
                }
                if (!targetCube.IsPlayer && isActivated)
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

        if (GameManager.Instance != null) GameManager.Instance.AnnouncePlayerButtonActivated();
        if (SoundsManager.Instance != null) SoundsManager.Instance.PlaySFX(buttonActivatedAudioID);
        base.OnActivated();
    }

    protected override void OnDeactivated()
    {
        if (GameManager.Instance != null) GameManager.Instance.AnnouncePlayerButtonDeactivated();
        if (SoundsManager.Instance != null) SoundsManager.Instance.PlaySFX(buttonDeactivatedAudioID);
        base.OnDeactivated();
    }

    public override void Init()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AssignPlayerButton();
            hasAssignedToGameManager = true;
        }

        base.Init();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && hasAssignedToGameManager)
        {
            GameManager.Instance.UnAssignPlayerButton();
            hasAssignedToGameManager = false;
        }
    }
}
