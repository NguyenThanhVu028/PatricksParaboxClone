using UnityEngine;

public class NormalTexture : MyTexture2D
{
    [SerializeField] private Texture2D _texture;
    public override Texture2D GetTexture()
    {
        return _texture;
    }
}
