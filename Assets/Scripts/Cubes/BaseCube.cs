using System;

public class BaseCube : MyObject
{
    protected ColorType _color;
    protected bool _isPlayer;
    protected bool _isPossessable;
    protected bool _isSecondaryPlayer;

    protected BaseRenderer _renderer;

    public ColorType Color
    {
        get => _color;
        set => _color = value;
    }

    public bool IsPlayer
    {
        get => _isPlayer;
        set => _isPlayer = value;
    }

    public bool IsPossessable
    {
        get => _isPossessable;
    }

    public bool IsSecondaryPlayer
    {
        get => _isSecondaryPlayer;
    }

    public BaseRenderer Renderer
    {
        get => _renderer;
        set
        {
            RemoveComponent(_renderer);
            _renderer = value;
            RegisterComponent(_renderer);
        }
    }

    public BaseCube(bool isActive) : base(isActive) { }

    public void Init(CubeDetails cubeDetails)
    {
        _color = Enum.Parse<ColorType>(cubeDetails.ColorID);
        _isPlayer = cubeDetails.IsPlayer;
        _isPossessable = cubeDetails.IsPossessable;
        _isSecondaryPlayer = cubeDetails.IsSecondaryPlayer;
    }
}
