using UnityEngine;

public class ChangeActionMap : MonoBehaviour
{
    [SerializeField] PlayerInputsManager.ActionMaps actionMap;
    [SerializeField] bool onEnable = true;
    [SerializeField] bool onDisable = true;

    private void OnEnable()
    {
        if (onEnable && PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.PushActionMap(actionMap); 
    }

    private void OnDisable()
    {
        Debug.Log("Try pop");
        if (onDisable && PlayerInputsManager.Instance != null) PlayerInputsManager.Instance.PopActionMap(actionMap);
    }
}
