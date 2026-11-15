using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TexturePack", menuName = "Scriptable Objects/TexturePack")]
public class TexturePack : ScriptableObject
{
    [SerializeField] List<NormalTextureInfo> _normalTextures = new();
    [SerializeField] List<AnimatedTextureInfo> _animatedTexture = new();

    public MyTexture2D GetTexture(TextureName textureName)
    {
        foreach (var texture in _normalTextures)
        {
            if (texture.TextureName == textureName)
            {
                return texture.Texture;
            }
        }

        foreach (var texture in _animatedTexture)
        {
            if (texture.TextureName == textureName)
            {
                return texture.Texture;
            }
        }

        return null;
    }

    [Serializable]
    public struct NormalTextureInfo
    {
        public TextureName TextureName;
        public NormalTexture Texture;
    }

    [Serializable]
    public struct AnimatedTextureInfo
    {
        public TextureName TextureName;
        public AnimatedTexture Texture;
    }
}
