public class MyComponent
{
    protected MyObject _object;
    protected bool _isActive = true;

    public MyObject Object
    {
        get => _object;
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (value == _isActive)
            {
                return;
            }

            if (value)
            {
                OnEnable();
                SubscribeObjectLifeCycle();
            }
            else
            {
                OnDisable();
                UnSubscribeObjectLifeCycle();
            }

            _isActive = value;
        }
    }

    public bool RegisterObject(MyObject myObject)
    {
        UnSubscribeObjectLifeCycle();
        _object = myObject;
        SubscribeObjectLifeCycle();
        return true;
    }

    protected void SubscribeObjectLifeCycle()
    {
        if (_object == null)
        {
            return;
        }

        _object.OnEnable += OnEnable;
        _object.OnDisable += OnDisable;
        _object.OnStart += OnStart;
        _object.OnUpdate += OnUpdate;
        _object.OnLateUpdate += OnLateUpdate;
        _object.OnFixedUpdate += OnFixedUpdate;
    }

    protected void UnSubscribeObjectLifeCycle()
    {
        if (_object == null)
        {
            return;
        }

        _object.OnEnable -= OnEnable;
        _object.OnDisable -= OnDisable;
        _object.OnStart -= OnStart;
        _object.OnUpdate -= OnUpdate;
        _object.OnLateUpdate -= OnLateUpdate;
        _object.OnFixedUpdate -= OnFixedUpdate;
    }
    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate(float dt) { }
    protected virtual void OnLateUpdate(float dt) { }
    protected virtual void OnFixedUpdate(float fixedDt) { }
}
