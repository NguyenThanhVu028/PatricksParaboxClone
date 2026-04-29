using UnityEngine;

[CreateAssetMenu(fileName = "CustomTexture", menuName = "Scriptable Objects/CustomTexture")]
public class CustomTexture : ScriptableObject
{
    [SerializeField] Texture texture;
    public virtual Texture GetTexture()
    {
        if (texture == null) return Texture2D.whiteTexture;
        return texture;
    }
}
