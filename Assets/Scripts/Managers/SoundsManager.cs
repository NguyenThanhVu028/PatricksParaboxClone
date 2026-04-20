using System;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    public enum SoundTypes { Music, SFX};
    private static SoundsManager instance = null;
    public static SoundsManager Instance { get => instance; }

    [Range(0f, 1f)]
    [SerializeField] float maxVolume = 0.5f;

    [SerializeField] List<AudioDetails> audioClips = new();

    [SerializeField] AudioSource audioSourcePrefab;
    [SerializeField] AudioPlayer audioPlayerPrefab;

    [SerializeField] AudioSource sfxAudioSource = new();
    [SerializeField] List<AudioSource> uniqueSFXAudioSources = new();
    [SerializeField] List<AudioPlayer> uniqueMusicAudioPlayers = new();
    [SerializeField] AudioPlayer backgroundMusicAudioPlayer;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else
        {
            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (SaveAndLoadManager.GeneralGameData == null) return;
        var gameData = SaveAndLoadManager.GeneralGameData;
        if (gameData == null) return;

        if (sfxAudioSource != null) sfxAudioSource.volume = (gameData.SFXVolume / 100f) * maxVolume;
        foreach (var audioSource in uniqueSFXAudioSources)
        {
            if (audioSource == null) continue;
            audioSource.volume = (gameData.SFXVolume / 100f) * maxVolume;
        }

        if (backgroundMusicAudioPlayer != null) backgroundMusicAudioPlayer.Volume = (gameData.MusicVolume / 100f) * maxVolume;
        foreach (var audioSource in uniqueMusicAudioPlayers)
        {
            if (audioSource == null) continue;
            audioSource.Volume = (gameData.MusicVolume / 100f) * maxVolume;
        }

        gameData.OnMusicVolumeChanged.AddListener(OnMusicVolumeChanged);
        gameData.OnSFXVolumeChanged.AddListener(OnSFXVolumeChanged);
    }

    private void OnMusicVolumeChanged(float newValue)
    {
        if (backgroundMusicAudioPlayer != null)  backgroundMusicAudioPlayer.Volume = (newValue / 100f) * maxVolume;
        foreach (var audioSource in uniqueMusicAudioPlayers)
        {
            if (audioSource == null) continue;
            audioSource.Volume = (newValue / 100f) * maxVolume;
        }
    }
    private void OnSFXVolumeChanged(float newValue)
    {
        if (sfxAudioSource != null) sfxAudioSource.volume = (newValue / 100f) * maxVolume;
        foreach (var audioSource in uniqueSFXAudioSources)
        {
            if (audioSource == null) continue;
            audioSource.volume = (newValue / 100f) * maxVolume;
        }
    }

    public AudioSource PlaySFX(string clipID)
    {
        if (sfxAudioSource == null) return null;
        var clip = GetAudio(clipID);
        if (clip == null) return null;
        sfxAudioSource.PlayOneShot(clip, sfxAudioSource.volume);
        return sfxAudioSource;
    }
    public AudioSource GetUniqueSFX()
    {
        foreach (var audioSource in uniqueSFXAudioSources)
        {
            if (audioSource == null) continue;
            if (audioSource.isPlaying) continue;
            return audioSource;
        }

        AudioSource newAudioSource;
        if (audioSourcePrefab != null)
        {
            newAudioSource = Instantiate(audioSourcePrefab);
        }
        else
        {
            GameObject newAudioSourceObj = new GameObject("SFX", typeof(AudioSource));
            newAudioSource = newAudioSourceObj.GetComponent<AudioSource>();
        }
        if (newAudioSource == null) return null;
        if (SaveAndLoadManager.GeneralGameData != null) newAudioSource.volume = (SaveAndLoadManager.GeneralGameData.SFXVolume / 100f) * maxVolume;
        newAudioSource.transform.SetParent(transform);
        uniqueSFXAudioSources.Add(newAudioSource);
        return newAudioSource;
    }
    public AudioSource PlayUniqueSFX(string clipID, float targetTime = -1, float pitch = 1)
    {
        AudioSource targetAudioSource = GetUniqueSFX();

        var clip = GetAudio(clipID);
        if (clip == null) return null;

        targetAudioSource.clip = clip;
        targetAudioSource.Play();
        if (targetTime > 0) targetAudioSource.pitch = (clip.length / targetTime);
        else targetAudioSource.pitch = pitch;
        return targetAudioSource;
    }
    public AudioPlayer GetUniqueMusicPlayer()
    {
        foreach (var audioSource in uniqueMusicAudioPlayers)
        {
            if (audioSource == null) continue;
            if (audioSource.IsPlaying) continue;
            return audioSource;
        }

        AudioPlayer newAudioPlayer;
        if (audioPlayerPrefab != null)
        {
            newAudioPlayer = Instantiate(audioPlayerPrefab);
        }
        else
        {
            GameObject newAudioSourceObj = new GameObject("SFX", typeof(AudioPlayer));
            newAudioPlayer = newAudioSourceObj.GetComponent<AudioPlayer>();
        }
        if (SaveAndLoadManager.GeneralGameData != null) newAudioPlayer.Volume = (SaveAndLoadManager.GeneralGameData.MusicVolume / 100f) * maxVolume;
        newAudioPlayer.transform.SetParent(transform);
        uniqueMusicAudioPlayers.Add(newAudioPlayer);
        return newAudioPlayer;
    }

    public AudioPlayer GetBackgroundMusicPlayer() => backgroundMusicAudioPlayer;

    public AudioClip GetAudio(string id)
    {
        foreach(var audioClip in audioClips)
        {
            if (audioClip == null || audioClip.AudioClip == null) continue;
            if (audioClip.ID == id) return audioClip.AudioClip;
        }
        return null;
    }

    [Serializable]
    public class AudioDetails
    {
        [SerializeField] string audioID;
        [SerializeField] AudioClip audioClip;

        public string ID { get => audioID; }
        public AudioClip AudioClip { get => audioClip; }
    }
}
