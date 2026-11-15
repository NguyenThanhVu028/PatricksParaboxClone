using System;
using System.Collections.Generic;

public abstract class MyObject
{
    protected List<MyComponent> _components = new();
    protected RelativeTransform _transform;
    protected bool _isActive = true;

    public event Action OnEnable;
    public event Action OnDisable;
    public event Action OnStart;
    public event Action<float> OnUpdate;
    public event Action<float> OnLateUpdate;
    public event Action<float> OnFixedUpdate;

    public RelativeTransform Transform
    {
        get => _transform;
        set
        {
            RemoveComponent(_transform);
            _transform = value;
            RegisterComponent(value);
        }
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
                SubscribeLifeCycle();
                OnEnable?.Invoke();
            }
            else
            {
                UnSubscribeLifeCycle();
                OnDisable?.Invoke();
            }

            _isActive = value;
        }
    }

    protected void SubscribeLifeCycle()
    {
        Vux.Core.OnUpdate += OnUpdate;
        Vux.Core.OnLateUpdate += OnLateUpdate;
        Vux.Core.OnFixedUpdate += OnFixedUpdate;
    }

    protected void UnSubscribeLifeCycle()
    {
        Vux.Core.OnUpdate -= OnUpdate;
        Vux.Core.OnLateUpdate -= OnLateUpdate;
        Vux.Core.OnFixedUpdate -= OnFixedUpdate;
    }

    public MyObject()
    {
        // Every MyObject must have the transform component
        Transform = new RelativeTransform();

        SubscribeLifeCycle();
    }

    public virtual bool RegisterComponent(MyComponent component)
    {
        if (component == null)
        {
            return false;
        }

        if (component.RegisterObject(this))
        {
            _components.Add(component);
            return true;
        }

        return false;
    }

    public virtual void RemoveComponent(MyComponent component)
    {
        if (component == null)
        {
            return;
        }
        _components.Remove(component);
    }

    public T GetComponent<T>() where T : MyComponent
    {
        foreach (var component in _components)
        {
            if (component == null)
            {
                continue;
            }

            if (component is T result)
            {
                return result;
            }
        }

        return null;
    }

    public bool TryGetComponent<T>(out T resultComponent) where T : MyComponent
    {
        foreach (var component in _components)
        {
            if (component == null)
            {
                continue;
            }

            if (component is T result)
            {
                resultComponent = result;
                return true;
            }
        }

        resultComponent = null;
        return false;
    }

    public List<T> GetComponents<T>() where T : MyComponent
    {
        List<T> result = null;

        foreach (var component in _components)
        {
            if (component == null)
            {
                continue;
            }

            if (component is T value)
            {
                result ??= new();
                result.Add(value);
            }
        }

        return result;
    }
}
