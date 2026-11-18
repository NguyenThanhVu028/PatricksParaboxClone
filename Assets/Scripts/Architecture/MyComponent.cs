public class MyComponent
{
    protected MyObject _thisObject;
    protected bool _isActive = true;

    public MyObject ThisObject
    {
        get => _thisObject;
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
        _thisObject = myObject;
        SubscribeObjectLifeCycle();
        return true;
    }

    protected virtual void SubscribeObjectLifeCycle()
    {
        if (_thisObject == null)
        {
            return;
        }

        _thisObject.OnEnable += OnEnable;
        _thisObject.OnDisable += OnDisable;
        _thisObject.OnStart += OnStart;
        _thisObject.OnUpdate += OnUpdate;
        _thisObject.OnLateUpdate += OnLateUpdate;
        _thisObject.OnFixedUpdate += OnFixedUpdate;
    }

    protected virtual void UnSubscribeObjectLifeCycle()
    {
        if (_thisObject == null)
        {
            return;
        }

        _thisObject.OnEnable -= OnEnable;
        _thisObject.OnDisable -= OnDisable;
        _thisObject.OnStart -= OnStart;
        _thisObject.OnUpdate -= OnUpdate;
        _thisObject.OnLateUpdate -= OnLateUpdate;
        _thisObject.OnFixedUpdate -= OnFixedUpdate;
    }
    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate(float dt) { }
    protected virtual void OnLateUpdate(float dt) { }
    protected virtual void OnFixedUpdate(float fixedDt) { }
}
