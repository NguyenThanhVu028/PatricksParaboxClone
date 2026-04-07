using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsManager : MonoBehaviour
{
    private static AnimationsManager instance;
    public static AnimationsManager Instance { get => instance; }

    [SerializeField] List<TextureAnimationDetails> normalTextureAnimations = new();
    [SerializeField] List<TextureAnimationDetails> playerFacesTextureAnimation = new();

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }

    private void Start()
    {
        foreach (var aniDetail in normalTextureAnimations)
        {
            if (aniDetail == null) continue;
            aniDetail.Animation.StartAnimation();
        }
        foreach (var aniDetail in playerFacesTextureAnimation)
        {
            if (aniDetail == null) continue;
            aniDetail.Animation.StartAnimation();
        }
    }

    public CustomTextureAnimation GetNormalTextureAnimation(string id)
    {
        foreach(var aniDetail in normalTextureAnimations)
        {
            if (aniDetail == null) continue;
            if (aniDetail.AniID == id) return aniDetail.Animation;
        }
        return null;
    }

    public CustomTextureAnimation GetPlayerFaceTextureAnimation(string id)
    {
        // Get the real id by calling API to the server -> Develop later
        foreach (var aniDetail in playerFacesTextureAnimation)
        {
            if (aniDetail == null) continue;
            if (aniDetail.AniID == id) return aniDetail.Animation;
        }
        return null;
    }

    [Serializable]
    public class TextureAnimationDetails
    {
        [SerializeField] string aniID = "";
        [SerializeField] CustomTextureAnimation animation;

        public string AniID { get => aniID; }
        public CustomTextureAnimation Animation { get => animation; }
    }
}
