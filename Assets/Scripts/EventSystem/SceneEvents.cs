using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneEvents : EventPublisher
{
    private static SceneEvents _instance = null;

    private static SceneEvents Instance
    {
        get
        {
            if (_instance == null)
            {
                SceneManager.sceneUnloaded += OnSceneUnload;
                _instance = new();
            }

            return _instance;
        }
    }

    private static void OnSceneUnload(Scene scene)
    {
        // Refresh the instance after each scene
        _instance = null;
        SceneManager.sceneUnloaded -= OnSceneUnload;
    }
}
