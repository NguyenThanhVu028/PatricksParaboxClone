using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedTexture : MyTexture2D
{
    [SerializeField] bool _autoStart = true;
    [SerializeField] List<Frame> _frames = new();

    private bool _hasStarted;
    private bool _isRunning = false;

    private int _currentFrameIndex = 0;
    private float _lastFrameTime;
    public override Texture2D GetTexture()
    {
        if (_autoStart && !_hasStarted)
        {
            StartAnimation();
        }

        if (_isRunning)
        {
            float _elapsedTime = Time.timeSinceLevelLoad - _lastFrameTime;

            if (_elapsedTime >= _frames[_currentFrameIndex].Duration)
            {
                _lastFrameTime = Time.timeSinceLevelLoad;
            }

            while (_elapsedTime >= _frames[_currentFrameIndex].Duration)
            {
                _currentFrameIndex = (_currentFrameIndex + 1) % _frames.Count;
                _elapsedTime -= _frames[_currentFrameIndex].Duration;
            }
        }

        return _frames[_currentFrameIndex].Texture;
    }

    public void StartAnimation()
    {
        if (_isRunning)
        {
            return;
        }

        _hasStarted = true;
        _isRunning = true;

        _lastFrameTime = Time.timeSinceLevelLoad;
    }

    public void StopAnimation()
    {
        _isRunning = false;
    }

    [Serializable]
    public struct Frame
    {
        public float Duration;
        public Texture2D Texture;
    }
}
