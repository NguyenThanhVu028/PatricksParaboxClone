using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomAnimatedTexture", menuName = "Scriptable Objects/CustomAnimatedTexture")]
public class CustomAnimatedTexture : CustomTexture
{
    [SerializeField] List<FrameDetails> frames = new();

    private bool isRunning = false;
    private float lastFrameTime = 0;
    private int currentFrameIndex = 0;
    [SerializeField] float totalDuration = 0;

    public string ID { get => name; }
    public List<FrameDetails> Frames { get => frames; }
    public float TotalDuration { get => totalDuration; set => totalDuration = value; }

    public void StartAnimation() 
    {
        Debug.Log("Start " + name);
        currentFrameIndex = 0;

        ContinueAnimation();
    }

    public void StopAnimation()
    {
        isRunning = false;
    }

    private void ContinueAnimation()
    {
        isRunning = true;
        lastFrameTime = Time.timeSinceLevelLoad;

        Debug.Log("Continue: " + lastFrameTime);

    }

    public override Texture GetTexture()
    {
        if (frames.Count == 0 || totalDuration == 0) return null;
        if (currentFrameIndex < 0) currentFrameIndex = 0;
        if (currentFrameIndex >= frames.Count) currentFrameIndex = frames.Count - 1;

        if (!isRunning) return frames[currentFrameIndex].Texture;

        int tempIndex = currentFrameIndex;
        float secondsElapsed = (Time.timeSinceLevelLoad - lastFrameTime) % totalDuration;
        //Debug.Log($"Name: {name}, last frame: {lastFrameTime}, current time: {Time.timeSinceLevelLoad}, seconds elapsed: {secondsElapsed}");
        do
        {
            if (secondsElapsed <= frames[tempIndex].Duration)
            {
                if (tempIndex != currentFrameIndex) lastFrameTime = Time.timeSinceLevelLoad - secondsElapsed;
                currentFrameIndex = tempIndex;
                return frames[currentFrameIndex].Texture;
            }
            secondsElapsed -= frames[tempIndex].Duration;
            tempIndex++;
            tempIndex %= frames.Count;
        }
        while (tempIndex != currentFrameIndex);
        return null;
    }

    [Serializable]
    public class FrameDetails
    {
        [SerializeField] Texture texture;
        [Min(0)]
        [SerializeField] float duration = 0.1f;

        public Texture Texture { get => texture; }
        public float Duration { get => duration; }
    }
}
