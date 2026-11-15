using UnityEngine;

[CreateAssetMenu(fileName = "CustomTexture", menuName = "Scriptable Objects/CustomTexture")]
public class CustomTexture : ScriptableObject
{
    [SerializeField] Texture2D texture;
    public virtual Texture2D GetTexture()
    {
        if (texture == null) return Texture2D.whiteTexture;
        return texture;
    }
}
