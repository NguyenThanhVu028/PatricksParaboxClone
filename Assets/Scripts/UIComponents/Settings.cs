using UnityEngine;

public class Settings : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] SliderMenuItem moveTimeSlider;
    [SerializeField] SliderMenuItem enterTimeSlider;
    [Header("Sound")]
    [SerializeField] SliderMenuItem musicVolumeSlider;
    [SerializeField] SliderMenuItem sfxVolumeSlider;

    [SerializeField] SaveAndLoadManager.GameData gameData;
    private void Start()
    {
        if (SaveAndLoadManager.Instance != null)
        {
            gameData = SaveAndLoadManager.Instance.GeneralGameData;
        }

        UpdateSettingValues();

        if (moveTimeSlider != null) moveTimeSlider.OnValueChanged.AddListener(OnMoveTimeChanged);
        if (enterTimeSlider != null) enterTimeSlider.OnValueChanged.AddListener(OnEnterTimeChanged);

        if (musicVolumeSlider != null) musicVolumeSlider.OnValueChanged.AddListener(OnMusicVolumnChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.OnValueChanged.AddListener(OnSFXVolumnChanged);
    }

    private void UpdateSettingValues()
    {
        if (moveTimeSlider != null) moveTimeSlider.CurrentValue = gameData.MoveTime;
        if (enterTimeSlider != null) enterTimeSlider.CurrentValue = gameData.EnterTime;

        if (musicVolumeSlider != null) musicVolumeSlider.CurrentValue = gameData.MusicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.CurrentValue = gameData.SFXVolume;
    }

    public void OnMoveTimeChanged(float value)
    {
        if (gameData != null) gameData.MoveTime = value;
    }
    public void OnEnterTimeChanged(float value)
    {
        if (gameData != null) gameData.EnterTime = value;
    }
    public void OnMusicVolumnChanged(float value)
    {
        if (gameData != null) gameData.MusicVolume = value;
    }
    public void OnSFXVolumnChanged(float value)
    {
        if (gameData != null) gameData.SFXVolume = value;
    }
}
