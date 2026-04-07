using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsManager : MonoBehaviour
{
    private static AnimationsManager instance;
    public static AnimationsManager Instance { get => instance; }

    [SerializeField] List<CustomTextureAnimation> normalTextureAnimations = new();
    [SerializeField] List<CustomTextureAnimation> playerFacesTextureAnimation = new();

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }

    private void Start()
    {
        foreach (var animation in normalTextureAnimations)
        {
            if (animation == null) continue;
            animation.StartAnimation();
        }
        foreach (var animation in playerFacesTextureAnimation)
        {
            if (animation == null) continue;
            animation.StartAnimation();
        }
    }

    public CustomTextureAnimation GetNormalTextureAnimation(string id)
    {
        foreach(var animation in normalTextureAnimations)
        {
            if (animation == null) continue;
            if (animation.ID == id) return animation;
        }
        return null;
    }

    public CustomTextureAnimation GetPlayerFaceTextureAnimation(string id)
    {
        // Get the real id by calling API to the server -> Develop later
        foreach (var animation in playerFacesTextureAnimation)
        {
            if (animation == null) continue;
            if (animation.ID == id) return animation;
        }
        return null;
    }

    //[Serializable]
    //public class TextureAnimationDetails
    //{
    //    [SerializeField] string aniID = "";
    //    [SerializeField] CustomTextureAnimation animation;

    //    public string AniID { get => aniID; }
    //    public CustomTextureAnimation Animation { get => animation; }
    //}
}
