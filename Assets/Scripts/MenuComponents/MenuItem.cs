using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuItem : MonoBehaviour
{
    [SerializeField] Image cursor;
    [SerializeField] ColorPalette cursorColorPalette;
    [SerializeField] ColorPalette.ColorEnum cursorColor = ColorPalette.ColorEnum.Red;
    [SerializeField] UnityEvent onTriggerEvent = new();

    public UnityEvent OnTriggerEvent { get => onTriggerEvent; }

    private void OnEnable()
    {
        if (cursor != null && cursorColorPalette != null)
        {
            var cursorColor = cursorColorPalette.GetColor(this.cursorColor);
            cursor.color = cursorColor;
        }
    }

    public void OnSelected(bool isSelected)
    {
        if (cursor != null) cursor.gameObject.SetActive(isSelected);
    }
}
