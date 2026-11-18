public class BaseCube : MyObject
{
    protected ColorType _color;

    protected BaseRenderer _renderer;

    public ColorType Color
    {
        get => _color;
        set => _color = value;
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
}
