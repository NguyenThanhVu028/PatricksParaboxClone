using UnityEngine;
using UnityEngine.Events;

public class TriggerMenuItem : MenuItem
{
    [SerializeField] UnityEvent onTriggerEvent = new();

    public UnityEvent OnTriggerEvent { get => onTriggerEvent; }

    public override void OnSelected(bool isSelected)
    {
        base.OnSelected(isSelected);

        if (isSelected)
        {
            PlayerInputsManager.Instance.UIInputs.OnAcceptEvent.AddListener(OnTrigger);
        }
        else PlayerInputsManager.Instance.UIInputs.OnAcceptEvent.RemoveListener(OnTrigger);
    }

    public void OnTrigger()
    {
        onTriggerEvent.Invoke();
    }
}
