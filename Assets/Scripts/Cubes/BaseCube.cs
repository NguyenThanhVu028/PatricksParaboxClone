public class BaseCube : MyObject
{
    private BaseRenderer _renderer;

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
}
