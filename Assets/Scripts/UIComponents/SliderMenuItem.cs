using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderMenuItem : MenuItem
{
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI valueText;
    [SerializeField] Button increaseButton;
    [SerializeField] Button decreaseButton;
    [SerializeField] float incremental = 20f;
    [SerializeField] float minValue = 20f;
    [SerializeField] float maxValue = 100f;
    [SerializeField] float currentValue = 0f;

    [SerializeField] UnityEvent<float> onValueChanged = new();

    public float CurrentValue 
    { 
        get => currentValue; 
        set
        {
            if (value > maxValue) value = maxValue;
            if (value < minValue) value = minValue;
            currentValue = value;
            if (slider != null) slider.value = Mathf.Clamp01((currentValue - minValue) / (maxValue - minValue));
            if (valueText != null) valueText.text = currentValue.ToString();
        }
    }
    public float MaxValue
    {
        get => maxValue;
        set
        {
            if (value <= minValue) return;
            if (slider != null) slider.value = Mathf.Clamp01((currentValue - minValue) / (maxValue - minValue));
            maxValue = value;
        }
    }
    public float MinValue
    {
        get => minValue;
        set
        {
            if (value >= maxValue) return;
            if (slider != null) slider.value = Mathf.Clamp01((currentValue - minValue) / (maxValue - minValue));
            minValue = value;
        }
    }

    public UnityEvent<float> OnValueChanged { get => onValueChanged; }

    private void Start()
    {
        if (increaseButton != null) increaseButton.onClick.AddListener(OnIncrease);
        if (decreaseButton != null) decreaseButton.onClick.AddListener(OnDecrease);
    }

    public override void OnSelected(bool isSelected)
    {
        base.OnSelected(isSelected);
        Debug.Log("On selected");

        if (PlayerInputsManager.Instance == null) return;
        if (isSelected)
        {
            PlayerInputsManager.Instance.UIInputs.OnNavigateRightEvent.AddListener(OnIncrease);
            PlayerInputsManager.Instance.UIInputs.OnNavigateLeftEvent.AddListener(OnDecrease);
        }
        else
        {
            PlayerInputsManager.Instance.UIInputs.OnNavigateRightEvent.RemoveListener(OnIncrease);
            PlayerInputsManager.Instance.UIInputs.OnNavigateLeftEvent.RemoveListener(OnDecrease);
        }

    }

    public void OnIncrease()
    {
        CurrentValue += incremental;
        onValueChanged.Invoke(currentValue);
    }

    public void OnDecrease()
    {
        CurrentValue -= incremental;
        onValueChanged.Invoke(currentValue);
    }
}
