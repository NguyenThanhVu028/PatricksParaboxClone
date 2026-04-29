using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    public enum PlayModes { Normal, Loop, Repeat, Random};
    [SerializeField] List<AudioClip> playList = new();
    [SerializeField] PlayModes playMode = PlayModes.Normal;

    private AudioSource mainAudioSource;
    private bool isPlaying = false;
    private int currentIndex = 0;

    public bool IsPlaying { get => isPlaying; }
    public float Volume
    {
        get => mainAudioSource.volume;
        set => mainAudioSource.volume = value;
    }

    private void Awake()
    {
        mainAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isPlaying && !mainAudioSource.isPlaying)
        {
            if (playList.Count == 0)
            {
                isPlaying = false; return;
            }
            if (currentIndex >= playList.Count) currentIndex = playList.Count - 1;

            switch (playMode)
            {
                case PlayModes.Random:
                    int nextIndex = -1;
                    do
                    {
                        nextIndex = Random.Range(0, playList.Count);
                    } while (playList.Count > 1 && nextIndex == currentIndex);
                    currentIndex = nextIndex;
                    break;
                case PlayModes.Loop:
                    currentIndex++; currentIndex %= playList.Count;
                    break;
            }
            PlayCurrent();
        }
    }

    public void SetPlayMode(PlayModes playMode) => this.playMode = playMode;
    public void AddToPlayList(AudioClip clip)
    {
        if (clip == null) return;
        playList.Remove(clip);
        playList.Add(clip);
    }
    public void RemoveFromPlayList(AudioClip clip)
    {
        playList.Remove(clip);
    }
    public void ClearPlayList()
    {
        playList.Clear();
        mainAudioSource.Stop();
    }
    public void Play()
    {
        if (playList.Count == 0) return;

        if (playMode == PlayModes.Random) currentIndex = Random.Range(0, playList.Count);
        else
        {
            if (currentIndex < 0) currentIndex = 0;
            if (currentIndex >= playList.Count) currentIndex = playList.Count - 1;
        }

        PlayCurrent();
    }
    public void PlayAt(int index)
    {
        if (index < 0 || index >= playList.Count) return;
        currentIndex = index;

        PlayCurrent();
    }
    public void PlayCurrent()
    {
        mainAudioSource.clip = playList[currentIndex];
        mainAudioSource.Play();
        isPlaying = true;
    }
    public void PlayNext()
    {
        mainAudioSource.Stop();
        isPlaying = true;
    }
    public void Stop()
    {
        mainAudioSource.Stop();
        isPlaying = false;
    }
}
